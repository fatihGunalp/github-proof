namespace GitHubProof.Infrastructure.GitHub.Profile;

// The Profile-only languages shadow seam (§§18-19, slice S2).
//
// It observes. It never supplies the languages surface, never produces a
// DataAvailability and never influences the collection result: the REST value
// the caller already holds is what reaches analysis, whatever this returns.
public interface IGitHubGraphQlLanguagesShadow
{
    Task<GraphQlLanguagesShadowOutcome> CompareAsync(
        GitHubRepositoryUrl repository,
        IReadOnlyDictionary<string, long> restLanguages,
        CancellationToken cancellationToken = default);
}
