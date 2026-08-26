using System.Net;
using System.Reflection;
using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 127 — S3 seam proofs at COMPOSITION level
// (docs/architecture/public-profile-runtime-collection.md §§18-19).
//
// The S2 seam tests proved that an observation cannot change the collection.
// This slice claims something stronger and riskier: that GraphQL may SUPPLY
// three surfaces, and that the result is still the one REST would have produced.
//
// So the assertions come in pairs. Every parity claim is matched by a
// request-level claim, because a hybrid run that quietly kept issuing the two
// REST requests would pass every parity test while consolidating nothing.
public sealed class ProfileHybridRepositoryEnvelopeSeamTests
{
    private static readonly Guid AnalysisId =
        Guid.Parse("12700000-0000-0000-0000-000000000001");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-26T09:30:00Z");

    private const string MetadataRequest = "/repos/octocat/hello-world";
    private const string LanguagesRequest = "/repos/octocat/hello-world/languages";
    private const string ReadmeRequest = "/repos/octocat/hello-world/readme";
    private const string LicenseRequest = "/repos/octocat/hello-world/license";

    // Session 129. The two identity requests an accepted envelope replaces, and
    // the recursive tree request it does NOT replace.
    private const string GitRefRequest =
        "/repos/octocat/hello-world/git/ref/heads/main";

    private const string GitCommitRequest =
        "/repos/octocat/hello-world/git/commits/"
        + FakeGitHubApiHandler.HeadCommitSha;

    private const string RecursiveTreeRequest =
        "/repos/octocat/hello-world/git/trees/"
        + FakeGitHubApiHandler.RootTreeSha
        + "?recursive=1";

    // ------------------------------------- 1. default OFF is the existing path

    // The three-argument and four-argument overloads with no envelope source are
    // the Session 099 composition. No GraphQL request exists to be counted, and
    // the two REST requests happen exactly as they always have.
    [Fact]
    public async Task WithoutAnEnvelopeSource_TheRequestGraphIsUnchanged()
    {
        Collected rest = await CollectAsync(envelope: null);

        Assert.Equal(0, rest.Handler.CountRequests("/graphql"));
        Assert.Equal(1, Exactly(rest, MetadataRequest));
        Assert.Equal(1, Exactly(rest, LanguagesRequest));

        // Session 129: with no envelope source the identity chain is the REST
        // one, in full.
        Assert.Equal(1, Exactly(rest, GitRefRequest));
        Assert.Equal(1, Exactly(rest, GitCommitRequest));
        Assert.Equal(1, Exactly(rest, RecursiveTreeRequest));
    }

    // ------------------------- 2. an accepted envelope REPLACES two REST calls

    [Fact]
    public async Task AnAcceptedEnvelope_SkipsTheRestMetadataAndLanguagesRequests()
    {
        Collected hybrid = await CollectAsync(FakeGitHubApiHandler.RepositoryEnvelopeJson);

        // One physical POST for the repository.
        Assert.Equal(1, hybrid.Handler.CountRequests("/graphql"));

        // The replaced requests are GONE, not merely equal-in-result.
        Assert.Equal(0, Exactly(hybrid, MetadataRequest));
        Assert.Equal(0, Exactly(hybrid, LanguagesRequest));

        // Session 129: and so are the two identity requests.
        Assert.Equal(0, Exactly(hybrid, GitRefRequest));
        Assert.Equal(0, Exactly(hybrid, GitCommitRequest));

        // And the REST-retained surfaces still happen — including the RECURSIVE
        // TREE, which is the whole point of adopting identity only.
        Assert.Equal(1, Exactly(hybrid, RecursiveTreeRequest));
        Assert.Equal(1, Exactly(hybrid, ReadmeRequest));
        Assert.Equal(1, Exactly(hybrid, LicenseRequest));
    }

    // The consolidation is a real reduction in physical provider requests for
    // this repository: FOUR REST GETs — metadata, languages, git-ref and
    // git-commit — become one POST, so the repository costs three fewer physical
    // requests than the REST-only run. Session 127 removed one; Session 129
    // removes two more.
    //
    // This is a request-graph fact, not a latency or performance claim (§18.11).
    [Fact]
    public async Task AnAcceptedEnvelope_CostsThreeFewerPhysicalRequests()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(FakeGitHubApiHandler.RepositoryEnvelopeJson);

        Assert.Equal(rest.Handler.RequestPaths.Count - 3, hybrid.Handler.RequestPaths.Count);

        // Named rather than only counted, so an unrelated request disappearing
        // could never satisfy this.
        Assert.Equal(
            new[] { MetadataRequest, LanguagesRequest, GitRefRequest, GitCommitRequest }
                .Order(StringComparer.Ordinal),
            rest.Handler.RequestPaths
                .Except(hybrid.Handler.RequestPaths)
                .Order(StringComparer.Ordinal));
    }

    // The REST-retained surfaces are asserted as a set, so a future slice that
    // quietly moves one of them onto GraphQL fails here (§18.2, §19.8).
    [Fact]
    public async Task AnAcceptedEnvelope_LeavesEveryRestRetainedRequestInPlace()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(FakeGitHubApiHandler.RepositoryEnvelopeJson);

        Assert.Equal(
            rest.Handler.RequestPaths
                .Where(path => path != MetadataRequest
                    && path != LanguagesRequest
                    && path != GitRefRequest
                    && path != GitCommitRequest),
            hybrid.Handler.RequestPaths.Where(path => path != "/graphql"));
    }

    // ------------------------------------------------- 2/17. semantic parity

    [Fact]
    public async Task AnAcceptedEnvelope_ProducesTheIdenticalSnapshot()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(FakeGitHubApiHandler.RepositoryEnvelopeJson);

        Assert.Equal(Fingerprint(rest.Snapshot), Fingerprint(hybrid.Snapshot));
    }

    [Fact]
    public async Task AnAcceptedEnvelope_ProducesTheIdenticalAnalysis()
    {
        RepositoryAnalysis rest = Analyze((await CollectAsync(envelope: null)).Snapshot);
        RepositoryAnalysis hybrid =
            Analyze((await CollectAsync(FakeGitHubApiHandler.RepositoryEnvelopeJson)).Snapshot);

        Assert.Equal(rest.Status, hybrid.Status);
        Assert.Equal(rest.ScoreBreakdown.EarnedScore, hybrid.ScoreBreakdown.EarnedScore);
        Assert.Equal(
            rest.ScoreBreakdown.ApplicableMaximumScore,
            hybrid.ScoreBreakdown.ApplicableMaximumScore);
        Assert.Equal(
            rest.ScoreBreakdown.NormalizedScore,
            hybrid.ScoreBreakdown.NormalizedScore);
        Assert.Equal(SignalFingerprint(rest), SignalFingerprint(hybrid));
        Assert.Equal(
            rest.CategoryResults.Select(category =>
                $"{category.Category}:{category.Status}:{category.Score}:{category.NormalizedScore}"),
            hybrid.CategoryResults.Select(category =>
                $"{category.Category}:{category.Status}:{category.Score}:{category.NormalizedScore}"));
        Assert.Equal(rest.LimitationKeys, hybrid.LimitationKeys);
        Assert.Equal(
            rest.Warnings.Select(warning => warning.Code),
            hybrid.Warnings.Select(warning => warning.Code));
    }

    // The three adopted surfaces individually, so the fingerprint above cannot
    // hide a compensating pair of differences.
    [Fact]
    public async Task AnAcceptedEnvelope_ProducesTheIdenticalAdoptedSurfaces()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(FakeGitHubApiHandler.RepositoryEnvelopeJson);

        Assert.Equal(rest.Snapshot.Repository, hybrid.Snapshot.Repository);
        Assert.Equal(rest.Snapshot.Metadata, hybrid.Snapshot.Metadata);
        Assert.Equal(rest.Snapshot.Topics, hybrid.Snapshot.Topics);
        Assert.Equal(rest.Snapshot.Languages, hybrid.Snapshot.Languages);
    }

    // ------------------------------------------------ 6. one accepted instant

    // §18.7: the surfaces this envelope supplied share ONE retrieval instant,
    // and the surfaces REST supplied keep their own. Both halves are asserted,
    // because "all equal" would be just as wrong as "all different".
    [Fact]
    public async Task AnAcceptedEnvelope_SharesOneInstantAcrossTheSurfacesItSupplied()
    {
        Collected hybrid = await CollectAsync(FakeGitHubApiHandler.RepositoryEnvelopeJson);

        DateTimeOffset metadata = RetrievedAt(hybrid.Snapshot, "metadata");
        DateTimeOffset languages = RetrievedAt(hybrid.Snapshot, "languages");

        Assert.Equal(metadata, languages);
        Assert.Equal(hybrid.EnvelopeAcceptedAt, metadata);

        // README and license are REST surfaces and keep the REST collector's own
        // live stamp, which this fixture's clock never produces.
        Assert.NotEqual(metadata, RetrievedAt(hybrid.Snapshot, "readme"));
        Assert.NotEqual(metadata, RetrievedAt(hybrid.Snapshot, "license"));
    }

    // A fallen-back surface carries the REST collector's own instant, exactly as
    // it would in a REST-only run.
    [Fact]
    public async Task AFallenBackRepository_CarriesTheRestInstantsThroughout()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        Collected hybrid = await CollectAsync(ErroredEnvelope);
        DateTimeOffset after = DateTimeOffset.UtcNow;

        Assert.InRange(RetrievedAt(hybrid.Snapshot, "metadata"), before, after);
        Assert.InRange(RetrievedAt(hybrid.Snapshot, "languages"), before, after);
    }

    // --------------------------- 17/19. every fallback equals the REST-only run

    public static TheoryData<string, HttpStatusCode> FallbackCases =>
        new()
        {
            { ErroredEnvelope, HttpStatusCode.OK },
            { """{ "data": { "repository": null } }""", HttpStatusCode.OK },
            { """{ "unexpected": true }""", HttpStatusCode.OK },
            { "not json", HttpStatusCode.OK },
            { "{ }", HttpStatusCode.Unauthorized },
            { "{ }", HttpStatusCode.BadGateway },
            { IncompleteLanguages, HttpStatusCode.OK },
            { MissingDefaultBranch, HttpStatusCode.OK },
            { DuplicateLanguage, HttpStatusCode.OK },
            { PrivateRepository, HttpStatusCode.OK },

            // Session 129 — every break in the identity chain, each falling all
            // the way back to the REST-only run.
            { NullTarget, HttpStatusCode.OK },
            { NonCommitTarget, HttpStatusCode.OK },
            { MissingTree, HttpStatusCode.OK },
            { NullTree, HttpStatusCode.OK },
            { InvalidTreeOid, HttpStatusCode.OK },
            { InvalidHeadOid, HttpStatusCode.OK }
        };

    [Theory]
    [MemberData(nameof(FallbackCases))]
    public async Task EveryFallback_ProducesTheIdenticalSnapshotAndRestRequests(
        string body,
        HttpStatusCode status)
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(body, status);

        Assert.Equal(Fingerprint(rest.Snapshot), Fingerprint(hybrid.Snapshot));

        // The whole candidate was discarded: EVERY replaced request happened,
        // which is what "no partial merge" means in the request graph (§18.5).
        Assert.Equal(1, Exactly(hybrid, MetadataRequest));
        Assert.Equal(1, Exactly(hybrid, LanguagesRequest));
        Assert.Equal(1, Exactly(hybrid, GitRefRequest));
        Assert.Equal(1, Exactly(hybrid, GitCommitRequest));
    }

    [Theory]
    [MemberData(nameof(FallbackCases))]
    public async Task EveryFallback_AddsNoWarningLimitationOrAvailabilityEntry(
        string body,
        HttpStatusCode status)
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(body, status);

        Assert.Equal(
            rest.Snapshot.Availability.Select(entry => $"{entry.DataKey}|{entry.Status}|{entry.Reason}"),
            hybrid.Snapshot.Availability.Select(entry => $"{entry.DataKey}|{entry.Status}|{entry.Reason}"));
        Assert.Equal(rest.Snapshot.CollectionWarnings, hybrid.Snapshot.CollectionWarnings);
        Assert.Equal(
            Analyze(rest.Snapshot).LimitationKeys,
            Analyze(hybrid.Snapshot).LimitationKeys);
    }

    // ------------------------------------ 20. no new availability vocabulary

    // A served envelope introduces no data key, no status and no reason of its
    // own. The availability list is compared whole, so an added entry fails
    // here as loudly as a changed one (§18.7).
    [Fact]
    public async Task AnAcceptedEnvelope_IntroducesNoNewAvailabilityVocabulary()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(FakeGitHubApiHandler.RepositoryEnvelopeJson);

        Assert.Equal(
            rest.Snapshot.Availability.Select(entry => $"{entry.DataKey}|{entry.Status}|{entry.Reason}"),
            hybrid.Snapshot.Availability.Select(entry => $"{entry.DataKey}|{entry.Status}|{entry.Reason}"));
        Assert.DoesNotContain(
            hybrid.Snapshot.Availability,
            entry => entry.DataKey.Contains("graphql", StringComparison.OrdinalIgnoreCase)
                || entry.DataKey.Contains("envelope", StringComparison.OrdinalIgnoreCase)
                || entry.DataKey.Contains("hybrid", StringComparison.OrdinalIgnoreCase));
        Assert.All(
            hybrid.Snapshot.Availability,
            entry => Assert.True(Enum.IsDefined(entry.Status)));
    }

    // ----------------------------------------- the license boundary (§18.2)

    // `GitHubRepositoryMetadataCollection.License` has no Profile or Repository
    // semantic consumer. That is proven behaviourally rather than asserted: the
    // fixture reports DIFFERENT licenses on the two endpoints, and the snapshot
    // carries the /license one. The repository-metadata license therefore
    // reaches nothing the analysis reads.
    //
    // Consequence for S3: the hybrid path leaves that field null instead of
    // creating a GraphQL `licenseInfo` dependency to populate a value nothing
    // reads — which is also why §18.2 keeps license on REST.
    [Fact]
    public async Task TheSnapshotLicense_ComesFromTheLicenseEndpointNotFromMetadata()
    {
        Collected diverging = await CollectAsync(
            envelope: null,
            metadataLicenseKey: "apache-2.0");

        Assert.Equal("mit", diverging.Snapshot.License!.Key);
    }

    // And the hybrid run, whose metadata license is null rather than merely
    // different, produces exactly the same license evidence.
    [Fact]
    public async Task TheHybridRun_ProducesTheIdenticalLicenseEvidence()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(FakeGitHubApiHandler.RepositoryEnvelopeJson);

        Assert.Equal("mit", rest.Snapshot.License!.Key);
        Assert.Equal(rest.Snapshot.License, hybrid.Snapshot.License);
        Assert.Equal(rest.Snapshot.LicenseFile!.Path, hybrid.Snapshot.LicenseFile!.Path);
        Assert.Equal(
            rest.Snapshot.FindAvailability("license")!.Status,
            hybrid.Snapshot.FindAvailability("license")!.Status);
    }

    // ------------------------- the supplied-languages seam is not trusted blindly

    // A supplied map goes through the SAME normalization a REST response does,
    // and a map the REST collector would have refused produces the SAME typed
    // unavailability rather than an exception escaping into snapshot assembly.
    //
    // The S3 envelope source can never produce such a map — it already refuses
    // duplicates and negative sizes — but the delegate is public, so the guard is
    // proven rather than assumed.
    [Theory]
    [MemberData(nameof(RefusedLanguageMaps))]
    public async Task ASuppliedMapTheRestCollectorWouldRefuse_BecomesTypedUnavailability(
        Dictionary<string, long> supplied)
    {
        FakeGitHubApiHandler handler = new();
        using HttpClient client = CreateClient(handler);

        GitHubRepositoryMetadataCollection metadata =
            await new GitHubRepositoryMetadataCollector(client, new FixedTimeProvider(CapturedAt))
                .CollectAsync(GitHubRepositoryUrl.Parse(FakeGitHubApiHandler.RepositoryUrl));

        GitHubRepositoryDescriptiveDataCollection collected =
            await new GitHubRepositoryDescriptiveDataCollector(
                    client,
                    new FixedTimeProvider(CapturedAt),
                    _ => new GitHubRepositoryLanguages(supplied, EnvelopeInstant))
                .CollectAsync(
                    GitHubRepositoryUrl.Parse(FakeGitHubApiHandler.RepositoryUrl),
                    metadata);

        DataAvailability languages = collected.FindAvailability("languages")!;

        Assert.False(languages.IsAvailable);
        Assert.Equal(DataAvailabilityStatus.Failed, languages.Status);
        Assert.Empty(collected.Languages);

        // The languages request was still not issued: the supplier claimed the
        // surface, and claiming it badly does not silently fall back to REST.
        Assert.Equal(0, handler.RequestPaths.Count(path => path == LanguagesRequest));
    }

    public static TheoryData<Dictionary<string, long>> RefusedLanguageMaps =>
        new()
        {
            new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["C#"] = 1200,
                ["c#"] = 300
            },
            new Dictionary<string, long>(StringComparer.Ordinal) { ["C#"] = -1 },
            new Dictionary<string, long>(StringComparer.Ordinal) { ["  "] = 1200 }
        };

    // A well-formed supplied map is used as-is, so the guard above cannot be
    // passing because the seam refuses everything.
    [Fact]
    public async Task AWellFormedSuppliedMap_IsUsedWithoutAnyLanguagesRequest()
    {
        FakeGitHubApiHandler handler = new();
        using HttpClient client = CreateClient(handler);

        GitHubRepositoryMetadataCollection metadata =
            await new GitHubRepositoryMetadataCollector(client, new FixedTimeProvider(CapturedAt))
                .CollectAsync(GitHubRepositoryUrl.Parse(FakeGitHubApiHandler.RepositoryUrl));

        GitHubRepositoryDescriptiveDataCollection collected =
            await new GitHubRepositoryDescriptiveDataCollector(
                    client,
                    new FixedTimeProvider(CapturedAt),
                    _ => new GitHubRepositoryLanguages(
                        new Dictionary<string, long> { ["Rust"] = 42 },
                        EnvelopeInstant))
                .CollectAsync(
                    GitHubRepositoryUrl.Parse(FakeGitHubApiHandler.RepositoryUrl),
                    metadata);

        Assert.Equal(new Dictionary<string, long> { ["Rust"] = 42 }, collected.Languages);
        Assert.Equal(EnvelopeInstant, collected.FindAvailability("languages")!.RetrievedAt);
        Assert.Equal(0, handler.RequestPaths.Count(path => path == LanguagesRequest));
    }

    // ------------------------------------ 22. Repository Proof stays untouched

    // The envelope overload exists only on the Profile factory. Repository
    // Proof's composition cannot be handed one, which is the §3.1 boundary in
    // code rather than in a comment.
    [Fact]
    public void RepositoryProofComposition_HasNoEnvelopeSeam()
    {
        Assert.DoesNotContain(
            FactoryParameters(nameof(GitHubRepositorySnapshotOrchestrator.CreateDefault)),
            parameter => parameter.ParameterType == typeof(IGitHubGraphQlRepositoryEnvelopeSource));

        Assert.Contains(
            FactoryParameters(
                nameof(GitHubRepositorySnapshotOrchestrator.CreateProfileComposition)),
            parameter => parameter.ParameterType == typeof(IGitHubGraphQlRepositoryEnvelopeSource));
    }

    // The Repository Proof composition still collects languages over REST: the
    // supplied-languages seam is opt-in, and CreateDefault opts out.
    [Fact]
    public async Task RepositoryProofComposition_StillIssuesTheRestLanguagesRequest()
    {
        FakeGitHubApiHandler handler = new(_ =>
            FakeGitHubApiHandler.JsonResponse(FakeGitHubApiHandler.RepositoryEnvelopeJson));
        using HttpClient client = CreateClient(handler);

        await GitHubRepositorySnapshotOrchestrator
            .CreateDefault(client, new FixedTimeProvider(CapturedAt))
            .CollectAsync(FakeGitHubApiHandler.RepositoryUrl);

        Assert.Equal(0, handler.CountRequests("/graphql"));
        Assert.Equal(1, handler.RequestPaths.Count(path => path == MetadataRequest));
        Assert.Equal(1, handler.RequestPaths.Count(path => path == LanguagesRequest));
    }

    // ------------------------------------------ the shadow/envelope collision

    // The two GraphQL consumers are mutually exclusive by construction, so the
    // "one accepted envelope must not also cost a shadow request" rule cannot be
    // violated by a wiring mistake — it fails at composition time instead.
    [Fact]
    public void TheShadowAndTheEnvelope_CannotBeComposedTogether()
    {
        using HttpClient client = CreateClient(new FakeGitHubApiHandler());

        ArgumentException failure = Assert.Throws<ArgumentException>(() =>
            GitHubRepositorySnapshotOrchestrator.CreateProfileComposition(
                client,
                TimeProvider.System,
                new GitHubGraphQlLanguagesShadow(new GitHubGraphQlTransport(client)),
                new GitHubGraphQlRepositoryEnvelopeSource(new GitHubGraphQlTransport(client))));

        Assert.Contains("shadow", failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    // -------------------------------------------------------- test support

    private sealed record Collected(
        RepositorySnapshot Snapshot,
        FakeGitHubApiHandler Handler,
        DateTimeOffset EnvelopeAcceptedAt);

    // A clock used ONLY by the envelope source, so an instant it produced is
    // distinguishable from every REST collector's own live stamp. That is what
    // lets a test assert which side supplied a surface.
    private static readonly DateTimeOffset EnvelopeInstant =
        DateTimeOffset.Parse("2019-05-04T11:22:33Z");

    private static async Task<Collected> CollectAsync(
        string? envelope,
        HttpStatusCode status = HttpStatusCode.OK,
        string metadataLicenseKey = "mit")
    {
        FakeGitHubApiHandler handler = envelope is null
            ? new FakeGitHubApiHandler(metadataLicenseKey: metadataLicenseKey)
            : new FakeGitHubApiHandler(
                _ => FakeGitHubApiHandler.JsonResponse(envelope, status),
                metadataLicenseKey);
        using HttpClient client = CreateClient(handler);

        GitHubRepositorySnapshotResult result = await GitHubRepositorySnapshotOrchestrator
            .CreateProfileComposition(
                client,
                new FixedTimeProvider(CapturedAt),
                languagesShadow: null,
                envelope is null
                    ? null
                    : new GitHubGraphQlRepositoryEnvelopeSource(
                        new GitHubGraphQlTransport(client),
                        new FixedTimeProvider(EnvelopeInstant)))
            .CollectAsync(FakeGitHubApiHandler.RepositoryUrl);

        Assert.True(result.IsSuccessful);

        return new Collected(result.Snapshot!, handler, EnvelopeInstant);
    }

    private static int Exactly(Collected collected, string path) =>
        collected.Handler.RequestPaths.Count(request => request == path);

    private static DateTimeOffset RetrievedAt(RepositorySnapshot snapshot, string dataKey)
    {
        DataAvailability availability = snapshot.FindAvailability(dataKey)!;

        Assert.True(availability.IsAvailable);

        return availability.RetrievedAt!.Value;
    }

    private static IEnumerable<ParameterInfo> FactoryParameters(string name) =>
        typeof(GitHubRepositorySnapshotOrchestrator)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == name)
            .SelectMany(method => method.GetParameters());

    private static HttpClient CreateClient(FakeGitHubApiHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://api.github.com/") };

    private static RepositoryAnalysis Analyze(RepositorySnapshot snapshot) =>
        new RepositoryAnalysisRunner(new FixedTimeProvider(CapturedAt))
            .Analyze(snapshot, Version, AnalysisId);

    // Everything the collection produced, flattened — except RetrievedAt, which
    // §18.7 / §19.2 exclude from parity because two runs are two observations.
    // The instants are proven separately, and deliberately, above.
    private static string Fingerprint(RepositorySnapshot snapshot) =>
        string.Join(
            "\n",
            $"identity={snapshot.Repository.Owner}/{snapshot.Repository.Name}",
            $"url={snapshot.Repository.Url}",
            $"branch={snapshot.Repository.DefaultBranch}",
            $"flags={snapshot.Repository.IsPublic}|{snapshot.Repository.IsArchived}"
                + $"|{snapshot.Repository.IsFork}|{snapshot.Repository.IsTemplate}",
            $"primaryLanguage={snapshot.Repository.PrimaryLanguage}",
            $"description={snapshot.Metadata?.Description}",
            $"homepage={snapshot.Metadata?.Homepage}",
            $"createdAt={snapshot.Metadata?.CreatedAt:O}",
            $"issuesEnabled={snapshot.Metadata?.IssuesEnabled}",
            $"head={snapshot.HeadCommitSha}",
            "languages=" + string.Join(
                ",",
                snapshot.Languages
                    .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                    .Select(entry => $"{entry.Key}={entry.Value}")),
            "topics=" + string.Join(",", snapshot.Topics),
            "availability=" + string.Join(
                ",",
                snapshot.Availability.Select(entry =>
                    $"{entry.DataKey}|{entry.Status}|{entry.Reason}")),
            "warnings=" + string.Join("|", snapshot.CollectionWarnings),
            $"readme={snapshot.Readme?.Path}",
            $"license={snapshot.License?.Key}|{snapshot.LicenseFile?.Path}",
            $"fileTree={snapshot.FileTree.Items.Count}|{snapshot.FileTree.IsComplete}",
            $"workflows={snapshot.Workflows.Items.Count}",
            $"docs={snapshot.DocumentationMarkdown.Items.Count}",
            $"commits={snapshot.Commits.Items.Count}|{snapshot.Commits.ObservationState}",
            $"issues={snapshot.Issues.Items.Count}|{snapshot.Issues.ObservationState}",
            $"pulls={snapshot.PullRequests.Items.Count}|{snapshot.PullRequests.ObservationState}",
            $"releases={snapshot.Releases.Items.Count}|{snapshot.Releases.ObservationState}",
            $"tags={snapshot.Tags.Items.Count}|{snapshot.Tags.ObservationState}");

    private static IReadOnlyList<string> SignalFingerprint(RepositoryAnalysis analysis) =>
    [
        .. analysis.SignalResults
            .OrderBy(signal => signal.SignalKey, StringComparer.Ordinal)
            .Select(signal =>
                $"{signal.SignalKey}|{signal.Status}|{signal.Score}|"
                + string.Join(
                    ";",
                    signal.Evidence
                        .OrderBy(item => item.Key, StringComparer.Ordinal)
                        .Select(item => $"{item.Key}={item.Value}")))
    ];

    private const string ErroredEnvelope = """
        {
          "data": { "repository": null },
          "errors": [ { "message": "Something went wrong." } ]
        }
        """;

    private static readonly string PrivateRepository = FakeGitHubApiHandler
        .RepositoryEnvelopeJson
        .Replace("\"isPrivate\": false", "\"isPrivate\": true", StringComparison.Ordinal);

    private static readonly string MissingDefaultBranch =
        WithDefaultBranchRef("null");

    private static readonly string DuplicateLanguage = FakeGitHubApiHandler
        .RepositoryEnvelopeJson
        .Replace(
            "{ \"size\": 300, \"node\": { \"name\": \"JavaScript\" } }",
            "{ \"size\": 300, \"node\": { \"name\": \"c#\" } }",
            StringComparison.Ordinal);

    // The languages connection is the second one in the envelope.
    private static readonly string IncompleteLanguages = Incomplete(
        FakeGitHubApiHandler.RepositoryEnvelopeJson);

    private static string Incomplete(string envelope)
    {
        const string complete = "{ \"hasNextPage\": false }";
        int index = envelope.LastIndexOf(complete, StringComparison.Ordinal);

        return envelope[..index]
            + "{ \"hasNextPage\": true }"
            + envelope[(index + complete.Length)..];
    }

    // Every §18.13 break, expressed against the WHOLE `defaultBranchRef` value so
    // none of them can silently stop matching.
    private static readonly string NullTarget = WithDefaultBranchRef(
        """
        {
          "name": "main",
          "target": null
        }
        """);

    // What an unmatched `... on Commit` fragment actually produces: an object
    // that simply has no `oid`. It is refused exactly like a null target.
    private static readonly string NonCommitTarget = WithDefaultBranchRef(
        """
        {
          "name": "main",
          "target": { }
        }
        """);

    private static readonly string MissingTree = WithDefaultBranchRef(
        $$"""
        {
          "name": "main",
          "target": { "oid": "{{FakeGitHubApiHandler.HeadCommitSha}}" }
        }
        """);

    private static readonly string NullTree = WithDefaultBranchRef(
        $$"""
        {
          "name": "main",
          "target": {
            "oid": "{{FakeGitHubApiHandler.HeadCommitSha}}",
            "tree": null
          }
        }
        """);

    private static readonly string InvalidTreeOid = WithDefaultBranchRef(
        $$"""
        {
          "name": "main",
          "target": {
            "oid": "{{FakeGitHubApiHandler.HeadCommitSha}}",
            "tree": { "oid": "not-a-git-object-id" }
          }
        }
        """);

    private static readonly string InvalidHeadOid = WithDefaultBranchRef(
        $$"""
        {
          "name": "main",
          "target": {
            "oid": "0BAD",
            "tree": { "oid": "{{FakeGitHubApiHandler.RootTreeSha}}" }
          }
        }
        """);

    private static string WithDefaultBranchRef(string value)
    {
        // The anchor must still be present in the fixture. Asserting on the
        // RESULT instead would misfire whenever a substitution is legitimately
        // identical to what it replaces.
        Assert.Contains(
            FakeGitHubApiHandler.DefaultBranchRefJson,
            FakeGitHubApiHandler.RepositoryEnvelopeJson,
            StringComparison.Ordinal);

        return FakeGitHubApiHandler.RepositoryEnvelopeJson.Replace(
            FakeGitHubApiHandler.DefaultBranchRefJson,
            value,
            StringComparison.Ordinal);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
