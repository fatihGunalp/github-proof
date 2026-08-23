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

    [Fact]
    public void Profile_identity_header_copy_is_localized_and_deterministic()
    {
        Assert.Equal(
            "View of public GitHub work",
            localizer.GetProfileIdentityContext(WebCulture.English));
        Assert.Equal(
            "Herkese açık GitHub çalışmalarının görünümü",
            localizer.GetProfileIdentityContext(WebCulture.Turkish));
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
            localizer.GetProfileIdentityContext(WebCulture.English),
            localizer.GetProfileIdentityContext(WebCulture.English));
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
            "Each observation is explained with the repositories that support it.",
            localizer.GetWhyDisclosureDescription(WebCulture.English));
        Assert.Equal(
            "Gözlemler hangi repository'lere dayandığıyla birlikte açıklanır.",
            localizer.GetWhyDisclosureDescription(WebCulture.Turkish));
        Assert.Equal(
            "The public repositories examined for this view appear here.",
            localizer.GetRepositoryEvidenceDisclosureDescription(WebCulture.English));
        Assert.Equal(
            "İncelenen herkese açık repository'ler burada görülür.",
            localizer.GetRepositoryEvidenceDisclosureDescription(WebCulture.Turkish));
    }

    [Theory]
    [InlineData(ProfileReportSectionKey.TechnicalFocusAreas, "Visible technical direction", "Görünen teknik yön")]
    [InlineData(ProfileReportSectionKey.EngineeringPracticePatterns, "Working style", "Çalışma biçimi")]
    [InlineData(ProfileReportSectionKey.RepresentativeRepositories, "Starting point", "Başlangıç noktası")]
    [InlineData(ProfileReportSectionKey.WhyThisConclusion, "What is this based on?", "Bunu neye dayanarak söylüyoruz?")]
    [InlineData(ProfileReportSectionKey.Limitations, "Limits of this view", "Bu görünümün sınırları")]
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

        Assert.StartsWith(MeaningIdentity, english.TechnicalNarrative, StringComparison.Ordinal);
        Assert.StartsWith(MeaningIdentity, turkish.TechnicalNarrative, StringComparison.Ordinal);
        Assert.Equal(
            "This summary is based only on traces visible in public GitHub work under the current analysis rules.",
            english.Provenance);
        Assert.Equal(
            "Bu anlatım yalnızca herkese açık GitHub çalışmalarında görülebilen izlere dayanıyor.",
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
                Assert.Contains("more limited visible traces", english.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("tekrar eden belirgin bir yöntem görünmüyor", turkish.PracticeNarrative, StringComparison.Ordinal);
            }
            else
            {
                Assert.Contains("limited traces", english.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("sınırlı izi", turkish.PracticeNarrative, StringComparison.Ordinal);
            }
        }
        else
        {
            Assert.Contains("appears across multiple repositories", english.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("birden fazla repository'de görülüyor", turkish.PracticeNarrative, StringComparison.OrdinalIgnoreCase);
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
            "Bu anlatım yalnızca herkese açık GitHub çalışmalarında görülebilen izlere dayanıyor.",
            meaning.Provenance);
    }

    public static IEnumerable<object[]> LiveReviewMeaningCases()
    {
        yield return
        [
            "fatihGunalp",
            new[] { "C#" },
            Array.Empty<string>(),
            "fatihGunalp GitHub çalışmalarında C# farklı repository'lerde tekrar karşımıza çıkıyor. Bu nedenle C# profil boyunca görünür olan tekrar eden teknik çalışma alanı.",
            "Çalışma biçiminde ise repository'ler arasında aynı şekilde tekrar eden belirgin bir yöntem görünmüyor. Yani bu GitHub teknik yön konusunda daha fazla şey gösterirken, çalışmaların nasıl yürütüldüğü konusunda daha sınırlı iz sunuyor."
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
            "yusuftalhaklc GitHub çalışmalarında C#, Go, HTML, Java, Kotlin ve Python farklı repository'lerde tekrar karşımıza çıkıyor. Bu profil, tek bir teknolojiyle sınırlı olmayan farklı teknik çalışmalar gösteriyor.",
            "Çalışma biçiminde de bazı ortak yöntemler tekrar ediyor. Otomatik test kullanımı, otomatik kontrol süreçleri ve dokümantasyon birden fazla repository'de görülüyor. Yani bu GitHub yalnızca hangi teknolojilerle çalışıldığını değil; çalışmaların nasıl yürütüldüğüne ilişkin yöntemlerin de farklı çalışmalarda tekrarlandığını gösteriyor."
        ];
        yield return
        [
            "irmakcosar",
            new[] { "C#", "HTML" },
            Array.Empty<string>(),
            "irmakcosar GitHub çalışmalarında C# ve HTML farklı repository'lerde tekrar karşımıza çıkıyor. Bu profil, tek bir teknolojiyle sınırlı olmayan farklı teknik çalışmalar gösteriyor.",
            "Çalışma biçiminde ise repository'ler arasında aynı şekilde tekrar eden belirgin bir yöntem görünmüyor. Yani bu GitHub teknik yön konusunda daha fazla şey gösterirken, çalışmaların nasıl yürütüldüğü konusunda daha sınırlı iz sunuyor."
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
            "fatihGunalp's GitHub work shows C#, .NET, ASP.NET Core, and F# recurring across different repositories. The profile shows technical work spanning more than one technology.",
            meaning.TechnicalNarrative);

        string[] labels =
        [
            "Pull request workflow",
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
        Assert.Equal(2, meaning.TechnicalNarrative.Split(". ", StringSplitOptions.None).Length);
        Assert.Equal(3, meaning.PracticeNarrative.Split(". ", StringSplitOptions.None).Length);
        Assert.True(meaning.TechnicalNarrative.Length < 300);
        Assert.True(meaning.PracticeNarrative.Length < 700);
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
            "mrtkhrmn3 GitHub çalışmalarında C#, Java ve TypeScript farklı repository'lerde tekrar karşımıza çıkıyor. Bu profil, tek bir teknolojiyle sınırlı olmayan farklı teknik çalışmalar gösteriyor.",
            meaning.TechnicalNarrative);
        Assert.Contains("Otomatik test kullanımı", meaning.PracticeNarrative, StringComparison.Ordinal);
        Assert.Contains("pull request ile değişiklik akışı", meaning.PracticeNarrative, StringComparison.Ordinal);
        Assert.Contains("yöntemlerin de farklı çalışmalarda tekrarlandığını", meaning.PracticeNarrative, StringComparison.Ordinal);

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

        Assert.Contains(expectedEnglishLabel, english.PracticeNarrative, StringComparison.Ordinal);
        Assert.Contains("across multiple repositories", english.PracticeNarrative, StringComparison.Ordinal);
        Assert.Contains(expectedTurkishLabel, turkish.PracticeNarrative, StringComparison.Ordinal);
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
        "The visible public evidence did not establish a repeated technical direction under the current analysis rules.",
        "Görünen herkese açık kanıtlar, mevcut analiz kuralları altında tekrar eden bir teknik yön ortaya koymadı.")]
    [InlineData(
        ProfileReportSectionKey.EngineeringPracticePatterns,
        "No recurring working method is visible across the current public work.",
        "Farklı çalışmalarda tekrar eden belirgin bir yöntem görünmüyor.")]
    [InlineData(
        ProfileReportSectionKey.RepresentativeRepositories,
        "No starting-point repository was selected from the current visible public evidence.",
        "Mevcut görünen herkese açık kanıtlardan bir başlangıç noktası repository'si seçilmedi.")]
    [InlineData(
        ProfileReportSectionKey.Limitations,
        "No limits were recorded for this profile view.",
        "Bu profil görünümü için bir sınır kaydedilmedi.")]
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
        "The public GitHub work did not provide a recurring conclusion to explain.",
        "Herkese açık GitHub çalışmaları, açıklanacak tekrar eden bir sonuç ortaya koymadı.")]
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
            "Supporting repositories",
            localizer.GetWhySupportingRepositoriesLeadIn(WebCulture.English));
        Assert.Equal(
            "Destekleyen repository'ler",
            localizer.GetWhySupportingRepositoriesLeadIn(WebCulture.Turkish));
    }

    [Theory]
    [InlineData(
        ProfilePatternFamily.TechnicalFocus,
        "C#",
        "Why is C# here?",
        "C# neden burada?",
        "recurring technical directions",
        "tekrar eden teknik yönlerinden biri")]
    [InlineData(
        ProfilePatternFamily.EngineeringPractice,
        ProfilePatternPromotionPolicy.AutomatedTestingPatternKey,
        "Why is Automated testing here?",
        "Otomatik test kullanımı neden burada?",
        "recurring ways of working",
        "çalışma biçiminde tekrar eden yöntemlerden biri")]
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
            "Supporting repositories",
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
