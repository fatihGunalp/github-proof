namespace GitHubProof.Domain.Analysis;

// Canonical snapshot data keys shared by evidence consumers.
public static class RepositoryDataKeys
{
    public const string DependencyManifests = "dependency-manifests";

    // Bounded raw source collection surface contracted by
    // docs/architecture/repository-source-evidence-collection.md §18.
    public const string SourceFiles = "source-files";
}
