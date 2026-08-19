using System.Collections.ObjectModel;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// Shared structured identity for profile limitations and warnings.
//
// Identity is a semantic code, the scope it applies to and the canonical
// identities it affects. Localized message text is presentation produced from
// that identity; it is never the identity, the equality key or the ordering
// key, so no localized string may reach this type.
public abstract class ProfileCondition<TCode> : IEquatable<ProfileCondition<TCode>>
    where TCode : struct, Enum
{
    protected ProfileCondition(
        TCode code,
        ProfileConditionScope scope,
        IEnumerable<ProfileRepositoryReference>? affectedRepositories)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "Profile condition code is not defined.");
        }

        if (!Enum.IsDefined(scope))
        {
            throw new ArgumentOutOfRangeException(
                nameof(scope),
                scope,
                "Profile condition scope is not defined.");
        }

        Code = code;
        Scope = scope;
        AffectedRepositories = NormalizeAffected(scope, affectedRepositories);
    }

    public TCode Code { get; }

    public ProfileConditionScope Scope { get; }

    // Canonical order: Owner then Name, ordinal. Duplicates are collapsed, so
    // the same repository can never be listed twice for one condition.
    public IReadOnlyList<ProfileRepositoryReference> AffectedRepositories { get; }

    public bool Equals(ProfileCondition<TCode>? other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }

        return EqualityComparer<TCode>.Default.Equals(Code, other.Code)
            && Scope == other.Scope
            && AffectedRepositories.SequenceEqual(other.AffectedRepositories);
    }

    public override bool Equals(object? obj) =>
        Equals(obj as ProfileCondition<TCode>);

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(GetType());
        hash.Add(Code);
        hash.Add(Scope);

        foreach (ProfileRepositoryReference repository in AffectedRepositories)
        {
            hash.Add(repository);
        }

        return hash.ToHashCode();
    }

    // Canonical ordering keys, in order: semantic code, then scope, then the
    // affected canonical identities compared element by element in their own
    // canonical order. Deduplication already guarantees no two entries share
    // all three, so these keys are jointly total.
    internal int CompareOrderTo(ProfileCondition<TCode> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        int code = Comparer<TCode>.Default.Compare(Code, other.Code);

        if (code != 0)
        {
            return code;
        }

        int scope = Scope.CompareTo(other.Scope);

        if (scope != 0)
        {
            return scope;
        }

        int shared = Math.Min(
            AffectedRepositories.Count,
            other.AffectedRepositories.Count);

        for (int index = 0; index < shared; index++)
        {
            int repository = AffectedRepositories[index]
                .CompareTo(other.AffectedRepositories[index]);

            if (repository != 0)
            {
                return repository;
            }
        }

        return AffectedRepositories.Count.CompareTo(
            other.AffectedRepositories.Count);
    }

    private static IReadOnlyList<ProfileRepositoryReference> NormalizeAffected(
        ProfileConditionScope scope,
        IEnumerable<ProfileRepositoryReference>? affectedRepositories)
    {
        List<ProfileRepositoryReference> normalized = affectedRepositories
            ?.ToList() ?? [];

        if (normalized.Any(repository => repository is null))
        {
            throw new ArgumentException(
                "Affected repositories cannot contain null values.",
                nameof(affectedRepositories));
        }

        List<ProfileRepositoryReference> distinct = [.. normalized.Distinct()];
        distinct.Sort(static (left, right) => left.CompareTo(right));

        return scope switch
        {
            ProfileConditionScope.Repository when distinct.Count == 0 =>
                throw new ArgumentException(
                    "A repository scoped condition must name the repositories it affects.",
                    nameof(affectedRepositories)),

            ProfileConditionScope.Profile when distinct.Count > 0 =>
                throw new ArgumentException(
                    "A profile scoped condition cannot name affected repositories.",
                    nameof(affectedRepositories)),

            _ => new ReadOnlyCollection<ProfileRepositoryReference>(distinct)
        };
    }
}
