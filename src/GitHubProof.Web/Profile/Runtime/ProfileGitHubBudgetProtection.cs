using GitHubProof.Infrastructure.GitHub.Profile;
using Microsoft.Extensions.Options;

namespace GitHubProof.Web.Profile.Runtime;

public sealed class ProfileGitHubBudgetProtectionOptions
{
    public const string ConfigurationSectionName = "GitHubApiBudget";

    // Null means unselected/disabled. No reserve value is embedded in code or
    // configuration by Session 109.
    public int? ProfileAdmissionRemainingFloor { get; set; }
}

// Reads only the current Profile execution's sanitized metadata-response
// observation. Missing or malformed headers cannot be turned into a provider
// failure; they simply provide no basis for a proactive internal rejection.
public sealed class ProfileGitHubBudgetProtection : IGitHubProfileBudgetProtection
{
    private readonly ProfileGitHubBudgetProtectionOptions options;

    public ProfileGitHubBudgetProtection(
        IOptions<ProfileGitHubBudgetProtectionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options.Value;
    }

    public void EnsureCanContinueAfterAvailableMetadata()
    {
        int? remainingFloor = options.ProfileAdmissionRemainingFloor;
        ProfileGitHubBudgetObservation? observation =
            ProfileGitHubBudgetScope.Current?.MetadataObservation;

        if (remainingFloor is not null
            && observation is not null
            && observation.Remaining <= remainingFloor.Value)
        {
            throw new GitHubProfileBudgetProtectionRejectedException();
        }
    }
}
