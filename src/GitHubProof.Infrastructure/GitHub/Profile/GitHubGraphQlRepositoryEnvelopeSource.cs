using System.Text.Json;
using System.Text.Json.Nodes;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// S3 of docs/architecture/public-profile-runtime-collection.md §§18-19: the
// first AUTHORITATIVE hybrid envelope. One POST replaces the REST repository
// metadata GET and the REST languages GET for a repository whose candidate is
// accepted.
//
// Three surfaces are adopted and no more: repository metadata, topics and the
// complete language map. Everything else — README, license, the recursive tree,
// file contents, issues, commits, pull requests, releases and tags — stays REST
// (§18.2), and the `DecisionBounded` surfaces stay out because §19.7 ordering
// parity is unproven. The default-branch head and tree identity stay out because
// §18.13 empty-repository parity is a separate proof obligation; only the branch
// NAME is read here.
//
// SINGLE-ENVELOPE COMPLETENESS. The candidate is accepted only when the initial
// response contains all three surfaces completely. `hasNextPage` on either
// adopted connection discards the candidate — the repository is then collected
// over REST in full. That is a fallback, never a cap (§7): no evidence is
// omitted and nothing is truncated. It keeps §18.7 simple, because one accepted
// physical envelope means exactly one accepted retrieval instant.
//
// Deliberately absent from the query: `totalCount`, `totalSize`, `isEmpty`,
// history, pull requests, releases, refs, tree objects, README and
// `licenseInfo`. §18.3 and §18.12 refuse the first three as evidence-strength
// upgrades, and requesting a field that is never read would still be a step
// toward adopting it.
public sealed class GitHubGraphQlRepositoryEnvelopeSource
    : IGitHubGraphQlRepositoryEnvelopeSource
{
    // The GraphQL connection maximum. It is not a cap on evidence: a connection
    // that does not fit is discarded rather than truncated, and REST then
    // collects the whole surface.
    private const int PageSize = 100;

    // Repository identity travels in variables, never interpolated into the
    // query text, so the operation is a constant.
    private const string RepositoryQuery = """
        query ProfileRepositoryEnvelope(
          $owner: String!
          $name: String!
          $topics: Int!
          $languages: Int!
        ) {
          repository(owner: $owner, name: $name) {
            name
            owner {
              login
            }
            isPrivate
            isArchived
            isFork
            isTemplate
            description
            homepageUrl
            createdAt
            hasIssuesEnabled
            primaryLanguage {
              name
            }
            defaultBranchRef {
              name
            }
            repositoryTopics(first: $topics) {
              pageInfo {
                hasNextPage
              }
              nodes {
                topic {
                  name
                }
              }
            }
            languages(first: $languages) {
              pageInfo {
                hasNextPage
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
    private readonly TimeProvider timeProvider;

    public GitHubGraphQlRepositoryEnvelopeSource(
        IGitHubGraphQlTransport transport,
        TimeProvider? timeProvider = null)
    {
        this.transport = transport
            ?? throw new ArgumentNullException(nameof(transport));
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<GitHubGraphQlRepositoryEnvelope?> ResolveAsync(
        GitHubRepositoryUrl repository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        ProfileGraphQlExecutionState? state = ProfileGraphQlExecutionScope.Current;

        // §18.6: once an envelope-level failure has disabled GraphQL for this
        // execution, every later repository skips it entirely — no POST at all,
        // so a systemic failure cannot be multiplied by the admitted repository
        // count. The repository is still collected in full, over REST.
        if (state?.IsDisabledForRun == true)
        {
            return Fallback(state);
        }

        GitHubGraphQlResponse response = await transport.SendAsync(
            CreateRequest(repository),
            cancellationToken);

        // §18.6 envelope-level: no response, a non-200 status, a body that could
        // not be read as a GraphQL envelope at all, or — Session 126 — an
        // HTTP 200 errored envelope whose HEADERS report a provider-wide
        // condition. All of them disable GraphQL for the rest of the execution.
        //
        // Error text is never read. The Session 126 residual stands: a secondary
        // rate limit that GitHub identifies only through the error message is
        // still unclassifiable here, and it is not guessed at.
        if (response.Outcome is GitHubGraphQlOutcome.EnvelopeFailure
            or GitHubGraphQlOutcome.Malformed
            || response.ReportsProviderWideCondition)
        {
            state?.DisableForRun();

            return Fallback(state);
        }

        // §18.4: only a clean Success may supply a surface. A payload alongside
        // `errors` is unresolved, not data — and it is a per-repository
        // abstention, not a reason to disable the run.
        if (!response.IsSuccess || response.Data is null)
        {
            return Fallback(state);
        }

        GitHubGraphQlRepositoryEnvelope? envelope = TryRead(response.Data.Value);

        if (envelope is null)
        {
            return Fallback(state);
        }

        state?.RecordEnvelopeServed();

        return envelope;
    }

    // Bounded structural observation only (§18.9): two counters, never a
    // repository, a topic, a language, a byte count, a payload or error text.
    // They are not HTTP request counts and must never be summed with one.
    private static GitHubGraphQlRepositoryEnvelope? Fallback(
        ProfileGraphQlExecutionState? state)
    {
        state?.RecordEnvelopeFallback();

        return null;
    }

    private static GitHubGraphQlRequest CreateRequest(GitHubRepositoryUrl repository)
    {
        JsonObject variables = new()
        {
            ["owner"] = repository.Owner,
            ["name"] = repository.Name,
            ["topics"] = PageSize,
            ["languages"] = PageSize
        };

        return new GitHubGraphQlRequest(RepositoryQuery, variables);
    }

    // Returns null for every ambiguous, incomplete or invalid shape. The caller
    // turns that into a whole-repository REST fallback: there is no path here
    // that produces some of the three surfaces and leaves the rest to REST,
    // which is what §18.5's no-merge rule requires.
    private GitHubGraphQlRepositoryEnvelope? TryRead(JsonElement data)
    {
        if (!data.TryGetProperty("repository", out JsonElement repository)
            || repository.ValueKind != JsonValueKind.Object)
        {
            // Includes `repository: null`. A null repository is not observed
            // absence — it is an unusable candidate, and REST decides what the
            // repository actually is.
            return null;
        }

        if (!TryReadRequiredString(repository, "name", out string? name)
            || !TryReadOwnerLogin(repository, out string? owner)
            || !TryReadBoolean(repository, "isPrivate", out bool isPrivate)
            || !TryReadBoolean(repository, "isArchived", out bool isArchived)
            || !TryReadBoolean(repository, "isFork", out bool isFork)
            || !TryReadBoolean(repository, "isTemplate", out bool isTemplate)
            || !TryReadBoolean(repository, "hasIssuesEnabled", out bool hasIssues)
            || !TryReadTimestamp(repository, "createdAt", out DateTimeOffset createdAt)
            || !TryReadOptionalString(repository, "description", out string? description)
            || !TryReadOptionalString(repository, "homepageUrl", out string? homepage)
            || !TryReadNestedName(repository, "primaryLanguage", required: false, out string? primaryLanguage)
            || !TryReadNestedName(repository, "defaultBranchRef", required: true, out string? defaultBranch))
        {
            return null;
        }

        // A private repository has its own canonical REST outcome — a
        // NotApplicable metadata availability with a fixed reason. Reproducing
        // that here would be a second implementation of a Profile-visible
        // result, so the candidate is discarded and the REST collector answers
        // exactly as it does today.
        if (isPrivate)
        {
            return null;
        }

        if (!TryReadTopics(repository, out IReadOnlyList<string>? topics)
            || !TryReadLanguages(repository, out IReadOnlyDictionary<string, long>? languages))
        {
            return null;
        }

        // §18.7: one instant, captured only now — after the body was read,
        // parsed, structurally validated and accepted — and shared by every
        // surface this envelope supplies.
        DateTimeOffset retrievedAt = timeProvider.GetUtcNow();

        return new GitHubGraphQlRepositoryEnvelope(
            new RepositoryIdentity(
                owner!,
                name!,
                CreateCanonicalRepositoryUri(owner!, name!),
                defaultBranch!,
                isPublic: true,
                isArchived,
                isFork,
                isTemplate,
                primaryLanguage),
            new RepositoryMetadata(description, homepage, createdAt, hasIssues),
            topics!,
            languages!,
            retrievedAt);
    }

    // Byte-identical to the REST collector's construction, deliberately rather
    // than reading GraphQL `url`: the canonical URL is derived from owner and
    // name in both compositions, so it cannot diverge on a provider-side
    // formatting difference.
    private static Uri CreateCanonicalRepositoryUri(string owner, string name) =>
        new(
            $"https://github.com/{Uri.EscapeDataString(owner.Trim())}/{Uri.EscapeDataString(name.Trim())}",
            UriKind.Absolute);

    // Exactly the REST normalization: blanks ignored, values trimmed,
    // case-insensitive distinct, first occurrence kept.
    private static bool TryReadTopics(
        JsonElement repository,
        out IReadOnlyList<string>? topics)
    {
        topics = null;

        if (!TryReadCompleteConnection(repository, "repositoryTopics", out JsonElement connection)
            || !connection.TryGetProperty("nodes", out JsonElement nodes)
            || nodes.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        List<string> collected = [];

        foreach (JsonElement node in nodes.EnumerateArray())
        {
            if (node.ValueKind != JsonValueKind.Object
                || !node.TryGetProperty("topic", out JsonElement topic)
                || topic.ValueKind != JsonValueKind.Object
                || !topic.TryGetProperty("name", out JsonElement topicName)
                || topicName.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            string? value = topicName.GetString();

            // REST drops blank topics rather than failing on them, so this must
            // too. Anything structurally wrong above is a different case and
            // already discarded the candidate.
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            collected.Add(value.Trim());
        }

        topics = collected
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return true;
    }

    // `LanguageEdge.size` is the exact byte count, mapped onto the REST language
    // byte value with no percentage, no total and no rounding (§18.2).
    private static bool TryReadLanguages(
        JsonElement repository,
        out IReadOnlyDictionary<string, long>? languages)
    {
        languages = null;

        if (!TryReadCompleteConnection(repository, "languages", out JsonElement connection)
            || !connection.TryGetProperty("edges", out JsonElement edges)
            || edges.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        // The same comparer the REST collector and the domain snapshot use, so
        // the accepted map is the one the REST path would have produced.
        Dictionary<string, long> collected = new(StringComparer.OrdinalIgnoreCase);

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
                || !node.TryGetProperty("name", out JsonElement languageName)
                || languageName.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            string? language = languageName.GetString();

            if (string.IsNullOrWhiteSpace(language))
            {
                return false;
            }

            // The REST collector refuses a duplicate language rather than
            // letting one win. An authoritative envelope must not be more
            // permissive than the path it replaces, so a duplicate discards the
            // candidate and REST collects the surface.
            if (!collected.TryAdd(language.Trim(), bytes))
            {
                return false;
            }
        }

        languages = collected;

        return true;
    }

    // A connection is usable only when it is COMPLETE in this response.
    // `hasNextPage == true` discards the candidate: S3 does not paginate an
    // authoritative envelope, and it never truncates one either.
    private static bool TryReadCompleteConnection(
        JsonElement repository,
        string name,
        out JsonElement connection)
    {
        connection = default;

        if (!repository.TryGetProperty(name, out JsonElement value)
            || value.ValueKind != JsonValueKind.Object
            || !value.TryGetProperty("pageInfo", out JsonElement pageInfo)
            || pageInfo.ValueKind != JsonValueKind.Object
            || !pageInfo.TryGetProperty("hasNextPage", out JsonElement hasNextPage)
            || (hasNextPage.ValueKind != JsonValueKind.True
                && hasNextPage.ValueKind != JsonValueKind.False))
        {
            return false;
        }

        if (hasNextPage.ValueKind == JsonValueKind.True)
        {
            return false;
        }

        connection = value;

        return true;
    }

    private static bool TryReadOwnerLogin(JsonElement repository, out string? login)
    {
        login = null;

        return repository.TryGetProperty("owner", out JsonElement owner)
            && owner.ValueKind == JsonValueKind.Object
            && TryReadRequiredString(owner, "login", out login);
    }

    // `primaryLanguage` is legitimately null for a repository GitHub reports no
    // language for, exactly as REST `language` is. `defaultBranchRef` is not:
    // §18.13 forbids reading its null as evidence, so a required nested name
    // that is absent discards the candidate and REST decides.
    private static bool TryReadNestedName(
        JsonElement repository,
        string name,
        bool required,
        out string? value)
    {
        value = null;

        if (!repository.TryGetProperty(name, out JsonElement nested))
        {
            return !required;
        }

        if (nested.ValueKind == JsonValueKind.Null)
        {
            return !required;
        }

        return nested.ValueKind == JsonValueKind.Object
            && TryReadRequiredString(nested, "name", out value);
    }

    private static bool TryReadRequiredString(
        JsonElement owner,
        string name,
        out string? value)
    {
        value = null;

        if (!owner.TryGetProperty(name, out JsonElement property)
            || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        string? read = property.GetString();

        if (string.IsNullOrWhiteSpace(read))
        {
            return false;
        }

        value = read.Trim();

        return true;
    }

    // Absent and JSON null are both "no value", which is what REST reports for
    // an unset description or homepage. Any other kind is a shape this envelope
    // does not understand, and understanding it partially is what §18.5 forbids.
    private static bool TryReadOptionalString(
        JsonElement owner,
        string name,
        out string? value)
    {
        value = null;

        if (!owner.TryGetProperty(name, out JsonElement property)
            || property.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();

        return true;
    }

    private static bool TryReadBoolean(JsonElement owner, string name, out bool value)
    {
        value = false;

        if (!owner.TryGetProperty(name, out JsonElement property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.True)
        {
            value = true;

            return true;
        }

        return property.ValueKind == JsonValueKind.False;
    }

    private static bool TryReadTimestamp(
        JsonElement owner,
        string name,
        out DateTimeOffset value)
    {
        value = default;

        return owner.TryGetProperty(name, out JsonElement property)
            && property.ValueKind == JsonValueKind.String
            && property.TryGetDateTimeOffset(out value);
    }
}
