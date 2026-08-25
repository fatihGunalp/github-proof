using System.Text.Json;
using System.Text.Json.Nodes;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// S2 of docs/architecture/public-profile-runtime-collection.md §§18-19: the
// first real REST <-> GraphQL parity observation, on `languages`.
//
// It reads the complete language map over GraphQL and compares it with the REST
// map the caller already collected. It returns an observation and nothing else —
// no DataAvailability, no domain status, no evidence (§18.7). REST stays
// authoritative for the surface no matter what this concludes (§18.1).
//
// Deliberately absent: `totalCount`, `isEmpty`, and every field this comparison
// does not need. §18.12 forbids adopting exact-total provider fields, and
// requesting one that is never read would still be a step toward it.
public sealed class GitHubGraphQlLanguagesShadow : IGitHubGraphQlLanguagesShadow
{
    // The GraphQL connection maximum. Not a cap on the comparison: pagination
    // below follows `endCursor` until the connection is exhausted, so the map is
    // always complete or the comparison is NotCompared. No sampling (§7).
    private const int PageSize = 100;

    // Repository identity travels in variables, never interpolated into the
    // query text, so the operation is a constant and the identity stays a
    // parameter.
    private const string LanguagesQuery = """
        query ProfileRepositoryLanguages(
          $owner: String!
          $name: String!
          $first: Int!
          $after: String
        ) {
          repository(owner: $owner, name: $name) {
            languages(first: $first, after: $after) {
              pageInfo {
                hasNextPage
                endCursor
              }
              edges {
                size
                node {
                  name
                }
              }
            }
          }
        }
        """;

    private readonly IGitHubGraphQlTransport transport;

    public GitHubGraphQlLanguagesShadow(IGitHubGraphQlTransport transport)
    {
        this.transport = transport
            ?? throw new ArgumentNullException(nameof(transport));
    }

    public async Task<GraphQlLanguagesShadowOutcome> CompareAsync(
        GitHubRepositoryUrl repository,
        IReadOnlyDictionary<string, long> restLanguages,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(restLanguages);

        GraphQlLanguagesShadowState? state = GraphQlLanguagesShadowScope.Current;

        // §18.6: once an envelope-level failure has disabled the shadow for this
        // execution, every later repository skips it entirely — no POST at all,
        // so a systemic failure cannot be multiplied by the admitted repository
        // count.
        if (state?.IsDisabledForRun == true)
        {
            return Record(state, GraphQlLanguagesShadowOutcome.NotCompared);
        }

        // Same comparer as the REST collector and the domain snapshot, so
        // "equal" means equal under exactly the rules the production path uses.
        Dictionary<string, long> observed = new(StringComparer.OrdinalIgnoreCase);
        string? after = null;

        while (true)
        {
            GitHubGraphQlResponse response = await transport.SendAsync(
                CreateRequest(repository, after),
                cancellationToken);

            // §18.6 envelope-level: no response, a non-200 status, or a body
            // this transport could not read as a GraphQL envelope at all. Both
            // disable the shadow for the rest of the execution.
            //
            // DataWithErrors deliberately does NOT disable. §18.4 makes it
            // unusable for THIS repository, and it stays a per-repository
            // abstention.
            //
            // Session 125 (G-2) corrects the earlier reasoning here. One bounded
            // structural fact does exist — a DataWithErrors envelope can carry a
            // null `data`, which is the shape of a top-level failure. But that
            // fact alone does not PROVE an authentication failure, an
            // authorization failure, a primary or secondary rate limit, or any
            // other provider-wide fault: an ordinary top-level query error looks
            // the same. Disabling the whole run on it would be inference, not
            // classification.
            //
            // Message text is still not parsed and error codes are still not
            // invented. Session 126 narrowed what remains unproven: a 200 whose
            // HEADERS report the condition is now classified (below), and only
            // the shape GitHub identifies through the message alone —
            // remaining > 0, no Retry-After — is left open. That residual shape
            // is the named pre-enablement blocker, and it is not papered over
            // here.
            //
            // Session 126 adds the third case: an HTTP 200 errored envelope whose
            // HEADERS report a provider-wide condition — zero remaining GraphQL
            // points, or a Retry-After the provider set. §18.6 already names a
            // primary or secondary rate-limit response as envelope-level, so
            // acting on it is applying that rule, not extending it. The error
            // text is still never read.
            if (response.Outcome is GitHubGraphQlOutcome.EnvelopeFailure
                or GitHubGraphQlOutcome.Malformed
                || response.ReportsProviderWideCondition)
            {
                state?.DisableForRun();

                return Record(state, GraphQlLanguagesShadowOutcome.NotCompared);
            }

            // §18.4: only a clean Success may be read as an observation. A
            // partial payload alongside `errors` is unresolved, not data.
            if (!response.IsSuccess || response.Data is null)
            {
                return Record(state, GraphQlLanguagesShadowOutcome.NotCompared);
            }

            if (!TryReadPage(response.Data.Value, observed, out bool hasNextPage, out string? endCursor))
            {
                return Record(state, GraphQlLanguagesShadowOutcome.NotCompared);
            }

            if (!hasNextPage)
            {
                break;
            }

            // A connection that claims another page without handing back a
            // cursor cannot be enumerated deterministically. Stopping here is
            // the only honest answer; looping on the same cursor would be worse
            // than saying nothing.
            if (string.IsNullOrEmpty(endCursor)
                || string.Equals(endCursor, after, StringComparison.Ordinal))
            {
                return Record(state, GraphQlLanguagesShadowOutcome.NotCompared);
            }

            after = endCursor;
        }

        return Record(
            state,
            IsSameMap(restLanguages, observed)
                ? GraphQlLanguagesShadowOutcome.Match
                : GraphQlLanguagesShadowOutcome.Mismatch);
    }

    // Bounded structural observation only (§18.9): three counters and a boolean,
    // never a repository, a language, a byte count or any payload or error text.
    // With no ambient scope — a direct unit-test call — nothing is recorded and
    // the outcome is simply returned.
    private static GraphQlLanguagesShadowOutcome Record(
        GraphQlLanguagesShadowState? state,
        GraphQlLanguagesShadowOutcome outcome)
    {
        state?.Record(outcome);

        return outcome;
    }

    private static GitHubGraphQlRequest CreateRequest(
        GitHubRepositoryUrl repository,
        string? after)
    {
        JsonObject variables = new()
        {
            ["owner"] = repository.Owner,
            ["name"] = repository.Name,
            ["first"] = PageSize
        };

        if (after is not null)
        {
            variables["after"] = after;
        }

        return new GitHubGraphQlRequest(LanguagesQuery, variables);
    }

    // Returns false for any structurally ambiguous or unreadable page. The
    // caller turns that into NotCompared rather than guessing: a shadow that
    // guesses is worse than a shadow that abstains.
    private static bool TryReadPage(
        JsonElement data,
        Dictionary<string, long> observed,
        out bool hasNextPage,
        out string? endCursor)
    {
        hasNextPage = false;
        endCursor = null;

        if (!data.TryGetProperty("repository", out JsonElement repository)
            || repository.ValueKind != JsonValueKind.Object
            || !repository.TryGetProperty("languages", out JsonElement languages)
            || languages.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!languages.TryGetProperty("edges", out JsonElement edges)
            || edges.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (JsonElement edge in edges.EnumerateArray())
        {
            if (edge.ValueKind != JsonValueKind.Object
                || !edge.TryGetProperty("size", out JsonElement size)
                || size.ValueKind != JsonValueKind.Number
                || !size.TryGetInt64(out long bytes)
                || bytes < 0)
            {
                return false;
            }

            if (!edge.TryGetProperty("node", out JsonElement node)
                || node.ValueKind != JsonValueKind.Object
                || !node.TryGetProperty("name", out JsonElement name)
                || name.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            string? language = name.GetString();

            if (string.IsNullOrWhiteSpace(language))
            {
                return false;
            }

            // The REST collector throws on a duplicate language rather than
            // letting one win. The shadow must not be more permissive than the
            // path it is checking, and it must not throw into the production
            // call, so a duplicate makes the comparison unavailable.
            if (!observed.TryAdd(language.Trim(), bytes))
            {
                return false;
            }
        }

        if (!languages.TryGetProperty("pageInfo", out JsonElement pageInfo)
            || pageInfo.ValueKind != JsonValueKind.Object
            || !pageInfo.TryGetProperty("hasNextPage", out JsonElement next)
            || (next.ValueKind != JsonValueKind.True && next.ValueKind != JsonValueKind.False))
        {
            return false;
        }

        hasNextPage = next.ValueKind == JsonValueKind.True;

        if (pageInfo.TryGetProperty("endCursor", out JsonElement cursor)
            && cursor.ValueKind == JsonValueKind.String)
        {
            endCursor = cursor.GetString();
        }

        return true;
    }

    // Semantic equality of the complete maps. Both sides are keyed with the same
    // case-insensitive comparer, so GraphQL edge order cannot affect the result.
    private static bool IsSameMap(
        IReadOnlyDictionary<string, long> rest,
        Dictionary<string, long> observed)
    {
        if (rest.Count != observed.Count)
        {
            return false;
        }

        foreach ((string language, long bytes) in rest)
        {
            if (!observed.TryGetValue(language, out long observedBytes)
                || observedBytes != bytes)
            {
                return false;
            }
        }

        return true;
    }
}
