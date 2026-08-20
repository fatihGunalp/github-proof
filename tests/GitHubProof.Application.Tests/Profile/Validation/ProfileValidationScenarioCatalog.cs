namespace GitHubProof.Application.Tests.Profile.Validation;

// Data-only catalog of the 11 canonical scenario identities and their
// composition specification, verbatim from
// docs/product/p9-profile-validation-contract.md §2. This proves the 11
// identities are representable; it does not itself run or validate any
// scenario (that is P9-I3's responsibility — see
// ProfileValidationHarnessTests for the bounded proof this slice actually
// performs).
internal static class ProfileValidationScenarioCatalog
{
    public static readonly IReadOnlyDictionary<ProfileValidationScenario, string> CompositionSpecifications =
        new Dictionary<ProfileValidationScenario, string>
        {
            [ProfileValidationScenario.DotNetHeavy] =
                "Most eligible repositories carry dense C#/.NET technical evidence.",
            [ProfileValidationScenario.FrontendHeavy] =
                "Most eligible repositories carry dense JavaScript/TypeScript/"
                    + "frontend-framework technical evidence.",
            [ProfileValidationScenario.MixedFullStack] =
                "Multiple independent technical areas (e.g. backend + frontend) "
                    + "coexist in the same profile.",
            [ProfileValidationScenario.AiMlHeavy] =
                "Most eligible repositories carry dense Python/ML dependency-"
                    + "semantic-category evidence.",
            [ProfileValidationScenario.FewRepositories] =
                "Eligible repository count is low, at or below the P6/P7 "
                    + "repetition threshold.",
            [ProfileValidationScenario.ManyRepositories] =
                "Eligible repository count is high, potentially mixing "
                    + "collection-availability outcomes.",
            [ProfileValidationScenario.MostlyForks] =
                "The repository set is mostly Fork = Present.",
            [ProfileValidationScenario.MostlyArchived] =
                "The repository set is mostly Archived = Present.",
            [ProfileValidationScenario.StudentTutorialHeavy] =
                "Small, repetitive repositories resembling tutorial/boilerplate "
                    + "structure.",
            [ProfileValidationScenario.Polyglot] =
                "Multiple independent technical areas sit side by side with none "
                    + "dominant.",
            [ProfileValidationScenario.SparsePublicProfile] =
                "Total eligible and collectible evidence surface is low or "
                    + "unreachable."
        };
}
