using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Application.Analysis.Presentation;

public sealed record SignalPresentation(
    string Locale,
    string SignalKey,
    string Title,
    string Description,
    SignalStatus Status,
    string StatusLabel,
    int? Score,
    int MaximumScore,
    string Explanation,
    string WhyItMatters,
    string? Recommendation,
    IReadOnlyList<string> Limitations,
    IReadOnlyList<EvidenceItem> Evidence,
    int SignalVersion);
