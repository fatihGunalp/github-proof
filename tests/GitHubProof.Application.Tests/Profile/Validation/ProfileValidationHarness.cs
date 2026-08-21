using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;
using static GitHubProof.Application.Tests.Profile.ProfileCompositionTestSupport;

namespace GitHubProof.Application.Tests.Profile.Validation;

// The single, named entry point P9-I2/I3/I4 use to run one profile validation
// scenario through the real production ProfilePipeline. It reimplements
// nothing: it is a thin call onto ProfileCompositionTestSupport.CreatePipeline
// (the same real pipeline construction ProfilePipelineTests already proves),
// so a scenario run and a P8 integration-test run are always the exact same
// production wiring.
//
// Session 099: the eligibility policy is pinned to the version production
// actually registers (EligibilityPolicy.V2, carrying the Fork Policy B
// admission rule). P9 validates production truth, so it must not keep
// exercising a superseded policy version.
internal static class ProfileValidationHarness
{
    public static Task<ProfilePipelineResult> RunAsync(
        FakeSnapshotSource source,
        GitHubProfileSnapshot snapshot,
        Func<Guid>? analysisIdFactory = null,
        TimeProvider? timeProvider = null,
        AnalysisVersion? analysisVersion = null) =>
        CreatePipeline(source, analysisIdFactory, timeProvider, EligibilityPolicy.V2)
            .RunAsync(snapshot, analysisVersion ?? TestAnalysisVersion);
}
