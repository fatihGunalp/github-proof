using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile;

// Deterministic profile-surface ordering. The canonical key stays the
// ProfileRepositoryReference ordering itself; the URL only breaks ties so the
// order cannot depend on execution order.
internal sealed class ProfileRepositoryReferenceOrder
    : IComparer<ProfileRepositoryReference>
{
    public static ProfileRepositoryReferenceOrder Instance { get; } = new();

    public int Compare(
        ProfileRepositoryReference? left,
        ProfileRepositoryReference? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        int reference = left.CompareTo(right);

        return reference != 0 || right is null
            ? reference
            : string.CompareOrdinal(
                left.Url.AbsoluteUri,
                right.Url.AbsoluteUri);
    }
}
