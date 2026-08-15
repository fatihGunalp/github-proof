using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Web.Presentation;

public sealed record StatusBadgeModel(
    SignalStatus Status,
    string StatusLabel);