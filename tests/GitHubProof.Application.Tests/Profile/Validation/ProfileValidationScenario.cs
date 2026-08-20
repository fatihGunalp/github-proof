namespace GitHubProof.Application.Tests.Profile.Validation;

// The 11 canonical P9 profile validation scenario identities locked by
// docs/product/p9-profile-validation-contract.md §2. Each identity is a
// validation scenario label only: it names an evidence-distribution
// composition to run through the real ProfilePipeline, never a developer
// role, capability, seniority or job-fit claim.
//
// Public (unlike the other Validation/ types, which stay internal): xUnit
// [Theory]/TheoryData<T> require the data type to be at least as accessible
// as the public [Fact]/[Theory] test methods that consume it
// (ProfileValidationScenarioFixturesTests).
public enum ProfileValidationScenario
{
    DotNetHeavy,
    FrontendHeavy,
    MixedFullStack,
    AiMlHeavy,
    FewRepositories,
    ManyRepositories,
    MostlyForks,
    MostlyArchived,
    StudentTutorialHeavy,
    Polyglot,
    SparsePublicProfile
}
