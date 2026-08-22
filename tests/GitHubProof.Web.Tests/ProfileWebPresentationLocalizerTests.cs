using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Web.Localization;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class ProfileWebPresentationLocalizerTests
{
    private readonly ProfileWebPresentationLocalizer localizer = new();

    [Theory]
    [InlineData(ProfileReportSectionKey.TechnicalFocusAreas, "Visible technical direction", "Görünen teknik yön")]
    [InlineData(ProfileReportSectionKey.EngineeringPracticePatterns, "Recurring engineering practices", "Tekrar eden üretim pratikleri")]
    [InlineData(ProfileReportSectionKey.RepresentativeRepositories, "Starting point", "Başlangıç noktası")]
    [InlineData(ProfileReportSectionKey.WhyThisConclusion, "Why this view", "Bu görünümü neden görüyoruz?")]
    public void Required_profile_section_labels_are_keyed_by_canonical_identity(
        ProfileReportSectionKey section,
        string english,
        string turkish)
    {
        Assert.Equal(english, localizer.GetSectionLabel(section, WebCulture.English));
        Assert.Equal(turkish, localizer.GetSectionLabel(section, WebCulture.Turkish));
    }

    [Theory]
    [InlineData("C#")]
    [InlineData("JavaScript")]
    [InlineData("Python")]
    public void Technical_focus_language_identity_is_never_translated(string language)
    {
        Assert.Equal(
            language,
            localizer.GetPatternLabel(
                ProfilePatternFamily.TechnicalFocus,
                language,
                WebCulture.English));
        Assert.Equal(
            language,
            localizer.GetPatternLabel(
                ProfilePatternFamily.TechnicalFocus,
                language,
                WebCulture.Turkish));
    }

    [Fact]
    public void Engineering_practice_mapping_uses_only_canonical_pattern_keys()
    {
        Assert.Equal(
            "Automated testing",
            localizer.GetPatternLabel(
                ProfilePatternFamily.EngineeringPractice,
                ProfilePatternPromotionPolicy.AutomatedTestingPatternKey,
                WebCulture.English));
        Assert.Equal(
            "Otomatik testler",
            localizer.GetPatternLabel(
                ProfilePatternFamily.EngineeringPractice,
                ProfilePatternPromotionPolicy.AutomatedTestingPatternKey,
                WebCulture.Turkish));
        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetPatternLabel(
                ProfilePatternFamily.EngineeringPractice,
                "invented-capability",
                WebCulture.English));
    }

    [Theory]
    [InlineData(ProfileReportSectionKey.ProblemAreaPatterns)]
    [InlineData(ProfileReportSectionKey.RoleEvidenceAlignment)]
    public void Unsupported_and_deferred_sections_have_no_active_mapping(
        ProfileReportSectionKey section)
    {
        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetSectionLabel(section, WebCulture.English));
    }

    // --- P10-I3 Meaning layer (presentation-language-contract.md §8.4) ---

    private static ProfileSummaryFact SupportedFact(
        ProfileReportSectionKey section,
        int itemCount) =>
        new(section, ProfileReportSectionAvailability.Supported, itemCount);

    private static IReadOnlyList<ProfileSummaryFact> MeaningFacts(
        int technicalFocusItemCount,
        int engineeringPracticeItemCount) =>
        [
            SupportedFact(ProfileReportSectionKey.TechnicalFocusAreas, technicalFocusItemCount),
            SupportedFact(ProfileReportSectionKey.EngineeringPracticePatterns, engineeringPracticeItemCount)
        ];

    [Fact]
    public void Meaning_M1_technical_direction_and_engineering_practice_present()
    {
        IReadOnlyList<ProfileSummaryFact> facts = MeaningFacts(
            technicalFocusItemCount: 1,
            engineeringPracticeItemCount: 1);

        Assert.Equal(
            "The visible public evidence shows recurring technical direction and recurring engineering practices.",
            localizer.GetMeaningSummary(facts, WebCulture.English));
        Assert.Equal(
            "Görünen herkese açık kanıtlar, tekrar eden teknik yön ve tekrar eden üretim pratikleri gösteriyor.",
            localizer.GetMeaningSummary(facts, WebCulture.Turkish));
    }

    [Fact]
    public void Meaning_M2_technical_direction_present_engineering_practice_empty()
    {
        IReadOnlyList<ProfileSummaryFact> facts = MeaningFacts(
            technicalFocusItemCount: 1,
            engineeringPracticeItemCount: 0);

        Assert.Equal(
            "The visible public evidence shows recurring technical direction.",
            localizer.GetMeaningSummary(facts, WebCulture.English));
        Assert.Equal(
            "Görünen herkese açık kanıtlar, tekrar eden teknik yön gösteriyor.",
            localizer.GetMeaningSummary(facts, WebCulture.Turkish));
    }

    [Fact]
    public void Meaning_M3_technical_direction_empty_engineering_practice_present()
    {
        IReadOnlyList<ProfileSummaryFact> facts = MeaningFacts(
            technicalFocusItemCount: 0,
            engineeringPracticeItemCount: 1);

        Assert.Equal(
            "The visible public evidence shows recurring engineering practices.",
            localizer.GetMeaningSummary(facts, WebCulture.English));
        Assert.Equal(
            "Görünen herkese açık kanıtlar, tekrar eden üretim pratikleri gösteriyor.",
            localizer.GetMeaningSummary(facts, WebCulture.Turkish));
    }

    [Fact]
    public void Meaning_M4_technical_direction_and_engineering_practice_empty()
    {
        IReadOnlyList<ProfileSummaryFact> facts = MeaningFacts(
            technicalFocusItemCount: 0,
            engineeringPracticeItemCount: 0);

        Assert.Equal(
            "The current analysis did not establish a recurring technical direction or engineering-practice pattern from the visible public evidence.",
            localizer.GetMeaningSummary(facts, WebCulture.English));
        Assert.Equal(
            "Mevcut analiz, görünen herkese açık kanıtlardan tekrar eden bir teknik yön veya üretim pratiği ortaya koymadı.",
            localizer.GetMeaningSummary(facts, WebCulture.Turkish));
    }

    [Fact]
    public void Meaning_ignores_every_fact_other_than_the_two_locked_dimensions()
    {
        // RepresentativeRepositories, WhyThisConclusion, Limitations,
        // RepositoryDrillDown, ProblemAreaPatterns and RoleEvidenceAlignment
        // facts must not influence the Meaning synthesis at all.
        IReadOnlyList<ProfileSummaryFact> facts =
        [
            SupportedFact(ProfileReportSectionKey.TechnicalFocusAreas, 1),
            SupportedFact(ProfileReportSectionKey.EngineeringPracticePatterns, 0),
            SupportedFact(ProfileReportSectionKey.RepresentativeRepositories, 5),
            SupportedFact(ProfileReportSectionKey.WhyThisConclusion, 5),
            SupportedFact(ProfileReportSectionKey.Limitations, 2),
            SupportedFact(ProfileReportSectionKey.RepositoryDrillDown, 5),
            new(ProfileReportSectionKey.ProblemAreaPatterns, ProfileReportSectionAvailability.Unsupported, 0),
            new(ProfileReportSectionKey.RoleEvidenceAlignment, ProfileReportSectionAvailability.Deferred, 0)
        ];

        Assert.Equal(
            "The visible public evidence shows recurring technical direction.",
            localizer.GetMeaningSummary(facts, WebCulture.English));
    }

    [Fact]
    public void Meaning_never_renders_ItemCount_regardless_of_its_magnitude()
    {
        IReadOnlyList<ProfileSummaryFact> oneItem = MeaningFacts(1, 1);
        IReadOnlyList<ProfileSummaryFact> manyItems = MeaningFacts(47, 312);

        Assert.Equal(
            localizer.GetMeaningSummary(oneItem, WebCulture.English),
            localizer.GetMeaningSummary(manyItems, WebCulture.English));
    }

    [Fact]
    public void Meaning_fails_loudly_when_an_expected_fact_is_missing()
    {
        IReadOnlyList<ProfileSummaryFact> facts =
        [
            SupportedFact(ProfileReportSectionKey.EngineeringPracticePatterns, 1)
        ];

        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetMeaningSummary(facts, WebCulture.English));
    }

    [Fact]
    public void Meaning_fails_loudly_when_an_expected_fact_is_duplicated()
    {
        IReadOnlyList<ProfileSummaryFact> facts =
        [
            SupportedFact(ProfileReportSectionKey.TechnicalFocusAreas, 1),
            SupportedFact(ProfileReportSectionKey.TechnicalFocusAreas, 0),
            SupportedFact(ProfileReportSectionKey.EngineeringPracticePatterns, 1)
        ];

        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetMeaningSummary(facts, WebCulture.English));
    }

    [Fact]
    public void Meaning_fails_loudly_when_an_expected_fact_is_not_Supported()
    {
        IReadOnlyList<ProfileSummaryFact> facts =
        [
            new(ProfileReportSectionKey.TechnicalFocusAreas, ProfileReportSectionAvailability.Unsupported, 0),
            SupportedFact(ProfileReportSectionKey.EngineeringPracticePatterns, 1)
        ];

        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetMeaningSummary(facts, WebCulture.English));
    }

    [Theory]
    [InlineData(
        ProfileReportSectionKey.TechnicalFocusAreas,
        "The visible public evidence did not establish a repeated technical direction under the current analysis rules.",
        "Görünen herkese açık kanıtlar, mevcut analiz kuralları altında tekrar eden bir teknik yön ortaya koymadı.")]
    [InlineData(
        ProfileReportSectionKey.EngineeringPracticePatterns,
        "The visible public evidence did not establish recurring engineering practices under the current analysis rules.",
        "Görünen herkese açık kanıtlar, mevcut analiz kuralları altında tekrar eden üretim pratikleri ortaya koymadı.")]
    [InlineData(
        ProfileReportSectionKey.RepresentativeRepositories,
        "No starting-point repository was selected from the current visible public evidence.",
        "Mevcut görünen herkese açık kanıtlardan bir başlangıç noktası repository'si seçilmedi.")]
    public void Supported_empty_section_copy_is_locked_EN_TR(
        ProfileReportSectionKey section,
        string english,
        string turkish)
    {
        Assert.Equal(english, localizer.GetEmptySectionMessage(section, WebCulture.English));
        Assert.Equal(turkish, localizer.GetEmptySectionMessage(section, WebCulture.Turkish));
    }

    [Theory]
    [InlineData(ProfileReportSectionKey.ProblemAreaPatterns)]
    [InlineData(ProfileReportSectionKey.RoleEvidenceAlignment)]
    public void Supported_empty_section_copy_has_no_mapping_for_unsupported_or_deferred_sections(
        ProfileReportSectionKey section)
    {
        // WhyThisConclusion and RepositoryDrillDown are Supported and gained
        // their own locked empty copy in P10-I4 (§8.5); only genuinely
        // Unsupported/Deferred sections must still have no mapping.
        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetEmptySectionMessage(section, WebCulture.English));
    }

    [Fact]
    public void Every_current_limitation_and_warning_code_has_EN_TR_mapping()
    {
        foreach (ProfileLimitationCode code in Enum.GetValues<ProfileLimitationCode>())
        {
            Assert.NotEmpty(localizer.GetLimitationLabel(code, WebCulture.English));
            Assert.NotEmpty(localizer.GetLimitationLabel(code, WebCulture.Turkish));
        }

        foreach (ProfileWarningCode code in Enum.GetValues<ProfileWarningCode>())
        {
            Assert.NotEmpty(localizer.GetWarningLabel(code, WebCulture.English));
            Assert.NotEmpty(localizer.GetWarningLabel(code, WebCulture.Turkish));
        }
    }

    // --- P10-I4 Why + Repository evidence (presentation-language-contract.md §8.5/§8.6) ---

    [Theory]
    [InlineData(
        ProfileReportSectionKey.WhyThisConclusion,
        "The visible public evidence did not establish a repeated pattern to explain under the current analysis rules.",
        "Görünen herkese açık kanıtlar, mevcut analiz kuralları altında açıklanacak tekrar eden bir örüntü ortaya koymadı.")]
    [InlineData(
        ProfileReportSectionKey.RepositoryDrillDown,
        "The visible public evidence contains no public repository to inspect.",
        "Görünen herkese açık kanıtlar, incelenebilecek herkese açık repository içermiyor.")]
    public void Supported_empty_copy_is_locked_for_the_two_P10I4_sections(
        ProfileReportSectionKey section,
        string english,
        string turkish)
    {
        Assert.Equal(english, localizer.GetEmptySectionMessage(section, WebCulture.English));
        Assert.Equal(turkish, localizer.GetEmptySectionMessage(section, WebCulture.Turkish));
    }

    [Theory]
    [InlineData(
        RepositoryEligibilityStatus.Contributing,
        "Contributed evidence to this view",
        "Bu görünüme kanıt sağladı")]
    [InlineData(
        RepositoryEligibilityStatus.Restricted,
        "Contributed evidence to this view with restrictions",
        "Bu görünüme kısıtlamalarla kanıt sağladı")]
    [InlineData(
        RepositoryEligibilityStatus.NonContributing,
        "Did not contribute evidence to this view",
        "Bu görünüme kanıt sağlamadı")]
    [InlineData(
        RepositoryEligibilityStatus.Undetermined,
        "Its contribution to this view could not be determined",
        "Bu görünüme katkısı belirlenemedi")]
    public void Repository_status_copy_is_locked_and_exhaustive(
        RepositoryEligibilityStatus status,
        string english,
        string turkish)
    {
        Assert.Equal(english, localizer.GetRepositoryStatusLabel(status, WebCulture.English));
        Assert.Equal(turkish, localizer.GetRepositoryStatusLabel(status, WebCulture.Turkish));
    }

    [Fact]
    public void Repository_status_fails_loudly_for_an_undefined_status()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            localizer.GetRepositoryStatusLabel((RepositoryEligibilityStatus)99, WebCulture.English));
    }

    [Fact]
    public void Why_supporting_repositories_lead_in_is_locked_EN_TR()
    {
        Assert.Equal(
            "Observed across these public repositories",
            localizer.GetWhySupportingRepositoriesLeadIn(WebCulture.English));
        Assert.Equal(
            "Şu herkese açık repository'lerde gözlemlendi",
            localizer.GetWhySupportingRepositoriesLeadIn(WebCulture.Turkish));
    }

    [Fact]
    public void Repository_fresh_analysis_lead_in_is_locked_EN_TR()
    {
        Assert.Equal(
            "Selecting a repository runs a fresh Repository Proof analysis.",
            localizer.GetRepositoryFreshAnalysisLeadIn(WebCulture.English));
        Assert.Equal(
            "Bir repository seçmek yeni bir Repository Proof analizi başlatır.",
            localizer.GetRepositoryFreshAnalysisLeadIn(WebCulture.Turkish));
    }

    [Fact]
    public void Repository_action_visible_label_is_locked_EN_TR()
    {
        Assert.Equal(
            "Analyze this repository",
            localizer.GetRepositoryActionLabel(WebCulture.English));
        Assert.Equal(
            "Bu repository'yi analiz et",
            localizer.GetRepositoryActionLabel(WebCulture.Turkish));
    }

    [Fact]
    public void Repository_action_accessible_name_substitutes_the_literal_full_name()
    {
        Assert.Equal(
            "Analyze repository octocat/Hello-World",
            localizer.GetRepositoryActionAccessibleName("octocat/Hello-World", WebCulture.English));
        Assert.Equal(
            "octocat/Hello-World repository'sini analiz et",
            localizer.GetRepositoryActionAccessibleName("octocat/Hello-World", WebCulture.Turkish));
    }

    [Fact]
    public void Repository_action_accessible_name_rejects_a_blank_full_name()
    {
        Assert.Throws<ArgumentException>(() =>
            localizer.GetRepositoryActionAccessibleName(" ", WebCulture.English));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("fr")]
    [InlineData("de-DE")]
    public void P10I4_fixed_slot_and_status_copy_falls_back_to_English_for_unsupported_culture(
        string? culture)
    {
        Assert.Equal(
            "Contributed evidence to this view",
            localizer.GetRepositoryStatusLabel(RepositoryEligibilityStatus.Contributing, culture));
        Assert.Equal(
            "Observed across these public repositories",
            localizer.GetWhySupportingRepositoriesLeadIn(culture));
        Assert.Equal(
            "Selecting a repository runs a fresh Repository Proof analysis.",
            localizer.GetRepositoryFreshAnalysisLeadIn(culture));
        Assert.Equal(
            "Analyze this repository",
            localizer.GetRepositoryActionLabel(culture));
        Assert.Equal(
            "Analyze repository octocat",
            localizer.GetRepositoryActionAccessibleName("octocat", culture));
    }
}
