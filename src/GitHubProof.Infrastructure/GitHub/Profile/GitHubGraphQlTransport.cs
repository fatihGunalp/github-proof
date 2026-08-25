using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// S1 of docs/architecture/public-profile-runtime-collection.md §§18-19.
//
// One physical POST /graphql on the EXISTING profile named client, so the
// Profile-only observation handler sees it exactly once and Repository Proof
// traffic is untouched (§3.1). It adds no client, no credential source and no
// package.
//
// Authorization is deliberately absent from this file: it belongs to the named
// client's own configuration and is never read, copied or logged here.
//
// The REST collectors send X-GitHub-Api-Version because the REST API is
// versioned that way; GraphQL is not, so that header is deliberately NOT
// invented here. The User-Agent is sent because GitHub requires one on every
// API request, REST and GraphQL alike.
//
// This transport creates no DataAvailability and no domain status (§18.7). Its
// whole job is to make §18.4's four states structurally distinguishable, so
// that an HTTP 200 carrying `errors` can never be mistaken for ordinary
// success.
public sealed class GitHubGraphQlTransport : IGitHubGraphQlTransport
{
    private const string Endpoint = "graphql";

    private readonly HttpClient httpClient;

    // The client arrives already configured — base address, timeout and
    // authorization all belong to the profile named client's registration. This
    // transport trusts that dependency contract and mutates nothing on it.
    public GitHubGraphQlTransport(HttpClient httpClient)
    {
        this.httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<GitHubGraphQlResponse> SendAsync(
        GitHubGraphQlRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpStatusCode? statusCode = null;

        try
        {
            using HttpRequestMessage message = CreateRequest(request);
            using HttpResponseMessage response = await httpClient.SendAsync(
                message,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            statusCode = response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                return GitHubGraphQlResponse.EnvelopeFailure(statusCode);
            }

            // Session 126. Headers only, and read before the body so the
            // classification cannot depend on anything inside it.
            GitHubGraphQlProviderSignal providerSignal = ReadProviderSignal(response);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            return ReadEnvelope(body, statusCode, providerSignal);
        }
        // Caller cancellation is not a GraphQL failure and must not be converted
        // into one (§11.1, §18.8). It keeps propagating exactly as every REST
        // collector already lets it propagate.
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        // Session 125 (G-1). A cancellation whose supplied token is NOT
        // cancelled is not a caller cancellation at all — it is this client's
        // own transport timeout, which HttpClient surfaces as a
        // TaskCanceledException. §18.6 names transport failure and server-side
        // timeout as envelope-level, so it is reported as one; otherwise a
        // systemic timeout would escape the run-scoped disable and be paid once
        // per admitted repository.
        //
        // The filter above runs first, so a genuine caller or execution-deadline
        // cancellation can never reach here.
        catch (OperationCanceledException)
        {
            return GitHubGraphQlResponse.EnvelopeFailure(statusCode);
        }
        // No response, or a broken one: envelope-level by §18.6. The exception
        // message is not carried anywhere — nothing downstream may branch on it
        // and nothing may log it.
        catch (HttpRequestException)
        {
            return GitHubGraphQlResponse.EnvelopeFailure(statusCode);
        }
    }

    private static HttpRequestMessage CreateRequest(GitHubGraphQlRequest request)
    {
        // DeepClone keeps the caller's JsonObject reusable: a JsonNode may have
        // only one parent, so attaching the caller's instance directly would
        // make a second send throw.
        JsonObject body = new()
        {
            ["query"] = request.Query
        };

        if (request.Variables is not null)
        {
            body["variables"] = request.Variables.DeepClone();
        }

        HttpRequestMessage message = new(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(
                body.ToJsonString(),
                Encoding.UTF8,
                "application/json")
        };

        message.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        message.Headers.UserAgent.Add(new ProductInfoHeaderValue("GitHubProof", "0.1"));

        return message;
    }

    // The §18.4 decision, made structurally and from the envelope shape only —
    // never from an error message's wording.
    // Session 126 (§18.6). The two conditions GitHub reports through headers on
    // an otherwise-200 response. Deliberately nothing else: the secondary
    // rate-limit shape that GitHub identifies only through the error message is
    // NOT classified here, because reading that message is forbidden and no
    // documented stable structural discriminator exists for it.
    //
    // `x-ratelimit-resource: graphql` is normal for every GraphQL response and
    // is never treated as a signal.
    private static GitHubGraphQlProviderSignal ReadProviderSignal(
        HttpResponseMessage response)
    {
        if (IsRateLimitExhausted(response))
        {
            return GitHubGraphQlProviderSignal.RateLimitExhausted;
        }

        return HasRetryAfter(response)
            ? GitHubGraphQlProviderSignal.RetryAfterRequested
            : GitHubGraphQlProviderSignal.None;
    }

    // Exactly zero. A malformed, absent or repeated header is not a signal:
    // guessing would be the inference this session exists to avoid.
    private static bool IsRateLimitExhausted(HttpResponseMessage response) =>
        response.Headers.TryGetValues("x-ratelimit-remaining", out IEnumerable<string>? values)
        && values.Count() == 1
        && int.TryParse(
            values.Single(),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out int remaining)
        && remaining == 0;

    // Present and readable as either delta-seconds or a timestamp. The value
    // itself is not retained: this slice implements no retry or backoff, so the
    // bounded fact that the provider asked us to wait is the whole signal.
    //
    // The timestamp branch is DateTimeOffset.TryParse, which accepts more than
    // the strict HTTP-date grammar. That looseness is deliberate and one-way:
    // the only consequence of accepting a value GitHub would not have sent is
    // that the shadow stops early, which costs an observation and can never
    // cost evidence — REST supplies the surface either way.
    private static bool HasRetryAfter(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Retry-After", out IEnumerable<string>? values)
            || values.Count() != 1)
        {
            return false;
        }

        string value = values.Single();

        // NumberStyles.None rejects a sign, so a negative delta never parses.
        return int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out _)
            || DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out _);
    }

    private static GitHubGraphQlResponse ReadEnvelope(
        string body,
        HttpStatusCode? statusCode,
        GitHubGraphQlProviderSignal providerSignal)
    {
        JsonElement? data;
        int errorCount;

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            JsonElement root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return GitHubGraphQlResponse.Malformed(statusCode);
            }

            if (HasDuplicateContractMember(root))
            {
                return GitHubGraphQlResponse.Malformed(statusCode);
            }

            if (!TryReadErrorCount(root, out errorCount))
            {
                // An `errors` member that is not an array is not an envelope
                // this transport can reason about. Treating it as success would
                // be exactly the optimistic read §18.4 forbids.
                return GitHubGraphQlResponse.Malformed(statusCode);
            }

            // Clone so the element outlives the JsonDocument it was parsed from.
            data = root.TryGetProperty("data", out JsonElement dataElement)
                && dataElement.ValueKind == JsonValueKind.Object
                    ? dataElement.Clone()
                    : null;
        }
        catch (JsonException)
        {
            return GitHubGraphQlResponse.Malformed(statusCode);
        }

        GitHubGraphQlRateLimit? rateLimit = ReadRateLimit(data);

        if (errorCount > 0)
        {
            return GitHubGraphQlResponse.DataWithErrors(
                data,
                errorCount,
                rateLimit,
                providerSignal);
        }

        // No errors and no readable `data`: an empty or `"data": null` envelope.
        // This is NOT successful absence — it is a body this transport could not
        // read as an observation at all.
        return data is null
            ? GitHubGraphQlResponse.Malformed(statusCode)
            : GitHubGraphQlResponse.Success(data.Value, rateLimit);
    }

    // `data` and `errors` are the two contract-bearing members of a GraphQL
    // envelope, and System.Text.Json resolves a duplicate member name to the
    // LAST occurrence. That precedence would let
    // {"data":{…},"errors":[{…}],"errors":[]} read as ordinary Success — an
    // errored response becoming success, which is precisely what §18.4 forbids.
    //
    // A duplicated member means the envelope cannot be reasoned about at all, so
    // it is Malformed rather than resolved by an arbitrary precedence rule. The
    // conservative outcome is required wherever two readings exist (§18.4).
    private static bool HasDuplicateContractMember(JsonElement root)
    {
        int data = 0;
        int errors = 0;

        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (property.NameEquals("data"))
            {
                data++;
            }
            else if (property.NameEquals("errors"))
            {
                errors++;
            }
        }

        return data > 1 || errors > 1;
    }

    private static bool TryReadErrorCount(JsonElement root, out int errorCount)
    {
        errorCount = 0;

        if (!root.TryGetProperty("errors", out JsonElement errors))
        {
            return true;
        }

        if (errors.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (errors.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        errorCount = errors.GetArrayLength();

        return true;
    }

    // Bounded numeric and time fields only. Anything unreadable simply yields
    // less context; it never changes the outcome and never fails the send.
    private static GitHubGraphQlRateLimit? ReadRateLimit(JsonElement? data)
    {
        if (data is null
            || !data.Value.TryGetProperty("rateLimit", out JsonElement rateLimit)
            || rateLimit.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return new GitHubGraphQlRateLimit(
            ReadInt32(rateLimit, "cost"),
            ReadInt32(rateLimit, "limit"),
            ReadInt32(rateLimit, "remaining"),
            ReadInt32(rateLimit, "used"),
            ReadTimestamp(rateLimit, "resetAt"));
    }

    private static int? ReadInt32(JsonElement owner, string propertyName) =>
        owner.TryGetProperty(propertyName, out JsonElement value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out int number)
                ? number
                : null;

    private static DateTimeOffset? ReadTimestamp(JsonElement owner, string propertyName) =>
        owner.TryGetProperty(propertyName, out JsonElement value)
            && value.ValueKind == JsonValueKind.String
            && value.TryGetDateTimeOffset(out DateTimeOffset timestamp)
                ? timestamp
                : null;
}
