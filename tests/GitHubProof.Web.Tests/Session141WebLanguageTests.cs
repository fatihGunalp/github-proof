using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Web.Localization;
using GitHubProof.Web.Presentation;
using Xunit;

namespace GitHubProof.Web.Tests;

// Session 141 — Web presentation language. Two concerns:
//
//   1. The approved EN/TR copy reaches the surfaces the contracts name.
//   2. No localized string is an input to a program decision any more.
//
// The second is the point of the notes tests below: they vary the message text
// independently of the canonical identity and assert the rendering decision
// does not move.
public sealed class Session141WebLanguageTests
{
    private readonly WebPresentationLocalizer _text = new();

    // ------------------------------------------------ identity-driven notes

    // One warning identity can be raised once per affected signal, so the same
    // sentence arrives several times with different Codes. It renders once —
    // decided by PresentationKey, not by comparing the sentences.
    [Fact]
    public void Repeated_warning_identities_render_once_even_with_distinct_codes()
    {
        RepositoryReport report = Report(
            warnings:
            [
                Warning("signal.not-applicable.a", RepositoryReportWarningKeys.SignalNotApplicable, "Same sentence."),
                Warning("signal.not-applicable.b", RepositoryReportWarningKeys.SignalNotApplicable, "Same sentence."),
                Warning("repository.fork", RepositoryReportWarningKeys.RepositoryFork, "This repository is a fork.")
            ]);

        RepositoryReportNotesModel notes =
            RepositoryReportNotesPresentation.Create(report);

        Assert.Equal(["Same sentence.", "This repository is a fork."], notes.Warnings);
    }

    // The decisive proof that the dedupe is identity-based: two warnings that
    // share an identity but carry different text still collapse to one, which a
    // string-based Distinct could never do.
    [Fact]
    public void Warning_dedupe_follows_identity_even_when_the_localized_text_differs()
    {
        RepositoryReport report = Report(
            warnings:
            [
                Warning("signal.not-applicable.a", RepositoryReportWarningKeys.SignalNotApplicable, "One wording."),
                Warning("signal.not-applicable.b", RepositoryReportWarningKeys.SignalNotApplicable, "A different wording.")
            ]);

        RepositoryReportNotesModel notes =
            RepositoryReportNotesPresentation.Create(report);

        Assert.Equal(["One wording."], notes.Warnings);
    }

    // The mirror case: identical text under two different canonical identities
    // is NOT merged. Similar copy is not a reason to drop a distinct fact.
    [Fact]
    public void Distinct_warning_identities_are_never_merged_because_their_text_matches()
    {
        RepositoryReport report = Report(
            warnings:
            [
                Warning("repository.fork", RepositoryReportWarningKeys.RepositoryFork, "Identical sentence."),
                Warning("repository.template", RepositoryReportWarningKeys.RepositoryTemplate, "Identical sentence.")
            ]);

        RepositoryReportNotesModel notes =
            RepositoryReportNotesPresentation.Create(report);

        Assert.Equal(["Identical sentence.", "Identical sentence."], notes.Warnings);
    }

    // Contract 14 §12: the NotApplicable warning and the NotApplicable
    // limitation state the same canonical fact, and the warning already carries
    // the scoring consequence, so the limitation is the redundant one.
    [Fact]
    public void The_not_applicable_limitation_is_suppressed_when_its_warning_renders()
    {
        RepositoryReport report = Report(
            warnings:
            [
                Warning("signal.not-applicable.a", RepositoryReportWarningKeys.SignalNotApplicable, "Some signals do not apply.")
            ],
            limitations:
            [
                new RepositoryReportLimitation(
                    RepositoryReportLimitationKeys.AnalysisNotApplicableSignals,
                    "Some signals were excluded because they do not apply."),
                new RepositoryReportLimitation(
                    RepositoryReportLimitationKeys.RepositoryFork,
                    "Fork limitation.")
            ]);

        RepositoryReportNotesModel notes =
            RepositoryReportNotesPresentation.Create(report);

        Assert.Equal(["Fork limitation."], notes.Limitations);
    }

    // Without its warning, the limitation is the only carrier of that fact and
    // must survive.
    [Fact]
    public void The_not_applicable_limitation_survives_when_its_warning_is_absent()
    {
        RepositoryReport report = Report(
            limitations:
            [
                new RepositoryReportLimitation(
                    RepositoryReportLimitationKeys.AnalysisNotApplicableSignals,
                    "Some signals were excluded because they do not apply.")
            ]);

        RepositoryReportNotesModel notes =
            RepositoryReportNotesPresentation.Create(report);

        Assert.Equal(
            ["Some signals were excluded because they do not apply."],
            notes.Limitations);
    }

    // Contract 14 §5/§10 and §6/§11 keep these two pairs semantically distinct:
    // the warning says what happened, the limitation says how to read the
    // result. They must NOT be deduped against each other.
    [Theory]
    [InlineData(
        RepositoryReportWarningKeys.SignalUnavailableEvidence,
        RepositoryReportLimitationKeys.AnalysisUnavailableEvidence)]
    [InlineData(
        RepositoryReportWarningKeys.SignalRequiresHumanReview,
        RepositoryReportLimitationKeys.AnalysisRequiresHumanReview)]
    public void Warning_and_limitation_pairs_that_answer_different_questions_both_render(
        string warningKey,
        string limitationKey)
    {
        RepositoryReport report = Report(
            warnings: [Warning("code", warningKey, "What happened.")],
            limitations: [new RepositoryReportLimitation(limitationKey, "How to read it.")]);

        RepositoryReportNotesModel notes =
            RepositoryReportNotesPresentation.Create(report);

        Assert.Equal(["What happened."], notes.Warnings);
        Assert.Equal(["How to read it."], notes.Limitations);
    }

    [Fact]
    public void Canonical_warning_and_limitation_order_is_preserved()
    {
        RepositoryReport report = Report(
            warnings:
            [
                Warning("a", RepositoryReportWarningKeys.RepositoryArchived, "First."),
                Warning("b", RepositoryReportWarningKeys.RepositoryFork, "Second."),
                Warning("c", RepositoryReportWarningKeys.RepositoryTemplate, "Third.")
            ],
            limitations:
            [
                new RepositoryReportLimitation(RepositoryReportLimitationKeys.RepositoryArchived, "L1."),
                new RepositoryReportLimitation(RepositoryReportLimitationKeys.RepositoryFork, "L2.")
            ]);

        RepositoryReportNotesModel notes =
            RepositoryReportNotesPresentation.Create(report);

        Assert.Equal(["First.", "Second.", "Third."], notes.Warnings);
        Assert.Equal(["L1.", "L2."], notes.Limitations);
    }

    // ----------------------------------------------------- landing + errors

    [Fact]
    public void Landing_copy_matches_the_approved_wording()
    {
        Assert.Equal(
            "See what your GitHub communicates.",
            _text.Get(WebTextKey.LandingTitle, WebCulture.English));
        Assert.Equal(
            "GitHub'ınızda ne olduğunu değil, ne anlattığını görün.",
            _text.Get(WebTextKey.LandingTitle, WebCulture.Turkish));

        Assert.Equal(
            "See what the visible traces across your public GitHub work communicate.",
            _text.Get(WebTextKey.LandingSummary, WebCulture.English));
        Assert.Equal(
            "Public GitHub çalışmalarınızdaki görünür izleri bir araya getirerek ne anlattığını görün.",
            _text.Get(WebTextKey.LandingSummary, WebCulture.Turkish));

        Assert.Equal(
            "See what it communicates",
            _text.Get(WebTextKey.Analyze, WebCulture.English));
        Assert.Equal(
            "Ne anlattığını gör",
            _text.Get(WebTextKey.Analyze, WebCulture.Turkish));

        Assert.Equal(
            "GitHub username, profile, or repository URL",
            _text.Get(WebTextKey.InputLabel, WebCulture.English));
        Assert.Equal(
            "GitHub kullanıcı adı, profil veya repository URL'si",
            _text.Get(WebTextKey.InputLabel, WebCulture.Turkish));
    }

    // Contract 10 §10/§11 and 13: operator and roadmap vocabulary must not
    // appear in ordinary end-user copy.
    [Fact]
    public void No_Web_copy_exposes_operator_or_roadmap_vocabulary()
    {
        string[] prohibited =
        [
            "GITHUB_TOKEN", "MVP", "kanıt yüzeyi", "evidence surface",
            "profil görünümü", "gözlemlenen profil", "GitHub hedefi",
            "deterministik", "deterministic"
        ];

        foreach (WebTextKey key in Enum.GetValues<WebTextKey>())
        {
            foreach (string culture in new[] { WebCulture.English, WebCulture.Turkish })
            {
                string value = _text.Get(key, culture);

                foreach (string term in prohibited)
                {
                    Assert.DoesNotContain(term, value, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }

    // ------------------------------------------------------------- progress

    // Contract 02: every canonical stage has a distinct sentence for each of
    // the three runtime states, in both cultures.
    [Theory]
    [InlineData(
        WebTextKey.ProgressProfileTargetPending,
        WebTextKey.ProgressProfileTargetActive,
        WebTextKey.ProgressProfileTargetCompleted,
        "GitHub profile check",
        "Checking the GitHub profile",
        "GitHub profile checked",
        "GitHub profili kontrol edilecek",
        "GitHub profilini kontrol ediyoruz",
        "GitHub profili kontrol edildi")]
    [InlineData(
        WebTextKey.ProgressRepositoryProofPending,
        WebTextKey.ProgressRepositoryProofActive,
        WebTextKey.ProgressRepositoryProofCompleted,
        "Repository Proof",
        "Preparing Repository Proof",
        "Repository Proof ready",
        "Repository Proof hazırlanacak",
        "Repository Proof'u hazırlıyoruz",
        "Repository Proof hazır")]
    public void Progress_stages_carry_state_aware_copy(
        WebTextKey pending,
        WebTextKey active,
        WebTextKey completed,
        string englishPending,
        string englishActive,
        string englishCompleted,
        string turkishPending,
        string turkishActive,
        string turkishCompleted)
    {
        Assert.Equal(englishPending, _text.Get(pending, WebCulture.English));
        Assert.Equal(englishActive, _text.Get(active, WebCulture.English));
        Assert.Equal(englishCompleted, _text.Get(completed, WebCulture.English));
        Assert.Equal(turkishPending, _text.Get(pending, WebCulture.Turkish));
        Assert.Equal(turkishActive, _text.Get(active, WebCulture.Turkish));
        Assert.Equal(turkishCompleted, _text.Get(completed, WebCulture.Turkish));
    }

    // A sentence per state only works if the three sentences differ; otherwise
    // the state would no longer be readable as text.
    [Fact]
    public void Every_progress_stage_reads_differently_in_each_state()
    {
        (WebTextKey Pending, WebTextKey Active, WebTextKey Completed)[] stages =
        [
            (WebTextKey.ProgressRepositoryTargetPending, WebTextKey.ProgressRepositoryTargetActive, WebTextKey.ProgressRepositoryTargetCompleted),
            (WebTextKey.ProgressRepositoryDataPending, WebTextKey.ProgressRepositoryDataActive, WebTextKey.ProgressRepositoryDataCompleted),
            (WebTextKey.ProgressRepositoryTracesPending, WebTextKey.ProgressRepositoryTracesActive, WebTextKey.ProgressRepositoryTracesCompleted),
            (WebTextKey.ProgressRepositoryProofPending, WebTextKey.ProgressRepositoryProofActive, WebTextKey.ProgressRepositoryProofCompleted),
            (WebTextKey.ProgressProfileTargetPending, WebTextKey.ProgressProfileTargetActive, WebTextKey.ProgressProfileTargetCompleted),
            (WebTextKey.ProgressProfileRepositoriesPending, WebTextKey.ProgressProfileRepositoriesActive, WebTextKey.ProgressProfileRepositoriesCompleted),
            (WebTextKey.ProgressProfileTracesPending, WebTextKey.ProgressProfileTracesActive, WebTextKey.ProgressProfileTracesCompleted)
        ];

        Assert.Equal(7, stages.Length);

        foreach (string culture in new[] { WebCulture.English, WebCulture.Turkish })
        {
            foreach ((WebTextKey pending, WebTextKey active, WebTextKey completed) in stages)
            {
                string[] sentences =
                [
                    _text.Get(pending, culture),
                    _text.Get(active, culture),
                    _text.Get(completed, culture)
                ];

                Assert.Equal(3, sentences.Distinct(StringComparer.Ordinal).Count());
            }
        }
    }

    // Contract 02 keeps the runtime contract's prohibitions: no percentage,
    // ETA, count or outcome the runtime cannot prove.
    [Fact]
    public void Progress_copy_never_promises_a_quantity_or_an_outcome()
    {
        string[] prohibited = ["%", " of ", "ETA", "remaining", "kalan", "yüzde", "tahmini"];

        foreach (WebTextKey key in Enum.GetValues<WebTextKey>())
        {
            if (!key.ToString().StartsWith("Progress", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (string culture in new[] { WebCulture.English, WebCulture.Turkish })
            {
                string value = _text.Get(key, culture);

                Assert.False(value.Any(char.IsDigit), value);

                foreach (string term in prohibited)
                {
                    Assert.DoesNotContain(term, value, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }

    // --------------------------------------------------- signal field labels

    [Fact]
    public void Signal_detail_field_labels_match_the_approved_questions()
    {
        Assert.Equal("What we check", _text.Get(WebTextKey.SignalFieldWhatWeCheck, WebCulture.English));
        Assert.Equal("What we found", _text.Get(WebTextKey.SignalFieldWhatWeFound, WebCulture.English));
        Assert.Equal("Why it matters", _text.Get(WebTextKey.SignalFieldWhyItMatters, WebCulture.English));
        Assert.Equal("What you can do", _text.Get(WebTextKey.SignalFieldWhatYouCanDo, WebCulture.English));
        Assert.Equal("Limit", _text.Get(WebTextKey.SignalFieldLimit, WebCulture.English));
        Assert.Equal("Evidence", _text.Get(WebTextKey.SignalFieldEvidence, WebCulture.English));

        Assert.Equal("Neye bakıyoruz?", _text.Get(WebTextKey.SignalFieldWhatWeCheck, WebCulture.Turkish));
        Assert.Equal("Ne gördük?", _text.Get(WebTextKey.SignalFieldWhatWeFound, WebCulture.Turkish));
        Assert.Equal("Neden önemli?", _text.Get(WebTextKey.SignalFieldWhyItMatters, WebCulture.Turkish));
        Assert.Equal("Ne yapabilirsiniz?", _text.Get(WebTextKey.SignalFieldWhatYouCanDo, WebCulture.Turkish));
        Assert.Equal("Sınır", _text.Get(WebTextKey.SignalFieldLimit, WebCulture.Turkish));
        Assert.Equal("Kanıt", _text.Get(WebTextKey.SignalFieldEvidence, WebCulture.Turkish));
    }

    // ------------------------------------------------------------- fixtures

    private static RepositoryReportWarning Warning(
        string code,
        string presentationKey,
        string message) =>
        new(code, WarningSeverity.Info, presentationKey, message, [], []);

    private static RepositoryReport Report(
        IReadOnlyList<RepositoryReportWarning>? warnings = null,
        IReadOnlyList<RepositoryReportLimitation>? limitations = null)
    {
        DateTimeOffset capturedAt = DateTimeOffset.Parse("2026-08-28T00:00:00Z");

        RepositoryReportIdentity identity = new(
            RepositoryReportSectionKeys.Identity,
            "Analysis source",
            Guid.Parse("04100000-0000-0000-0000-000000000141"),
            "octocat",
            "Hello-World",
            "octocat/Hello-World",
            new Uri("https://github.com/octocat/Hello-World"),
            "main",
            null,
            capturedAt,
            true,
            false,
            false,
            false,
            "C#",
            AnalysisStatus.Completed,
            "Analysis complete",
            "0.1.0",
            "2026.08.28",
            1);

        return new RepositoryReport(
            "en",
            RepositoryReportSectionKeys.CanonicalOrder,
            identity,
            new RepositoryReportSection<RepositoryReportHighlight>(
                RepositoryReportSectionKeys.Proves, "Proves", []),
            new RepositoryReportSection<RepositoryReportHighlight>(
                RepositoryReportSectionKeys.Weakens, "Weakens", []),
            new RepositoryReportSection<RepositoryReportAction>(
                RepositoryReportSectionKeys.DoFirst, "Do first", []),
            new RepositoryReportSection<RepositoryReportUncertainty>(
                RepositoryReportSectionKeys.Uncertainty, "Uncertainty", []),
            new RepositoryReportScoreSummary(
                RepositoryReportSectionKeys.CategoriesAndScore,
                "Categories and score",
                0,
                0,
                null,
                null,
                null,
                ScoreCalculationMethod.RawApplicableScoreNormalization,
                []),
            new RepositoryReportSection<SignalPresentation>(
                RepositoryReportSectionKeys.SignalDetails, "Evidence details", []),
            warnings ?? [],
            limitations ?? [],
            new RepositoryReportDisclaimer(
                RepositoryReportSectionKeys.Disclaimer,
                "What this analysis does not claim",
                RepositoryReportLocalizer.ProductBoundaryPolicyVersion,
                "Boundary."));
    }
}
