namespace GitHubProof.Web.Profile.Runtime;

// Runtime/deployment settings only. The concurrency defaults are the
// provisional Session 107 implementation recommendations; visitor values have
// no default because neither the window nor the trusted proxy topology is a
// canonical product decision yet.
public sealed class ProfileAdmissionOptions
{
    public const string ConfigurationSectionName = "ProfileGuardrails:Admission";

    public int GlobalConcurrency { get; set; } = 1;

    public int SameProfileConcurrency { get; set; } = 1;

    public int? VisitorPermitLimit { get; set; }

    public TimeSpan? VisitorWindow { get; set; }

    public bool IsVisitorLimitEnabled =>
        VisitorPermitLimit is not null && VisitorWindow is not null;
}
