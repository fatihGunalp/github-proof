using System.Net;
using System.Text;
using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 129 — the default-branch head/tree IDENTITY slice of
// docs/architecture/public-profile-runtime-collection.md §§18-19.
//
// What is adopted is two Git object IDs and nothing else. The recursive file
// tree stays REST (§18.2), and `defaultBranchRef: null` stays "candidate
// unusable" rather than emptiness (§18.13).
//
// The riskiest claim in this slice is the one about NULL, so it is proven from
// both directions: the null shape must reproduce the REST successful-empty
// result exactly, AND a 409 that is not the empty one must stay a failure. A
// bug that read null as emptiness would pass the first half and fail the second.
public sealed class ProfileHybridHeadTreeIdentityTests
{
    private static readonly Guid AnalysisId =
        Guid.Parse("12900000-0000-0000-0000-000000000001");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-26T09:30:00Z");

    private static readonly DateTimeOffset EnvelopeInstant =
        DateTimeOffset.Parse("2019-05-04T11:22:33Z");

    private const string GitRefRequest = "/repos/octocat/hello-world/git/ref/heads/main";

    private const string GitCommitRequest =
        "/repos/octocat/hello-world/git/commits/" + FakeGitHubApiHandler.HeadCommitSha;

    private const string RecursiveTreeRequest =
        "/repos/octocat/hello-world/git/trees/" + FakeGitHubApiHandler.RootTreeSha
        + "?recursive=1";

    // A root tree the REST commit response never names, used to prove the
    // recursive tree request follows the ENVELOPE rather than REST.
    private const string EnvelopeOnlyTreeSha = "c0ffee00c0ffee11c0ffee22c0ffee33c0ffee44";

    // -------------------------------------- 2/17. the mapping, proven exactly

    // §19.17: `Commit.oid` maps onto what REST reports as `reference.object.sha`
    // for `heads/{defaultBranch}`, and `Commit.tree.oid` onto what REST reports
    // as `commit.tree.sha`. The fixture carries the same two SHAs on both
    // transports, so an accepted envelope must reproduce the REST head exactly.
    [Fact]
    public async Task AnAcceptedEnvelope_ProducesTheSameHeadCommitShaAsRest()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(Envelope());

        Assert.Equal(FakeGitHubApiHandler.HeadCommitSha, rest.Snapshot.HeadCommitSha);
        Assert.Equal(rest.Snapshot.HeadCommitSha, hybrid.Snapshot.HeadCommitSha);
    }

    // The strong half. If the envelope names a DIFFERENT root tree, the
    // recursive tree request must follow the envelope — which is only possible
    // if the OID genuinely came from GraphQL and was not re-derived from REST.
    //
    // The head SHA is unchanged here, so this isolates the tree OID alone.
    [Fact]
    public async Task TheRecursiveTreeRequest_UsesTheEnvelopeTreeOid()
    {
        Collected hybrid = await CollectAsync(Envelope(rootTreeSha: EnvelopeOnlyTreeSha));

        Assert.Contains(
            "/repos/octocat/hello-world/git/trees/" + EnvelopeOnlyTreeSha + "?recursive=1",
            hybrid.Handler.RequestPaths);
        Assert.DoesNotContain(RecursiveTreeRequest, hybrid.Handler.RequestPaths);

        // And neither identity request was issued, so the OID cannot have come
        // from REST.
        Assert.DoesNotContain(GitRefRequest, hybrid.Handler.RequestPaths);
        Assert.DoesNotContain(GitCommitRequest, hybrid.Handler.RequestPaths);
    }

    // The head SHA travels the same way: an envelope naming a different head
    // puts that head on the snapshot. Together with the test above this pins
    // both members of the identity to the envelope rather than to the fixture.
    [Fact]
    public async Task TheSnapshotHead_IsTheEnvelopeCommitOid()
    {
        const string other = "beef0000beef1111beef2222beef3333beef4444";

        Collected hybrid = await CollectAsync(Envelope(headCommitSha: other));

        Assert.Equal(other, hybrid.Snapshot.HeadCommitSha);
    }

    // ------------------------------- 3/4/5. the request graph, named exactly

    [Fact]
    public async Task AnAcceptedEnvelope_IssuesNeitherIdentityRequestButStillFetchesTheTree()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(Envelope());

        Assert.Contains(GitRefRequest, rest.Handler.RequestPaths);
        Assert.Contains(GitCommitRequest, rest.Handler.RequestPaths);
        Assert.Contains(RecursiveTreeRequest, rest.Handler.RequestPaths);

        Assert.DoesNotContain(GitRefRequest, hybrid.Handler.RequestPaths);
        Assert.DoesNotContain(GitCommitRequest, hybrid.Handler.RequestPaths);

        // The recursive tree is NOT replaced. GraphQL exposes no recursive tree
        // and no `truncated`, so adopting it was never on the table (§18.2).
        Assert.Contains(RecursiveTreeRequest, hybrid.Handler.RequestPaths);
    }

    // §18.2 / §19.8: the file CONTENTS requests the tree drives are all still
    // REST, so adopting identity did not quietly move a content surface.
    [Fact]
    public async Task AnAcceptedEnvelope_LeavesEveryContentRequestOnRest()
    {
        Collected hybrid = await CollectAsync(Envelope());

        Assert.Contains("/repos/octocat/hello-world/readme", hybrid.Handler.RequestPaths);
        Assert.Contains("/repos/octocat/hello-world/license", hybrid.Handler.RequestPaths);
        Assert.Contains(
            hybrid.Handler.RequestPaths,
            path => path.StartsWith(
                "/repos/octocat/hello-world/contents/",
                StringComparison.Ordinal));
    }

    // ------------------------------------- 8/9/16. NULL IS NEVER EMPTINESS

    // The positive half of §19.16, at composition level. GitHub answers an empty
    // repository with `defaultBranchRef: null`; the candidate is discarded, the
    // repository goes through REST, and the exact 409 body produces the current
    // successful-empty result — identical to the REST-only run in every respect.
    [Fact]
    public async Task ANullDefaultBranchRefWithTheExactEmpty409_IsTheRestSuccessfulEmptyResult()
    {
        Collected rest = await CollectAsync(envelope: null, gitReference: Empty409);
        Collected hybrid = await CollectAsync(NullDefaultBranchRef, gitReference: Empty409);

        // Successful emptiness, not failure: no head, an AVAILABLE file tree.
        Assert.Null(rest.Snapshot.HeadCommitSha);
        Assert.True(rest.Snapshot.FindAvailability("file-tree")!.IsAvailable);

        Assert.Equal(Fingerprint(rest.Snapshot), Fingerprint(hybrid.Snapshot));
        Assert.Equal(
            Analyze(rest.Snapshot).LimitationKeys,
            Analyze(hybrid.Snapshot).LimitationKeys);

        // The whole candidate fell back, so the REST identity chain ran.
        Assert.Contains(GitRefRequest, hybrid.Handler.RequestPaths);
    }

    // The negative half. The SAME GraphQL null, with a 409 whose body is not the
    // empty one, must stay a failure on both transports. A reading of null as
    // emptiness would make this repository successfully empty and this test is
    // what refuses it.
    [Fact]
    public async Task ANullDefaultBranchRefWithAnyOther409_StaysAFailure()
    {
        Collected rest = await CollectAsync(envelope: null, gitReference: Other409);
        Collected hybrid = await CollectAsync(NullDefaultBranchRef, gitReference: Other409);

        DataAvailability fileTree = hybrid.Snapshot.FindAvailability("file-tree")!;

        Assert.False(fileTree.IsAvailable);
        Assert.Null(hybrid.Snapshot.HeadCommitSha);

        // A real typed failure, not "we did not ask" and not "does not apply" —
        // §18.8 forbids a failure quietly becoming successful absence.
        Assert.Contains(
            fileTree.Status,
            new[]
            {
                DataAvailabilityStatus.Unavailable,
                DataAvailabilityStatus.RateLimited,
                DataAvailabilityStatus.Forbidden,
                DataAvailabilityStatus.Failed
            });

        Assert.Equal(Fingerprint(rest.Snapshot), Fingerprint(hybrid.Snapshot));
    }

    // The two 409s must not agree with each other, or the pair above would prove
    // nothing at all.
    [Fact]
    public async Task TheEmpty409AndAnyOther409_AreDifferentOutcomes()
    {
        Collected empty = await CollectAsync(envelope: null, gitReference: Empty409);
        Collected failed = await CollectAsync(envelope: null, gitReference: Other409);

        Assert.True(empty.Snapshot.FindAvailability("file-tree")!.IsAvailable);
        Assert.False(failed.Snapshot.FindAvailability("file-tree")!.IsAvailable);
    }

    // Every break in the identity chain reaches the same place as the null one:
    // the REST path, where the 409 rule decides. None of them can shortcut to
    // emptiness, because the envelope is never consulted about emptiness at all.
    [Theory]
    [MemberData(nameof(BrokenIdentityChains))]
    public async Task EveryBrokenIdentityChain_LeavesEmptinessToRest(string envelope)
    {
        Collected rest = await CollectAsync(envelope: null, gitReference: Empty409);
        Collected hybrid = await CollectAsync(envelope, gitReference: Empty409);

        Assert.Equal(Fingerprint(rest.Snapshot), Fingerprint(hybrid.Snapshot));
        Assert.Contains(GitRefRequest, hybrid.Handler.RequestPaths);
    }

    public static TheoryData<string> BrokenIdentityChains =>
        new()
        {
            NullDefaultBranchRef,
            NullTarget,
            NonCommitTarget,
            NullTree
        };

    // ------------------------------- 18. an explicit commit stays authoritative

    // A caller that named a commit is asking for THAT commit. The supplier is not
    // consulted at all, so a current-HEAD identity resolved over GraphQL can
    // never silently answer in its place — including when the two disagree,
    // which is the only case where the difference is observable.
    [Fact]
    public async Task AnExplicitContentReference_IsNeverOverriddenByTheEnvelopeHead()
    {
        const string requested = FakeGitHubApiHandler.HeadCommitSha;

        FakeGitHubApiHandler handler = new(
            _ => FakeGitHubApiHandler.JsonResponse(
                Envelope(headCommitSha: "beef0000beef1111beef2222beef3333beef4444")));
        using HttpClient client = CreateClient(handler);

        GitHubRepositorySnapshotResult result = await CreateHybrid(client, handler)
            .CollectAtCommitAsync(FakeGitHubApiHandler.RepositoryUrl, requested);

        Assert.True(result.IsSuccessful);

        // The REQUESTED commit, not the envelope's head.
        Assert.Equal(requested, result.Snapshot!.HeadCommitSha);

        // And the commit request for it was actually issued, which is what
        // resolves the tree for that commit rather than for the current head.
        Assert.Contains(GitCommitRequest, handler.RequestPaths);

        // The ref request is skipped because a commit was named, exactly as it
        // is on the REST-only path — not because of anything GraphQL did.
        Assert.DoesNotContain(GitRefRequest, handler.RequestPaths);
    }

    // The same run with the envelope OFF, so the assertion above is a property of
    // the contract rather than of this fixture.
    [Fact]
    public async Task AnExplicitContentReference_BehavesIdenticallyWithAndWithoutTheEnvelope()
    {
        const string requested = FakeGitHubApiHandler.HeadCommitSha;

        FakeGitHubApiHandler restHandler = new();
        using HttpClient restClient = CreateClient(restHandler);
        GitHubRepositorySnapshotResult rest = await GitHubRepositorySnapshotOrchestrator
            .CreateProfileComposition(restClient, new FixedTimeProvider(CapturedAt))
            .CollectAtCommitAsync(FakeGitHubApiHandler.RepositoryUrl, requested);

        FakeGitHubApiHandler hybridHandler = new(
            _ => FakeGitHubApiHandler.JsonResponse(Envelope()));
        using HttpClient hybridClient = CreateClient(hybridHandler);
        GitHubRepositorySnapshotResult hybrid =
            await CreateHybrid(hybridClient, hybridHandler)
                .CollectAtCommitAsync(FakeGitHubApiHandler.RepositoryUrl, requested);

        Assert.Equal(Fingerprint(rest.Snapshot!), Fingerprint(hybrid.Snapshot!));

        // The identity seam removed the METADATA and LANGUAGES requests, as S3
        // already did, and nothing else: the commit request stays.
        Assert.Contains(GitCommitRequest, hybridHandler.RequestPaths);
    }

    // ---------------------------------- 19/20. the file tree keeps its own truth

    // `truncated` is why the recursive tree stays REST. It still drives
    // `FileTree.IsComplete` and the truncation warning when the identity came
    // from GraphQL, because the tree response is the same REST response.
    [Fact]
    public async Task ATruncatedTree_KeepsIdenticalCompletenessAndWarningSemantics()
    {
        Collected rest = await CollectAsync(envelope: null, truncatedTree: true);
        Collected hybrid = await CollectAsync(Envelope(), truncatedTree: true);

        Assert.False(rest.Snapshot.FileTree.IsComplete);
        Assert.False(hybrid.Snapshot.FileTree.IsComplete);
        Assert.Equal(rest.Snapshot.CollectionWarnings, hybrid.Snapshot.CollectionWarnings);
        Assert.Contains(
            hybrid.Snapshot.CollectionWarnings,
            warning => warning.Contains("truncated", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(Fingerprint(rest.Snapshot), Fingerprint(hybrid.Snapshot));
    }

    // §18.7: the `file-tree` availability is produced by the REST request that
    // actually supplied it, so it keeps the REST collector's own instant. The
    // envelope's instant is deliberately NOT on it — head/tree identity is
    // structural input to collection, not an observed surface.
    [Fact]
    public async Task TheFileTreeAvailability_KeepsTheRestInstantNotTheEnvelopeInstant()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        Collected hybrid = await CollectAsync(Envelope());
        DateTimeOffset after = DateTimeOffset.UtcNow;

        DataAvailability fileTree = hybrid.Snapshot.FindAvailability("file-tree")!;

        Assert.True(fileTree.IsAvailable);
        Assert.NotEqual(EnvelopeInstant, fileTree.RetrievedAt);
        Assert.InRange(fileTree.RetrievedAt!.Value, before, after);

        // While the surfaces the envelope really supplied do share its instant.
        Assert.Equal(EnvelopeInstant, hybrid.Snapshot.FindAvailability("metadata")!.RetrievedAt);
    }

    // §19.2 / §18.7: no new availability key, status or reason exists because
    // identity moved transport.
    [Fact]
    public async Task TheIdentitySeam_IntroducesNoAvailabilityVocabulary()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(Envelope());

        Assert.Equal(
            rest.Snapshot.Availability.Select(entry =>
                $"{entry.DataKey}|{entry.Status}|{entry.Reason}"),
            hybrid.Snapshot.Availability.Select(entry =>
                $"{entry.DataKey}|{entry.Status}|{entry.Reason}"));
    }

    // ------------------------------------------- 21/22. semantic parity intact

    [Fact]
    public async Task AnAcceptedEnvelope_ProducesTheIdenticalAnalysis()
    {
        RepositoryAnalysis rest = Analyze((await CollectAsync(envelope: null)).Snapshot);
        RepositoryAnalysis hybrid = Analyze((await CollectAsync(Envelope())).Snapshot);

        Assert.Equal(rest.Status, hybrid.Status);
        Assert.Equal(rest.ScoreBreakdown.EarnedScore, hybrid.ScoreBreakdown.EarnedScore);
        Assert.Equal(
            rest.ScoreBreakdown.ApplicableMaximumScore,
            hybrid.ScoreBreakdown.ApplicableMaximumScore);
        Assert.Equal(rest.ScoreBreakdown.NormalizedScore, hybrid.ScoreBreakdown.NormalizedScore);
        Assert.Equal(15, hybrid.SignalResults.Count);
        Assert.Equal(SignalFingerprint(rest), SignalFingerprint(hybrid));
        Assert.Equal(rest.LimitationKeys, hybrid.LimitationKeys);
        Assert.Equal(
            rest.Warnings.Select(warning => warning.Code),
            hybrid.Warnings.Select(warning => warning.Code));
        Assert.Equal(
            rest.CategoryResults.Select(category =>
                $"{category.Category}:{category.Status}:{category.Score}"),
            hybrid.CategoryResults.Select(category =>
                $"{category.Category}:{category.Status}:{category.Score}"));
    }

    // The snapshot-consistency warning is unchanged. The hybrid does not claim
    // the repository was observed atomically — it never did, and one envelope
    // covering three surfaces does not change that.
    [Fact]
    public async Task TheSnapshotConsistencyWarning_IsUnchanged()
    {
        Collected rest = await CollectAsync(envelope: null);
        Collected hybrid = await CollectAsync(Envelope());

        Assert.Equal(rest.Snapshot.CollectionWarnings, hybrid.Snapshot.CollectionWarnings);
        Assert.Contains(
            hybrid.Snapshot.CollectionWarnings,
            warning => warning.Contains("may not be atomic", StringComparison.Ordinal));
    }

    // ------------------------------------------------ 17. cancellation is intact

    // §11.1 / §18.8: caller cancellation propagates and is never converted into a
    // GraphQL failure or a typed unavailability.
    [Fact]
    public async Task CallerCancellation_StillPropagates()
    {
        FakeGitHubApiHandler handler = new(_ => FakeGitHubApiHandler.JsonResponse(Envelope()));
        using HttpClient client = CreateClient(handler);
        using CancellationTokenSource cancellation = new();

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateHybrid(client, handler)
                .CollectAsync(FakeGitHubApiHandler.RepositoryUrl, cancellation.Token));
    }

    // ------------------------------------- 22. Repository Proof is untouched

    // CreateDefault has no identity seam, so Repository Proof resolves the head
    // and tree over REST exactly as it always has (§3.1).
    [Fact]
    public async Task RepositoryProofComposition_StillIssuesBothIdentityRequests()
    {
        FakeGitHubApiHandler handler = new(
            _ => FakeGitHubApiHandler.JsonResponse(Envelope()));
        using HttpClient client = CreateClient(handler);

        await GitHubRepositorySnapshotOrchestrator
            .CreateDefault(client, new FixedTimeProvider(CapturedAt))
            .CollectAsync(FakeGitHubApiHandler.RepositoryUrl);

        Assert.Equal(0, handler.CountRequests("/graphql"));
        Assert.Contains(GitRefRequest, handler.RequestPaths);
        Assert.Contains(GitCommitRequest, handler.RequestPaths);
        Assert.Contains(RecursiveTreeRequest, handler.RequestPaths);
    }

    // -------------------------------------------------------- test support

    private sealed record Collected(RepositorySnapshot Snapshot, FakeGitHubApiHandler Handler);

    private static async Task<Collected> CollectAsync(
        string? envelope,
        Func<HttpResponseMessage>? gitReference = null,
        bool truncatedTree = false)
    {
        FakeGitHubApiHandler handler = new(
            envelope is null
                ? null
                : _ => FakeGitHubApiHandler.JsonResponse(envelope),
            gitReference: gitReference,
            truncatedTree: truncatedTree);
        using HttpClient client = CreateClient(handler);

        GitHubRepositorySnapshotResult result = envelope is null
            ? await GitHubRepositorySnapshotOrchestrator
                .CreateProfileComposition(client, new FixedTimeProvider(CapturedAt))
                .CollectAsync(FakeGitHubApiHandler.RepositoryUrl)
            : await CreateHybrid(client, handler)
                .CollectAsync(FakeGitHubApiHandler.RepositoryUrl);

        Assert.True(result.IsSuccessful);

        return new Collected(result.Snapshot!, handler);
    }

    private static GitHubRepositorySnapshotOrchestrator CreateHybrid(
        HttpClient client,
        FakeGitHubApiHandler handler)
    {
        _ = handler;

        return GitHubRepositorySnapshotOrchestrator.CreateProfileComposition(
            client,
            new FixedTimeProvider(CapturedAt),
            languagesShadow: null,
            new GitHubGraphQlRepositoryEnvelopeSource(
                new GitHubGraphQlTransport(client),
                new FixedTimeProvider(EnvelopeInstant)));
    }

    // The shared fixture envelope with either identity member overridden, so a
    // test can prove which side the snapshot actually followed.
    private static string Envelope(
        string headCommitSha = FakeGitHubApiHandler.HeadCommitSha,
        string rootTreeSha = FakeGitHubApiHandler.RootTreeSha) =>
        WithDefaultBranchRef($$"""
            {
              "name": "main",
              "target": {
                "oid": "{{headCommitSha}}",
                "tree": { "oid": "{{rootTreeSha}}" }
              }
            }
            """);

    private static readonly string NullDefaultBranchRef = WithDefaultBranchRef("null");

    private static readonly string NullTarget = WithDefaultBranchRef("""
        { "name": "main", "target": null }
        """);

    private static readonly string NonCommitTarget = WithDefaultBranchRef("""
        { "name": "main", "target": { } }
        """);

    private static readonly string NullTree = WithDefaultBranchRef($$"""
        {
          "name": "main",
          "target": { "oid": "{{FakeGitHubApiHandler.HeadCommitSha}}", "tree": null }
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

    private static HttpResponseMessage Empty409() =>
        Conflict("""{ "message": "Git Repository is empty." }""");

    // A 409 GitHub also documents — "or unavailable, for example while
    // repository creation is still in progress". The current REST rule treats
    // every 409 that is not the exact empty body as a failure, and this slice
    // does not relax that.
    private static HttpResponseMessage Other409() =>
        Conflict("""{ "message": "Repository is unavailable." }""");

    private static HttpResponseMessage Conflict(string body) =>
        new(HttpStatusCode.Conflict)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static HttpClient CreateClient(FakeGitHubApiHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://api.github.com/") };

    private static RepositoryAnalysis Analyze(RepositorySnapshot snapshot) =>
        new RepositoryAnalysisRunner(new FixedTimeProvider(CapturedAt))
            .Analyze(snapshot, Version, AnalysisId);

    // Everything the collection produced, except the RetrievedAt instants §18.7
    // and §19.2 exclude from parity. They are proven separately above.
    private static string Fingerprint(RepositorySnapshot snapshot) =>
        string.Join(
            "\n",
            $"identity={snapshot.Repository.Owner}/{snapshot.Repository.Name}",
            $"branch={snapshot.Repository.DefaultBranch}",
            $"head={snapshot.HeadCommitSha}",
            $"fileTree={snapshot.FileTree.Items.Count}|{snapshot.FileTree.IsComplete}",
            "files=" + string.Join(",", snapshot.FileTree.Items.Select(file => file.Path)),
            $"workflows={snapshot.Workflows.Items.Count}",
            $"docs={snapshot.DocumentationMarkdown.Items.Count}",
            $"readme={snapshot.Readme?.Path}",
            $"license={snapshot.License?.Key}|{snapshot.LicenseFile?.Path}",
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
            "warnings=" + string.Join("|", snapshot.CollectionWarnings));

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

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
