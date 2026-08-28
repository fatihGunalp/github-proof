using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Web.Localization;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class ProfileWebPresentationLocalizerTests
{
    private const string MeaningIdentity = "fatihGunalp";

    private readonly ProfileWebPresentationLocalizer localizer = new();

    // Session 141 (contract 03): the header fixes identity and no longer
    // carries an identity-context caption. What it keeps is the canonical
    // GitHub external action and its accessible name, both unchanged.
    [Fact]
    public void Profile_identity_header_copy_is_localized_and_deterministic()
    {
        Assert.Equal(
            "Open on GitHub",
            localizer.GetOpenProfileOnGitHubLabel(WebCulture.English));
        Assert.Equal(
            "GitHub'da aç",
            localizer.GetOpenProfileOnGitHubLabel(WebCulture.Turkish));
        Assert.Equal(
            "Open octocat on GitHub; opens in a new tab",
            localizer.GetOpenProfileOnGitHubAccessibleName("octocat", WebCulture.English));
        Assert.Equal(
            "octocat profilini GitHub'da aç; yeni sekmede açılır",
            localizer.GetOpenProfileOnGitHubAccessibleName("octocat", WebCulture.Turkish));

        Assert.Equal(
            localizer.GetOpenProfileOnGitHubLabel(WebCulture.English),
            localizer.GetOpenProfileOnGitHubLabel(WebCulture.English));
    }

    [Fact]
    public void Avatar_accessible_name_is_localized_deterministic_and_carries_the_username()
    {
        Assert.Equal(
            "octocat's GitHub profile photo",
            localizer.GetAvatarAccessibleName("octocat", WebCulture.English));
        Assert.Equal(
            "octocat GitHub profil fotoğrafı",
            localizer.GetAvatarAccessibleName("octocat", WebCulture.Turkish));

        Assert.Equal(
            localizer.GetAvatarAccessibleName("octocat", WebCulture.English),
            localizer.GetAvatarAccessibleName("octocat", WebCulture.English));

        Assert.Throws<ArgumentException>(
            () => localizer.GetAvatarAccessibleName(string.Empty, WebCulture.English));
    }

    [Fact]
    public void Starting_point_description_fallback_is_localized_deterministic_and_neutral()
    {
        Assert.Equal(
            "No short description is provided on GitHub.",
            localizer.GetStartingPointDescriptionFallback(WebCulture.English));
        Assert.Equal(
            "GitHub'da kısa bir açıklama bulunmuyor.",
            localizer.GetStartingPointDescriptionFallback(WebCulture.Turkish));

        Assert.Equal(
            localizer.GetStartingPointDescriptionFallback(WebCulture.Turkish),
            localizer.GetStartingPointDescriptionFallback(WebCulture.Turkish));
    }

    [Fact]
    public void Mockup_first_read_copy_is_localized_and_deterministic_without_creating_project_type_truth()
    {
        Assert.Equal(
            "What kinds of work are visible?",
            localizer.GetWorkTypeCardHeading(WebCulture.English));
        Assert.Equal(
            "Ne tür çalışmalar görünüyor?",
            localizer.GetWorkTypeCardHeading(WebCulture.Turkish));
        Assert.Equal(
            "Recurring technologies",
            localizer.GetRecurringTechnologiesLeadIn(WebCulture.English));
        Assert.Equal(
            "Tekrar eden teknolojiler",
            localizer.GetRecurringTechnologiesLeadIn(WebCulture.Turkish));
        Assert.Equal(
            "See which repositories each statement is based on.",
            localizer.GetWhyDisclosureDescription(WebCulture.English));
        Assert.Equal(
            "Buradaki ifadelerin hangi repository'lere dayandığını görebilirsiniz.",
            localizer.GetWhyDisclosureDescription(WebCulture.Turkish));
        Assert.Equal(
            "See the public repositories examined in this analysis.",
            localizer.GetRepositoryEvidenceDisclosureDescription(WebCulture.English));
        Assert.Equal(
            "Analizde incelenen public repository'leri burada görebilirsiniz.",
            localizer.GetRepositoryEvidenceDisclosureDescription(WebCulture.Turkish));
    }

    [Theory]
    [InlineData(ProfileReportSectionKey.TechnicalFocusAreas, "Technical direction", "Teknik yön")]
    [InlineData(ProfileReportSectionKey.EngineeringPracticePatterns, "Working style", "Çalışma biçimi")]
    [InlineData(ProfileReportSectionKey.RepresentativeRepositories, "Starting point", "Başlangıç noktası")]
    [InlineData(ProfileReportSectionKey.WhyThisConclusion, "What is this based on?", "Bunu neye dayanarak söylüyoruz?")]
    [InlineData(ProfileReportSectionKey.Limitations, "Limits of this analysis", "Bu analizin sınırları")]
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

    [Theory]
    [MemberData(nameof(MeaningCases))]
    public void Meaning_is_a_human_username_led_narrative_in_both_supported_cultures(
        string[] technicalPatterns,
        string[] practicePatterns)
    {
        ProfileMeaningPresentation english = localizer.GetMeaningPresentation(
            MeaningIdentity,
            MeaningSection(
                ProfileReportSectionKey.TechnicalFocusAreas,
                ProfilePatternFamily.TechnicalFocus,
                technicalPatterns),
            MeaningSection(
                ProfileReportSectionKey.EngineeringPracticePatterns,
                ProfilePatternFamily.EngineeringPractice,
                practicePatterns),
            WebCulture.English);
        ProfileMeaningPresentation turkish = localizer.GetMeaningPresentation(
            MeaningIdentity,
            MeaningSection(
                ProfileReportSectionKey.TechnicalFocusAreas,
                ProfilePatternFamily.TechnicalFocus,
                technicalPatterns),
            MeaningSection(
                ProfileReportSectionKey.EngineeringPracticePatterns,
                ProfilePatternFamily.EngineeringPractice,
                practicePatterns),
            WebCulture.Turkish);

        // Session 141 (contract 04): the accepted identity still leads Meaning
        // whenever a technical direction recurs. When none does, the approved
        // copy states the absence instead of attaching the username to it.
        if (technicalPatterns.Length > 0)
        {
            Assert.StartsWith(MeaningIdentity, english.TechnicalNarrative, StringComparison.Ordinal);
            Assert.StartsWith(MeaningIdentity, turkish.TechnicalNarrative, StringComparison.Ordinal);
        }

        Assert.Equal(
            "This summary is based only on traces visible in public GitHub work.",
            english.Provenance);
        Assert.Equal(
            "Bu anlatım yalnızca public GitHub çalışmalarında görülebilen izlere dayanıyor.",
            turkish.Provenance);

        foreach (string technicalPattern in technicalPatterns)
        {
            Assert.Contains(technicalPattern, english.TechnicalNarrative, StringComparison.Ordinal);
            Assert.Contains(technicalPattern, turkish.TechnicalNarrative, StringComparison.Ordinal);
        }

        if (practicePatterns.Length == 0)
        {
            if (technicalPatterns.Length > 0)
            {
                Assert.Contains("No clear working method recurs", english.PracticeNarrative, StringComparison.Ordinal);
                Assert.Contains("tekrar eden belirgin bir çalışma biçimi görünmüyor", turkish.PracticeNarrative, StringComparison.Ordinal);
            }
            else
            {
                // Variant D: the second paragraph must not repeat the absence
                // the first paragraph already stated.
                Assert.Contains("No additional recurring working method", english.PracticeNarrative, StringComparison.Ordinal);
                Assert.Contains("ek bir tekrar görünmüyor", turkish.PracticeNarrative, StringComparison.Ordinal);
                Assert.NotEqual(english.TechnicalNarrative, english.PracticeNarrative);
                Assert.NotEqual(turkish.TechnicalNarrative, turkish.PracticeNarrative);
            }
        }
        else
        {
            Assert.Contains("recur across multiple repositories", english.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("birden fazla repository'de tekrar görülüyor", turkish.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData(
        ProfilePatternPromotionPolicy.AutomatedTestingPatternKey,
        "Automated testing",
        "Otomatik test kullanımı")]
    [InlineData(
        ProfilePatternPromotionPolicy.ContinuousIntegrationPatternKey,
        "Automated integration checks",
        "Otomatik kontrol süreçleri")]
    [InlineData(
        ProfilePatternPromotionPolicy.DocumentationPatternKey,
        "Documentation",
        "Dokümantasyon")]
    [InlineData(
        ProfilePatternPromotionPolicy.ReleaseUsagePatternKey,
        "Published releases",
        "Sürüm yayınları")]
    [InlineData(
        ProfilePatternPromotionPolicy.IssueUsagePatternKey,
        "Issue tracking",
        "Issue ile takip")]
    [InlineData(
        ProfilePatternPromotionPolicy.PullRequestUsagePatternKey,
        "Pull request workflow",
        "Pull request ile değişiklik akışı")]
    public void Working_style_uses_concise_deterministic_EN_TR_labels_for_every_canonical_practice(
        string patternKey,
        string english,
        string turkish)
    {
        Assert.Equal(english, localizer.GetWorkingStyleLabel(patternKey, WebCulture.English));
        Assert.Equal(turkish, localizer.GetWorkingStyleLabel(patternKey, WebCulture.Turkish));
        Assert.Equal(english, localizer.GetWorkingStyleLabel(patternKey, WebCulture.English));
        Assert.Equal(turkish, localizer.GetWorkingStyleLabel(patternKey, WebCulture.Turkish));

        string labels = $"{english} {turkish}".ToLowerInvariant();

        foreach (string forbidden in new[]
                 {
                     "test ederek ilerliyor", "kodunu sürekli kontrol ediyor",
                     "profesyonelce", "disiplinli", "senior", "expert",
                     "skilled", "capable", "quality", "kalite"
                 })
        {
            Assert.DoesNotContain(forbidden, labels, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Working_style_rejects_an_unknown_practice_instead_of_deriving_a_label_from_display_text()
    {
        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetWorkingStyleLabel("invented-working-method", WebCulture.English));
    }

    public static IEnumerable<object[]> MeaningCases()
    {
        yield return
        [
            new[] { "C#" }, Array.Empty<string>()
        ];
        yield return
        [
            new[] { "C#", ".NET", "ASP.NET Core" }, Array.Empty<string>()
        ];
        yield return
        [
            Array.Empty<string>(), new[] { ProfilePatternPromotionPolicy.AutomatedTestingPatternKey }
        ];
        yield return
        [
            Array.Empty<string>(), new[]
            {
                ProfilePatternPromotionPolicy.DocumentationPatternKey,
                ProfilePatternPromotionPolicy.ContinuousIntegrationPatternKey
            }
        ];
        yield return
        [
            new[] { "C#", ".NET" }, new[] { ProfilePatternPromotionPolicy.DocumentationPatternKey }
        ];
        yield return
        [
            Array.Empty<string>(), Array.Empty<string>()
        ];
    }

    [Theory]
    [MemberData(nameof(LiveReviewMeaningCases))]
    public void Live_review_examples_generate_the_bounded_TR_Meaning(
        string identity,
        string[] technicalPatterns,
        string[] practicePatterns,
        string expectedTechnicalNarrative,
        string expectedPracticeNarrative)
    {
        ProfileMeaningPresentation meaning = localizer.GetMeaningPresentation(
            identity,
            MeaningSection(
                ProfileReportSectionKey.TechnicalFocusAreas,
                ProfilePatternFamily.TechnicalFocus,
                technicalPatterns),
            MeaningSection(
                ProfileReportSectionKey.EngineeringPracticePatterns,
                ProfilePatternFamily.EngineeringPractice,
                practicePatterns),
            WebCulture.Turkish);

        Assert.Equal(expectedTechnicalNarrative, meaning.TechnicalNarrative);
        Assert.Equal(expectedPracticeNarrative, meaning.PracticeNarrative);
        Assert.Equal(
            "Bu anlatım yalnızca public GitHub çalışmalarında görülebilen izlere dayanıyor.",
            meaning.Provenance);
    }

    public static IEnumerable<object[]> LiveReviewMeaningCases()
    {
        yield return
        [
            "fatihGunalp",
            new[] { "C#" },
            Array.Empty<string>(),
            "fatihGunalp GitHub çalışmalarında C# farklı repository'lerde tekrar ediyor.",
            "Repository'ler arasında tekrar eden belirgin bir çalışma biçimi görünmüyor."
        ];
        yield return
        [
            "yusuftalhaklc",
            new[] { "C#", "Go", "HTML", "Java", "Kotlin", "Python" },
            new[]
            {
                ProfilePatternPromotionPolicy.AutomatedTestingPatternKey,
                ProfilePatternPromotionPolicy.ContinuousIntegrationPatternKey,
                ProfilePatternPromotionPolicy.DocumentationPatternKey
            },
            "yusuftalhaklc GitHub çalışmalarında C#, Go, HTML, Java, Kotlin ve Python farklı repository'lerde tekrar ediyor.",
            "Otomatik test kullanımı, otomatik kontrol süreçleri ve dokümantasyon gibi çalışma biçimleri de birden fazla repository'de tekrar görülüyor."
        ];
        yield return
        [
            "irmakcosar",
            new[] { "C#", "HTML" },
            Array.Empty<string>(),
            "irmakcosar GitHub çalışmalarında C# ve HTML farklı repository'lerde tekrar ediyor.",
            "Repository'ler arasında tekrar eden belirgin bir çalışma biçimi görünmüyor."
        ];
    }

    [Fact]
    public void Meaning_preserves_all_canonical_item_labels_in_report_order_without_a_cap()
    {
        string[] practices =
        [
            ProfilePatternPromotionPolicy.PullRequestUsagePatternKey,
            ProfilePatternPromotionPolicy.AutomatedTestingPatternKey,
            ProfilePatternPromotionPolicy.ContinuousIntegrationPatternKey,
            ProfilePatternPromotionPolicy.DocumentationPatternKey,
            ProfilePatternPromotionPolicy.ReleaseUsagePatternKey,
            ProfilePatternPromotionPolicy.IssueUsagePatternKey
        ];

        ProfileMeaningPresentation meaning = localizer.GetMeaningPresentation(
            MeaningIdentity,
            MeaningSection(
                ProfileReportSectionKey.TechnicalFocusAreas,
                ProfilePatternFamily.TechnicalFocus,
                ["C#", ".NET", "ASP.NET Core", "F#"]),
            MeaningSection(
                ProfileReportSectionKey.EngineeringPracticePatterns,
                ProfilePatternFamily.EngineeringPractice,
                practices),
            WebCulture.English);

        Assert.Equal(
            "fatihGunalp's GitHub work shows C#, .NET, ASP.NET Core, and F# recurring across different repositories.",
            meaning.TechnicalNarrative);

        string[] labels =
        [
            "pull request workflow",
            "automated testing",
            "automated integration checks",
            "documentation",
            "published releases",
            "issue tracking"
        ];

        int previous = -1;
        foreach (string label in labels)
        {
            int current = meaning.PracticeNarrative.IndexOf(label, StringComparison.Ordinal);
            Assert.True(current > previous, $"Expected '{label}' in canonical practice order.");
            previous = current;
        }

        Assert.DoesNotContain("tests that check", meaning.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("in other words", meaning.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
        // Session 141 (contract 04): one sentence each, roughly half the
        // previous length. Each paragraph ends with its single full stop and
        // contains no sentence break before it.
        Assert.EndsWith(".", meaning.TechnicalNarrative, StringComparison.Ordinal);
        Assert.EndsWith(".", meaning.PracticeNarrative, StringComparison.Ordinal);
        Assert.DoesNotContain(". ", meaning.TechnicalNarrative, StringComparison.Ordinal);
        Assert.DoesNotContain(". ", meaning.PracticeNarrative, StringComparison.Ordinal);
        Assert.True(meaning.TechnicalNarrative.Length < 150);
        Assert.True(meaning.PracticeNarrative.Length < 250);
    }

    [Fact]
    public void Meaning_uses_only_the_two_typed_pattern_sections_and_is_byte_identical_for_the_same_input()
    {
        ProfileReportSection<PatternReportItem> technical = MeaningSection(
            ProfileReportSectionKey.TechnicalFocusAreas,
            ProfilePatternFamily.TechnicalFocus,
            ["C#"]);
        ProfileReportSection<PatternReportItem> practices = MeaningSection(
            ProfileReportSectionKey.EngineeringPracticePatterns,
            ProfilePatternFamily.EngineeringPractice,
            Array.Empty<string>());

        ProfileMeaningPresentation first = localizer.GetMeaningPresentation(
            MeaningIdentity,
            technical,
            practices,
            WebCulture.English);
        ProfileMeaningPresentation second = localizer.GetMeaningPresentation(
            MeaningIdentity,
            technical,
            practices,
            WebCulture.English);

        Assert.Equal(first.TechnicalNarrative, second.TechnicalNarrative);
        Assert.Equal(first.PracticeNarrative, second.PracticeNarrative);
        Assert.Equal(first.Provenance, second.Provenance);

        var method = typeof(ProfileWebPresentationLocalizer).GetMethod(
            nameof(ProfileWebPresentationLocalizer.GetMeaningPresentation));
        Assert.NotNull(method);
        Assert.Collection(
            method!.GetParameters(),
            parameter => Assert.Equal(typeof(string), parameter.ParameterType),
            parameter => Assert.Equal(typeof(ProfileReportSection<PatternReportItem>), parameter.ParameterType),
            parameter => Assert.Equal(typeof(ProfileReportSection<PatternReportItem>), parameter.ParameterType),
            parameter => Assert.Equal(typeof(string), parameter.ParameterType));
    }

    [Fact]
    public void Meaning_leads_with_the_accepted_username_and_describes_evidence_not_developer_judgment()
    {
        ProfileMeaningPresentation meaning = localizer.GetMeaningPresentation(
            "mrtkhrmn3",
            MeaningSection(
                ProfileReportSectionKey.TechnicalFocusAreas,
                ProfilePatternFamily.TechnicalFocus,
                ["C#", "Java", "TypeScript"]),
            MeaningSection(
                ProfileReportSectionKey.EngineeringPracticePatterns,
                ProfilePatternFamily.EngineeringPractice,
                [
                    ProfilePatternPromotionPolicy.AutomatedTestingPatternKey,
                    ProfilePatternPromotionPolicy.PullRequestUsagePatternKey
                ]),
            WebCulture.Turkish);

        Assert.Equal(
            "mrtkhrmn3 GitHub çalışmalarında C#, Java ve TypeScript farklı repository'lerde tekrar ediyor.",
            meaning.TechnicalNarrative);
        Assert.Contains("Otomatik test kullanımı", meaning.PracticeNarrative, StringComparison.Ordinal);
        Assert.Contains("pull request ile değişiklik akışı", meaning.PracticeNarrative, StringComparison.Ordinal);
        Assert.Contains("birden fazla repository'de tekrar görülüyor", meaning.PracticeNarrative, StringComparison.Ordinal);

        Assert.StartsWith("mrtkhrmn3", meaning.TechnicalNarrative, StringComparison.Ordinal);

        string rendered = string.Join(
            " ",
            [meaning.TechnicalNarrative, meaning.PracticeNarrative, meaning.Provenance])
            .ToLowerInvariant();
        string[] prohibited =
        [
            "senior", "junior", "expert", "skilled", "capability",
            "confidence", "score", "quality", "backend developer",
            "full-stack developer", "%"
        ];

        foreach (string term in prohibited)
        {
            Assert.DoesNotContain(term, rendered, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData(ProfilePatternPromotionPolicy.AutomatedTestingPatternKey, "Automated testing", "Otomatik test kullanımı")]
    [InlineData(ProfilePatternPromotionPolicy.PullRequestUsagePatternKey, "Pull request workflow", "Pull request ile değişiklik akışı")]
    [InlineData(ProfilePatternPromotionPolicy.ContinuousIntegrationPatternKey, "Automated integration checks", "Otomatik kontrol süreçleri")]
    [InlineData(ProfilePatternPromotionPolicy.DocumentationPatternKey, "Documentation", "Dokümantasyon")]
    [InlineData(ProfilePatternPromotionPolicy.ReleaseUsagePatternKey, "Published releases", "Sürüm yayınları")]
    [InlineData(ProfilePatternPromotionPolicy.IssueUsagePatternKey, "Issue tracking", "Issue ile takip")]
    public void Meaning_represents_each_canonical_practice_without_glossary_expansion(
        string practicePattern,
        string expectedEnglishLabel,
        string expectedTurkishLabel)
    {
        ProfileMeaningPresentation english = localizer.GetMeaningPresentation(
            MeaningIdentity,
            MeaningSection(
                ProfileReportSectionKey.TechnicalFocusAreas,
                ProfilePatternFamily.TechnicalFocus,
                Array.Empty<string>()),
            MeaningSection(
                ProfileReportSectionKey.EngineeringPracticePatterns,
                ProfilePatternFamily.EngineeringPractice,
                [practicePattern]),
            WebCulture.English);
        ProfileMeaningPresentation turkish = localizer.GetMeaningPresentation(
            MeaningIdentity,
            MeaningSection(
                ProfileReportSectionKey.TechnicalFocusAreas,
                ProfilePatternFamily.TechnicalFocus,
                Array.Empty<string>()),
            MeaningSection(
                ProfileReportSectionKey.EngineeringPracticePatterns,
                ProfilePatternFamily.EngineeringPractice,
                [practicePattern]),
            WebCulture.Turkish);

        // The canonical label reaches the sentence unchanged apart from the
        // pre-existing sentence-position lowercasing of its first letter, which
        // is grammar, not a different label.
        Assert.Contains(expectedEnglishLabel, english.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("across multiple repositories", english.PracticeNarrative, StringComparison.Ordinal);
        Assert.Contains(expectedTurkishLabel, turkish.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("birden fazla repository'de", turkish.PracticeNarrative, StringComparison.Ordinal);

        string combined = english.PracticeNarrative + turkish.PracticeNarrative;
        Assert.DoesNotContain("tests that check whether code behaves", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("kodun beklenen şekilde davranıp davranmadığını", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("in other words", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("yani projeyi açıklayan", combined, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Meaning_fails_loudly_for_a_noncanonical_section_or_pattern_family()
    {
        Assert.Throws<InvalidOperationException>(() => localizer.GetMeaningPresentation(
            MeaningIdentity,
            MeaningSection(
                ProfileReportSectionKey.RepresentativeRepositories,
                ProfilePatternFamily.TechnicalFocus,
                Array.Empty<string>()),
            MeaningSection(
                ProfileReportSectionKey.EngineeringPracticePatterns,
                ProfilePatternFamily.EngineeringPractice,
                Array.Empty<string>()),
            WebCulture.English));

        Assert.Throws<InvalidOperationException>(() => localizer.GetMeaningPresentation(
            MeaningIdentity,
            MeaningSection(
                ProfileReportSectionKey.TechnicalFocusAreas,
                ProfilePatternFamily.EngineeringPractice,
                [ProfilePatternPromotionPolicy.DocumentationPatternKey]),
            MeaningSection(
                ProfileReportSectionKey.EngineeringPracticePatterns,
                ProfilePatternFamily.EngineeringPractice,
                Array.Empty<string>()),
            WebCulture.English));
    }

    [Fact]
    public void Profile_report_projector_remains_locale_neutral_for_Meaning()
    {
        var project = typeof(ProfileReportProjector).GetMethod(
            nameof(ProfileReportProjector.Project));

        Assert.NotNull(project);
        Assert.Collection(
            project!.GetParameters(),
            parameter => Assert.Equal("DeveloperPublicEvidenceProfile", parameter.ParameterType.Name));
    }

    // --- Session 137 / p10 8.6.1b Starting Point pattern relationship ---

    [Fact]
    public void Starting_point_relationship_uses_the_literal_language_identity_in_both_cultures()
    {
        IReadOnlyList<RepresentativePatternSupport> supports =
            [Support(ProfilePatternFamily.TechnicalFocus, "C#")];

        Assert.Equal(
            "Starting point for C#",
            localizer.GetStartingPointPatternRelationship(supports, WebCulture.English));
        Assert.Equal(
            "C# için başlangıç noktası",
            localizer.GetStartingPointPatternRelationship(supports, WebCulture.Turkish));
    }

    [Fact]
    public void Starting_point_relationship_uses_the_concise_working_style_label_in_both_cultures()
    {
        IReadOnlyList<RepresentativePatternSupport> supports =
        [
            Support(
                ProfilePatternFamily.EngineeringPractice,
                ProfilePatternPromotionPolicy.AutomatedTestingPatternKey)
        ];

        // The concise §8.4.8 Working Style label, never the raw canonical key.
        Assert.Equal(
            "Starting point for Automated testing",
            localizer.GetStartingPointPatternRelationship(supports, WebCulture.English));
        Assert.Equal(
            "Otomatik test kullanımı için başlangıç noktası",
            localizer.GetStartingPointPatternRelationship(supports, WebCulture.Turkish));
    }

    [Fact]
    public void Starting_point_relationship_renders_every_support_in_canonical_order()
    {
        IReadOnlyList<RepresentativePatternSupport> supports =
        [
            Support(ProfilePatternFamily.TechnicalFocus, "C#"),
            Support(ProfilePatternFamily.TechnicalFocus, "TypeScript"),
            Support(
                ProfilePatternFamily.EngineeringPractice,
                ProfilePatternPromotionPolicy.ContinuousIntegrationPatternKey),
            Support(
                ProfilePatternFamily.EngineeringPractice,
                ProfilePatternPromotionPolicy.DocumentationPatternKey)
        ];

        // Every support appears, in the collection's existing order, joined by
        // the existing locale-aware rule. No cap, sort, filter or
        // first-pattern shortcut.
        Assert.Equal(
            "Starting point for "
            + "C#, TypeScript, Automated integration checks, and Documentation",
            localizer.GetStartingPointPatternRelationship(supports, WebCulture.English));
        Assert.Equal(
            "C#, TypeScript, Otomatik kontrol süreçleri ve Dokümantasyon"
            + " için başlangıç noktası",
            localizer.GetStartingPointPatternRelationship(supports, WebCulture.Turkish));
    }

    // Session 141 (contract 07) removed the internal "recurring result" phrase,
    // so the relationship no longer needs a singular/plural split. What it must
    // still never do is render a quantity.
    [Fact]
    public void Starting_point_relationship_never_renders_a_count()
    {
        IReadOnlyList<RepresentativePatternSupport> single =
            [Support(ProfilePatternFamily.TechnicalFocus, "Go")];
        IReadOnlyList<RepresentativePatternSupport> multiple =
        [
            Support(ProfilePatternFamily.TechnicalFocus, "Go"),
            Support(ProfilePatternFamily.TechnicalFocus, "Rust")
        ];

        foreach (string culture in new[] { WebCulture.English, WebCulture.Turkish })
        {
            string one = localizer.GetStartingPointPatternRelationship(single, culture);
            string many = localizer.GetStartingPointPatternRelationship(multiple, culture);

            Assert.NotEqual(one, many);

            // Grammatical agreement only: no digit, ratio, counter or
            // "2 patterns"-style quantity reaches this first-read surface.
            Assert.False(one.Any(char.IsDigit), one);
            Assert.False(many.Any(char.IsDigit), many);
        }
    }

    [Fact]
    public void Starting_point_relationship_fails_loudly_for_problem_area_and_unknown_practice()
    {
        // ProblemArea is Unsupported in V1 and has no active Web presentation
        // mapping: no fallback label, no silent omission.
        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetStartingPointPatternRelationship(
                [Support(ProfilePatternFamily.ProblemArea, "problem-area-key")],
                WebCulture.English));

        // An unknown EngineeringPractice identity is a contract violation, not
        // a display-text fallback.
        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetStartingPointPatternRelationship(
                [Support(ProfilePatternFamily.EngineeringPractice, "invented-capability")],
                WebCulture.English));

        // An empty support collection can only mean broken upstream truth.
        Assert.Throws<InvalidOperationException>(() =>
            localizer.GetStartingPointPatternRelationship([], WebCulture.English));

        Assert.Throws<ArgumentNullException>(() =>
            localizer.GetStartingPointPatternRelationship(null!, WebCulture.English));
    }

    [Fact]
    public void Starting_point_relationship_falls_back_to_English_for_an_unsupported_culture()
    {
        Assert.Equal(
            "Starting point for C#",
            localizer.GetStartingPointPatternRelationship(
                [Support(ProfilePatternFamily.TechnicalFocus, "C#")],
                "de-DE"));
    }

    [Fact]
    public void Starting_point_relationship_never_claims_ranking_quality_or_capability()
    {
        string[] forbidden =
        [
            "best", "top", "strongest", "recommended", "featured", "primary",
            "highest", "quality", "score", "rank", "senior", "junior",
            "skilled", "expert", "experienced", "capability", "confidence"
        ];

        IReadOnlyList<RepresentativePatternSupport> supports =
        [
            Support(ProfilePatternFamily.TechnicalFocus, "C#"),
            Support(
                ProfilePatternFamily.EngineeringPractice,
                ProfilePatternPromotionPolicy.ReleaseUsagePatternKey)
        ];

        foreach (string culture in new[] { WebCulture.English, WebCulture.Turkish })
        {
            string rendered = localizer
                .GetStartingPointPatternRelationship(supports, culture)
                .ToLowerInvariant();

            foreach (string word in forbidden)
            {
                Assert.DoesNotContain(word, rendered, StringComparison.Ordinal);
            }
        }
    }

    private static RepresentativePatternSupport Support(
        ProfilePatternFamily family,
        string patternKey)
    {
        RepeatedProfilePattern pattern = CreatePattern(family, patternKey);
        ProfileRepositoryReference repository = pattern.IndependentSupportingRepositories[0];

        return new RepresentativePatternSupport(
            pattern,
            repository,
            pattern.SupportingObservations[0].EvidenceReferences
                .Where(reference => reference.Repository == repository));
    }

    private static ProfileReportSection<PatternReportItem> MeaningSection(
        ProfileReportSectionKey section,
        ProfilePatternFamily family,
        IEnumerable<string> patternKeys) =>
        new(
            section,
            ProfileReportSectionAvailability.Supported,
            patternKeys.Select(key => new PatternReportItem(CreatePattern(family, key), null)));

    private static RepeatedProfilePattern CreatePattern(
        ProfilePatternFamily family,
        string patternKey)
    {
        string observationKey = $"meaning-{family}-{patternKey}";
        ProfileRepositoryReference alpha = CreateRepository("alpha");
        ProfileRepositoryReference beta = CreateRepository("beta");
        ObservedProfileEvidence observation = new(
            observationKey,
            ObservedProfileEvidenceKind.RepositorySignalEvidence,
            [
                new ProfileEvidenceReference(alpha, CreateAnalysisReference(alpha), observationKey, [observationKey]),
                new ProfileEvidenceReference(beta, CreateAnalysisReference(beta), observationKey, [observationKey])
            ],
            independentSupportingRepositoryCount: 2,
            eligibleRepositoryCount: 2,
            undeterminedEvidenceRepositoryCount: 0,
            new ObservationRuleVersion("meaning-observation-rules"));

        return new RepeatedProfilePattern(
            family,
            patternKey,
            [observation],
            [alpha, beta],
            new PatternRuleVersion("meaning-pattern-rules"));
    }

    private static ProfileRepositoryReference CreateRepository(string name) =>
        new("meaning", name, new Uri($"https://github.com/meaning/{name}"));

    private static RepositoryAnalysisReference CreateAnalysisReference(
        ProfileRepositoryReference repository) =>
        new(
            Guid.NewGuid(),
            new RepositoryIdentity(
                repository.Owner,
                repository.Name,
                repository.Url,
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            new AnalysisVersion("meaning-tests", "meaning-tests", 1));

    [Theory]
    [InlineData(
        ProfileReportSectionKey.TechnicalFocusAreas,
        "No clear technical direction recurs across the public repositories.",
        "Public repository'ler arasında tekrar eden belirgin bir teknik yön görünmüyor.")]
    [InlineData(
        ProfileReportSectionKey.EngineeringPracticePatterns,
        "No clear working method recurs across the public repositories.",
        "Farklı çalışmalarda tekrar eden belirgin bir yöntem görünmüyor.")]
    [InlineData(
        ProfileReportSectionKey.RepresentativeRepositories,
        "No starting point is visible right now.",
        "Şu anda bir başlangıç noktası görünmüyor.")]
    [InlineData(
        ProfileReportSectionKey.Limitations,
        "No additional limitations are visible for this analysis.",
        "Bu analiz için ek bir sınırlama görünmüyor.")]
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
        "There is no recurring trace to explain here.",
        "Burada açıklanacak tekrar eden bir iz yok.")]
    [InlineData(
        ProfileReportSectionKey.RepositoryDrillDown,
        "No public repository is available to inspect in this analysis.",
        "Bu analizde incelenebilecek bir public repository görünmüyor.")]
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
        "Contributed evidence to the profile analysis",
        "Profil analizine kanıt sağladı")]
    [InlineData(
        RepositoryEligibilityStatus.Restricted,
        "Contributed evidence to the profile analysis with some restrictions",
        "Profil analizine bazı kısıtlamalarla kanıt sağladı")]
    [InlineData(
        RepositoryEligibilityStatus.NonContributing,
        "Did not contribute evidence to the profile analysis",
        "Profil analizine kanıt sağlamadı")]
    [InlineData(
        RepositoryEligibilityStatus.Undetermined,
        "It could not be determined whether this repository contributed evidence to the profile analysis.",
        "Profil analizine kanıt sağlayıp sağlamadığı belirlenemedi")]
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
            "Repositories where it appears",
            localizer.GetWhySupportingRepositoriesLeadIn(WebCulture.English));
        Assert.Equal(
            "Görüldüğü repository'ler",
            localizer.GetWhySupportingRepositoriesLeadIn(WebCulture.Turkish));
    }

    [Theory]
    [InlineData(
        ProfilePatternFamily.TechnicalFocus,
        "C#",
        "Why is C# here?",
        "C# neden burada?",
        "Because C# appears across multiple repositories.",
        "Çünkü C# birden fazla repository'de tekrar görülüyor.")]
    [InlineData(
        ProfilePatternFamily.EngineeringPractice,
        ProfilePatternPromotionPolicy.AutomatedTestingPatternKey,
        "Why is Automated testing here?",
        "Otomatik test kullanımı neden burada?",
        "Because Automated testing appears across multiple repositories.",
        "Çünkü Otomatik test kullanımı birden fazla repository'de tekrar görülüyor.")]
    public void Why_translates_canonical_trace_into_a_visitor_question_and_bounded_reason(
        ProfilePatternFamily family,
        string patternKey,
        string englishQuestion,
        string turkishQuestion,
        string englishReason,
        string turkishReason)
    {
        Assert.Equal(
            englishQuestion,
            localizer.GetWhyEntryQuestion(family, patternKey, WebCulture.English));
        Assert.Equal(
            turkishQuestion,
            localizer.GetWhyEntryQuestion(family, patternKey, WebCulture.Turkish));
        Assert.Contains(
            englishReason,
            localizer.GetWhyExplanation(family, patternKey, WebCulture.English),
            StringComparison.Ordinal);
        Assert.Contains(
            turkishReason,
            localizer.GetWhyExplanation(family, patternKey, WebCulture.Turkish),
            StringComparison.Ordinal);

        string rendered = string.Join(
            " ",
            localizer.GetWhyEntryQuestion(family, patternKey, WebCulture.English),
            localizer.GetWhyEntryQuestion(family, patternKey, WebCulture.Turkish),
            localizer.GetWhyExplanation(family, patternKey, WebCulture.English),
            localizer.GetWhyExplanation(family, patternKey, WebCulture.Turkish));

        foreach (string internalTerm in new[]
                 {
                     "signal", "promoted pattern", "evidence key", "supported",
                     "pattern identity", "projection"
                 })
        {
            Assert.DoesNotContain(internalTerm, rendered, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Repository_fresh_analysis_lead_in_is_locked_EN_TR()
    {
        Assert.Equal(
            "Selecting a repository starts a new Repository Proof analysis.",
            localizer.GetRepositoryFreshAnalysisLeadIn(WebCulture.English));
        Assert.Equal(
            "Bir repository seçtiğinizde yeni bir Repository Proof analizi başlar.",
            localizer.GetRepositoryFreshAnalysisLeadIn(WebCulture.Turkish));
    }

    [Fact]
    public void Repository_action_visible_label_is_locked_EN_TR()
    {
        Assert.Equal(
            "View Repository Proof",
            localizer.GetRepositoryActionLabel(WebCulture.English));
        Assert.Equal(
            "Repository Proof'u gör",
            localizer.GetRepositoryActionLabel(WebCulture.Turkish));
    }

    [Fact]
    public void Repository_action_accessible_name_substitutes_the_literal_full_name()
    {
        Assert.Equal(
            "View Repository Proof for octocat/Hello-World",
            localizer.GetRepositoryActionAccessibleName("octocat/Hello-World", WebCulture.English));
        Assert.Equal(
            "octocat/Hello-World için Repository Proof'u gör",
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
            "Contributed evidence to the profile analysis",
            localizer.GetRepositoryStatusLabel(RepositoryEligibilityStatus.Contributing, culture));
        Assert.Equal(
            "Repositories where it appears",
            localizer.GetWhySupportingRepositoriesLeadIn(culture));
        Assert.Equal(
            "Selecting a repository starts a new Repository Proof analysis.",
            localizer.GetRepositoryFreshAnalysisLeadIn(culture));
        Assert.Equal(
            "View Repository Proof",
            localizer.GetRepositoryActionLabel(culture));
        Assert.Equal(
            "View Repository Proof for octocat",
            localizer.GetRepositoryActionAccessibleName("octocat", culture));
    }
}
