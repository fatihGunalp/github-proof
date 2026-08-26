namespace GitHubProof.Infrastructure.GitHub;

// A languages surface that was already collected for this repository by another
// transport, together with the instant it was retrieved.
//
// It exists so a composition that has already obtained the complete language
// map can hand it to the descriptive collector instead of making that collector
// issue its own `GET /repos/{owner}/{name}/languages`. The only such
// composition today is the Profile hybrid repository envelope
// (docs/architecture/public-profile-runtime-collection.md §18.2, Session 127);
// the Repository Proof composition supplies nothing and collects languages over
// REST exactly as it always has.
//
// `RetrievedAt` travels with the map because §18.7 binds the retrieval instant
// to the response that actually supplied the surface. Reading the clock again
// here would report an instant no observation ever happened at.
public sealed record GitHubRepositoryLanguages(
    IReadOnlyDictionary<string, long> Values,
    DateTimeOffset RetrievedAt);
