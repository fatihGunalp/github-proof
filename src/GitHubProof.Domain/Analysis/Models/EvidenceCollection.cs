namespace GitHubProof.Domain.Analysis.Models;

public sealed class EvidenceCollection
{
    private readonly List<EvidenceItem> _items = [];

    public EvidenceCollection(
        IEnumerable<EvidenceItem>? items = null,
        IEnumerable<string>? warnings = null)
    {
        if (items is not null)
        {
            foreach (EvidenceItem item in items)
            {
                Add(item);
            }
        }

        Warnings = NormalizeWarnings(warnings);
    }

    public IReadOnlyList<EvidenceItem> Items => _items.AsReadOnly();

    public IReadOnlyList<string> Warnings { get; }

    public int Count => _items.Count;

    public bool IsEmpty => _items.Count == 0;

    public void Add(EvidenceItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        bool duplicateExists = _items.Any(existing =>
            string.Equals(
                existing.Key,
                item.Key,
                StringComparison.Ordinal)
            && string.Equals(
                existing.Source,
                item.Source,
                StringComparison.Ordinal)
            && string.Equals(
                existing.Location,
                item.Location,
                StringComparison.Ordinal)
            && string.Equals(
                existing.Value,
                item.Value,
                StringComparison.Ordinal));

        if (duplicateExists)
        {
            return;
        }

        _items.Add(item);
    }

    public IReadOnlyList<EvidenceItem> FindByKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(
                "Key cannot be null, empty, or whitespace.",
                nameof(key));
        }

        return _items
            .Where(item => string.Equals(
                item.Key,
                key.Trim(),
                StringComparison.Ordinal))
            .ToArray();
    }

    public bool ContainsKey(string key)
    {
        return FindByKey(key).Count > 0;
    }

    private static IReadOnlyList<string> NormalizeWarnings(
        IEnumerable<string>? warnings)
    {
        if (warnings is null)
        {
            return Array.Empty<string>();
        }

        return warnings
            .Where(warning => !string.IsNullOrWhiteSpace(warning))
            .Select(warning => warning.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
