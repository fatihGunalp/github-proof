using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;

namespace GitHubProof.Web.Entry;

// The single unified-entry routing boundary. It answers one question at the
// landing: which of the two separate analysis flows owns this submission, and
// is the submission acceptable at all?
//
// Two stages, in order:
//
//   1. SHAPE selects the canonical owner:
//
//        no scheme, no '/'        -> Profile candidate    (bare login)
//        scheme + 1 path segment  -> Profile candidate    (profile URL)
//        scheme + 2 path segments -> Repository candidate (repository URL)
//        anything else            -> not accepted
//
//   2. The selected CANONICAL PARSER decides acceptance. The classifier never
//      re-implements the login pattern, scheme, host, userinfo, query or
//      fragment rules — it delegates to GitHubProfileHandle.Parse and
//      GitHubRepositoryUrl.Parse, the existing single definitions of those
//      rules, and only records whether they accepted.
//
// Stage 2 is a pre-flight acceptance check, not an identity derivation: the
// parse result is discarded and the RAW trimmed string is what travels
// onward, so the collector's own GitHubProfileHandle.Parse remains the
// authoritative parse of record (the P10-I1 boundary). Validating here is
// what keeps a non-canonical shape on the unified entry, with the entry-level
// message listing the accepted shapes, instead of dead-ending on a
// flow-specific error page.
//
// Deterministic by construction: no network call, no existence check, no
// AI/intent classification, no probabilistic scoring. The same input always
// resolves the same way, and an input that cannot be resolved is rejected
// rather than guessed at.
public static class GitHubEntryClassifier
{
    private const string SchemeSeparator = "://";

    public static bool TryClassify(
        string? rawValue,
        out GitHubEntryRoute? route)
    {
        route = null;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        string value = rawValue.Trim();

        GitHubEntryKind? kind = SelectCanonicalOwner(value);

        if (kind is null || !IsAcceptedByCanonicalParser(kind.Value, value))
        {
            return false;
        }

        route = new GitHubEntryRoute(kind.Value, value);
        return true;
    }

    // Stage 1 — shape only.
    private static GitHubEntryKind? SelectCanonicalOwner(string value)
    {
        if (!value.Contains(SchemeSeparator, StringComparison.Ordinal))
        {
            // A bare login carries no path. "owner/repo" and
            // "github.com/owner/repo" are therefore NOT bare logins: they are
            // non-canonical shapes, and the accepted input contract does not
            // include them. Rejecting them here is what stops them from being
            // silently reinterpreted as either flow.
            return value.Contains('/', StringComparison.Ordinal)
                ? null
                : GitHubEntryKind.Profile;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            return null;
        }

        int segmentCount = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Length;

        return segmentCount switch
        {
            1 => GitHubEntryKind.Profile,
            2 => GitHubEntryKind.Repository,
            _ => null
        };
    }

    // Stage 2 — delegate acceptance to the canonical owner. The parsed value
    // is intentionally discarded; only the accept/reject answer is used.
    private static bool IsAcceptedByCanonicalParser(
        GitHubEntryKind kind,
        string value)
    {
        try
        {
            if (kind == GitHubEntryKind.Profile)
            {
                GitHubProfileHandle.Parse(value);
            }
            else
            {
                GitHubRepositoryUrl.Parse(value);
            }

            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
