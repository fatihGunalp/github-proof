namespace GitHubProof.Domain.Analysis.Enums;

public enum DataAvailabilityStatus
{
    Available = 1,
    Unavailable = 2,
    NotRequested = 3,
    NotApplicable = 4,
    RateLimited = 5,
    Forbidden = 6,
    Failed = 7
}
