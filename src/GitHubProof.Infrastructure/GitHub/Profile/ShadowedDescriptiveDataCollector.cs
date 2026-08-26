using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// The S2 seam (§§18-19). It does not replace the production languages
// collector: it delegates to it, returns its result **unchanged**, and — only
// when a shadow is supplied — runs an observation on the side.
//
// Everything the caller receives comes from the REST collection: the languages
// map, the `languages` DataAvailability with its own RetrievedAt, the README,
// the license and every other availability entry. Nothing on this path can add,
// remove, reorder or reinterpret any of it, which is what makes "REST remains
// authoritative" a structural property rather than a promise.
//
// This type exists only in the Profile composition. Repository Proof never
// constructs it (§3.1).
internal sealed class ShadowedDescriptiveDataCollector
    : IGitHubRepositoryDescriptiveDataCollector
{
    private readonly IGitHubRepositoryDescriptiveDataCollector inner;
    private readonly IGitHubGraphQlLanguagesShadow shadow;

    public ShadowedDescriptiveDataCollector(
        IGitHubRepositoryDescriptiveDataCollector inner,
        IGitHubGraphQlLanguagesShadow shadow)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        this.shadow = shadow ?? throw new ArgumentNullException(nameof(shadow));
    }

    public async Task<GitHubRepositoryDescriptiveDataCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryMetadataCollection metadata,
        GitHubCommitReference? contentReference = null,
        CancellationToken cancellationToken = default)
    {
        GitHubRepositoryDescriptiveDataCollection collected = await inner.CollectAsync(
            repository,
            metadata,
            contentReference,
            cancellationToken);

        await ObserveAsync(repository, collected, cancellationToken);

        // The exact instance the REST collector produced. No copy, no rebuild,
        // no merge.
        return collected;
    }

    private async Task ObserveAsync(
        GitHubRepositoryUrl repository,
        GitHubRepositoryDescriptiveDataCollection collected,
        CancellationToken cancellationToken)
    {
        DataAvailability? languages =
            collected.FindAvailability(ProfileEvidenceKeys.LanguagesDataKey);

        // Nothing to compare against. A failed or not-requested REST languages
        // surface has no map, and comparing GraphQL with an empty one would
        // manufacture a meaningless mismatch.
        if (languages?.IsAvailable != true)
        {
            return;
        }

        try
        {
            // The outcome is not returned to the caller — the collection
            // result cannot depend on it. It is recorded into the
            // execution-scoped state instead, as the bounded §18.9 observation
            // Session 125 added: three counts and a boolean, never a request
            // count, never evidence, never carrying identity or content.
            _ = await shadow.CompareAsync(
                repository,
                collected.Languages,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Caller cancellation is not a shadow outcome and is never swallowed
            // (§11.1, §18.8).
            throw;
        }
        catch (Exception)
        {
            // Any other shadow failure is contained here on purpose. The core
            // rule of this slice is that an observation can never change the
            // collection result, and that has to hold for a bug in the shadow
            // just as much as for a provider error.
            //
            // It is still recorded as an unusable comparison, so a contained
            // shadow bug is operationally visible rather than silent (§18.9).
            ProfileGraphQlExecutionScope.Current?.Record(
                GraphQlLanguagesShadowOutcome.NotCompared);
        }
    }
}
