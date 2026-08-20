namespace GitHubProof.Web.Presentation;

// The one unified landing entry field. It accepts a GitHub username, a
// canonical profile URL or a canonical repository URL; which analysis flow
// runs is decided deterministically by GitHubEntryClassifier, never guessed.
public sealed record GitHubInputModel(
    string? GitHubTarget,
    string? ErrorMessage)
{
    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);
}
