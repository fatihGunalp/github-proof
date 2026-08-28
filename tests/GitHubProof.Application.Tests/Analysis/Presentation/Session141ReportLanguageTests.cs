using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using Xunit;

namespace GitHubProof.Application.Tests.Analysis.Presentation;

// Session 141 — human language copy. These tests pin the approved EN/TR copy to
// its canonical identity and guard the semantic distinctions the copy must not
// blur. They assert mappings, never the wording of any single sentence in
// isolation: what matters is that a canonical state always reaches the same
// approved sentence, and that two different canonical states never reach the
// same one.
public sealed class Session141ReportLanguageTests
{
    private readonly RepositoryReportLocalizer _report = new();
    private readonly SignalPresentationLocalizer _signals = new();

    // ---------------------------------------------------------------- score

    [Theory]
    [InlineData(RepositoryReportScoreLabelKeys.LimitedEvidence, ScoreEvidenceStrength.Limited)]
    [InlineData(RepositoryReportScoreLabelKeys.BasicEvidence, ScoreEvidenceStrength.Basic)]
    [InlineData(RepositoryReportScoreLabelKeys.DevelopingEvidence, ScoreEvidenceStrength.Developing)]
    [InlineData(RepositoryReportScoreLabelKeys.StrongEvidence, ScoreEvidenceStrength.Strong)]
    [InlineData(RepositoryReportScoreLabelKeys.VeryStrongEvidence, ScoreEvidenceStrength.VeryStrong)]
    public void Score_evidence_strength_comes_from_the_canonical_label_key(
        string labelKey,
        ScoreEvidenceStrength expected)
    {
        Assert.Equal(
            expected,
            RepositoryReportScoreSemantics.FromLabelKey(labelKey));
    }

    // No score is not a zero score and not a weakest-evidence claim.
    [Fact]
    public void A_null_label_key_maps_to_no_strength_rather_than_the_weakest_band()
    {
        Assert.Null(RepositoryReportScoreSemantics.FromLabelKey(null));
        Assert.NotEqual(
            ScoreEvidenceStrength.Limited,
            RepositoryReportScoreSemantics.FromLabelKey(null));
        Assert.False(RepositoryReportScoreSemantics.IsStrongEvidenceLabelKey(null));
        Assert.Null(_report.GetScoreLabel(null, "en"));
        Assert.Null(_report.GetScoreLabel(null, "tr"));
    }

    [Fact]
    public void An_unknown_label_key_fails_loudly_instead_of_guessing_a_strength()
    {
        Assert.Throws<ArgumentException>(() =>
            RepositoryReportScoreSemantics.FromLabelKey("score.label.invented"));
    }

    // The exact predicate the Identity card used to obtain by searching the
    // localized label for "Güçlü" / "Strong".
    [Theory]
    [InlineData(RepositoryReportScoreLabelKeys.LimitedEvidence, false)]
    [InlineData(RepositoryReportScoreLabelKeys.BasicEvidence, false)]
    [InlineData(RepositoryReportScoreLabelKeys.DevelopingEvidence, false)]
    [InlineData(RepositoryReportScoreLabelKeys.StrongEvidence, true)]
    [InlineData(RepositoryReportScoreLabelKeys.VeryStrongEvidence, true)]
    public void Strong_evidence_covers_exactly_the_two_strong_bands(
        string labelKey,
        bool expected)
    {
        Assert.Equal(
            expected,
            RepositoryReportScoreSemantics.IsStrongEvidenceLabelKey(labelKey));
    }

    // The old behavior was a substring search over the localized label. This
    // proves the typed mapping is independent of that text: the decision is
    // identical in both cultures even though the sentences differ, and it does
    // not track whether the word "strong"/"güçlü" happens to appear.
    [Fact]
    public void Evidence_strength_is_independent_of_the_localized_score_label()
    {
        foreach (string labelKey in RepositoryReportScoreLabelKeys.All)
        {
            string english = _report.GetScoreLabel(labelKey, "en")!;
            string turkish = _report.GetScoreLabel(labelKey, "tr")!;

            Assert.NotEqual(english, turkish);

            bool strong =
                RepositoryReportScoreSemantics.IsStrongEvidenceLabelKey(labelKey);

            Assert.Equal(
                strong,
                english.Contains("strong", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(
                strong,
                turkish.Contains("güçlü", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Theory]
    [InlineData(RepositoryReportScoreLabelKeys.LimitedEvidence, "Limited evidence", "Sınırlı kanıt")]
    [InlineData(RepositoryReportScoreLabelKeys.BasicEvidence, "Basic evidence", "Temel kanıt")]
    [InlineData(RepositoryReportScoreLabelKeys.DevelopingEvidence, "Developing evidence", "Gelişen kanıt")]
    [InlineData(RepositoryReportScoreLabelKeys.StrongEvidence, "Strong evidence", "Güçlü kanıt")]
    [InlineData(RepositoryReportScoreLabelKeys.VeryStrongEvidence, "Very strong evidence", "Çok güçlü kanıt")]
    public void Score_labels_use_the_approved_evidence_wording(
        string labelKey,
        string english,
        string turkish)
    {
        Assert.Equal(english, _report.GetScoreLabel(labelKey, "en"));
        Assert.Equal(turkish, _report.GetScoreLabel(labelKey, "tr"));
    }

    [Fact]
    public void Score_labels_never_describe_the_person_or_the_repository()
    {
        string[] prohibited =
        [
            "developer", "geliştirici", "seniority", "kıdem",
            "hire", "işe al", "profile", "profil",
            "repository quality", "repository kalite"
        ];

        foreach (string labelKey in RepositoryReportScoreLabelKeys.All)
        {
            foreach (string locale in new[] { "en", "tr" })
            {
                string label = _report.GetScoreLabel(labelKey, locale)!;

                foreach (string term in prohibited)
                {
                    Assert.DoesNotContain(term, label, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }

    // ------------------------------------------------------------- statuses

    [Theory]
    [InlineData(AnalysisStatus.Completed, "Analysis complete", "Analiz tamamlandı")]
    [InlineData(AnalysisStatus.Partial, "Some data is missing", "Bazı veriler eksik")]
    [InlineData(AnalysisStatus.InsufficientData, "Not enough data", "Yeterli veri bulunamadı")]
    [InlineData(AnalysisStatus.Failed, "Analysis could not be completed", "Analiz tamamlanamadı")]
    public void Analysis_status_copy_is_keyed_by_the_canonical_status(
        AnalysisStatus status,
        string english,
        string turkish)
    {
        Assert.Equal(english, _report.GetAnalysisStatusLabel(status, "en"));
        Assert.Equal(turkish, _report.GetAnalysisStatusLabel(status, "tr"));
    }

    [Theory]
    [InlineData(CategoryStatus.Completed, "Result available", "Sonuç var")]
    [InlineData(CategoryStatus.Partial, "Some data is missing", "Bazı veriler eksik")]
    [InlineData(CategoryStatus.InsufficientData, "Not enough data", "Yeterli veri yok")]
    [InlineData(CategoryStatus.NotEvaluated, "Not evaluated", "Değerlendirilmedi")]
    public void Category_status_copy_is_keyed_by_the_canonical_status(
        CategoryStatus status,
        string english,
        string turkish)
    {
        Assert.Equal(english, _report.GetCategoryStatusLabel(status, "en"));
        Assert.Equal(turkish, _report.GetCategoryStatusLabel(status, "tr"));
    }

    // Contract 15 §4: the two taxonomies stay separate. Every canonical signal
    // status keeps its own sentence, distinct from every category status, so a
    // reader can never mistake a category result for a signal result.
    [Fact]
    public void Signal_status_and_category_status_taxonomies_never_share_a_label()
    {
        foreach (string locale in new[] { "en", "tr" })
        {
            string[] categoryLabels = Enum.GetValues<CategoryStatus>()
                .Select(status => _report.GetCategoryStatusLabel(status, locale))
                .ToArray();

            string[] signalLabels = Enum.GetValues<SignalStatus>()
                .Select(status => SignalStatusLabel(status, locale))
                .ToArray();

            Assert.Empty(categoryLabels.Intersect(signalLabels, StringComparer.Ordinal));
        }
    }

    [Theory]
    [InlineData(SignalStatus.Strong, "Strong evidence", "Güçlü kanıt")]
    [InlineData(SignalStatus.Present, "Evidence found", "Kanıt var")]
    [InlineData(SignalStatus.Partial, "Partial evidence", "Kısmi kanıt")]
    [InlineData(SignalStatus.Missing, "No evidence found", "Kanıt bulunamadı")]
    [InlineData(SignalStatus.NotApplicable, "Does not apply", "Geçerli değil")]
    [InlineData(SignalStatus.UnavailableEvidence, "Data unavailable", "Veriye erişilemedi")]
    [InlineData(SignalStatus.RequiresHumanReview, "Human review required", "İnsan incelemesi gerekli")]
    public void Signal_status_copy_is_keyed_by_the_canonical_status(
        SignalStatus status,
        string english,
        string turkish)
    {
        Assert.Equal(english, SignalStatusLabel(status, "en"));
        Assert.Equal(turkish, SignalStatusLabel(status, "tr"));
    }

    // The three non-scoring uncertainty statuses must stay visibly different
    // from Missing and from each other. Collapsing any pair would turn a
    // data-availability fact into an evidence-absence claim.
    [Fact]
    public void The_seven_signal_statuses_all_read_differently_in_both_cultures()
    {
        foreach (string locale in new[] { "en", "tr" })
        {
            string[] labels = Enum.GetValues<SignalStatus>()
                .Select(status => SignalStatusLabel(status, locale))
                .ToArray();

            Assert.Equal(labels.Length, labels.Distinct(StringComparer.Ordinal).Count());
        }
    }

    // ------------------------------------------------- warnings/limitations

    // Contract 14 §5/§10: unavailable data must never be worded as missing
    // evidence, and the copy has to say so explicitly.
    [Fact]
    public void Unavailable_evidence_copy_denies_the_missing_reading()
    {
        Assert.Contains(
            "does not mean the evidence is missing",
            _report.GetWarning(RepositoryReportWarningKeys.SignalUnavailableEvidence, "en"),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "kanıtın eksik olduğu anlamına gelmez",
            _report.GetWarning(RepositoryReportWarningKeys.SignalUnavailableEvidence, "tr"),
            StringComparison.OrdinalIgnoreCase);
    }

    // Contract 16 §"Missing" hard rule: the scope qualifier is what keeps
    // Missing from becoming an absolute absence claim.
    [Fact]
    public void Missing_copy_keeps_its_inspected_scope_qualifier()
    {
        Assert.Contains(
            "in the inspected scope",
            SignalLimitation(SignalStatus.Missing, "en"),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "incelenen kapsamda",
            SignalLimitation(SignalStatus.Missing, "tr"),
            StringComparison.OrdinalIgnoreCase);
    }

    // Contract 14 §6/§11 and 16 §7: "deterministic" is internal vocabulary.
    [Fact]
    public void Ordinary_report_copy_never_exposes_deterministic_analysis_vocabulary()
    {
        List<string> copy = [];

        foreach (string locale in new[] { "en", "tr" })
        {
            copy.AddRange(
                RepositoryReportWarningKeys.All
                    .Select(key => _report.GetWarning(key, locale)));
            copy.AddRange(
                RepositoryReportLimitationKeys.All
                    .Select(key => _report.GetLimitation(key, locale)));
            copy.AddRange(
                Enum.GetValues<SignalStatus>()
                    .Select(status => SignalLimitation(status, locale)));
        }

        foreach (string sentence in copy)
        {
            Assert.DoesNotContain("deterministic", sentence, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("deterministik", sentence, StringComparison.OrdinalIgnoreCase);
        }
    }

    // --------------------------------------------------- product boundary

    // Contract 17 §10: the same canonical product boundary is stored in two
    // presentation resources. They may differ in wording only if they still say
    // the same thing — this pins them to the same six exclusions in both
    // cultures so one can never drift away from the other.
    [Theory]
    [InlineData("en")]
    [InlineData("tr")]
    public void Both_product_boundary_resources_carry_the_same_six_exclusions(string locale)
    {
        string reportDisclaimer = _report.GetProductBoundaryDisclaimer(locale);
        string signalDisclaimer = _signals.GetProductBoundaryDisclaimer(locale);

        string[] exclusions = locale == "tr"
            ?
            [
                "kıdemi", "işe alınabilirliği", "yazarlığı",
                "özgünlüğü", "AI kullanımı", "genel kalitesi"
            ]
            :
            [
                "seniority", "employability", "authorship",
                "originality", "AI usage", "overall quality"
            ];

        foreach (string exclusion in exclusions)
        {
            Assert.Contains(exclusion, reportDisclaimer, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(exclusion, signalDisclaimer, StringComparison.OrdinalIgnoreCase);
        }

        // The boundary is person-level: repository evidence IS analyzed, so the
        // disclaimer may never be softened into "GitHub Proof evaluates
        // nothing" (contract 17 §6).
        foreach (string disclaimer in new[] { reportDisclaimer, signalDisclaimer })
        {
            Assert.Contains("repository", disclaimer, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ------------------------------------------------------------- ordering

    [Fact]
    public void The_canonical_report_section_order_is_unchanged()
    {
        Assert.Equal(
            [
                "report.section.identity",
                "report.section.proves",
                "report.section.weakens",
                "report.section.do-first",
                "report.section.uncertainty",
                "report.section.categories-score",
                "report.section.signal-details",
                "report.section.disclaimer"
            ],
            RepositoryReportSectionKeys.CanonicalOrder);
    }

    [Fact]
    public void Every_canonical_presentation_key_resolves_in_both_cultures()
    {
        foreach (string locale in new[] { "en", "tr" })
        {
            foreach (string key in RepositoryReportWarningKeys.All)
            {
                Assert.False(string.IsNullOrWhiteSpace(_report.GetWarning(key, locale)));
            }

            foreach (string key in RepositoryReportLimitationKeys.All)
            {
                Assert.False(string.IsNullOrWhiteSpace(_report.GetLimitation(key, locale)));
            }

            foreach (string key in RepositoryReportScoreLabelKeys.All)
            {
                Assert.False(string.IsNullOrWhiteSpace(_report.GetScoreLabel(key, locale)));
            }
        }
    }

    private string SignalStatusLabel(SignalStatus status, string locale) =>
        _signals.Present(CreateResult(status), locale).StatusLabel;

    private string SignalLimitation(SignalStatus status, string locale) =>
        _signals.Present(CreateResult(status), locale).Limitations.Single();

    private static SignalResult CreateResult(SignalStatus status)
    {
        DateTimeOffset evaluatedAt = DateTimeOffset.Parse("2026-08-28T00:00:00Z");

        bool scorable = status is SignalStatus.Strong
            or SignalStatus.Present
            or SignalStatus.Partial
            or SignalStatus.Missing;

        // Missing means the required data WAS inspected and nothing qualifying
        // was found, so the Domain requires evidence of that inspection. That
        // invariant is exactly why Missing and UnavailableEvidence must keep
        // separate copy.
        IReadOnlyList<EvidenceItem>? evidence = status == SignalStatus.Missing
            ?
            [
                new EvidenceItem(
                    "repository.readme",
                    "GitHub repository metadata",
                    "no README recognized",
                    location: null,
                    evaluatedAt,
                    EvidenceConfidence.High,
                    notes: null)
            ]
            : null;

        return new SignalResult(
            "repository.readme.exists",
            1,
            AnalysisCategory.RepositoryUnderstandability,
            status,
            scorable ? (status == SignalStatus.Missing ? 0 : 5) : null,
            5,
            true,
            evidence,
            evaluatedAt);
    }
}
