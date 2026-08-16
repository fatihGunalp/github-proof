namespace GitHubProof.Web.Presentation;

public sealed record RepositoryInputModel(
    string? RepositoryUrl,
    string? ErrorMessage)
{
    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);
}
