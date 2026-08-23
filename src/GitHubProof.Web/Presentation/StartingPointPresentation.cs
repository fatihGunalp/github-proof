using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Web.Presentation;

// Session 111 STEP 2: Starting Point's presentation bound. This takes the
// first PresentedItemLimit items from the existing canonical
// RepresentativeRepositories order — never a re-ranking, score or
// alphabetical/name-based reordering of its own. The canonical report keeps
// every item; only this bounded Web presentation caps what is shown.
public static class StartingPointPresentation
{
    public const int PresentedItemLimit = 3;

    public static IReadOnlyList<RepresentativeRepository> Select(
        IReadOnlyList<RepresentativeRepository> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items.Count <= PresentedItemLimit
            ? items
            : items.Take(PresentedItemLimit).ToList();
    }
}
