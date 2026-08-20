namespace GitHubProof.Web.Entry;

// One classified entry submission: which analysis flow owns it, and the
// trimmed value to hand to that flow. Value is NOT a parsed or normalized
// identity — canonical parsing stays owned by GitHubProfileHandle.Parse
// (Infrastructure, via the profile collector) and GitHubRepositoryUrl.Parse
// (via LiveRepositoryReportService).
public sealed record GitHubEntryRoute(
    GitHubEntryKind Kind,
    string Value);
