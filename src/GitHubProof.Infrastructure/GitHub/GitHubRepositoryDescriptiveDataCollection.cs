using System.Collections.ObjectModel;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Infrastructure.GitHub;

public sealed record GitHubRepositoryDescriptiveDataCollection(
    RepositoryReadme? Readme,
    RepositoryLicense? License,
    RepositoryFile? LicenseFile,
    IReadOnlyDictionary<string, long> Languages,
    IReadOnlyList<DataAvailability> Availability)
{
    public DataAvailability? FindAvailability(string dataKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataKey);

        return Availability.SingleOrDefault(item => string.Equals(
            item.DataKey,
            dataKey.Trim(),
            StringComparison.Ordinal));
    }

    public static IReadOnlyDictionary<string, long> EmptyLanguages { get; } =
        new ReadOnlyDictionary<string, long>(
            new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase));
}
