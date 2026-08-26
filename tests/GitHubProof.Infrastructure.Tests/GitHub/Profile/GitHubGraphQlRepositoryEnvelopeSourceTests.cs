using System.Net;
using System.Text;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using Xunit;

namespace GitHubProof.Infrastructure.Tests.GitHub.Profile;

// Session 127 — S3 authoritative repository envelope, unit level
// (docs/architecture/public-profile-runtime-collection.md §§18-19).
//
// Every test drives the REAL transport over a stub handler, so the Session 126
// header classification and the §18.4 outcome mapping are exercised rather than
// simulated.
//
// The recurring assertion is not "the envelope is correct" but "the envelope is
// ABSENT". S3's whole safety argument is that anything less than a complete,
// valid, single-response candidate produces no envelope at all, which is what
// makes a REST fallback the only other outcome.
public sealed class GitHubGraphQlRepositoryEnvelopeSourceTests
{
    private static readonly DateTimeOffset Accepted =
        DateTimeOffset.Parse("2026-08-26T09:30:00Z");

    // -------------------------------------- 3. explicit adopted-field mapping

    [Fact]
    public async Task AnAcceptedEnvelope_MapsEveryAdoptedMetadataField()
    {
        GitHubGraphQlRepositoryEnvelope envelope = await ResolveAcceptedAsync(CompleteEnvelope);

        RepositoryIdentity identity = envelope.Repository;

        Assert.Equal("octocat", identity.Owner);
        Assert.Equal("hello-world", identity.Name);
        Assert.Equal("https://github.com/octocat/hello-world", identity.Url.AbsoluteUri);
        Assert.Equal("main", identity.DefaultBranch);
        Assert.True(identity.IsPublic);
        Assert.False(identity.IsArchived);
        Assert.False(identity.IsFork);
        Assert.False(identity.IsTemplate);
        Assert.Equal("C#", identity.PrimaryLanguage);

        RepositoryMetadata metadata = envelope.Metadata;

        Assert.Equal("A deterministic fixture repository.", metadata.Description);
        Assert.Equal("https://example.invalid/demo", metadata.Homepage);
        Assert.Equal(
            DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
            metadata.CreatedAt);
        Assert.True(metadata.IssuesEnabled);
    }

    // The mapping proof that matters: the SAME logical repository, fed once as a
    // REST body and once as a GraphQL envelope, produces the same identity and
    // metadata. A field read from the wrong place fails here even if it is
    // individually well-formed.
    [Fact]
    public async Task AnAcceptedEnvelope_ProducesTheSameIdentityAndMetadataAsRest()
    {
        GitHubRepositoryMetadataCollection rest = await CollectRestAsync(RestBody);
        GitHubGraphQlRepositoryEnvelope envelope = await ResolveAcceptedAsync(CompleteEnvelope);

        Assert.Equal(rest.Repository, envelope.Repository);
        Assert.Equal(rest.Metadata, envelope.Metadata);
        Assert.Equal(rest.Topics, envelope.Topics);
    }

    // Each adopted boolean and optional field is pinned individually, so an
    // envelope that flips one of them is not silently absorbed by the shared
    // fixture above.
    [Theory]
    [InlineData("\"isArchived\": false", "\"isArchived\": true")]
    [InlineData("\"isFork\": false", "\"isFork\": true")]
    [InlineData("\"isTemplate\": false", "\"isTemplate\": true")]
    [InlineData("\"hasIssuesEnabled\": true", "\"hasIssuesEnabled\": false")]
    public async Task AnAcceptedEnvelope_CarriesEachAdoptedFlagThrough(
        string original,
        string flipped)
    {
        GitHubGraphQlRepositoryEnvelope untouched =
            await ResolveAcceptedAsync(CompleteEnvelope);
        GitHubGraphQlRepositoryEnvelope changed =
            await ResolveAcceptedAsync(CompleteEnvelope.Replace(original, flipped));

        Assert.NotEqual(
            Describe(untouched.Repository, untouched.Metadata),
            Describe(changed.Repository, changed.Metadata));
    }

    // A repository GitHub reports no language for is a legitimate shape, exactly
    // as REST `language: null` is, and it does not discard the candidate.
    [Fact]
    public async Task ANullPrimaryLanguage_IsCarriedAsNoPrimaryLanguage()
    {
        GitHubGraphQlRepositoryEnvelope envelope = await ResolveAcceptedAsync(
            CompleteEnvelope.Replace(
                """ "primaryLanguage": { "name": "C#" },""",
                """ "primaryLanguage": null,"""));

        Assert.Null(envelope.Repository.PrimaryLanguage);
    }

    // Absent and JSON null both mean "unset", exactly as an unset REST
    // `description` or `homepage` does — and the OTHER field must survive, so a
    // null cannot quietly blank the pair.
    [Theory]
    [InlineData(""" "description": "A deterministic fixture repository.",""", """ "description": null,""")]
    [InlineData(""" "description": "A deterministic fixture repository.",""", " ")]
    public async Task AnUnsetDescription_IsCarriedAsNull(string original, string replacement)
    {
        GitHubGraphQlRepositoryEnvelope envelope =
            await ResolveAcceptedAsync(CompleteEnvelope.Replace(original, replacement));

        Assert.Null(envelope.Metadata.Description);
        Assert.Equal("https://example.invalid/demo", envelope.Metadata.Homepage);
    }

    [Theory]
    [InlineData(""" "homepageUrl": "https://example.invalid/demo",""", """ "homepageUrl": null,""")]
    [InlineData(""" "homepageUrl": "https://example.invalid/demo",""", " ")]
    public async Task AnUnsetHomepage_IsCarriedAsNull(string original, string replacement)
    {
        GitHubGraphQlRepositoryEnvelope envelope =
            await ResolveAcceptedAsync(CompleteEnvelope.Replace(original, replacement));

        Assert.Null(envelope.Metadata.Homepage);
        Assert.Equal("A deterministic fixture repository.", envelope.Metadata.Description);
    }

    // ---------------------------------------------- 4. topic parity with REST

    // The normalization is not re-implemented for GraphQL: the same blank,
    // trimming and case-insensitive-distinct rules must produce the same list
    // the REST collector produces from the same topics.
    [Theory]
    [InlineData(new[] { "dotnet", "github-api" }, new[] { "dotnet", "github-api" })]
    [InlineData(new[] { " dotnet ", "github-api" }, new[] { "dotnet", "github-api" })]
    [InlineData(new[] { "dotnet", "DotNet" }, new[] { "dotnet" })]
    [InlineData(new[] { "dotnet", "", "  ", "github-api" }, new[] { "dotnet", "github-api" })]
    [InlineData(new string[0], new string[0])]
    public async Task TopicsAreNormalizedExactlyAsRest(string[] raw, string[] expected)
    {
        GitHubGraphQlRepositoryEnvelope envelope = await ResolveAcceptedAsync(
            CompleteEnvelope.Replace(TopicNodes, GraphQlTopicNodes(raw)));
        GitHubRepositoryMetadataCollection rest =
            await CollectRestAsync(RestBody.Replace(RestTopics, RestTopicList(raw)));

        Assert.Equal(expected, envelope.Topics);
        Assert.Equal(rest.Topics, envelope.Topics);
    }

    // ------------------------------------------- 5. exact language byte parity

    // `LanguageEdge.size` is the byte count itself. No percentage, no total, no
    // rounding, and no ordering dependence — the map is keyed with the same
    // case-insensitive comparer the REST collector uses.
    [Fact]
    public async Task LanguageEdgeSizeIsTheExactRestByteCount()
    {
        GitHubGraphQlRepositoryEnvelope envelope = await ResolveAcceptedAsync(CompleteEnvelope);

        Assert.Equal(
            new Dictionary<string, long> { ["C#"] = 1200, ["JavaScript"] = 300 },
            envelope.Languages);
        Assert.Equal(1200, envelope.Languages["c#"]);
    }

    [Fact]
    public async Task ALargeLanguageSize_IsCarriedWithoutLoss()
    {
        const long exact = 9_007_199_254_740_993;

        GitHubGraphQlRepositoryEnvelope envelope = await ResolveAcceptedAsync(
            CompleteEnvelope.Replace("\"size\": 1200", $"\"size\": {exact}"));

        Assert.Equal(exact, envelope.Languages["C#"]);
    }

    [Fact]
    public async Task AZeroLanguageSize_IsAcceptedAsZeroBytes()
    {
        GitHubGraphQlRepositoryEnvelope envelope = await ResolveAcceptedAsync(
            CompleteEnvelope.Replace("\"size\": 1200", "\"size\": 0"));

        Assert.Equal(0, envelope.Languages["C#"]);
    }

    // ------------------------------------------------ 6. one accepted instant

    // The clock is read exactly once per accepted envelope, and only after the
    // whole candidate has been validated. The negative half is the real proof:
    // a discarded candidate consumes no instant at all, so there is no path that
    // stamps a retrieval time onto something never accepted (§18.7).
    [Fact]
    public async Task AnAcceptedEnvelope_CapturesExactlyOneInstant()
    {
        CountingTimeProvider clock = new(Accepted);

        GitHubGraphQlRepositoryEnvelope? envelope = await ResolveAsync(CompleteEnvelope, clock);

        Assert.NotNull(envelope);
        Assert.Equal(Accepted, envelope.RetrievedAt);
        Assert.Equal(1, clock.Reads);
    }

    public static TheoryData<string> DiscardedCandidates =>
        new()
        {
            NullRepositoryEnvelope,
            MissingDefaultBranchEnvelope,
            IncompleteLanguagesEnvelope,
            IncompleteTopicsEnvelope
        };

    [Theory]
    [MemberData(nameof(DiscardedCandidates))]
    public async Task ADiscardedCandidate_CapturesNoInstantAtAll(string body)
    {
        CountingTimeProvider clock = new(Accepted);

        Assert.Null(await ResolveAsync(body, clock));
        Assert.Equal(0, clock.Reads);
    }

    // ------------------------------- 7/8. incomplete connections are discarded

    // The single-envelope completeness rule. `hasNextPage` does not truncate and
    // does not paginate: it discards the WHOLE candidate, and REST then collects
    // every adopted surface in full. That is a fallback, never a cap (§7).
    public static TheoryData<string> IncompleteConnections =>
        new() { IncompleteLanguagesEnvelope, IncompleteTopicsEnvelope };

    [Theory]
    [MemberData(nameof(IncompleteConnections))]
    public async Task AnIncompleteConnection_DiscardsTheWholeCandidate(string body)
    {
        StubTransport transport = new(body);

        Assert.Null(await ResolveAsync(transport));

        // One attempt, then nothing: S3 never follows a cursor.
        Assert.Equal(1, transport.Calls);
    }

    // The partial data is not salvaged. An incomplete languages connection
    // discards the topics and the metadata with it, which is what §18.5's
    // no-merge rule requires.
    [Fact]
    public async Task AnIncompleteLanguagesConnection_AlsoDiscardsMetadataAndTopics()
    {
        Assert.Null(await ResolveAsync(IncompleteLanguagesEnvelope));
    }

    // A missing `hasNextPage` is not read as "complete". An unknown completeness
    // is exactly the ambiguity §18.5 resolves by discarding.
    [Fact]
    public async Task AConnectionWithoutPageInfo_DiscardsTheCandidate()
    {
        Assert.Null(await ResolveAsync(
            CompleteEnvelope.Replace("""{ "hasNextPage": false }""", "{ }")));
    }

    // ------------------------- 9. errors without a provider-wide signal abstain

    [Fact]
    public async Task ADataWithErrorsEnvelope_FallsBackWithoutDisablingTheRun()
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);

        Assert.Null(await ResolveAsync(ErroredEnvelope));

        Assert.False(state.IsDisabledForRun);
        Assert.Equal(0, state.RepositoryEnvelopeServed);
        Assert.Equal(1, state.RepositoryEnvelopeFallback);
    }

    // The next repository still attempts GraphQL, because a per-repository
    // abstention is not a run-scoped disable.
    [Fact]
    public async Task AFollowingRepository_MayStillBeServedAfterAnAbstention()
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);
        StubTransport transport = new(ErroredEnvelope, CompleteEnvelope);

        Assert.Null(await ResolveAsync(transport));
        Assert.NotNull(await ResolveAsync(transport));

        Assert.Equal(2, transport.Calls);
        Assert.Equal(1, state.RepositoryEnvelopeServed);
        Assert.Equal(1, state.RepositoryEnvelopeFallback);
    }

    // ------------------------------- 10/11. envelope-level failures disable

    public static TheoryData<string, HttpStatusCode, string?, string?> ProviderWideCases =>
        new()
        {
            // Session 126 header classification, on an errored 200.
            { ErroredEnvelope, HttpStatusCode.OK, "x-ratelimit-remaining", "0" },
            { ErroredEnvelope, HttpStatusCode.OK, "Retry-After", "60" },

            // Non-200 -> EnvelopeFailure.
            { "{ }", HttpStatusCode.Unauthorized, null, null },
            { "{ }", HttpStatusCode.Forbidden, null, null },
            { "{ }", HttpStatusCode.BadGateway, null, null },
            { "{ }", HttpStatusCode.GatewayTimeout, null, null },

            // Unreadable as a GraphQL envelope -> Malformed.
            { "not json at all", HttpStatusCode.OK, null, null },
            { """{ "unexpected": true }""", HttpStatusCode.OK, null, null }
        };

    [Theory]
    [MemberData(nameof(ProviderWideCases))]
    public async Task AnEnvelopeLevelFailure_DisablesGraphQlForTheRun(
        string body,
        HttpStatusCode status,
        string? header,
        string? value)
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);
        StubTransport transport = new(
            body,
            status: status,
            headers: header is null ? [] : [(header, value!)]);

        Assert.Null(await ResolveAsync(transport));

        Assert.True(state.IsDisabledForRun);
        Assert.Equal(1, state.RepositoryEnvelopeFallback);
    }

    // The point of the disable: a systemic failure costs ONE attempt for the
    // whole execution, not one per admitted repository.
    [Fact]
    public async Task AfterTheDisable_LaterRepositoriesIssueNoGraphQlRequest()
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);
        StubTransport transport = new(
            "{ }",
            CompleteEnvelope,
            status: HttpStatusCode.BadGateway);

        Assert.Null(await ResolveAsync(transport));
        Assert.Null(await ResolveAsync(transport));
        Assert.Null(await ResolveAsync(transport));

        Assert.Equal(1, transport.Calls);
        Assert.Equal(3, state.RepositoryEnvelopeFallback);
        Assert.Equal(0, state.RepositoryEnvelopeServed);
    }

    // §18.6: the disable belongs to one execution. A second scope starts clean.
    [Fact]
    public async Task AFollowingExecution_StartsWithGraphQlEnabledAgain()
    {
        StubTransport transport = new(
            "{ }",
            CompleteEnvelope,
            status: HttpStatusCode.BadGateway);

        ProfileGraphQlExecutionState first = new();

        using (ProfileGraphQlExecutionScope.Begin(first))
        {
            Assert.Null(await ResolveAsync(transport));
            Assert.Null(await ResolveAsync(transport));
        }

        ProfileGraphQlExecutionState second = new();

        using (ProfileGraphQlExecutionScope.Begin(second))
        {
            Assert.NotNull(await ResolveAsync(transport));
        }

        Assert.True(first.IsDisabledForRun);
        Assert.False(second.IsDisabledForRun);
        Assert.Equal(2, transport.Calls);
    }

    // ---------------------------------- 12. a null repository is not evidence

    // `repository: null` on a clean 200 is the §18.13 shape in miniature: it is
    // NOT observed absence, and it is not read as an empty repository, a missing
    // repository or anything else. It is an unusable candidate, and REST decides
    // what the repository actually is.
    [Fact]
    public async Task ANullRepository_FallsBackWithoutDisablingTheRun()
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);

        Assert.Null(await ResolveAsync(NullRepositoryEnvelope));

        Assert.False(state.IsDisabledForRun);
        Assert.Equal(1, state.RepositoryEnvelopeFallback);
    }

    // ------------------------- 13. missing or invalid adopted metadata falls back

    public static TheoryData<string, string> InvalidMetadataCases =>
        new()
        {
            { """ "name": "hello-world",""", " " },
            { """ "name": "hello-world",""", """ "name": "",""" },
            { """ "name": "hello-world",""", """ "name": 7,""" },
            { """ "owner": { "login": "octocat" },""", " " },
            { """ "owner": { "login": "octocat" },""", """ "owner": null,""" },
            { """ "owner": { "login": "octocat" },""", """ "owner": { },""" },
            { """ "isArchived": false,""", " " },
            { """ "isArchived": false,""", """ "isArchived": "false",""" },
            { """ "isFork": false,""", " " },
            { """ "isTemplate": false,""", " " },
            { """ "hasIssuesEnabled": true,""", " " },
            { """ "createdAt": "2024-01-01T00:00:00Z",""", " " },
            { """ "createdAt": "2024-01-01T00:00:00Z",""", """ "createdAt": "not a date",""" },
            { """ "createdAt": "2024-01-01T00:00:00Z",""", """ "createdAt": 1704067200,""" },
            { """ "description": "A deterministic fixture repository.",""", """ "description": 7,""" },
            { """ "homepageUrl": "https://example.invalid/demo",""", """ "homepageUrl": 7,""" },
            { DefaultBranchRef, " " },
            { DefaultBranchRef, """ "defaultBranchRef": null,""" },
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "", "target": { "oid": "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b", "tree": { "oid": "0b9a8f7e6d5c4b3a2f1e0d9c8b7a6f5e4d3c2b1a" } } },""" },

            // Session 129 — every break in the §18.13 identity chain. Each is
            // "candidate unusable", and none of them is emptiness.
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "main", "target": null },""" },

            // What an unmatched `... on Commit` fragment actually produces: an
            // object with none of the fields the fragment gated.
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "main", "target": { } },""" },
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "main", "target": { "oid": "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b" } },""" },
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "main", "target": { "oid": "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b", "tree": null } },""" },
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "main", "target": { "oid": "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b", "tree": { } } },""" },

            // A short OID, an uppercase one and a non-hex one are all refused
            // rather than normalized: this path must never write a
            // `HeadCommitSha` string the REST path would not have produced.
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "main", "target": { "oid": "1a2b3c4", "tree": { "oid": "0b9a8f7e6d5c4b3a2f1e0d9c8b7a6f5e4d3c2b1a" } } },""" },
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "main", "target": { "oid": "1A2B3C4D5E6F7A8B9C0D1E2F3A4B5C6D7E8F9A0B", "tree": { "oid": "0b9a8f7e6d5c4b3a2f1e0d9c8b7a6f5e4d3c2b1a" } } },""" },
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "main", "target": { "oid": "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b", "tree": { "oid": "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" } } },""" },
            { DefaultBranchRef, """ "defaultBranchRef": { "name": "main", "target": { "oid": 7, "tree": { "oid": "0b9a8f7e6d5c4b3a2f1e0d9c8b7a6f5e4d3c2b1a" } } },""" },
            { """ "repositoryTopics":""", """ "somethingElse":""" },
            { """ "languages":""", """ "otherLanguages":""" }
        };

    [Theory]
    [MemberData(nameof(InvalidMetadataCases))]
    public async Task AMissingOrInvalidAdoptedField_DiscardsTheWholeCandidate(
        string original,
        string replacement)
    {
        string body = CompleteEnvelope.Replace(original, replacement);

        Assert.NotEqual(CompleteEnvelope, body);
        Assert.Null(await ResolveAsync(body));
    }

    // §18.13 in one assertion: an empty repository answers `defaultBranchRef:
    // null`, and that null must never be read as successful empty evidence. The
    // candidate is discarded and the REST 409 rule stays the only thing that
    // distinguishes empty from failed.
    [Fact]
    public async Task ANullDefaultBranchRef_IsNeverReadAsEmptyEvidence()
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);

        Assert.Null(await ResolveAsync(MissingDefaultBranchEnvelope));

        Assert.False(state.IsDisabledForRun);
    }

    // A private repository has its own canonical REST outcome. The candidate is
    // discarded so that outcome keeps exactly one implementation.
    [Fact]
    public async Task APrivateRepository_DiscardsTheCandidate()
    {
        Assert.Null(await ResolveAsync(
            CompleteEnvelope.Replace("\"isPrivate\": false", "\"isPrivate\": true")));
    }

    // --------------------------- 14. invalid languages discard the candidate

    public static TheoryData<string, string> InvalidLanguageCases =>
        new()
        {
            // Duplicate under the case-insensitive comparer the REST collector
            // uses, which refuses a duplicate rather than letting one win.
            { """{ "size": 300, "node": { "name": "JavaScript" } }""",
              """{ "size": 300, "node": { "name": "c#" } }""" },
            { """{ "size": 300, "node": { "name": "JavaScript" } }""",
              """{ "size": 300, "node": { "name": "C#" } }""" },

            // Negative bytes, which the REST collector rejects outright.
            { """{ "size": 300, "node": { "name": "JavaScript" } }""",
              """{ "size": -1, "node": { "name": "JavaScript" } }""" },

            // Blank and structurally wrong language names.
            { """{ "size": 300, "node": { "name": "JavaScript" } }""",
              """{ "size": 300, "node": { "name": "  " } }""" },
            { """{ "size": 300, "node": { "name": "JavaScript" } }""",
              """{ "size": 300, "node": { } }""" },
            { """{ "size": 300, "node": { "name": "JavaScript" } }""",
              """{ "size": 300, "node": null }""" },
            { """{ "size": 300, "node": { "name": "JavaScript" } }""",
              """{ "node": { "name": "JavaScript" } }""" },
            { """{ "size": 300, "node": { "name": "JavaScript" } }""",
              """{ "size": "300", "node": { "name": "JavaScript" } }""" },
            { """{ "size": 300, "node": { "name": "JavaScript" } }""",
              """{ "size": 1.5, "node": { "name": "JavaScript" } }""" }
        };

    [Theory]
    [MemberData(nameof(InvalidLanguageCases))]
    public async Task AnInvalidLanguageEdge_DiscardsTheWholeCandidate(
        string original,
        string replacement)
    {
        string body = CompleteEnvelope.Replace(original, replacement);

        Assert.NotEqual(CompleteEnvelope, body);
        Assert.Null(await ResolveAsync(body));
    }

    // A structurally broken topic node is a different case from a blank one: the
    // blank is dropped exactly as REST drops it, the broken one discards the
    // candidate.
    [Fact]
    public async Task AStructurallyBrokenTopicNode_DiscardsTheWholeCandidate()
    {
        Assert.Null(await ResolveAsync(
            CompleteEnvelope.Replace(
                """{ "topic": { "name": "github-api" } }""",
                """{ "topic": null }""")));
    }

    // -------------------------- 15. a clean Success is never demoted by a header

    // The Session 126 rule, re-proven at this seam: `x-ratelimit-remaining: 0`
    // on a response carrying NO errors is not a failure. Demoting it would throw
    // away a complete, valid envelope on a header alone.
    [Fact]
    public async Task ASuccessfulEnvelopeWithZeroRemaining_IsStillServed()
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);
        StubTransport transport = new(
            CompleteEnvelope,
            headers: [("x-ratelimit-remaining", "0"), ("x-ratelimit-resource", "graphql")]);

        Assert.NotNull(await ResolveAsync(transport));

        Assert.False(state.IsDisabledForRun);
        Assert.Equal(1, state.RepositoryEnvelopeServed);
        Assert.Equal(0, state.RepositoryEnvelopeFallback);
    }

    // -------------------------------------------- 16. cancellation propagates

    // Caller cancellation is not a provider failure. It propagates, and it must
    // NOT become a fallback, a served envelope or a run-scoped disable
    // (§11.1, §18.8).
    [Fact]
    public async Task CallerCancellation_PropagatesAndIsNeverAFallback()
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);
        using CancellationTokenSource cancellation = new();
        StubTransport transport = new(CompleteEnvelope) { CancelOnSend = cancellation };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Resolve(transport).ResolveAsync(RepositoryUrl, cancellation.Token));

        Assert.False(state.IsDisabledForRun);
        Assert.Equal(0, state.RepositoryEnvelopeServed);
        Assert.Equal(0, state.RepositoryEnvelopeFallback);
    }

    // An HttpClient timeout is NOT caller cancellation: the transport already
    // maps it to EnvelopeFailure, so it disables the run rather than escaping as
    // a cancellation the pipeline would misread (Session 125, G-1).
    [Fact]
    public async Task ATransportTimeout_DisablesTheRunRatherThanPropagating()
    {
        ProfileGraphQlExecutionState state = new();
        using IDisposable scope = ProfileGraphQlExecutionScope.Begin(state);
        StubTransport transport = new(CompleteEnvelope) { TimeOut = true };

        Assert.Null(await ResolveAsync(transport));

        Assert.True(state.IsDisabledForRun);
    }

    // ------------------------------------------------ 21. the query stays bare

    // §18.3, §18.12 and the S3 boundary, asserted against the operation text
    // itself rather than against a comment describing it.
    [Theory]
    [InlineData("totalCount")]
    [InlineData("totalSize")]
    [InlineData("isEmpty")]
    [InlineData("licenseInfo")]
    [InlineData("history")]
    [InlineData("pullRequests")]
    [InlineData("releases")]
    [InlineData("refs")]
    [InlineData("object")]
    [InlineData("stargazer")]
    [InlineData("mutation")]

    // Session 129. The tree OID is adopted; the tree CONTENTS are not. A
    // query that asked for `entries` would be fetching the file tree over
    // GraphQL, which §18.2 keeps on REST because only REST carries
    // `truncated`.
    [InlineData("entries")]
    public async Task TheQuery_NeverRequestsAnUnadoptedField(string field)
    {
        StubTransport transport = new(CompleteEnvelope);

        await ResolveAsync(transport);

        Assert.DoesNotContain(field, transport.LastQuery!, StringComparison.OrdinalIgnoreCase);
    }

    // The positive half, so the test above cannot pass vacuously against an
    // empty or unrelated query.
    [Theory]
    [InlineData("repositoryTopics")]
    [InlineData("languages")]
    [InlineData("hasNextPage")]
    [InlineData("defaultBranchRef")]
    [InlineData("primaryLanguage")]
    [InlineData("hasIssuesEnabled")]

    // Session 129 — the identity chain, including the `... on Commit`
    // fragment that is what makes a non-Commit target resolve to nothing.
    [InlineData("target")]
    [InlineData("... on Commit")]
    [InlineData("oid")]
    [InlineData("tree")]
    public async Task TheQuery_RequestsEveryAdoptedSurface(string field)
    {
        StubTransport transport = new(CompleteEnvelope);

        await ResolveAsync(transport);

        Assert.Contains(field, transport.LastQuery!, StringComparison.Ordinal);
    }

    // Identity travels as variables, so the operation text is a constant and no
    // repository name is ever interpolated into a query.
    [Fact]
    public async Task RepositoryIdentity_TravelsAsVariablesAndNeverInTheQuery()
    {
        StubTransport transport = new(CompleteEnvelope);

        await ResolveAsync(transport);

        Assert.DoesNotContain("octocat", transport.LastQuery!, StringComparison.Ordinal);
        Assert.DoesNotContain("hello-world", transport.LastQuery!, StringComparison.Ordinal);
        Assert.Equal("octocat", transport.LastVariables!["owner"]!.GetValue<string>());
        Assert.Equal("hello-world", transport.LastVariables["name"]!.GetValue<string>());
    }

    // One physical POST per repository. S3 never paginates an authoritative
    // envelope, so a served repository can never cost two.
    [Fact]
    public async Task AnAcceptedRepository_CostsExactlyOneRequest()
    {
        StubTransport transport = new(CompleteEnvelope);

        Assert.NotNull(await ResolveAsync(transport));

        Assert.Equal(1, transport.Calls);
    }

    // -------------------------------------------------------- test support

    private const string Repository = "https://github.com/octocat/hello-world";

    private static readonly GitHubRepositoryUrl RepositoryUrl =
        GitHubRepositoryUrl.Parse(Repository);

    private const string TopicNodes =
        """[ { "topic": { "name": "dotnet" } }, { "topic": { "name": "github-api" } } ]""";

    private const string RestTopics = """["dotnet", "github-api"]""";

    // Session 129. The same two SHAs the REST `git/ref` and `git/commits`
    // fixture bodies carry, so an accepted envelope must reproduce them exactly
    // (§19.17).
    private const string HeadCommitSha = "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b";
    private const string RootTreeSha = "0b9a8f7e6d5c4b3a2f1e0d9c8b7a6f5e4d3c2b1a";

    // Kept on ONE line, and as ONE constant, so every substitution below
    // replaces the whole `defaultBranchRef` member rather than a fragment of
    // it that could quietly stop matching.
    private const string DefaultBranchRef = $$"""
         "defaultBranchRef": { "name": "main", "target": { "oid": "{{HeadCommitSha}}", "tree": { "oid": "{{RootTreeSha}}" } } },
        """;


    private const string CompleteEnvelope = $$"""
        {
          "data": {
            "repository": {
              "name": "hello-world",
              "owner": { "login": "octocat" },
              "isPrivate": false,
              "isArchived": false,
              "isFork": false,
              "isTemplate": false,
              "description": "A deterministic fixture repository.",
              "homepageUrl": "https://example.invalid/demo",
              "createdAt": "2024-01-01T00:00:00Z",
              "hasIssuesEnabled": true,
              "primaryLanguage": { "name": "C#" },
            {{DefaultBranchRef}}
              "repositoryTopics": {
                "pageInfo": { "hasNextPage": false },
                "nodes": {{TopicNodes}}
              },
              "languages": {
                "pageInfo": { "hasNextPage": false },
                "edges": [
                  { "size": 1200, "node": { "name": "C#" } },
                  { "size": 300, "node": { "name": "JavaScript" } }
                ]
              }
            }
          }
        }
        """;

    private const string RestBody = $$"""
        {
          "name": "hello-world",
          "owner": { "login": "octocat" },
          "private": false,
          "archived": false,
          "fork": false,
          "is_template": false,
          "language": "C#",
          "default_branch": "main",
          "description": "A deterministic fixture repository.",
          "homepage": "https://example.invalid/demo",
          "created_at": "2024-01-01T00:00:00Z",
          "has_issues": true,
          "topics": {{RestTopics}}
        }
        """;

    private const string NullRepositoryEnvelope = """{ "data": { "repository": null } }""";

    private const string ErroredEnvelope = """
        {
          "data": { "repository": null },
          "errors": [
            {
              "type": "RATE_LIMITED",
              "path": ["repository"],
              "message": "API rate limit exceeded.",
              "extensions": { "code": "rateLimited" }
            }
          ]
        }
        """;

    private static readonly string MissingDefaultBranchEnvelope = CompleteEnvelope
        .Replace(DefaultBranchRef, """ "defaultBranchRef": null,""");

    // Both connections carry the same pageInfo line, so the two variants are
    // built by position: topics is the first connection in the envelope,
    // languages the second.
    private static readonly string IncompleteTopicsEnvelope =
        ReplaceAt(CompleteEnvelope, CompleteEnvelope.IndexOf(PageInfoComplete, StringComparison.Ordinal));

    private static readonly string IncompleteLanguagesEnvelope =
        ReplaceAt(CompleteEnvelope, CompleteEnvelope.LastIndexOf(PageInfoComplete, StringComparison.Ordinal));

    private const string PageInfoComplete = """{ "hasNextPage": false }""";

    private const string PageInfoIncomplete = """{ "hasNextPage": true }""";

    private static string ReplaceAt(string body, int index) =>
        body[..index] + PageInfoIncomplete + body[(index + PageInfoComplete.Length)..];

    private static string GraphQlTopicNodes(IEnumerable<string> topics) =>
        "[ " + string.Join(
            ", ",
            topics.Select(topic => $$"""{ "topic": { "name": "{{topic}}" } }""")) + " ]";

    private static string RestTopicList(IEnumerable<string> topics) =>
        "[" + string.Join(", ", topics.Select(topic => $"\"{topic}\"")) + "]";

    private static string Describe(RepositoryIdentity identity, RepositoryMetadata metadata) =>
        string.Join(
            "|",
            identity.Owner,
            identity.Name,
            identity.Url.AbsoluteUri,
            identity.DefaultBranch,
            identity.IsPublic,
            identity.IsArchived,
            identity.IsFork,
            identity.IsTemplate,
            identity.PrimaryLanguage,
            metadata.Description,
            metadata.Homepage,
            metadata.CreatedAt.ToString("O"),
            metadata.IssuesEnabled);

    private static async Task<GitHubGraphQlRepositoryEnvelope> ResolveAcceptedAsync(string body)
    {
        GitHubGraphQlRepositoryEnvelope? envelope = await ResolveAsync(body);

        Assert.NotNull(envelope);

        return envelope;
    }

    private static Task<GitHubGraphQlRepositoryEnvelope?> ResolveAsync(
        string body,
        TimeProvider? timeProvider = null) =>
        ResolveAsync(new StubTransport(body), timeProvider);

    private static async Task<GitHubGraphQlRepositoryEnvelope?> ResolveAsync(
        StubTransport transport,
        TimeProvider? timeProvider = null)
    {
        return await Resolve(transport, timeProvider).ResolveAsync(RepositoryUrl);
    }

    private static GitHubGraphQlRepositoryEnvelopeSource Resolve(
        StubTransport transport,
        TimeProvider? timeProvider = null) =>
        new(transport, timeProvider ?? new CountingTimeProvider(Accepted));

    private static async Task<GitHubRepositoryMetadataCollection> CollectRestAsync(string body)
    {
        using HttpClient client = new(new SingleResponseHandler(body))
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        return await new GitHubRepositoryMetadataCollector(client, new CountingTimeProvider(Accepted))
            .CollectAsync(GitHubRepositoryUrl.Parse(Repository));
    }

    // Drives the REAL transport, so the §18.4 outcome mapping and the Session
    // 126 header classification are under test rather than stubbed away.
    private sealed class StubTransport : IGitHubGraphQlTransport
    {
        private readonly GitHubGraphQlTransport inner;
        private readonly Handler handler;

        public StubTransport(
            string first,
            string? later = null,
            HttpStatusCode status = HttpStatusCode.OK,
            (string Name, string Value)[]? headers = null)
        {
            handler = new Handler(this, first, later, status, headers ?? []);
            Client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.github.com/")
            };
            inner = new GitHubGraphQlTransport(Client);
        }

        public HttpClient Client { get; }

        public int Calls => handler.Calls;

        public string? LastQuery { get; private set; }

        public System.Text.Json.Nodes.JsonObject? LastVariables { get; private set; }

        public CancellationTokenSource? CancelOnSend { get; init; }

        public bool TimeOut { get; init; }

        public Task<GitHubGraphQlResponse> SendAsync(
            GitHubGraphQlRequest request,
            CancellationToken cancellationToken = default)
        {
            LastQuery = request.Query;
            LastVariables = request.Variables;

            return inner.SendAsync(request, cancellationToken);
        }

        private sealed class Handler(
            StubTransport owner,
            string first,
            string? later,
            HttpStatusCode status,
            (string Name, string Value)[] headers) : HttpMessageHandler
        {
            private int calls;

            public int Calls => calls;

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                calls++;

                if (owner.CancelOnSend is not null)
                {
                    owner.CancelOnSend.Cancel();
                    cancellationToken.ThrowIfCancellationRequested();
                }

                // Exactly what HttpClient raises when its own Timeout elapses:
                // a TaskCanceledException while the caller's token is alive.
                if (owner.TimeOut)
                {
                    throw new TaskCanceledException("The request timed out.");
                }

                HttpResponseMessage response = new(calls == 1 || later is null ? status : HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        calls == 1 || later is null ? first : later,
                        Encoding.UTF8,
                        "application/json")
                };

                foreach ((string name, string value) in headers)
                {
                    response.Headers.TryAddWithoutValidation(name, value);
                }

                return Task.FromResult(response);
            }
        }
    }

    private sealed class SingleResponseHandler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
    }

    // Counts reads so a test can prove the retrieval instant is consumed exactly
    // once, and only for an accepted envelope.
    private sealed class CountingTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private int reads;

        public int Reads => reads;

        public override DateTimeOffset GetUtcNow()
        {
            Interlocked.Increment(ref reads);

            return utcNow;
        }
    }
}
