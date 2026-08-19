using System.Globalization;
using System.Reflection;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.Report.ProfileReportTestSupport;

namespace GitHubProof.Application.Tests.Profile.Report;

public sealed class ProfileReportProjectorTests
{
    private static readonly ProfileReportSectionKey[] CanonicalOrder =
    [
        ProfileReportSectionKey.PublicProfileSummary,
        ProfileReportSectionKey.TechnicalFocusAreas,
        ProfileReportSectionKey.EngineeringPracticePatterns,
        ProfileReportSectionKey.ProblemAreaPatterns,
        ProfileReportSectionKey.RepresentativeRepositories,
        ProfileReportSectionKey.WhyThisConclusion,
        ProfileReportSectionKey.Limitations,
        ProfileReportSectionKey.RepositoryDrillDown,
        ProfileReportSectionKey.RoleEvidenceAlignment
    ];

    // ------------------------------------------------------- section set/order

    [Fact]
    public void Project_ProducesExactlyNineSections()
    {
        ProfileReport report = Project(Build());

        Assert.Equal(9, report.Sections.Count);
    }

    [Fact]
    public void Project_SectionsAreInCanonicalOrder()
    {
        ProfileReport report = Project(Build());

        Assert.Equal(CanonicalOrder, report.Sections.Select(section => section.SectionKey));
    }

    [Fact]
    public void Project_SectionOrderUnchangedByLocale()
    {
        DeveloperPublicEvidenceProfile profile = Build();

        ProfileReportSectionKey[] enOrder = WithCulture(
            "en-US",
            () => Project(profile).Sections.Select(section => section.SectionKey).ToArray());
        ProfileReportSectionKey[] trOrder = WithCulture(
            "tr-TR",
            () => Project(profile).Sections.Select(section => section.SectionKey).ToArray());

        Assert.Equal(enOrder, trOrder);
        Assert.Equal(CanonicalOrder, enOrder);
    }

    // Broader than section ordering alone: proves culture cannot alter any
    // projected semantic content — section keys/availability, pattern item
    // identity, representative-repository references/order,
    // limitation/warning identities/order, drill-down membership/order, or
    // WhyThisConclusion pattern/evidence identity — for a non-trivial profile.
    [Fact]
    public void Project_UnderDifferentCultures_ProducesSemanticallyIdenticalReport()
    {
        RepeatedProfilePattern technicalFocus = CreatePattern(
            ProfilePatternFamily.TechnicalFocus,
            "csharp",
            ["alpha", "beta"],
            ObservedProfileEvidenceKind.RepositoryLanguageEvidence);
        RepeatedProfilePattern engineeringPractice = CreatePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            ["alpha", "beta"]);
        ProfilePatternSet patternSet = CreatePatternSet([technicalFocus, engineeringPractice]);

        DeveloperPublicEvidenceProfile profile = Build(
            status: ProfileAnalysisStatus.Partial,
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            patternSet: patternSet,
            limitations:
            [
                new ProfileLimitation(
                    ProfileLimitationCode.PartialEvidenceSurface,
                    ProfileConditionScope.Profile),
                new ProfileLimitation(
                    ProfileLimitationCode.LimitedIndependentEvidenceSurface,
                    ProfileConditionScope.Profile)
            ],
            warnings:
            [
                new ProfileWarning(
                    ProfileWarningCode.RepositoryEligibilityUndetermined,
                    ProfileConditionScope.Repository,
                    [CreateReference("alpha")])
            ]);

        ProfileReport en = WithCulture("en-US", () => Project(profile));
        ProfileReport tr = WithCulture("tr-TR", () => Project(profile));

        Assert.Equal(en.Sections, tr.Sections);

        Assert.Equal(
            en.TechnicalFocusAreas.Items.Select(item => (item.Pattern.Family, item.Pattern.PatternKey)),
            tr.TechnicalFocusAreas.Items.Select(item => (item.Pattern.Family, item.Pattern.PatternKey)));
        Assert.Equal(
            en.EngineeringPracticePatterns.Items.Select(item => (item.Pattern.Family, item.Pattern.PatternKey)),
            tr.EngineeringPracticePatterns.Items.Select(item => (item.Pattern.Family, item.Pattern.PatternKey)));

        Assert.Equal(en.RepresentativeRepositories.Items, tr.RepresentativeRepositories.Items);

        Assert.Equal(
            en.Limitations.Items.Select(limitation => (limitation.Code, limitation.Scope)),
            tr.Limitations.Items.Select(limitation => (limitation.Code, limitation.Scope)));

        // ProfileWarning has value equality (code + scope + affected
        // identities), so this compares warning identity, not container
        // instance.
        Assert.Equal(en.Warnings, tr.Warnings);

        Assert.Equal(
            en.RepositoryDrillDown.Items.Select(item => item.Repository),
            tr.RepositoryDrillDown.Items.Select(item => item.Repository));

        Assert.Equal(
            en.WhyThisConclusion.Items.Select(item => (item.Pattern.Family, item.Pattern.PatternKey)),
            tr.WhyThisConclusion.Items.Select(item => (item.Pattern.Family, item.Pattern.PatternKey)));

        for (int index = 0; index < en.WhyThisConclusion.Items.Count; index++)
        {
            // SupportingEvidence is rebuilt into a fresh list per projection,
            // so compare element identity rather than container identity.
            Assert.Equal(
                en.WhyThisConclusion.Items[index].SupportingEvidence,
                tr.WhyThisConclusion.Items[index].SupportingEvidence);
        }
    }

    [Fact]
    public void AllNineSections_RemainInStructuredReport_RegardlessOfAvailability()
    {
        ProfileReport report = Project(Build());

        Assert.NotNull(report.ProblemAreaPatterns);
        Assert.NotNull(report.RoleEvidenceAlignment);
        Assert.Equal(
            ProfileReportSectionAvailability.Unsupported,
            report.ProblemAreaPatterns.Availability);
        Assert.Equal(
            ProfileReportSectionAvailability.Deferred,
            report.RoleEvidenceAlignment.Availability);
    }

    // -------------------------------------------------------------- patterns

    [Fact]
    public void TechnicalFocusAreas_SupportedAndPopulated_WhenPatternExists()
    {
        RepeatedProfilePattern pattern = CreatePattern(
            ProfilePatternFamily.TechnicalFocus,
            "csharp",
            ["alpha", "beta"],
            ObservedProfileEvidenceKind.RepositoryLanguageEvidence);
        ProfilePatternSet patternSet = CreatePatternSet([pattern]);

        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            patternSet: patternSet);

        ProfileReportSection<PatternReportItem> section = Project(profile).TechnicalFocusAreas;

        Assert.Equal(ProfileReportSectionAvailability.Supported, section.Availability);
        Assert.Single(section.Items);
        Assert.Same(pattern, section.Items[0].Pattern);
    }

    [Fact]
    public void TechnicalFocusAreas_SupportedWhenEmpty()
    {
        ProfileReportSection<PatternReportItem> section = Project(Build()).TechnicalFocusAreas;

        Assert.Equal(ProfileReportSectionAvailability.Supported, section.Availability);
        Assert.Empty(section.Items);
    }

    [Fact]
    public void EngineeringPracticePatterns_SupportedAndPopulated_WhenPatternExists()
    {
        RepeatedProfilePattern pattern = CreatePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            ["alpha", "beta"]);
        ProfilePatternSet patternSet = CreatePatternSet([pattern]);

        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            patternSet: patternSet);

        ProfileReportSection<PatternReportItem> section =
            Project(profile).EngineeringPracticePatterns;

        Assert.Equal(ProfileReportSectionAvailability.Supported, section.Availability);
        Assert.Single(section.Items);
        Assert.Same(pattern, section.Items[0].Pattern);
    }

    [Fact]
    public void EngineeringPracticePatterns_SupportedWhenEmpty()
    {
        ProfileReportSection<PatternReportItem> section =
            Project(Build()).EngineeringPracticePatterns;

        Assert.Equal(ProfileReportSectionAvailability.Supported, section.Availability);
        Assert.Empty(section.Items);
    }

    [Fact]
    public void ProblemAreaPatterns_IsExactlyUnsupportedAndEmpty_EvenWhenAProblemAreaPatternExistsUpstream()
    {
        // A ProblemArea-family pattern in the upstream pattern set (however it
        // got there) must never surface here: V1 has no ProblemArea taxonomy
        // or promotion rule, so the projector must not read this family at all.
        RepeatedProfilePattern problemAreaPattern = CreatePattern(
            ProfilePatternFamily.ProblemArea,
            "cli-tooling",
            ["alpha", "beta"]);
        ProfilePatternSet patternSet = CreatePatternSet([problemAreaPattern]);

        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            patternSet: patternSet);

        ProfileReportSection<PatternReportItem> section = Project(profile).ProblemAreaPatterns;

        Assert.Equal(ProfileReportSectionAvailability.Unsupported, section.Availability);
        Assert.Empty(section.Items);
    }

    // Regression: WhyThisConclusion must never surface a ProblemArea pattern
    // — ProblemArea is Unsupported in V1, so its patterns are not a V1
    // supported conclusion family, even when one exists upstream.
    [Fact]
    public void WhyThisConclusion_ExcludesUnsupportedProblemAreaPatterns()
    {
        RepeatedProfilePattern problemAreaPattern = CreatePattern(
            ProfilePatternFamily.ProblemArea,
            "cli-tooling",
            ["alpha", "beta"]);
        ProfilePatternSet patternSet = CreatePatternSet([problemAreaPattern]);

        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            patternSet: patternSet);

        ProfileReport report = Project(profile);

        Assert.Equal(
            ProfileReportSectionAvailability.Unsupported,
            report.ProblemAreaPatterns.Availability);
        Assert.Empty(report.ProblemAreaPatterns.Items);
        Assert.Empty(report.WhyThisConclusion.Items);
        Assert.Equal(
            0,
            report.PublicProfileSummary.Items
                .Single(fact => fact.ReferencedSection
                    == ProfileReportSectionKey.WhyThisConclusion)
                .ItemCount);
    }

    [Fact]
    public void RoleEvidenceAlignment_IsExactlyDeferredAndEmpty()
    {
        ProfileReportSection<RoleEvidenceAlignment> section =
            Project(Build()).RoleEvidenceAlignment;

        Assert.Equal(ProfileReportSectionAvailability.Deferred, section.Availability);
        Assert.Empty(section.Items);
    }

    [Fact]
    public void UnsupportedAndDeferred_AreDistinctAvailabilityValues()
    {
        ProfileReport report = Project(Build());

        Assert.NotEqual(
            report.ProblemAreaPatterns.Availability,
            report.RoleEvidenceAlignment.Availability);
        Assert.NotEqual(
            ProfileReportSectionAvailability.Unsupported,
            ProfileReportSectionAvailability.Deferred);
    }

    // -------------------------------------------------- representative repos

    [Fact]
    public void RepresentativeRepositories_OrderPreservedFromRoot_AndNotReselected()
    {
        RepeatedProfilePattern first = CreatePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            ["alpha", "beta"]);
        RepeatedProfilePattern second = CreatePattern(
            ProfilePatternFamily.EngineeringPractice,
            "continuous-integration",
            ["gamma", "delta"]);
        ProfilePatternSet patternSet = CreatePatternSet([first, second]);

        DeveloperPublicEvidenceProfile profile = Build(
            eligibility:
            [
                Eligibility("alpha"),
                Eligibility("beta"),
                Eligibility("gamma"),
                Eligibility("delta")
            ],
            patternSet: patternSet);

        ProfileReport report = Project(profile);

        Assert.NotEmpty(profile.RepresentativeRepositories);
        Assert.Equal(
            profile.RepresentativeRepositories,
            report.RepresentativeRepositories.Items);

        // Not reselected/reranked: the report carries the exact same objects.
        for (int index = 0; index < profile.RepresentativeRepositories.Count; index++)
        {
            Assert.Same(
                profile.RepresentativeRepositories[index],
                report.RepresentativeRepositories.Items[index]);
        }
    }

    // ------------------------------------------------------- pattern ordering

    [Fact]
    public void RepeatedPatternOrder_PreservedFromRoot()
    {
        RepeatedProfilePattern zulu = CreatePattern(
            ProfilePatternFamily.TechnicalFocus,
            "zulu-language",
            ["alpha", "beta"],
            ObservedProfileEvidenceKind.RepositoryLanguageEvidence);
        RepeatedProfilePattern alpha = CreatePattern(
            ProfilePatternFamily.TechnicalFocus,
            "alpha-language",
            ["alpha", "beta"],
            ObservedProfileEvidenceKind.RepositoryLanguageEvidence);

        // Constructed out of alphabetical order deliberately.
        ProfilePatternSet patternSet = CreatePatternSet([zulu, alpha]);

        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            patternSet: patternSet);

        ProfileReport report = Project(profile);

        Assert.Equal(
            profile.RepeatedProfilePatterns
                .Where(pattern => pattern.Family == ProfilePatternFamily.TechnicalFocus)
                .Select(pattern => pattern.PatternKey),
            report.TechnicalFocusAreas.Items.Select(item => item.Pattern.PatternKey));
        Assert.Equal(
            ["alpha-language", "zulu-language"],
            report.TechnicalFocusAreas.Items.Select(item => item.Pattern.PatternKey));
    }

    // ------------------------------------------------------------ limitations

    [Fact]
    public void LimitationOrder_PreservedFromRoot()
    {
        DeveloperPublicEvidenceProfile profile = Build(
            limitations:
            [
                new ProfileLimitation(
                    ProfileLimitationCode.PartialEvidenceSurface,
                    ProfileConditionScope.Profile),
                new ProfileLimitation(
                    ProfileLimitationCode.LimitedIndependentEvidenceSurface,
                    ProfileConditionScope.Profile)
            ]);

        ProfileReport report = Project(profile);

        Assert.Equal(profile.Limitations, report.Limitations.Items);
        Assert.Equal(
            [
                ProfileLimitationCode.LimitedIndependentEvidenceSurface,
                ProfileLimitationCode.PartialEvidenceSurface
            ],
            report.Limitations.Items.Select(limitation => limitation.Code));
    }

    // ---------------------------------------------------------------warnings

    [Fact]
    public void Warnings_RemainStructuredTruth_WithoutBecomingATenthSection()
    {
        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha")],
            warnings:
            [
                new ProfileWarning(
                    ProfileWarningCode.RepositoryEligibilityUndetermined,
                    ProfileConditionScope.Repository,
                    [CreateReference("alpha")])
            ]);

        ProfileReport report = Project(profile);

        Assert.Equal(9, report.Sections.Count);
        Assert.DoesNotContain(
            report.Sections,
            section => section.SectionKey is not (ProfileReportSectionKey.PublicProfileSummary
                or ProfileReportSectionKey.TechnicalFocusAreas
                or ProfileReportSectionKey.EngineeringPracticePatterns
                or ProfileReportSectionKey.ProblemAreaPatterns
                or ProfileReportSectionKey.RepresentativeRepositories
                or ProfileReportSectionKey.WhyThisConclusion
                or ProfileReportSectionKey.Limitations
                or ProfileReportSectionKey.RepositoryDrillDown
                or ProfileReportSectionKey.RoleEvidenceAlignment));
        Assert.Equal(profile.Warnings, report.Warnings);
    }

    // ------------------------------------------------------------ drill-down

    [Fact]
    public void RepositoryDrillDown_UsesReferencesNotEmbeddedAnalysis()
    {
        RepositoryEligibilityResult eligibility = Eligibility("alpha");
        DeveloperPublicEvidenceProfile profile = Build(eligibility: [eligibility]);

        ProfileReport report = Project(profile);
        RepositoryDrillDownReportItem item = Assert.Single(report.RepositoryDrillDown.Items);

        Assert.Equal(eligibility.Reference, item.Repository);
        Assert.Equal(eligibility.Status, item.EligibilityStatus);
        Assert.Equal(eligibility.AnalysisReference, item.AnalysisReference);

        // No property of the drill-down item type embeds a full RepositoryAnalysis.
        Assert.DoesNotContain(
            typeof(RepositoryDrillDownReportItem).GetProperties(),
            property => property.PropertyType.Name.Contains(
                "RepositoryAnalysis",
                StringComparison.Ordinal)
                && !property.PropertyType.Name.Contains(
                    "Reference",
                    StringComparison.Ordinal));
    }

    // --------------------------------------------------------- why-conclusion

    [Fact]
    public void WhyThisConclusion_TracesPatternToRepositoryEvidence()
    {
        RepeatedProfilePattern pattern = CreatePattern(
            ProfilePatternFamily.EngineeringPractice,
            "automated-testing",
            ["alpha", "beta"]);
        ProfilePatternSet patternSet = CreatePatternSet([pattern]);

        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            patternSet: patternSet);

        ConclusionEvidenceReportItem item =
            Assert.Single(Project(profile).WhyThisConclusion.Items);

        Assert.Same(pattern, item.Pattern);
        Assert.Equal(
            pattern.SupportingObservations.SelectMany(o => o.EvidenceReferences),
            item.SupportingEvidence);
        Assert.NotEmpty(item.SupportingEvidence);
    }

    // -------------------------------------------------------------- summary

    [Fact]
    public void PublicProfileSummary_ContainsNoClaimAbsentFromProjectedSections()
    {
        RepeatedProfilePattern pattern = CreatePattern(
            ProfilePatternFamily.TechnicalFocus,
            "csharp",
            ["alpha", "beta"],
            ObservedProfileEvidenceKind.RepositoryLanguageEvidence);
        ProfilePatternSet patternSet = CreatePatternSet([pattern]);

        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")],
            patternSet: patternSet);

        ProfileReport report = Project(profile);

        Dictionary<ProfileReportSectionKey, ProfileReportSectionDescriptor> byKey =
            report.Sections.ToDictionary(section => section.SectionKey);

        foreach (ProfileSummaryFact fact in report.PublicProfileSummary.Items)
        {
            Assert.Equal(
                byKey[fact.ReferencedSection].Availability,
                fact.ReferencedSectionAvailability);
        }

        Assert.Equal(
            report.TechnicalFocusAreas.Items.Count,
            report.PublicProfileSummary.Items
                .Single(fact => fact.ReferencedSection
                    == ProfileReportSectionKey.TechnicalFocusAreas)
                .ItemCount);
        Assert.Equal(
            report.RepresentativeRepositories.Items.Count,
            report.PublicProfileSummary.Items
                .Single(fact => fact.ReferencedSection
                    == ProfileReportSectionKey.RepresentativeRepositories)
                .ItemCount);

        // The summary never references itself.
        Assert.DoesNotContain(
            report.PublicProfileSummary.Items,
            fact => fact.ReferencedSection == ProfileReportSectionKey.PublicProfileSummary);
    }

    // ------------------------------------------------------------- ordering

    [Fact]
    public void Project_EligibilityConstructorOrder_DoesNotAlterDrillDownOutput()
    {
        DeveloperPublicEvidenceProfile forward = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")]);
        DeveloperPublicEvidenceProfile reversed = Build(
            eligibility: [Eligibility("beta"), Eligibility("alpha")]);

        Assert.Equal(
            Project(forward).RepositoryDrillDown.Items.Select(item => item.Repository),
            Project(reversed).RepositoryDrillDown.Items.Select(item => item.Repository));
    }

    [Fact]
    public void Project_RepeatedProjection_OfSameProfile_IsSemanticallyIdentical()
    {
        DeveloperPublicEvidenceProfile profile = Build(
            eligibility: [Eligibility("alpha"), Eligibility("beta")]);

        ProfileReport first = Project(profile);
        ProfileReport second = Project(profile);

        Assert.Equal(
            first.Sections,
            second.Sections);
        Assert.Equal(
            first.RepositoryDrillDown.Items.Select(item => item.Repository),
            second.RepositoryDrillDown.Items.Select(item => item.Repository));
        Assert.Equal(
            first.PublicProfileSummary.Items.Select(fact => (fact.ReferencedSection, fact.ItemCount)),
            second.PublicProfileSummary.Items.Select(fact => (fact.ReferencedSection, fact.ItemCount)));
    }

    [Fact]
    public void Project_NoCurrentClockParticipates()
    {
        // No report type exposes a bare timestamp: the projector has nothing
        // it could leak a wall-clock read into.
        Type[] reportTypes =
        [
            typeof(ProfileReport),
            typeof(ProfileReportSection<>),
            typeof(ProfileSummaryFact),
            typeof(PatternReportItem),
            typeof(ConclusionEvidenceReportItem),
            typeof(RepositoryDrillDownReportItem)
        ];

        foreach (Type type in reportTypes)
        {
            Assert.DoesNotContain(
                type.GetProperties(),
                property => property.PropertyType == typeof(DateTime)
                    || property.PropertyType == typeof(DateTimeOffset)
                    || property.PropertyType == typeof(DateTime?)
                    || property.PropertyType == typeof(DateTimeOffset?));
        }

        // Repeated projection at two different real moments in time still
        // agrees semantically (see Project_RepeatedProjection_OfSameProfile_IsSemanticallyIdentical).
    }

    // -------------------------------------------------------------- contract

    public static IEnumerable<object[]> ReportTypesForGuard()
    {
        yield return [typeof(ProfileReport)];
        yield return [typeof(ProfileSummaryFact)];
        yield return [typeof(PatternReportItem)];
        yield return [typeof(ConclusionEvidenceReportItem)];
        yield return [typeof(RepositoryDrillDownReportItem)];
    }

    [Theory]
    [MemberData(nameof(ReportTypesForGuard))]
    public void ReportTypes_DoNotExposeProhibitedSemantics(Type type)
    {
        string[] prohibited =
        [
            "Score", "Rank", "Quality", "Confidence", "Capability", "Seniority",
            "Employab", "JobFit", "Hire", "Reject", "InferredRole",
            "YearsOfExperience", "Authorship", "Originality", "Narrat", "Prose",
            "UiState"
        ];

        string[] names = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        foreach (string prohibitedWord in prohibited)
        {
            Assert.DoesNotContain(
                names,
                name => name.Contains(prohibitedWord, StringComparison.OrdinalIgnoreCase));
        }
    }

    // -------------------------------------------------------------- fixtures

    private static ProfileReport Project(DeveloperPublicEvidenceProfile profile) =>
        new ProfileReportProjector().Project(profile);

    private static T WithCulture<T>(string cultureName, Func<T> action)
    {
        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo culture = new(cultureName);

        try
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }
}
