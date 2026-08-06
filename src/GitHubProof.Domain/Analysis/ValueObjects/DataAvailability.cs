using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.ValueObjects;

public sealed record DataAvailability
{
    public DataAvailability(
        string dataKey,
        DataAvailabilityStatus status,
        string? reason,
        DateTimeOffset? retrievedAt)
    {
        DataKey = RequireValue(dataKey, nameof(dataKey));
        Status = status;
        Reason = NormalizeOptional(reason);
        RetrievedAt = retrievedAt?.ToUniversalTime();

        Validate();
    }

    public string DataKey { get; }

    public DataAvailabilityStatus Status { get; }

    public string? Reason { get; }

    public DateTimeOffset? RetrievedAt { get; }

    public bool IsAvailable =>
        Status == DataAvailabilityStatus.Available;

    public static DataAvailability Available(
        string dataKey,
        DateTimeOffset retrievedAt)
    {
        return new DataAvailability(
            dataKey,
            DataAvailabilityStatus.Available,
            reason: null,
            retrievedAt);
    }

    public static DataAvailability Unavailable(
        string dataKey,
        DataAvailabilityStatus status,
        string reason)
    {
        if (status == DataAvailabilityStatus.Available)
        {
            throw new ArgumentException(
                "Unavailable factory cannot use Available status.",
                nameof(status));
        }

        return new DataAvailability(
            dataKey,
            status,
            reason,
            retrievedAt: null);
    }

    private void Validate()
    {
        if (Status == DataAvailabilityStatus.Available
            && RetrievedAt is null)
        {
            throw new ArgumentException(
                "Available data must include a retrieval timestamp.",
                nameof(RetrievedAt));
        }

        if (Status != DataAvailabilityStatus.Available
            && string.IsNullOrWhiteSpace(Reason))
        {
            throw new ArgumentException(
                "Unavailable data must include a reason.",
                nameof(Reason));
        }
    }

    private static string RequireValue(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
