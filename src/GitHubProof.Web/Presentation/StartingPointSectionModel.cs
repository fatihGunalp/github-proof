using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Web.Presentation;

// Session 111 STEP 3: the smallest typed carrier from Profile.cshtml to
// _ProfileStartingPoint.cshtml that lets the partial look up each presented
// repository's provider-returned Description alongside the existing
// canonical RepresentativeRepositories section — without threading
// Description through ProfileReport or ProfileModel's other properties.
public sealed record StartingPointSectionModel(
    ProfileReportSection<RepresentativeRepository> Section,
    RepositoryPresentationMetadata Descriptions);
