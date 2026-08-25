using System.Reflection;
using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 125 — S2 seam proofs for
// docs/architecture/public-profile-runtime-collection.md §§18-19.
//
// The core rule of this slice is a single sentence: REST remains authoritative.
// These tests run the real Profile composition against the same deterministic
// GitHub fixture with the shadow absent, matching, mismatching, failing and
// throwing, and prove the collected snapshot and the resulting analysis are
// identical in every one of those runs.
public sealed class ProfileLanguagesShadowSeamTests
{
    private static readonly Guid AnalysisId =
        Guid.Parse("12500000-0000-0000-0000-000000000001");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-25T09:30:00Z");

    // ------------------------------- C. shadow ON returns the REST collection

    [Fact]
    public async Task ShadowOn_ReturnsTheIdenticalLanguagesMapAndAvailability()
    {
        RepositorySnapshot off = await CollectAsync(shadow: null);
        RepositorySnapshot on = await CollectAsync(new StubShadow(
            GraphQlLanguagesShadowOutcome.Match));

        Assert.Equal(off.Languages, on.Languages);
        Assert.Equal(
            DescribeAvailability(off, ProfileEvidenceKey),
            DescribeAvailability(on, ProfileEvidenceKey));
    }

    // The shadow is handed the REST map the collector actually produced — it
    // observes the authoritative value rather than re-deriving one.
    [Fact]
    public async Task ShadowOn_ObservesTheRestMapItselfAndReturnsItUnchanged()
    {
        StubShadow shadow = new(GraphQlLanguagesShadowOutcome.Match);

        RepositorySnapshot snapshot = await CollectAsync(shadow);

        Assert.Equal(1, shadow.Calls);
        Assert.Equal(snapshot.Languages, shadow.ObservedLanguages!);
        Assert.Equal("octocat", shadow.ObservedRepository!.Owner);
        Assert.Equal("hello-world", shadow.ObservedRepository.Name);
    }

    // The languages RetrievedAt is the REST collector's own live stamp: it falls
    // inside the window of the run that produced it. The shadow neither supplies
    // it nor shifts it — and it cannot, because it produces no availability at
    // all (§18.7).
    [Fact]
    public async Task ShadowOn_LeavesTheRestRetrievedAtAuthoritative()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        RepositorySnapshot snapshot = await CollectAsync(new StubShadow(
            GraphQlLanguagesShadowOutcome.Mismatch));
        DateTimeOffset after = DateTimeOffset.UtcNow;

        DataAvailability languages = snapshot.FindAvailability(ProfileEvidenceKey)!;

        Assert.True(languages.IsAvailable);
        Assert.InRange(languages.RetrievedAt!.Value, before, after);
    }

    // ------------------------- E/F/Q. no outcome can move the collected truth

    [Theory]
    [InlineData(GraphQlLanguagesShadowOutcome.Match)]
    [InlineData(GraphQlLanguagesShadowOutcome.Mismatch)]
    [InlineData(GraphQlLanguagesShadowOutcome.NotCompared)]
    public async Task EveryShadowOutcome_LeavesTheWholeSnapshotIdentical(
        GraphQlLanguagesShadowOutcome outcome)
    {
        RepositorySnapshot off = await CollectAsync(shadow: null);
        RepositorySnapshot on = await CollectAsync(new StubShadow(outcome));

        Assert.Equal(Fingerprint(off), Fingerprint(on));
    }

    // A shadow that throws is a bug in the observation, never a change to the
    // collection: the core rule has to hold for that too.
    [Fact]
    public async Task AThrowingShadow_LeavesTheWholeSnapshotIdentical()
    {
        RepositorySnapshot off = await CollectAsync(shadow: null);
        RepositorySnapshot on = await CollectAsync(new ThrowingShadow());

        Assert.Equal(Fingerprint(off), Fingerprint(on));
    }

    // H/I end to end: the real shadow over the real transport, against a fixture
    // that answers 404 for /graphql. The envelope fails, the shadow abstains,
    // and the collection is untouched.
    [Fact]
    public async Task ARealShadowWhoseEnvelopeFails_LeavesTheWholeSnapshotIdentical()
    {
        RepositorySnapshot off = await CollectAsync(shadow: null);

        FakeGitHubApiHandler handler = new();
        using HttpClient httpClient = CreateClient(handler);
        RepositorySnapshot on = await CollectAsync(
            new GitHubGraphQlLanguagesShadow(new GitHubGraphQlTransport(httpClient)),
            handler,
            httpClient);

        Assert.Equal(Fingerprint(off), Fingerprint(on));
        Assert.Equal(1, handler.CountRequests("/graphql"));
    }

    // ------------------------------- P/Q. analysis and evidence cannot drift

    [Theory]
    [InlineData(GraphQlLanguagesShadowOutcome.Match)]
    [InlineData(GraphQlLanguagesShadowOutcome.Mismatch)]
    [InlineData(GraphQlLanguagesShadowOutcome.NotCompared)]
    public async Task EveryShadowOutcome_ProducesTheIdenticalAnalysis(
        GraphQlLanguagesShadowOutcome outcome)
    {
        RepositoryAnalysis off = Analyze(await CollectAsync(shadow: null));
        RepositoryAnalysis on = Analyze(await CollectAsync(new StubShadow(outcome)));

        Assert.Equal(off.Status, on.Status);
        Assert.Equal(off.ScoreBreakdown.NormalizedScore, on.ScoreBreakdown.NormalizedScore);
        Assert.Equal(off.ScoreBreakdown.EarnedScore, on.ScoreBreakdown.EarnedScore);
        Assert.Equal(
            off.ScoreBreakdown.ApplicableMaximumScore,
            on.ScoreBreakdown.ApplicableMaximumScore);
        Assert.Equal(SignalFingerprint(off), SignalFingerprint(on));
        Assert.Equal(
            off.CategoryResults.Select(category =>
                $"{category.Category}:{category.Status}:{category.Score}:{category.NormalizedScore}"),
            on.CategoryResults.Select(category =>
                $"{category.Category}:{category.Status}:{category.Score}:{category.NormalizedScore}"));
        Assert.Equal(off.LimitationKeys, on.LimitationKeys);
        Assert.Equal(
            off.Warnings.Select(warning => warning.Code),
            on.Warnings.Select(warning => warning.Code));
    }

    // ------------------------------------- N. the shadow emits no availability

    [Fact]
    public async Task ShadowOn_AddsNoAvailabilityEntryOfItsOwn()
    {
        RepositorySnapshot off = await CollectAsync(shadow: null);
        RepositorySnapshot on = await CollectAsync(new StubShadow(
            GraphQlLanguagesShadowOutcome.Mismatch));

        Assert.Equal(
            off.Availability.Select(entry => entry.DataKey),
            on.Availability.Select(entry => entry.DataKey));
        Assert.Equal(off.CollectionWarnings, on.CollectionWarnings);
        Assert.DoesNotContain(
            on.Availability,
            entry => entry.DataKey.Contains("graphql", StringComparison.OrdinalIgnoreCase)
                || entry.DataKey.Contains("shadow", StringComparison.OrdinalIgnoreCase));
    }

    // ------------------------------------- O. Repository Proof stays untouched

    // The shadow overload exists only on the Profile factory. Repository Proof's
    // composition cannot be handed one, which is the §3.1 boundary in code
    // rather than in a comment.
    [Fact]
    public void RepositoryProofComposition_HasNoShadowSeam()
    {
        Assert.DoesNotContain(
            typeof(GitHubRepositorySnapshotOrchestrator)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.Name == nameof(
                    GitHubRepositorySnapshotOrchestrator.CreateDefault))
                .SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType == typeof(IGitHubGraphQlLanguagesShadow));

        Assert.Contains(
            typeof(GitHubRepositorySnapshotOrchestrator)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.Name == nameof(
                    GitHubRepositorySnapshotOrchestrator.CreateProfileComposition))
                .SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType == typeof(IGitHubGraphQlLanguagesShadow));
    }

    // The existing two-argument Profile signature is unchanged, so every caller
    // that predates S2 still binds to a composition with no shadow at all.
    [Fact]
    public async Task TheDefaultProfileComposition_IssuesNoGraphQlRequest()
    {
        FakeGitHubApiHandler handler = new();
        using HttpClient httpClient = CreateClient(handler);

        await GitHubRepositorySnapshotOrchestrator
            .CreateProfileComposition(httpClient, new FixedTimeProvider(CapturedAt))
            .CollectAsync(FakeGitHubApiHandler.RepositoryUrl);

        Assert.Equal(0, handler.CountRequests("/graphql"));
    }

    // -------------------------------------------------------- test support

    private const string ProfileEvidenceKey = "languages";

    private static async Task<RepositorySnapshot> CollectAsync(
        IGitHubGraphQlLanguagesShadow? shadow,
        FakeGitHubApiHandler? handler = null,
        HttpClient? client = null)
    {
        handler ??= new FakeGitHubApiHandler();
        HttpClient httpClient = client ?? CreateClient(handler);

        try
        {
            GitHubRepositorySnapshotResult result =
                await GitHubRepositorySnapshotOrchestrator
                    .CreateProfileComposition(
                        httpClient,
                        new FixedTimeProvider(CapturedAt),
                        shadow)
                    .CollectAsync(FakeGitHubApiHandler.RepositoryUrl);

            Assert.True(result.IsSuccessful);

            return result.Snapshot!;
        }
        finally
        {
            if (client is null)
            {
                httpClient.Dispose();
            }
        }
    }

    private static HttpClient CreateClient(FakeGitHubApiHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://api.github.com/") };

    private static RepositoryAnalysis Analyze(RepositorySnapshot snapshot) =>
        new RepositoryAnalysisRunner(new FixedTimeProvider(CapturedAt))
            .Analyze(snapshot, Version, AnalysisId);

    // Deliberately without RetrievedAt. Two separate collections are two
    // separate observations, and §18.7 / §19.2 exclude wall-clock instants from
    // parity for exactly that reason. That the instant is still the REST
    // collector's own is proven directly by
    // ShadowOn_LeavesTheRestRetrievedAtAuthoritative.
    private static string DescribeAvailability(RepositorySnapshot snapshot, string dataKey)
    {
        DataAvailability? availability = snapshot.FindAvailability(dataKey);

        return availability is null
            ? "absent"
            : $"{availability.DataKey}|{availability.Status}|{availability.Reason}";
    }

    // Everything the collection produced, flattened — except RetrievedAt, which
    // is a per-execution wall-clock instant that legitimately differs between
    // two runs and is excluded from parity by §18.7 / §19.2. Every other field,
    // including each availability key, status and reason, is compared.
    private static string Fingerprint(RepositorySnapshot snapshot) =>
        string.Join(
            "\n",
            $"head={snapshot.HeadCommitSha}",
            $"capturedAt={snapshot.CapturedAt:O}",
            "languages=" + string.Join(
                ",",
                snapshot.Languages
                    .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                    .Select(entry => $"{entry.Key}={entry.Value}")),
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
            $"tags={snapshot.Tags.Items.Count}|{snapshot.Tags.ObservationState}",
            "topics=" + string.Join(",", snapshot.Topics));

    private static IReadOnlyList<string> SignalFingerprint(RepositoryAnalysis analysis) =>
    [
        .. analysis.SignalResults.Select(signal =>
            $"{signal.SignalKey}|{signal.Status}|{signal.Score}|"
            + string.Join('+', signal.Evidence.Select(evidence => evidence.Key)))
    ];

    private sealed class StubShadow(GraphQlLanguagesShadowOutcome outcome)
        : IGitHubGraphQlLanguagesShadow
    {
        public int Calls { get; private set; }

        public GitHubRepositoryUrl? ObservedRepository { get; private set; }

        public IReadOnlyDictionary<string, long>? ObservedLanguages { get; private set; }

        public Task<GraphQlLanguagesShadowOutcome> CompareAsync(
            GitHubRepositoryUrl repository,
            IReadOnlyDictionary<string, long> restLanguages,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            ObservedRepository = repository;
            ObservedLanguages = restLanguages;

            return Task.FromResult(outcome);
        }
    }

    private sealed class ThrowingShadow : IGitHubGraphQlLanguagesShadow
    {
        public Task<GraphQlLanguagesShadowOutcome> CompareAsync(
            GitHubRepositoryUrl repository,
            IReadOnlyDictionary<string, long> restLanguages,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("A shadow bug must not reach the caller.");
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
