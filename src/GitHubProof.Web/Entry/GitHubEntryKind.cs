namespace GitHubProof.Web.Entry;

// Which of the two separate analysis semantics a submitted entry value
// belongs to. This is a routing identity only: it never merges Profile Proof
// (macro) and Repository Proof (micro), and it carries no analysis meaning.
public enum GitHubEntryKind
{
    Profile,
    Repository
}
