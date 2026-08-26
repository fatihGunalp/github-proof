namespace GitHubProof.Web.Profile.Runtime;

// Session 131. Profile-only repository collection runtime settings.
//
// This is the R dimension — how many admitted repositories one profile
// execution may collect and analyze at once. It is deliberately separate from
// ProfileAdmissionOptions, which owns the U dimension (how many profile
// executions may run at once) at the Web admission boundary.
//
// The default is 1, which keeps the original sequential path. Raising it is an
// operational/deployment decision, exactly like the concurrency values
// public-profile-runtime-collection.md §10.5 records as provisional; it changes
// only when work is scheduled, never what any evidence means (§6).
public sealed class ProfileCollectionOptions
{
    public const string ConfigurationSectionName = "ProfileGuardrails:Collection";

    public int MaxRepositoryConcurrency { get; set; } = 1;
}
