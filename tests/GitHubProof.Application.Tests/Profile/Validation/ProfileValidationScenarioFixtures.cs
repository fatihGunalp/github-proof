using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;
using static GitHubProof.Application.Tests.Profile.ProfileCompositionTestSupport;

namespace GitHubProof.Application.Tests.Profile.Validation;

// Concrete, deterministic, network-free fixture inputs for all 11 canonical
// P9 scenarios (docs/product/p9-profile-validation-contract.md §10 item 1).
// Each Build(...) call constructs a fresh GitHubProfileSnapshot +
// FakeSnapshotSource pair from the existing test-support building blocks
// (ProfileAnalysisTestSupport / ProfileCompositionTestSupport) — no
// production semantics are reimplemented here, only repository/evidence
// composition data.
//
// A fixture existing here, and executing through
// ProfileValidationHarness.RunAsync, proves it is constructible and runnable
// through the real ProfilePipeline
// (ProfileValidationScenarioFixturesTests). It does NOT prove any P9 risk
// (docs/product/p9-profile-validation-contract.md §3) is validated for that
// scenario — that is P9-I3's responsibility.
internal static class ProfileValidationScenarioFixtures
{
    public static (GitHubProfileSnapshot Snapshot, FakeSnapshotSource Source) Build(
        ProfileValidationScenario scenario) =>
        scenario switch
        {
            ProfileValidationScenario.DotNetHeavy => DotNetHeavy(),
            ProfileValidationScenario.FrontendHeavy => FrontendHeavy(),
            ProfileValidationScenario.MixedFullStack => MixedFullStack(),
            ProfileValidationScenario.AiMlHeavy => AiMlHeavy(),
            ProfileValidationScenario.FewRepositories => FewRepositories(),
            ProfileValidationScenario.ManyRepositories => ManyRepositories(),
            ProfileValidationScenario.MostlyForks => MostlyForks(),
            ProfileValidationScenario.MostlyArchived => MostlyArchived(),
            ProfileValidationScenario.StudentTutorialHeavy => StudentTutorialHeavy(),
            ProfileValidationScenario.Polyglot => Polyglot(),
            ProfileValidationScenario.SparsePublicProfile => SparsePublicProfile(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(scenario),
                scenario,
                "Profile validation scenario is not defined.")
        };

    // Most (2 of 3) eligible repositories carry C#/.NET evidence.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) DotNetHeavy() =>
        LanguageMajority(
            "dotnet-heavy",
            majorityLanguage: "C#",
            majorityCount: 2,
            minorityLanguage: "JavaScript");

    // Most (2 of 3) eligible repositories carry frontend (TypeScript)
    // evidence.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) FrontendHeavy() =>
        LanguageMajority(
            "frontend-heavy",
            majorityLanguage: "TypeScript",
            majorityCount: 2,
            minorityLanguage: "Python");

    // Two independent technical areas (backend + frontend), each
    // independently promotable (2 repositories per area).
    private static (GitHubProfileSnapshot, FakeSnapshotSource) MixedFullStack() =>
        MultiLanguageRepositories(
            "mixed-full-stack",
            languages: ["C#", "C#", "TypeScript", "TypeScript"]);

    // Most (2 of 3) eligible repositories carry Python evidence, representing
    // the AI/ML-heavy composition at the language-evidence level.
    //
    // F2 (Session 090 independent review, kept unchanged, no production
    // code modified): today's production ProfilePatternPromotionPolicy
    // promotes TechnicalFocus only from the raw RepositoryLanguageEvidence
    // language literal, never from DependencySemanticCategoryEvidence, so
    // this fixture is indistinguishable, at the pattern-promotion level,
    // from a plain Python-heavy composition under the current pipeline.
    // That production-wiring limitation is carried forward as an explicit
    // P9-I3 input, not resolved here.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) AiMlHeavy() =>
        LanguageMajority(
            "aiml-heavy",
            majorityLanguage: "Python",
            majorityCount: 2,
            minorityLanguage: "Go");

    // Two independent repositories only: exactly at
    // RepeatedProfilePattern.MinimumIndependentSupportingRepositories.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) FewRepositories() =>
        MultiLanguageRepositories(
            "few-repositories",
            languages: ["C#", "C#"]);

    // Five eligible repositories, mixing a real collection-unavailable
    // outcome in with successfully collected, multi-language evidence.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) ManyRepositories()
    {
        const string prefix = "many-repositories";
        string[] names = [.. Enumerable.Range(1, 5).Select(i => $"{prefix}-{i}")];

        ProfileRepository[] repositories = [.. names.Select(name => CreateRepository(name))];
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(repositories);

        FakeSnapshotSource source = CreateSource(
            ($"octocat/{names[0]}", () => CreateCollected(
                CreateLanguageEvidenceSnapshot(names[0], Language("C#")))),
            ($"octocat/{names[1]}", () => CreateCollected(
                CreateLanguageEvidenceSnapshot(names[1], Language("C#")))),
            ($"octocat/{names[2]}", () => CreateCollected(
                CreateLanguageEvidenceSnapshot(names[2], Language("TypeScript")))),
            ($"octocat/{names[3]}", () => CreateCollected(
                CreateLanguageEvidenceSnapshot(names[3], Language("TypeScript")))),
            ($"octocat/{names[4]}", () => CreateUnavailable(DataAvailabilityStatus.RateLimited)));

        return (snapshot, source);
    }

    // Most (2 of 3) repositories are forks (non-independent lineage).
    private static (GitHubProfileSnapshot, FakeSnapshotSource) MostlyForks()
    {
        const string prefix = "mostly-forks";
        ProfileRepository originalRepository = CreateRepository($"{prefix}-original");
        ProfileRepository[] forkRepositories =
        [
            CreateRepository($"{prefix}-fork-1", fork: RepositoryConditionState.Present),
            CreateRepository($"{prefix}-fork-2", fork: RepositoryConditionState.Present)
        ];

        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(
            [originalRepository, .. forkRepositories]);

        FakeSnapshotSource source = CreateSource(
            (originalRepository.Reference.FullName, () => CreateCollected(
                CreateLanguageEvidenceSnapshot(originalRepository.Reference.Name, Language("C#")))),
            (forkRepositories[0].Reference.FullName, () => CreateCollected(
                CreateLanguageEvidenceSnapshot(forkRepositories[0].Reference.Name, Language("C#")))),
            (forkRepositories[1].Reference.FullName, () => CreateCollected(
                CreateLanguageEvidenceSnapshot(forkRepositories[1].Reference.Name, Language("C#")))));

        return (snapshot, source);
    }

    // Most (2 of 3) repositories are archived.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) MostlyArchived()
    {
        const string prefix = "mostly-archived";
        ProfileRepository activeRepository = CreateRepository($"{prefix}-active");
        ProfileRepository[] archivedRepositories =
        [
            CreateRepository($"{prefix}-archived-1", archived: RepositoryConditionState.Present),
            CreateRepository($"{prefix}-archived-2", archived: RepositoryConditionState.Present)
        ];

        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(
            [activeRepository, .. archivedRepositories]);

        FakeSnapshotSource source = CreateSource(
            (activeRepository.Reference.FullName, () => CreateCollected(
                CreateLanguageEvidenceSnapshot(activeRepository.Reference.Name, Language("C#")))),
            (archivedRepositories[0].Reference.FullName, () => CreateCollected(
                CreateLanguageEvidenceSnapshot(archivedRepositories[0].Reference.Name, Language("C#")))),
            (archivedRepositories[1].Reference.FullName, () => CreateCollected(
                CreateLanguageEvidenceSnapshot(archivedRepositories[1].Reference.Name, Language("C#")))));

        return (snapshot, source);
    }

    // Most (2 of 3) repositories are template-derived — the closest existing
    // production lineage concept to tutorial/boilerplate repetition.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) StudentTutorialHeavy()
    {
        const string prefix = "student-tutorial-heavy";
        ProfileRepository originalRepository = CreateRepository($"{prefix}-original");
        ProfileRepository[] templateRepositories =
        [
            CreateRepository(
                $"{prefix}-template-1",
                templateDerived: RepositoryConditionState.Present),
            CreateRepository(
                $"{prefix}-template-2",
                templateDerived: RepositoryConditionState.Present)
        ];

        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(
            [originalRepository, .. templateRepositories]);

        FakeSnapshotSource source = CreateSource(
            (originalRepository.Reference.FullName, () => CreateCollected(
                CreateReadmeEvidenceSnapshot(originalRepository.Reference.Name))),
            (templateRepositories[0].Reference.FullName, () => CreateCollected(
                CreateReadmeEvidenceSnapshot(templateRepositories[0].Reference.Name))),
            (templateRepositories[1].Reference.FullName, () => CreateCollected(
                CreateReadmeEvidenceSnapshot(templateRepositories[1].Reference.Name))));

        return (snapshot, source);
    }

    // Three independent technical areas, each represented by exactly two
    // repositories, none dominant over the others.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) Polyglot() =>
        MultiLanguageRepositories(
            "polyglot",
            languages: ["C#", "C#", "TypeScript", "TypeScript", "Python", "Python"]);

    // A single repository with minimal (README-only) evidence: the smallest
    // possible eligible+collectible evidence surface.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) SparsePublicProfile()
    {
        const string name = "sparse-public-profile";
        ProfileRepository repository = CreateRepository(name);
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot([repository]);

        FakeSnapshotSource source = CreateSource(
            (repository.Reference.FullName, () => CreateCollected(
                CreateReadmeEvidenceSnapshot(name))));

        return (snapshot, source);
    }

    // Shared "N repositories in one technical area, a majority count plus one
    // minority repository" builder, reused by the single-dominant-area
    // scenarios.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) LanguageMajority(
        string scenarioPrefix,
        string majorityLanguage,
        int majorityCount,
        string minorityLanguage)
    {
        List<(string Name, string Language)> plan = [];

        for (int i = 1; i <= majorityCount; i++)
        {
            plan.Add(($"{scenarioPrefix}-{majorityLanguage.ToLowerInvariant()}-{i}", majorityLanguage));
        }

        plan.Add(($"{scenarioPrefix}-{minorityLanguage.ToLowerInvariant()}-1", minorityLanguage));

        return RepositoriesWithLanguages(plan);
    }

    // "One independent repository per language entry" builder, reused by the
    // multi-area scenarios. Two entries sharing a language land on two
    // independent repositories — exactly
    // RepeatedProfilePattern.MinimumIndependentSupportingRepositories.
    private static (GitHubProfileSnapshot, FakeSnapshotSource) MultiLanguageRepositories(
        string scenarioPrefix,
        IReadOnlyList<string> languages)
    {
        Dictionary<string, int> occurrences = [];
        List<(string Name, string Language)> plan = [];

        foreach (string language in languages)
        {
            occurrences[language] = occurrences.GetValueOrDefault(language) + 1;
            plan.Add(($"{scenarioPrefix}-{language.ToLowerInvariant()}-{occurrences[language]}", language));
        }

        return RepositoriesWithLanguages(plan);
    }

    private static (GitHubProfileSnapshot, FakeSnapshotSource) RepositoriesWithLanguages(
        IReadOnlyList<(string Name, string Language)> plan)
    {
        ProfileRepository[] repositories = [.. plan.Select(entry => CreateRepository(entry.Name))];
        GitHubProfileSnapshot snapshot = CreateProfileSnapshot(repositories);

        FakeSnapshotSource source = CreateSource(
            [.. plan.Select(entry => (
                FullName: $"octocat/{entry.Name}",
                Response: (Func<ProfileRepositorySnapshotResult>)(() => CreateCollected(
                    CreateLanguageEvidenceSnapshot(entry.Name, Language(entry.Language))))))]);

        return (snapshot, source);
    }

    private static Dictionary<string, long> Language(string name) => new() { [name] = 100 };
}
