using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryContentsCollection(
    string? HeadCommitSha,
    RepositoryCollection<RepositoryFile> FileTree,
    RepositoryCollection<RepositoryFile> ProjectFiles,
    RepositoryCollection<RepositoryFile> Workflows,
    RepositoryCollection<RepositoryFile> DocumentationMarkdownFiles,
    DataAvailability FileTreeAvailability,
    DataAvailability ProjectFilesAvailability,
    DataAvailability WorkflowsAvailability,
    DataAvailability DocumentationMarkdownAvailability,
    IReadOnlyList<string> CollectionWarnings);
