using System.Runtime.CompilerServices;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// The S3 seam (§§18-19). Unlike the S2 shadow this one is AUTHORITATIVE: when
// the GraphQL candidate is accepted, the REST repository metadata GET is never
// issued and the accepted envelope supplies identity, metadata and topics.
//
// It also carries the accepted language map forward. The orchestrator hands the
// exact metadata instance this collector returned to the descriptive collector,
// so associating the languages with that instance is enough to reunite them —
// no repository identity is stored, no per-run cache is kept, and there is no
// key that could pair one repository's metadata with another's languages. The
// association is weak, so it disappears with the metadata it belongs to.
//
// Together those two facts make "no partial merge" (§18.5) structural rather
// than asserted: either the envelope was accepted and BOTH surfaces come from
// it, or it was not and NEITHER does.
//
// This type exists only in the Profile composition. Repository Proof never
// constructs it (§3.1).
internal sealed class HybridRepositoryMetadataCollector : IGitHubRepositoryMetadataCollector
{
    private readonly IGitHubRepositoryMetadataCollector inner;
    private readonly IGitHubGraphQlRepositoryEnvelopeSource envelopeSource;

    // Reference-keyed and weak: an entry exists only while the metadata
    // collection it belongs to is still alive, and it can only ever be found by
    // the caller holding that exact instance.
    private readonly ConditionalWeakTable<
        GitHubRepositoryMetadataCollection,
        GitHubRepositoryLanguages> servedLanguages = new();

    public HybridRepositoryMetadataCollector(
        IGitHubRepositoryMetadataCollector inner,
        IGitHubGraphQlRepositoryEnvelopeSource envelopeSource)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        this.envelopeSource = envelopeSource
            ?? throw new ArgumentNullException(nameof(envelopeSource));
    }

    public async Task<GitHubRepositoryMetadataCollection> CollectAsync(
        GitHubRepositoryUrl repository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        GitHubGraphQlRepositoryEnvelope? envelope = await envelopeSource.ResolveAsync(
            repository,
            cancellationToken);

        // Every fallback case — errored, malformed, envelope-level, missing
        // repository, invalid field, incomplete connection, private repository,
        // GraphQL already disabled for this run — arrives here as null, and the
        // repository is collected through the exact proven REST composition. No
        // warning, no limitation, no availability entry of its own (§18.7).
        if (envelope is null)
        {
            return await inner.CollectAsync(repository, cancellationToken);
        }

        GitHubRepositoryMetadataCollection collection = new(
            envelope.Repository,
            envelope.Metadata,
            envelope.Topics,

            // Deliberately null. The snapshot's license comes from the REST
            // license endpoint through the descriptive collector, and this
            // field has no Profile or Repository semantic consumer — a fact
            // pinned by test. Populating it from GraphQL `licenseInfo` would
            // create a license dependency on GraphQL to fill a value nothing
            // reads, which §18.2 keeps on REST precisely to avoid.
            License: null,

            // §18.7: the one instant this envelope was accepted at, shared with
            // the languages surface it also supplies.
            DataAvailability.Available(
                GitHubRepositoryMetadataCollector.MetadataDataKey,
                envelope.RetrievedAt));

        servedLanguages.Add(
            collection,
            new GitHubRepositoryLanguages(envelope.Languages, envelope.RetrievedAt));

        return collection;
    }

    // The descriptive collector asks this for the metadata instance it was
    // handed. A hit means the languages surface was already supplied by the
    // accepted envelope, so no REST languages request is built at all; a miss
    // means REST collects it exactly as it does today.
    public GitHubRepositoryLanguages? TryGetServedLanguages(
        GitHubRepositoryMetadataCollection metadata) =>
        metadata is not null
            && servedLanguages.TryGetValue(
                metadata,
                out GitHubRepositoryLanguages? languages)
            ? languages
            : null;
}
