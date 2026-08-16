using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Application.Analysis;

public interface IRepositoryAnalysisRunner
{
    RepositoryAnalysis Analyze(
        RepositorySnapshot snapshot,
        AnalysisVersion version,
        Guid analysisId);
}
