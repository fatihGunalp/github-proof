using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class SignalResultTests
{
    [Fact]
    public void Constructor_RejectsNonZeroMissingScore()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new SignalResult(
                "repository.readme.exists",
                signalVersion: 1,
                AnalysisCategory.RepositoryUnderstandability,
                SignalStatus.Missing,
                score: 1,
                maximumScore: 5,
                includeInOverallScore: true,
                evidence: [CreateEvidence()],
                explanation: "The README was inspected.",
                recommendation: "Add a README.",
                limitations: null,
                evaluatedAt: DateTimeOffset.UtcNow));

        Assert.Equal("Score", exception.ParamName);
    }

    private static EvidenceItem CreateEvidence() =>
        new(
            "readme.inspected",
            "README.md",
            "not found",
            "README.md",
            DateTimeOffset.UtcNow,
            EvidenceConfidence.High,
            null);
}
