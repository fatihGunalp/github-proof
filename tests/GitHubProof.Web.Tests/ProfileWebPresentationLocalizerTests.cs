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
}
