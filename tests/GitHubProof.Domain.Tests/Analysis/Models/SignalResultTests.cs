using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis.Models;

public sealed class SignalResultTests
{
    [Fact]
    public void SignalDefinition_ExposesLanguageNeutralVersionedRuleKeys()
    {
        SignalDefinition definition = new(
            " repository.readme.exists ",
            AnalysisCategory.RepositoryUnderstandability,
            maximumScore: 5,
            requiredDataKeys: ["readme"],
            includeInOverallScore: true,
            version: 2);

        Assert.Equal("repository.readme.exists.applicability.v2", definition.ApplicabilityRuleKey);
        Assert.Equal("repository.readme.exists.evaluation.v2", definition.EvaluationRuleKey);
        Assert.Equal("repository.readme.exists.evidence.v2", definition.EvidenceRequirementKey);
    }

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
