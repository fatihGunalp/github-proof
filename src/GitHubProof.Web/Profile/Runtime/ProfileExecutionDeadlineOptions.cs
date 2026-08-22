namespace GitHubProof.Web.Profile.Runtime;

public sealed class ProfileExecutionDeadlineOptions
{
    public const string ConfigurationSectionName = "ProfileExecutionDeadline";

    // Null deliberately means unconfigured/disabled. Session 108 does not
    // select a production duration and therefore supplies no fallback value.
    public TimeSpan? Timeout { get; set; }
}
