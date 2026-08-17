using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.Registries;

// Loads the canonical dependency category mapping registry. JSON, resource and
// schema concerns live here so Domain stays free of them, and the embedded
// artefact is the exact canonical file rather than a mirror.
public sealed class DependencyCategoryRegistryLoader
{
    public const string CanonicalMappingVersion = "1.0.0";

    private const string ResourcePrefix =
        "GitHubProof.Infrastructure.Registries.dependency-category-mapping.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Exactly one mapping version is active per execution and it is selected
    // explicitly, so a historical version can never be loaded by accident.
    public DependencyCategoryRegistry Load() => Load(CanonicalMappingVersion);

    public DependencyCategoryRegistry Load(string mappingVersion)
    {
        if (string.IsNullOrWhiteSpace(mappingVersion))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                nameof(mappingVersion));
        }

        string resourceName = $"{ResourcePrefix}{mappingVersion.Trim()}.json";

        using Stream stream = typeof(DependencyCategoryRegistryLoader).Assembly
            .GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Canonical dependency category registry '{resourceName}' is missing.");

        return Read(stream, mappingVersion.Trim());
    }

    public DependencyCategoryRegistry Read(Stream stream) =>
        Read(stream, expectedMappingVersion: null);

    public DependencyCategoryRegistry Read(
        Stream stream,
        string? expectedMappingVersion)
    {
        ArgumentNullException.ThrowIfNull(stream);

        RegistryDocument document =
            JsonSerializer.Deserialize<RegistryDocument>(stream, JsonOptions)
            ?? throw new InvalidOperationException(
                "The dependency category registry could not be read.");

        if (string.IsNullOrWhiteSpace(document.MappingVersion))
        {
            throw new InvalidOperationException(
                "The dependency category registry must declare a mapping version.");
        }

        // A registry artefact must declare the version it is identified by, so
        // a mislabelled or edited historical file cannot be replayed silently.
        if (expectedMappingVersion is not null
            && !string.Equals(
                document.MappingVersion.Trim(),
                expectedMappingVersion,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Registry artefact '{expectedMappingVersion}' declares mapping version '{document.MappingVersion}'.");
        }

        if (document.Entries is null || document.Entries.Count == 0)
        {
            throw new InvalidOperationException(
                "The dependency category registry must declare at least one entry.");
        }

        List<DependencyCategoryMappingEntry> entries = [];

        foreach (RegistryEntry entry in document.Entries)
        {
            // Rationale is required review metadata and is validated here, then
            // dropped, so no runtime result can depend on it.
            if (string.IsNullOrWhiteSpace(entry.Rationale))
            {
                throw new InvalidOperationException(
                    $"Registry entry '{entry.Identity}' must declare a rationale.");
            }

            if (!Enum.TryParse(entry.Ecosystem, ignoreCase: false, out DependencyEcosystem ecosystem)
                || !Enum.IsDefined(ecosystem))
            {
                throw new InvalidOperationException(
                    $"Registry entry '{entry.Identity}' declares unknown ecosystem '{entry.Ecosystem}'.");
            }

            if (!DependencySemanticCategoryKeys.TryRead(
                    entry.Category,
                    out DependencySemanticCategory category))
            {
                throw new InvalidOperationException(
                    $"Registry entry '{entry.Identity}' declares unknown category '{entry.Category}'.");
            }

            if (string.IsNullOrWhiteSpace(entry.Identity))
            {
                throw new InvalidOperationException(
                    "Registry entries must declare a lookup identity.");
            }

            string normalized = DependencyIdentityNormalizer.Normalize(
                ecosystem,
                entry.Identity);

            if (!string.Equals(normalized, entry.Identity, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Registry entry '{entry.Identity}' is not stored as its normalized lookup identity.");
            }

            entries.Add(new DependencyCategoryMappingEntry(
                ecosystem,
                normalized,
                category));

            foreach (string alias in entry.Aliases ?? [])
            {
                string normalizedAlias = DependencyIdentityNormalizer.Normalize(
                    ecosystem,
                    alias);

                if (!string.Equals(normalizedAlias, alias, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Registry alias '{alias}' is not stored as its normalized lookup identity.");
                }

                entries.Add(new DependencyCategoryMappingEntry(
                    ecosystem,
                    normalizedAlias,
                    category));
            }
        }

        return new DependencyCategoryRegistry(
            new DependencyCategoryMappingVersion(document.MappingVersion),
            entries);
    }

    private sealed record RegistryDocument(
        [property: JsonPropertyName("mappingVersion")] string? MappingVersion,
        [property: JsonPropertyName("entries")] IReadOnlyList<RegistryEntry>? Entries);

    private sealed record RegistryEntry(
        [property: JsonPropertyName("ecosystem")] string? Ecosystem,
        [property: JsonPropertyName("identity")] string? Identity,
        [property: JsonPropertyName("category")] string? Category,
        [property: JsonPropertyName("aliases")] IReadOnlyList<string>? Aliases,
        [property: JsonPropertyName("rationale")] string? Rationale);
}
