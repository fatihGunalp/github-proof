using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Models;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;
using static GitHubProof.Application.Tests.Profile.ProfileCompositionTestSupport;

namespace GitHubProof.Application.Tests.Profile.Validation;

// The single, named entry point P9-I2/I3/I4 use to run one profile validation
// scenario through the real production ProfilePipeline. It reimplements
// nothing: it is a thin call onto ProfileCompositionTestSupport.CreatePipeline
// (the same real pipeline construction ProfilePipelineTests already proves),
// so a scenario run and a P8 integration-test run are always the exact same
// production wiring.
internal static class ProfileValidationHarness
{
    public static Task<ProfilePipelineResult> RunAsync(
        FakeSnapshotSource source,
        GitHubProfileSnapshot snapshot,
        Func<Guid>? analysisIdFactory = null,
        TimeProvider? timeProvider = null,
        AnalysisVersion? analysisVersion = null) =>
        CreatePipeline(source, analysisIdFactory, timeProvider)
            .RunAsync(snapshot, analysisVersion ?? TestAnalysisVersion);
}
