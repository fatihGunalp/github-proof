namespace GitHubProof.Infrastructure.GitHub.Profile;

// Operational continuation boundary after truthful Available profile metadata
// has been published and before repository-manifest collection begins. It owns
// no provider availability or evidence semantics.
public interface IGitHubProfileBudgetProtection
{
    void EnsureCanContinueAfterAvailableMetadata();
}

public sealed class DisabledGitHubProfileBudgetProtection
    : IGitHubProfileBudgetProtection
{
    public static DisabledGitHubProfileBudgetProtection Instance { get; } = new();

    private DisabledGitHubProfileBudgetProtection()
    {
    }

    public void EnsureCanContinueAfterAvailableMetadata()
    {
    }
}

// A typed operational signal caught only by the outer live Profile execution
// boundary. It must never be translated into collector availability.
public sealed class GitHubProfileBudgetProtectionRejectedException : Exception
{
    public GitHubProfileBudgetProtectionRejectedException()
        : base("Profile GitHub budget protection rejected further execution.")
    {
    }
}
