using System.Text.RegularExpressions;
using Xunit;

namespace GitHubProof.Web.Tests;

// P10-I3 Meaning / Direction / Starting Point and P10-I4 Why / Repository
// Evidence result composition. These are static-source structural tests,
// matching this repository's existing RepositoryReportShellTests/
// CssArchitectureTests convention: they prove the .cshtml source keeps the
// canonical experience order and boundaries without standing up a full Razor
// rendering pipeline, which this Web test project does not otherwise carry.
public sealed class ProfileResultPresentationTests
{
    [Fact]
    public void Profile_page_renders_Profile_sections_in_canonical_experience_order_with_limitations_at_position_seven()
    {
        string content = ReadWebFile("Pages", "Profile.cshtml");

        int meaning = RequireSingleCall(content, "_ProfileMeaning");
        int technicalFocus = RequireSingleCall(
            content,
            "_ProfilePatternSection",
            "report.TechnicalFocusAreas");
        int engineeringPractice = RequireSingleCall(
            content,
            "_ProfilePatternSection",
            "report.EngineeringPracticePatterns");
        int startingPoint = RequireSingleCall(content, "_ProfileStartingPoint");
        int why = RequireSingleCall(
            content,
            "_ProfileWhyThisView",
            "report.WhyThisConclusion");
        int limitations = content.IndexOf(
            "report.Limitations.SectionKey",
            StringComparison.Ordinal);
        int repositoryEvidence = RequireSingleCall(
            content,
            "_ProfileRepositoryEvidence",
            "report.RepositoryDrillDown");

        Assert.True(meaning < technicalFocus, "Meaning must precede Visible technical direction.");
        Assert.True(technicalFocus < engineeringPractice, "Visible technical direction must precede Recurring engineering practices.");
        Assert.True(engineeringPractice < startingPoint, "Recurring engineering practices must precede Starting point.");
        Assert.True(startingPoint < why, "Starting point must precede What is this based on?.");
        Assert.True(why < limitations, "What is this based on? must precede Limitations.");
        Assert.True(limitations < repositoryEvidence, "Limitations must precede Repository evidence.");
    }

    [Fact]
    public void Profile_identity_leads_the_header_with_canonical_GitHub_context_and_action()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        Assert.Single(
            Regex.Matches(content, "<h1", RegexOptions.CultureInvariant).Cast<Match>());
        Assert.Contains("class=\"profile-result__identity\"", content, StringComparison.Ordinal);
        Assert.Contains("@Model.Identity", content, StringComparison.Ordinal);
        Assert.Contains("@Model.ProfileUrlDisplay", content, StringComparison.Ordinal);
        Assert.Contains("GetProfileIdentityContext", content, StringComparison.Ordinal);
        Assert.Contains("href=\"@Model.ProfileUrl\"", content, StringComparison.Ordinal);
        Assert.Contains("GetOpenProfileOnGitHubLabel", content, StringComparison.Ordinal);
        Assert.Contains("GetOpenProfileOnGitHubAccessibleName", content, StringComparison.Ordinal);
        Assert.Contains("target=\"_blank\"", content, StringComparison.Ordinal);
        Assert.Contains("rel=\"noopener noreferrer external\"", content, StringComparison.Ordinal);
        Assert.Contains("UiIcon.ExternalLink", content, StringComparison.Ordinal);
        Assert.Contains("profile-result__identity-mark", content, StringComparison.Ordinal);
        Assert.Contains("UiIcon.Github", content, StringComparison.Ordinal);
        Assert.Contains("class=\"profile-result__back\"", content, StringComparison.Ordinal);
        Assert.DoesNotContain("profile-result__eyebrow", content, StringComparison.Ordinal);
        Assert.DoesNotContain("WebTextKey.ProfileView", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Profile_identity_header_omits_unavailable_recency_share_and_content_metrics()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));
        int headerStart = content.IndexOf("<header class=\"profile-result__header\">", StringComparison.Ordinal);
        int headerEnd = content.IndexOf("</header>", headerStart, StringComparison.Ordinal);

        Assert.True(headerStart >= 0);
        Assert.True(headerEnd > headerStart);

        string header = content[headerStart..(headerEnd + "</header>".Length)];
        string[] forbidden =
        [
            "LastPushed", "Recency", "ActivityClassification",
            "Share", "LinkedIn", "clipboard", "navigator.share",
            "TechnicalFocusAreas", "EngineeringPracticePatterns", "Score", "Items.Count"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, header, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Profile_identity_header_renders_the_real_avatar_when_available_and_a_decorative_fallback_otherwise()
    {
        // Session 111 STEP 2: the header now renders the provider-returned
        // canonical GitHub avatar when the typed pipeline result carries one,
        // and falls back to the existing decorative GitHub mark only when it
        // is legitimately unavailable. Neither branch fabricates a
        // username-derived or generated image URL.
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        Assert.Contains(
            "if (Model.AvatarUrl is not null && Model.Identity is not null)",
            content,
            StringComparison.Ordinal);
        Assert.Contains("<img", content, StringComparison.Ordinal);
        Assert.Contains("src=\"@Model.AvatarUrl\"", content, StringComparison.Ordinal);
        Assert.Contains(
            "alt=\"@ProfileText.GetAvatarAccessibleName(Model.Identity, Text.Culture)\"",
            content,
            StringComparison.Ordinal);
        Assert.Contains("class=\"profile-result__identity-mark profile-result__identity-avatar\"", content, StringComparison.Ordinal);

        // The decorative fallback branch is still present and still
        // aria-hidden — it never claims to be the visitor's own photo.
        Assert.Contains("<span class=\"profile-result__identity-mark\" aria-hidden=\"true\">", content, StringComparison.Ordinal);
        Assert.Contains("UiIcon.Github", content, StringComparison.Ordinal);

        // No username-derived or otherwise fabricated avatar URL.
        Assert.DoesNotContain(".png\"", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Model.Identity}.png", content, StringComparison.Ordinal);
        Assert.DoesNotContain("gravatar", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("identicon", content, StringComparison.OrdinalIgnoreCase);

        string css = ReadWebCssFile("profile-result.css");
        Assert.Contains(".profile-result__identity-avatar", css, StringComparison.Ordinal);
        Assert.Contains("object-fit: cover", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Profile_share_is_not_faked_while_GET_is_redirect_only_and_no_social_metadata_exists()
    {
        string page = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));
        string pageModel = ReadWebFile("Pages", "Profile.cshtml.cs");
        string layout = ReadWebFile("Pages", "Shared", "_Layout.cshtml");

        Assert.Contains("public IActionResult OnGet()", pageModel, StringComparison.Ordinal);
        Assert.Contains("RedirectToPage(\"/Index\")", pageModel, StringComparison.Ordinal);
        Assert.DoesNotContain("data-share", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("LinkedIn", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("clipboard", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("navigator.share", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("og:", layout, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Profile_identity_header_wraps_actions_below_identity_without_horizontal_overflow()
    {
        string css = ReadWebCssFile("profile-result.css");

        Assert.Contains(".profile-result__header", css, StringComparison.Ordinal);
        Assert.Contains("min-width: 0", css, StringComparison.Ordinal);
        Assert.Contains("border: 1px solid var(--border-subtle)", css, StringComparison.Ordinal);
        Assert.Contains("background: var(--surface-raised)", css, StringComparison.Ordinal);
        Assert.Contains(".profile-result__header-actions", css, StringComparison.Ordinal);
        Assert.Contains("flex-wrap: wrap", css, StringComparison.Ordinal);
        Assert.Contains("@media (max-width: 45rem)", css, StringComparison.Ordinal);
        Assert.Contains("flex-direction: column", css, StringComparison.Ordinal);
        Assert.Contains("overflow-wrap: anywhere", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Profile_report_canvas_keeps_canonical_DOM_order_inside_the_dedicated_shell_region()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        int reportCanvas = content.IndexOf("profile-result__report", StringComparison.Ordinal);
        int meaning = RequireSingleCall(content, "_ProfileMeaning");
        int technicalFocus = RequireSingleCall(
            content,
            "_ProfilePatternSection",
            "report.TechnicalFocusAreas");
        int engineeringPractice = RequireSingleCall(
            content,
            "_ProfilePatternSection",
            "report.EngineeringPracticePatterns");
        int startingPoint = RequireSingleCall(content, "_ProfileStartingPoint");
        int why = RequireSingleCall(
            content,
            "_ProfileWhyThisView",
            "report.WhyThisConclusion");

        Assert.True(reportCanvas < meaning);
        Assert.True(meaning < technicalFocus);
        Assert.True(technicalFocus < engineeringPractice);
        Assert.True(engineeringPractice < startingPoint);
        Assert.True(startingPoint < why);
        Assert.Contains("profile-first-read", content, StringComparison.Ordinal);
        Assert.Contains("profile-first-read__primary", content, StringComparison.Ordinal);
        Assert.Contains("profile-first-read__patterns", content, StringComparison.Ordinal);
        Assert.True(why < content.IndexOf("ProfileReportSectionKey.Limitations", StringComparison.Ordinal));
        Assert.True(
            content.IndexOf("ProfileReportSectionKey.Limitations", StringComparison.Ordinal)
            < RequireSingleCall(content, "_ProfileRepositoryEvidence", "report.RepositoryDrillDown"));
        Assert.Contains("ProfileText.GetMeaningPresentation(", content, StringComparison.Ordinal);
        Assert.Contains("Model.Identity ?? throw new InvalidOperationException", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Profile_report_uses_the_handoff_card_grid_without_changing_existing_section_boundaries()
    {
        string patternPartial = ReadWebFile("Pages", "Shared", "_ProfilePatternSection.cshtml");
        string startingPointPartial = ReadWebFile("Pages", "Shared", "_ProfileStartingPoint.cshtml");
        string whyPartial = ReadWebFile("Pages", "Shared", "_ProfileWhyThisView.cshtml");
        string repositoryEvidencePartial = ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml");
        string css = ReadWebCssFile("profile-result.css");

        Assert.Contains("profile-result__section--technical-focus", patternPartial, StringComparison.Ordinal);
        Assert.Contains("profile-result__section--engineering-practices", patternPartial, StringComparison.Ordinal);
        Assert.Contains("profile-result__section--starting-point", startingPointPartial, StringComparison.Ordinal);
        Assert.Contains("profile-result__section--why", whyPartial, StringComparison.Ordinal);
        Assert.Contains("profile-result__section--repository-evidence", repositoryEvidencePartial, StringComparison.Ordinal);
        Assert.Contains(".profile-first-read", css, StringComparison.Ordinal);
        Assert.Contains("width: min(100%, 80rem)", css, StringComparison.Ordinal);
        Assert.Contains("minmax(0, 1.58fr) minmax(20rem, 0.92fr)", css, StringComparison.Ordinal);
        Assert.Contains(".profile-first-read__patterns", css, StringComparison.Ordinal);
        Assert.Contains("minmax(0, 1fr) minmax(0, 1fr)", css, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns:", css, StringComparison.Ordinal);
        Assert.Contains("@media (max-width: 60rem)", css, StringComparison.Ordinal);
        Assert.DoesNotContain("grid-row:", css, StringComparison.Ordinal);
        Assert.DoesNotContain("grid-template-rows:", css, StringComparison.Ordinal);
        Assert.DoesNotContain("grid-auto-flow: dense", css, StringComparison.Ordinal);
        Assert.DoesNotMatch(
            new Regex(@"(?m)^\s*order\s*:", RegexOptions.CultureInvariant),
            css);
    }

    [Fact]
    public void Working_style_card_keeps_a_compact_canonical_list_with_no_per_item_explanation_or_cap()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfilePatternSection.cshtml"));
        string css = ReadWebCssFile("profile-result.css");
        int engineeringLabel = content.IndexOf(
            "ProfileReportSectionKey.EngineeringPracticePatterns",
            StringComparison.Ordinal);
        int populatedListEnd = content.IndexOf("    else", StringComparison.Ordinal);

        Assert.True(engineeringLabel >= 0);
        Assert.True(populatedListEnd > engineeringLabel);
        Assert.Contains("GetWorkingStyleLabel", content, StringComparison.Ordinal);
        Assert.Contains("<ul class=\"profile-result__pattern-list\">", content, StringComparison.Ordinal);
        Assert.Contains("<li class=\"profile-result__pattern-item\">", content, StringComparison.Ordinal);
        Assert.DoesNotContain(".Take(", content, StringComparison.Ordinal);
        Assert.DoesNotContain(".Skip(", content, StringComparison.Ordinal);
        Assert.DoesNotContain(".OrderBy(", content, StringComparison.Ordinal);
        Assert.DoesNotContain(".OrderByDescending(", content, StringComparison.Ordinal);
        // Session 137 / G-2 keeps this guard but sharpens its mechanism. The
        // populated branch now legitimately carries ONE paragraph: the quiet
        // Technical Focus recurrence lead-in, which is gated to
        // TechnicalFocusAreas and therefore never renders on the Working Style
        // card. What must stay absent is a per-item explanation inside the
        // list itself, so that is now asserted directly.
        int listStart = content.IndexOf(
            "<ul class=\"profile-result__pattern-list\">",
            StringComparison.Ordinal);
        int listEnd = content.IndexOf("</ul>", StringComparison.Ordinal);

        Assert.True(listStart >= 0 && listEnd > listStart);
        Assert.DoesNotContain("<p", content[listStart..listEnd], StringComparison.Ordinal);

        string populatedRegion = content[..populatedListEnd];

        Assert.Equal(1, CountOccurrences(populatedRegion, "<p"));
        Assert.Contains(
            "<p class=\"profile-result__technical-focus-lead-in\">",
            populatedRegion,
            StringComparison.Ordinal);

        int technicalFocusGate = content.IndexOf(
            "Model.SectionKey == ProfileReportSectionKey.TechnicalFocusAreas",
            StringComparison.Ordinal);

        Assert.True(
            technicalFocusGate >= 0
            && technicalFocusGate < content.IndexOf(
                "profile-result__technical-focus-lead-in",
                StringComparison.Ordinal),
            "The recurrence lead-in must stay gated to TechnicalFocusAreas.");

        Assert.Contains(
            ".profile-result__section--engineering-practices .profile-result__pattern-item::before",
            css,
            StringComparison.Ordinal);
        Assert.Contains("border-radius: 50%", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Work_type_card_uses_the_mockup_heading_but_keeps_only_canonical_technical_focus_labels()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfilePatternSection.cshtml"));

        Assert.Contains("GetWorkTypeCardHeading", content, StringComparison.Ordinal);
        Assert.Contains("GetRecurringTechnologiesLeadIn", content, StringComparison.Ordinal);

        // Session 137 / G-2: the recurrence lead-in introduces ONE canonical
        // list. The former trailing paragraph that re-rendered the same
        // identities from Model.Items is gone, so a technical identity appears
        // exactly once in this card.
        Assert.DoesNotContain(
            "Model.Items.Select(item => item.Pattern.PatternKey)",
            content,
            StringComparison.Ordinal);
        Assert.DoesNotContain("technical-focus-footer", content, StringComparison.Ordinal);
        Assert.Contains("profile-result__technical-focus-lead-in", content, StringComparison.Ordinal);
        Assert.Equal(
            1,
            CountOccurrences(content, "GetRecurringTechnologiesLeadIn"));

        // The lead-in precedes the one canonical list; the card no longer
        // closes with a second rendering of the same values.
        int leadIn = content.IndexOf(
            "profile-result__technical-focus-lead-in",
            StringComparison.Ordinal);
        int list = content.IndexOf(
            "profile-result__pattern-list",
            StringComparison.Ordinal);

        Assert.True(leadIn >= 0 && list > leadIn);

        // Working Style is untouched: no recurrence lead-in is added there
        // while the Working Style Recurrence Lead-In Contract Gap is open.
        Assert.Contains(
            "Model.SectionKey == ProfileReportSectionKey.TechnicalFocusAreas",
            content,
            StringComparison.Ordinal);

        Assert.DoesNotContain("ProjectType", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Repository.Description", content, StringComparison.Ordinal);
        Assert.DoesNotContain(".OrderBy(", content, StringComparison.Ordinal);
        Assert.DoesNotContain(".Take(", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_uses_the_canonical_repository_target_in_the_existing_analysis_POST_flow()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfileStartingPoint.cshtml");
        string content = StripRazorComments(raw);

        // Session 111 STEP 2/3: presentation reads from the bounded
        // StartingPointPresentation.Select(Model.Section.Items) result, never
        // Model.Section.Items directly — the presentation bound lives in one
        // Web-owned helper, not inline Razor logic.
        Assert.Contains(
            "StartingPointPresentation.Select(Model.Section.Items)",
            content,
            StringComparison.Ordinal);
        Assert.Contains("foreach (RepresentativeRepository item in presentedItems)", content, StringComparison.Ordinal);
        Assert.Contains("@item.Repository.FullName", content, StringComparison.Ordinal);
        Assert.Contains("method=\"post\"", content, StringComparison.Ordinal);
        Assert.Contains("asp-page=\"/Index\"", content, StringComparison.Ordinal);
        Assert.Contains("name=\"GitHubTarget\"", content, StringComparison.Ordinal);
        Assert.Contains("value=\"@item.Repository.Url\"", content, StringComparison.Ordinal);
        Assert.Contains("data-analysis-form", content, StringComparison.Ordinal);
        Assert.Contains("data-analysis-submit", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryActionLabel", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryActionAccessibleName", content, StringComparison.Ordinal);
        Assert.DoesNotContain("href=", content, StringComparison.Ordinal);
        Assert.DoesNotContain("github.com", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Starting_point_card_stretches_to_match_the_paired_cards_and_scrolls_internally_on_desktop()
    {
        // Follow-up layout correction: a long right rail previously left an
        // awkward gap below it whenever the left column (Meaning + the paired
        // work-type/Working Style cards) was taller, because the shared grid
        // row's align-items: start left the rail at its own content height.
        // The card now stretches to the row's height (driven by the left
        // column). Its own repository list keeps its natural content height
        // (never force-grown) and auto top/bottom margins center it within
        // that stretched card — so a short (1-2 item) list never strands a
        // large empty gap beneath it — while still scrolling internally, from
        // its natural top, whenever content is taller than the available
        // height. The card's outer bottom edge always matches the paired
        // cards' bottom edge, with no dead space on the page.
        string css = ReadWebCssFile("profile-result.css");

        int stretchRule = css.IndexOf(
            ".profile-first-read > .profile-result__section--starting-point {",
            StringComparison.Ordinal);
        int desktopListRule = css.IndexOf(
            ".profile-result__section--starting-point .profile-result__repository-list {",
            StringComparison.Ordinal);
        int mobileBreakpoint = css.IndexOf(
            "@media (max-width: 60rem)",
            StringComparison.Ordinal);

        Assert.True(stretchRule >= 0);
        Assert.True(desktopListRule >= 0);
        Assert.True(mobileBreakpoint >= 0);
        Assert.True(stretchRule < mobileBreakpoint);
        Assert.True(desktopListRule < mobileBreakpoint);

        string stretchBlock = css[stretchRule..css.IndexOf('}', stretchRule)];
        string desktopListBlock = css[desktopListRule..css.IndexOf('}', desktopListRule)];

        Assert.Contains("align-self: stretch", stretchBlock, StringComparison.Ordinal);
        Assert.Contains("display: flex", stretchBlock, StringComparison.Ordinal);

        Assert.Contains("flex: 0 1 auto", desktopListBlock, StringComparison.Ordinal);
        Assert.Contains("min-height: 0", desktopListBlock, StringComparison.Ordinal);
        Assert.Contains("margin-block: auto", desktopListBlock, StringComparison.Ordinal);
        Assert.Contains("overflow-y: auto", desktopListBlock, StringComparison.Ordinal);

        // The bound lives on the internal list, never the outer page/card, and
        // introduces no horizontal scrolling of its own.
        Assert.DoesNotContain("overflow-x", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_rail_script_matches_the_primary_columns_height_and_resets_on_mobile()
    {
        // CSS grid's own auto row sizing is symmetric (driven by whichever
        // column is naturally taller), which cannot express "the rail always
        // matches the left column specifically, and never the reverse" on
        // its own — several full-length repository descriptions can make the
        // rail itself the taller natural content. A small, page-scoped script
        // makes the left column authoritative by measuring its rendered
        // height and applying it explicitly, only above the existing 60rem
        // breakpoint; it introduces no new network calls, timers or product
        // behavior.
        string layout = StripRazorComments(ReadWebFile("Pages", "Shared", "_Layout.cshtml"));

        Assert.Contains("src=\"~/js/starting-point-rail.js\"", layout, StringComparison.Ordinal);
        Assert.Contains("defer", layout, StringComparison.Ordinal);

        string script = ReadWebFile("wwwroot", "js", "starting-point-rail.js");

        Assert.Contains(".profile-first-read__primary", script, StringComparison.Ordinal);
        Assert.Contains(".profile-first-read > .profile-result__section--starting-point", script, StringComparison.Ordinal);
        Assert.Contains("(max-width: 60rem)", script, StringComparison.Ordinal);
        Assert.Contains("rail.style.removeProperty(\"height\")", script, StringComparison.Ordinal);
        Assert.Contains("rail.style.height", script, StringComparison.Ordinal);

        string[] forbidden = ["fetch(", "EventSource", "WebSocket", "setInterval", "XMLHttpRequest"];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, script, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Starting_point_card_returns_to_natural_flow_on_mobile()
    {
        string css = ReadWebCssFile("profile-result.css");

        int mobileBreakpoint = css.IndexOf(
            "@media (max-width: 60rem)",
            StringComparison.Ordinal);
        int mobileStretchOverride = css.IndexOf(
            ".profile-first-read > .profile-result__section--starting-point {",
            mobileBreakpoint,
            StringComparison.Ordinal);
        int mobileListOverride = css.IndexOf(
            ".profile-result__section--starting-point .profile-result__repository-list {",
            mobileBreakpoint,
            StringComparison.Ordinal);

        Assert.True(mobileBreakpoint >= 0);
        Assert.True(mobileStretchOverride > mobileBreakpoint);
        Assert.True(mobileListOverride > mobileBreakpoint);

        string mobileStretchBlock =
            css[mobileStretchOverride..css.IndexOf('}', mobileStretchOverride)];
        string mobileListBlock =
            css[mobileListOverride..css.IndexOf('}', mobileListOverride)];

        Assert.Contains("align-self: auto", mobileStretchBlock, StringComparison.Ordinal);
        Assert.Contains("margin-block: 0", mobileListBlock, StringComparison.Ordinal);
        Assert.Contains("overflow-y: visible", mobileListBlock, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_scrollbar_is_themed_rather_than_browser_default()
    {
        string css = ReadWebCssFile("profile-result.css");

        Assert.Contains(
            ".profile-result__section--starting-point .profile-result__repository-list::-webkit-scrollbar {",
            css,
            StringComparison.Ordinal);
        Assert.Contains(
            ".profile-result__section--starting-point .profile-result__repository-list::-webkit-scrollbar-thumb {",
            css,
            StringComparison.Ordinal);
        Assert.Contains("scrollbar-width: thin", css, StringComparison.Ordinal);
        Assert.Contains("scrollbar-color:", css, StringComparison.Ordinal);

        // Themed with the existing token system, not a hardcoded raw color.
        Assert.Contains("var(--border-strong)", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_description_is_visually_bounded_without_semantic_truncation_or_horizontal_overflow()
    {
        // Session 111 STEP 3: the description is VISUALLY bounded (line-clamp)
        // so a long provider Description cannot destabilize the bounded
        // Starting Point rail, but the underlying rendered string is never
        // semantically truncated, and long unbroken text cannot cause
        // horizontal overflow.
        string css = ReadWebCssFile("profile-result.css");
        int ruleStart = css.IndexOf(
            ".profile-result__starting-point-description {",
            StringComparison.Ordinal);

        Assert.True(ruleStart >= 0);

        string rule = css[ruleStart..css.IndexOf('}', ruleStart)];

        Assert.Contains("-webkit-line-clamp: 2", rule, StringComparison.Ordinal);
        Assert.Contains("overflow: hidden", rule, StringComparison.Ordinal);
        // break-word (not anywhere): an ordinary word wraps whole to the next
        // (clipped) line rather than splitting mid-word right before the
        // clamp's ellipsis, while a single unbreakable token still wraps to
        // avoid horizontal overflow.
        Assert.Contains("overflow-wrap: break-word", rule, StringComparison.Ordinal);

        string content = StripRazorComments(
            ReadWebFile("Pages", "Shared", "_ProfileStartingPoint.cshtml"));

        Assert.DoesNotContain(".Substring(", content, StringComparison.Ordinal);
        Assert.DoesNotContain(".Truncate(", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_renders_only_the_provider_description_and_no_other_inference_field()
    {
        // Session 111 STEP 3 supersedes the earlier "no Description at all"
        // restriction: Description now renders, but strictly from
        // Model.Descriptions (RepositoryPresentationMetadata, itself sourced
        // only from GitHub's own repository metadata response) — never
        // derived from repository name/language, selection support,
        // eligibility, score or status.
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileStartingPoint.cshtml"));
        string css = ReadWebCssFile("profile-result.css");

        Assert.Contains("Model.Descriptions.FindDescription(item.Repository)", content, StringComparison.Ordinal);
        Assert.Contains("GetStartingPointDescriptionFallback", content, StringComparison.Ordinal);

        // Session 137 / p10 8.6.1b supersedes the earlier "no PatternSupports
        // at all" restriction: the pattern relationship now renders, but only
        // from the PatternSupports IDENTITIES via the dedicated localizer
        // method. The Representative Support Evidence Trace stays out.
        Assert.Contains("item.PatternSupports", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SupportingEvidenceReferences", content, StringComparison.Ordinal);
        Assert.DoesNotContain("SupportingObservationKeys", content, StringComparison.Ordinal);
        Assert.DoesNotContain("EvidenceKey", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Eligibility", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Score", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Status", content, StringComparison.Ordinal);
        Assert.DoesNotContain("item.Repository.Name", content, StringComparison.Ordinal);
        Assert.DoesNotContain("item.Repository.Owner", content, StringComparison.Ordinal);
        Assert.DoesNotContain("PrimaryLanguage", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Readme", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Html.Raw", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Markdig", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(".profile-result__starting-point-identity", css, StringComparison.Ordinal);
        Assert.Contains(".profile-result__starting-point-description", css, StringComparison.Ordinal);
        Assert.Contains("overflow-wrap: anywhere", css, StringComparison.Ordinal);
        Assert.Contains("justify-self: start", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Profile_page_omits_only_the_unsupported_and_deferred_Profile_sections()
    {
        // Razor comments legitimately name deferred concepts to explain why
        // they are absent; only the executable markup/code must never
        // reference them. WhyThisConclusion and RepositoryDrillDown are now
        // legitimately present (P10-I4) and are intentionally excluded from
        // this forbidden list.
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        string[] forbidden =
        [
            "ProblemAreaPatterns",
            "RoleEvidenceAlignment"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Partial_notice_is_early_but_does_not_replace_the_report()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        int partial = content.IndexOf("Model.IsPartialProfile", StringComparison.Ordinal);
        int meaning = RequireSingleCall(content, "_ProfileMeaning");
        int repositoryEvidence = RequireSingleCall(content, "_ProfileRepositoryEvidence");

        Assert.True(partial >= 0 && partial < meaning);
        Assert.True(meaning < repositoryEvidence);
        Assert.Contains("WebTextKey.ProfilePartialTitle", content, StringComparison.Ordinal);
        Assert.Contains("WebTextKey.ProfilePartialMessage", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Canonical_warnings_are_a_notice_near_Meaning_not_a_numbered_report_section()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        int meaning = RequireSingleCall(content, "_ProfileMeaning");
        int warnings = content.IndexOf("report.Warnings.Count", StringComparison.Ordinal);
        int technicalFocus = RequireSingleCall(
            content,
            "_ProfilePatternSection",
            "report.TechnicalFocusAreas");
        int engineeringPractice = RequireSingleCall(
            content,
            "_ProfilePatternSection",
            "report.EngineeringPracticePatterns");
        int startingPoint = RequireSingleCall(content, "_ProfileStartingPoint");

        // Session 137 / G-4: the operational note stays in first read and
        // inside the primary column, but no longer interrupts
        // Meaning -> recurring results. It is subordinate: after both pattern
        // cards, still before the evidence layer.
        Assert.True(meaning < technicalFocus);
        Assert.True(technicalFocus < engineeringPractice);
        Assert.True(engineeringPractice < warnings);
        Assert.True(warnings < startingPoint);
        Assert.Contains("profile-first-read__primary", content, StringComparison.Ordinal);
        Assert.Contains("<aside", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<h2 class=\"profile-result__notice-title\">", content, StringComparison.Ordinal);
        Assert.Contains("GetWarningLabel", content, StringComparison.Ordinal);
        Assert.Contains("foreach (ProfileWarning warning in report.Warnings)", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Limitations_render_only_canonical_items_and_preserve_supported_empty_truth()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        Assert.Contains("ProfileReportSectionKey.Limitations", content, StringComparison.Ordinal);
        Assert.Contains("ProfileReportSectionAvailability.Supported", content, StringComparison.Ordinal);
        Assert.Contains("foreach (ProfileLimitation limitation in report.Limitations.Items)", content, StringComparison.Ordinal);
        Assert.Contains("GetLimitationLabel", content, StringComparison.Ordinal);
        Assert.Contains("GetEmptySectionMessage", content, StringComparison.Ordinal);

        Assert.DoesNotContain("ExecutionAvailability", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Admission", content, StringComparison.Ordinal);
        Assert.DoesNotContain("github-budget", content, StringComparison.Ordinal);
        Assert.DoesNotContain("execution-deadline", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Failure_state_never_renders_a_Profile_report_branch()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));

        Assert.Contains("@if (Model.ErrorMessage is not null)", content, StringComparison.Ordinal);
        Assert.Contains("else if (Model.Report is { } report)", content, StringComparison.Ordinal);
        Assert.Contains("role=\"alert\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Meaning_partial_is_bounded_to_the_precomposed_presentation_narrative_only()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfileMeaning.cshtml");

        Assert.Contains(
            "@model ProfileMeaningPresentation",
            raw,
            StringComparison.Ordinal);

        string content = StripRazorComments(raw);

        Assert.Contains("@Model.TechnicalNarrative", content, StringComparison.Ordinal);
        Assert.Contains("@Model.PracticeNarrative", content, StringComparison.Ordinal);
        Assert.Contains("@Model.Provenance", content, StringComparison.Ordinal);

        string[] forbidden =
        [
            "PatternReportItem",
            "RepresentativeRepository",
            "ProfileRepositoryReference",
            "ProfileReport ",
            "WhyThisConclusion",
            "RepositoryDrillDown",
            "Warnings",
            "Limitations",
            "Snapshot",
            "Signal",
            "Eligibility",
            "Score",
            "GetMeaning"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Pattern_section_partial_is_bounded_to_one_report_section_and_never_renders_ItemCount()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfilePatternSection.cshtml");

        Assert.Contains(
            "@model ProfileReportSection<PatternReportItem>",
            raw,
            StringComparison.Ordinal);

        string content = StripRazorComments(raw);

        // Items.Count is used only as a structural presence check
        // (Model.Items.Count > 0); the count itself is never interpolated
        // into rendered output.
        Assert.DoesNotContain("@Model.Items.Count", content, StringComparison.Ordinal);
        Assert.DoesNotContain("ItemCount", content, StringComparison.Ordinal);
        Assert.DoesNotContain("RepresentativeRepository", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_partial_renders_repository_identity_with_the_bounded_existing_analysis_action()
    {
        // Session 111 supersedes P10-I4's text-only restriction without
        // exposing selection support, eligibility or a direct GitHub link.
        string raw = ReadWebFile("Pages", "Shared", "_ProfileStartingPoint.cshtml");

        Assert.Contains(
            "@model StartingPointSectionModel",
            raw,
            StringComparison.Ordinal);

        Assert.Contains("FullName", raw, StringComparison.Ordinal);

        string content = StripRazorComments(raw);

        string[] forbidden =
        [
            "<a ",
            "href=",
            "SupportingEvidenceReferences",
            "SupportingObservationKeys",
            "AnalysisReference",
            "Eligibility",
            "\"best\"",
            "\"top\"",
            "\"strongest\"",
            "\"recommended\"",
            "ItemCount"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }

        Assert.Contains("<form", content, StringComparison.Ordinal);
        Assert.Contains("asp-page=\"/Index\"", content, StringComparison.Ordinal);
        Assert.Contains("name=\"GitHubTarget\"", content, StringComparison.Ordinal);
        Assert.Contains("value=\"@item.Repository.Url\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Why_partial_is_bounded_to_the_conclusion_evidence_model_and_guards_section_and_availability()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfileWhyThisView.cshtml");

        Assert.Contains(
            "@model ProfileReportSection<ConclusionEvidenceReportItem>",
            raw,
            StringComparison.Ordinal);
        Assert.Contains("ProfileReportSectionKey.WhyThisConclusion", raw, StringComparison.Ordinal);
        Assert.Contains("ProfileReportSectionAvailability.Supported", raw, StringComparison.Ordinal);
        Assert.Contains("InvalidOperationException", raw, StringComparison.Ordinal);
    }

    [Fact]
    public void Why_partial_uses_native_collapsed_disclosure_for_non_empty_and_plain_section_for_empty()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileWhyThisView.cshtml"));

        Assert.Contains("<details", content, StringComparison.Ordinal);
        Assert.Contains("<summary", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<details open", content, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-expanded", content, StringComparison.Ordinal);
        Assert.Contains("GetWhyDisclosureDescription", content, StringComparison.Ordinal);
        Assert.Contains("profile-result__disclosure-chevron", content, StringComparison.Ordinal);
        Assert.Contains("UiIcon.ChevronRight", content, StringComparison.Ordinal);
        Assert.Contains("GetEmptySectionMessage", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Why_partial_translates_the_existing_trace_into_a_question_reason_and_supporting_repositories()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileWhyThisView.cshtml"));

        Assert.Contains("GetWhyEntryQuestion", content, StringComparison.Ordinal);
        Assert.Contains("GetWhyExplanation", content, StringComparison.Ordinal);
        Assert.Contains("GetWhySupportingRepositoriesLeadIn", content, StringComparison.Ordinal);
        Assert.Contains("SupportingRepositories", content, StringComparison.Ordinal);
        Assert.Contains("repository.FullName", content, StringComparison.Ordinal);
        Assert.Contains("profile-result__why-explanation", content, StringComparison.Ordinal);

        string[] forbidden =
        [
            "IndependentSupportingRepositories",
            "SupportingEvidence",
            "EvidenceKey",
            "EvidenceKeys",
            "AnalysisReference",
            "ItemCount",
            "RuleVersion",
            "ObservationRuleVersion",
            "ObservationKey"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }

        // The presence check (Model.Items.Count > 0) is structural, never an
        // interpolated count.
        Assert.DoesNotContain("@Model.Items.Count", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_partial_is_bounded_to_the_drilldown_model_and_guards_section_and_availability()
    {
        string raw = ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml");

        Assert.Contains(
            "@model ProfileReportSection<RepositoryDrillDownReportItem>",
            raw,
            StringComparison.Ordinal);
        Assert.Contains("ProfileReportSectionKey.RepositoryDrillDown", raw, StringComparison.Ordinal);
        Assert.Contains("ProfileReportSectionAvailability.Supported", raw, StringComparison.Ordinal);
        Assert.Contains("InvalidOperationException", raw, StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_partial_uses_native_collapsed_disclosure_for_non_empty_and_plain_section_for_empty()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml"));

        Assert.Contains("<details", content, StringComparison.Ordinal);
        Assert.Contains("<summary", content, StringComparison.Ordinal);
        Assert.DoesNotContain("<details open", content, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-expanded", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryFreshAnalysisLeadIn", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryEvidenceDisclosureDescription", content, StringComparison.Ordinal);
        Assert.Contains("profile-result__disclosure-chevron", content, StringComparison.Ordinal);
        Assert.Contains("GetEmptySectionMessage", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_partial_iterates_every_item_with_no_presentation_filtering_or_analysis_reference()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml"));

        Assert.Contains("foreach", content, StringComparison.Ordinal);
        Assert.Contains("Model.Items", content, StringComparison.Ordinal);
        Assert.Contains("item.Repository.FullName", content, StringComparison.Ordinal);
        Assert.Contains("item.EligibilityStatus", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryStatusLabel", content, StringComparison.Ordinal);

        string[] forbidden =
        [
            "AnalysisReference",
            ".Where(",
            ".OrderBy(",
            ".OrderByDescending(",
            ".GroupBy(",
            ".Take(",
            ".Skip(",
            "ItemCount",
            "EvidenceKey",
            "EvidenceKeys"
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Repository_evidence_partial_posts_repository_url_as_GitHubTarget_and_never_uses_it_as_href()
    {
        string content = StripRazorComments(ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml"));

        Assert.Contains("<form", content, StringComparison.Ordinal);
        Assert.Contains("method=\"post\"", content, StringComparison.Ordinal);
        Assert.Contains("asp-page=\"/Index\"", content, StringComparison.Ordinal);
        Assert.Contains("name=\"GitHubTarget\"", content, StringComparison.Ordinal);
        Assert.Contains("value=\"@item.Repository.Url\"", content, StringComparison.Ordinal);
        Assert.Contains("data-analysis-form", content, StringComparison.Ordinal);
        Assert.Contains("data-analysis-submit", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryActionAccessibleName", content, StringComparison.Ordinal);
        Assert.Contains("GetRepositoryActionLabel", content, StringComparison.Ordinal);

        Assert.DoesNotContain("<a ", content, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"@item.Repository.Url\"", content, StringComparison.Ordinal);
        Assert.DoesNotContain("href=@item.Repository.Url", content, StringComparison.Ordinal);
        Assert.DoesNotContain("method=\"get\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<script", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void New_P10I4_partials_never_hardcode_a_ranking_quality_or_capability_adjective()
    {
        string[] partials =
        [
            "_ProfileMeaning.cshtml",
            "_ProfilePatternSection.cshtml",
            "_ProfileStartingPoint.cshtml",
            "_ProfileWhyThisView.cshtml",
            "_ProfileRepositoryEvidence.cshtml"
        ];

        string[] forbiddenWords =
        [
            "strong", "weak", "senior", "junior", "expert", "skilled",
            "employable", "suitable", "best", "top", "strongest",
            "recommended", "impressive", "consistent", "confidence",
            "capability", "rank", "score"
        ];

        foreach (string partial in partials)
        {
            string content = StripRazorComments(
                    ReadWebFile("Pages", "Shared", partial))
                .ToLowerInvariant();

            foreach (string word in forbiddenWords)
            {
                Assert.DoesNotContain(word, content, StringComparison.Ordinal);
            }
        }
    }

    // ------------------------------------------------------------------
    // P10-I6-B - Profile trace + repository inspection.
    //
    // Architecture-owner decision: a direct action from a Why entry is
    // deliberately NOT introduced. The canonical trace stays two-stage
    // (contract 8.6.3 -> 8.6.5): "What is this based on?" names supporting
    // repository identities, and Repository evidence owns the single existing
    // Repository Proof POST per repository. I6-B improves only the VISUAL
    // continuity and inspection usability between those two stages, so these
    // guards are deliberately CSS-shaped and assert no new markup, copy or
    // interaction.
    // ------------------------------------------------------------------

    [Fact]
    public void Why_supporting_repository_identities_share_the_repository_evidence_identity_typography()
    {
        string css = ReadWebCssFile("profile-result.css");

        string whyIdentityRule = RequireCssRule(
            css,
            ".profile-result__section--why .profile-result__repository-item {");
        string evidenceIdentityRule = RequireCssRule(
            css,
            ".profile-result__evidence-identity {");

        // The same literal repository name must read as the same kind of thing
        // at both stages of the trace.
        foreach (string declaration in new[]
        {
            "font-family: var(--font-mono)",
            "font-size: var(--font-size-mono)",
            "line-height: var(--line-height-mono)"
        })
        {
            Assert.Contains(declaration, whyIdentityRule, StringComparison.Ordinal);
            Assert.Contains(declaration, evidenceIdentityRule, StringComparison.Ordinal);
        }

        // Typography only: the Why identity must not acquire emphasis that
        // would read as importance, quality or ordering.
        foreach (string forbidden in new[]
        {
            "var(--accent-primary)",
            "var(--accent-soft)",
            "font-weight"
        })
        {
            Assert.DoesNotContain(forbidden, whyIdentityRule, StringComparison.Ordinal);
        }

        // Anchored: a plain substring check for "order:"/"content:" would also
        // match "border:" and "justify-content:".
        foreach (string property in new[] { "order", "content" })
        {
            Assert.DoesNotMatch(
                new Regex(
                    $@"(?m)^\s*{property}\s*:",
                    RegexOptions.CultureInvariant),
                whyIdentityRule);
        }
    }

    [Fact]
    public void Why_identity_typography_is_scoped_so_starting_point_keeps_its_own_treatment()
    {
        // Regression guard for a real coupling: Why's supporting list and the
        // Starting Point cards share .profile-result__repository-list /
        // .profile-result__repository-item. Restyling the shared base class
        // would silently restyle the first-read Starting Point rail.
        string css = ReadWebCssFile("profile-result.css");

        // The shared base rule is the grouped
        // ".profile-result__pattern-item, .profile-result__repository-item"
        // declaration. It must be located by its FIRST selector: searching for
        // ".profile-result__repository-item {" alone finds the earlier
        // Starting-Point-scoped rule instead, which would make this guard
        // vacuous.
        string sharedItemBlock = RequireCssRule(css, ".profile-result__pattern-item,");

        Assert.Contains(
            ".profile-result__repository-item {",
            sharedItemBlock,
            StringComparison.Ordinal);
        Assert.DoesNotContain("font-family", sharedItemBlock, StringComparison.Ordinal);

        Assert.Contains(
            ".profile-result__section--why .profile-result__repository-item {",
            css,
            StringComparison.Ordinal);

        string startingPointItemRule = RequireCssRule(
            css,
            ".profile-result__section--starting-point .profile-result__repository-item {");

        Assert.DoesNotContain(
            "var(--font-mono)",
            startingPointItemRule,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_rows_keep_identity_and_status_together_beside_one_action()
    {
        // The row previously wrapped as a flex line, so at intermediate widths
        // the status could detach onto its own line or the action could wrap
        // under the identity while the status stayed beside it.
        string css = ReadWebCssFile("profile-result.css");

        int narrowBreakpoint = css.IndexOf(
            "@media (max-width: 45rem)",
            StringComparison.Ordinal);
        int rowRuleStart = css.IndexOf(
            ".profile-result__evidence-row {",
            StringComparison.Ordinal);

        Assert.True(narrowBreakpoint >= 0);
        Assert.True(rowRuleStart >= 0);
        Assert.True(
            rowRuleStart < narrowBreakpoint,
            "The base row rule must precede its narrow override.");

        string rowRule = css[rowRuleStart..css.IndexOf('}', rowRuleStart)];
        string identityRule = RequireCssRule(css, ".profile-result__evidence-identity {");
        string statusRule = RequireCssRule(css, ".profile-result__evidence-status {");
        string actionRule = RequireCssRule(
            css,
            ".profile-result__evidence-row [data-analysis-form] {");

        Assert.Contains("display: grid", rowRule, StringComparison.Ordinal);
        Assert.Contains(
            "grid-template-columns: minmax(0, 1fr) minmax(0, auto) auto",
            rowRule,
            StringComparison.Ordinal);
        Assert.DoesNotContain("flex-wrap", rowRule, StringComparison.Ordinal);

        // Every cell is explicitly placed, so identity, status and the single
        // existing action can never reorder or detach at any width.
        Assert.Contains("grid-column: 1", identityRule, StringComparison.Ordinal);
        Assert.Contains("grid-column: 2", statusRule, StringComparison.Ordinal);
        Assert.Contains("grid-column: 3", actionRule, StringComparison.Ordinal);

        // The action is selected through the markup's existing hook, so the
        // partial gains no element, class or attribute.
        Assert.Contains("data-analysis-form", actionRule, StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_row_markup_still_carries_the_hook_the_I6B_layout_selects_on()
    {
        // The I6-B row layout places the single existing POST action through
        // .profile-result__evidence-row [data-analysis-form] rather than a new
        // class, so no element or attribute had to be added to the partial.
        // That leaves one real coupling: the CSS depends on an attribute the
        // markup carries for the analysis-progress script. If the attribute
        // were ever dropped from the partial, the action would silently lose
        // its column placement while every CSS-only guard stayed green. This
        // test closes that gap from the markup side.
        string css = ReadWebCssFile("profile-result.css");
        string partial = StripRazorComments(
            ReadWebFile("Pages", "Shared", "_ProfileRepositoryEvidence.cshtml"));

        Assert.Contains(
            ".profile-result__evidence-row [data-analysis-form] {",
            css,
            StringComparison.Ordinal);

        int rowStart = partial.IndexOf(
            "profile-result__evidence-row",
            StringComparison.Ordinal);

        Assert.True(rowStart >= 0, "The Repository evidence row class was not found in the partial.");

        int rowEnd = partial.IndexOf("</li>", rowStart, StringComparison.Ordinal);

        Assert.True(rowEnd > rowStart, "The Repository evidence row element was not closed.");

        string row = partial[rowStart..rowEnd];

        // The hook must live on the row's own form, not merely somewhere else
        // in the file.
        Assert.Contains("<form", row, StringComparison.Ordinal);
        Assert.Contains(
            "data-analysis-form",
            row,
            StringComparison.Ordinal);

        // The row still carries exactly the three cells the layout places.
        Assert.Contains("profile-result__evidence-identity", row, StringComparison.Ordinal);
        Assert.Contains("profile-result__evidence-status", row, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(row, "<form", RegexOptions.CultureInvariant).Cast<Match>());
    }

    [Fact]
    public void Repository_evidence_identity_and_status_survive_a_long_repository_full_name()
    {
        string css = ReadWebCssFile("profile-result.css");

        foreach (string selector in new[]
        {
            ".profile-result__evidence-identity {",
            ".profile-result__evidence-status {"
        })
        {
            string rule = RequireCssRule(css, selector);

            Assert.Contains("min-width: 0", rule, StringComparison.Ordinal);
            Assert.Contains("overflow-wrap: anywhere", rule, StringComparison.Ordinal);
            Assert.Contains("word-break: normal", rule, StringComparison.Ordinal);
        }

        string rowRule = RequireCssRule(css, ".profile-result__evidence-row {");

        // A fixed reading column would reintroduce horizontal overflow.
        Assert.Contains("minmax(0, 1fr)", rowRule, StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_row_separator_does_not_rule_against_the_lead_in_paragraph()
    {
        string css = ReadWebCssFile("profile-result.css");

        string firstRowRule = RequireCssRule(
            css,
            ".profile-result__evidence-row:first-child {");

        Assert.Contains("border-top: 0", firstRowRule, StringComparison.Ordinal);
        Assert.Contains("padding-top: 0", firstRowRule, StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_evidence_rows_stack_with_a_quiet_action_on_narrow_viewports()
    {
        string css = ReadWebCssFile("profile-result.css");

        int narrowBreakpoint = css.IndexOf(
            "@media (max-width: 45rem)",
            StringComparison.Ordinal);
        int narrowRowRule = css.IndexOf(
            ".profile-result__evidence-row {",
            narrowBreakpoint,
            StringComparison.Ordinal);
        int narrowActionRule = css.IndexOf(
            ".profile-result__evidence-row .profile-result__evidence-status,",
            narrowBreakpoint,
            StringComparison.Ordinal);

        Assert.True(narrowBreakpoint >= 0);
        Assert.True(narrowRowRule > narrowBreakpoint);
        Assert.True(narrowActionRule > narrowBreakpoint);

        string narrowRowBlock = css[narrowRowRule..css.IndexOf('}', narrowRowRule)];
        string narrowActionBlock = css[narrowActionRule..css.IndexOf('}', narrowActionRule)];

        Assert.Contains("grid-template-columns: minmax(0, 1fr)", narrowRowBlock, StringComparison.Ordinal);
        // Status and the existing action both fall into the single column, in
        // DOM order, beneath their own repository identity.
        Assert.Contains("grid-column: 1", narrowActionBlock, StringComparison.Ordinal);
        Assert.Contains("[data-analysis-form]", narrowActionBlock, StringComparison.Ordinal);

        // The action stays a quiet secondary step, not a full-width call to
        // action competing with the first-read layer.
        Assert.Contains("justify-self: start", narrowActionBlock, StringComparison.Ordinal);
        Assert.DoesNotContain("width: 100%", narrowActionBlock, StringComparison.Ordinal);
    }

    [Fact]
    public void Profile_result_css_generates_no_text_counter_or_ordering_behaviour()
    {
        // The inspection surface must not acquire anything that reads as a
        // count, rank, ordering or quality signal - including through CSS,
        // which the Razor-source guards cannot see.
        string css = ReadWebCssFile("profile-result.css");

        foreach (string forbidden in new[]
        {
            "counter-reset",
            "counter-increment",
            "counters(",
            "counter(",
            "attr("
        })
        {
            Assert.DoesNotContain(forbidden, css, StringComparison.Ordinal);
        }

        // Decorative shapes may use an empty content value; generated text may
        // never be introduced.
        foreach (Match match in GeneratedContentPattern.Matches(css))
        {
            const string EmptyDoubleQuoted = "\"\"";
            const string EmptySingleQuoted = "''";

            string value = match.Groups[1].Value.Trim();

            Assert.True(
                value == EmptyDoubleQuoted
                    || value == EmptySingleQuoted
                    || value == "none",
                $"profile-result.css must not generate content, but found: content: {value};");
        }
    }

    // ------------------------------------------------------------------
    // Session 137 - p10 8.6.1b Starting Point pattern relationship,
    // G-3 Why affordance perceptibility.
    // ------------------------------------------------------------------

    [Fact]
    public void Starting_point_names_the_repeated_result_between_identity_and_description()
    {
        string content = StripRazorComments(
            ReadWebFile("Pages", "Shared", "_ProfileStartingPoint.cshtml"));

        int identity = content.IndexOf(
            "profile-result__starting-point-identity",
            StringComparison.Ordinal);
        int relationship = content.IndexOf(
            "profile-result__starting-point-relationship",
            StringComparison.Ordinal);
        int description = content.IndexOf(
            "profile-result__starting-point-description",
            StringComparison.Ordinal);
        int action = content.IndexOf("<form", StringComparison.Ordinal);

        Assert.True(identity >= 0);
        Assert.True(identity < relationship);
        Assert.True(relationship < description);
        Assert.True(description < action);

        Assert.Contains(
            "ProfileText.GetStartingPointPatternRelationship(",
            content,
            StringComparison.Ordinal);
        Assert.Contains("item.PatternSupports", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_relationship_renders_every_support_with_no_cap_or_reordering()
    {
        string content = StripRazorComments(
            ReadWebFile("Pages", "Shared", "_ProfileStartingPoint.cshtml"));

        int call = content.IndexOf(
            "ProfileText.GetStartingPointPatternRelationship(",
            StringComparison.Ordinal);

        Assert.True(call >= 0);

        // The whole collection is handed to the localizer: no inline Take,
        // First, OrderBy, Where or index access could introduce a hidden
        // first-pattern-only bound in Razor.
        string call_region = content[call..content.IndexOf(')', call)];

        Assert.DoesNotContain(".Take(", call_region, StringComparison.Ordinal);
        Assert.DoesNotContain(".First", call_region, StringComparison.Ordinal);
        Assert.DoesNotContain(".OrderBy(", call_region, StringComparison.Ordinal);
        Assert.DoesNotContain(".Where(", call_region, StringComparison.Ordinal);
        Assert.DoesNotContain("[0]", call_region, StringComparison.Ordinal);
        Assert.DoesNotContain(".Count", call_region, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_relationship_is_quiet_text_not_a_badge_or_clamped_content()
    {
        string css = ReadWebCssFile("profile-result.css");
        string rule = RequireCssRule(css, ".profile-result__starting-point-relationship {");

        // Quiet supporting text on existing semantic tokens.
        Assert.Contains("var(--fg-tertiary)", rule, StringComparison.Ordinal);
        Assert.Contains("var(--font-size-small)", rule, StringComparison.Ordinal);

        // A repository can represent several patterns. Wrapping is allowed;
        // clamping, capping or ellipsis would remove semantic content.
        Assert.Contains("overflow-wrap: break-word", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("line-clamp", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("text-overflow", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("white-space: nowrap", rule, StringComparison.Ordinal);

        // Not a badge/chip: no pill background, border or radius treatment.
        Assert.DoesNotContain("background", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("border", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("font-weight", rule, StringComparison.Ordinal);
    }

    [Fact]
    public void Why_affordance_gains_prominence_without_changing_its_disclosure_or_order()
    {
        string page = StripRazorComments(ReadWebFile("Pages", "Profile.cshtml"));
        string partial = StripRazorComments(
            ReadWebFile("Pages", "Shared", "_ProfileWhyThisView.cshtml"));
        string css = ReadWebCssFile("profile-result.css");

        // Still exactly one Why control, still native and collapsed by default.
        Assert.Equal(1, CountOccurrences(page, "_ProfileWhyThisView"));
        Assert.Contains("<details", partial, StringComparison.Ordinal);
        Assert.Contains("<summary", partial, StringComparison.Ordinal);
        Assert.DoesNotContain("<details open", partial, StringComparison.Ordinal);
        Assert.DoesNotContain(" open>", partial, StringComparison.Ordinal);
        Assert.DoesNotContain("open=", partial, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-expanded", partial, StringComparison.Ordinal);
        Assert.DoesNotContain("<script", partial, StringComparison.Ordinal);

        // Canonical order is unchanged: Starting Point still precedes Why.
        Assert.True(
            RequireSingleCall(page, "_ProfileStartingPoint")
            < RequireSingleCall(page, "_ProfileWhyThisView"));

        // Prominence comes from SEPARATION only, using an existing spacing
        // token. p10 §8.8.3 forbids any other level taking visual priority
        // over Meaning and the two recurring-result cards, so the Why card may
        // not gain border, background, gradient, animation or weight emphasis
        // that the L2 cards do not have.
        string rule = RequireCssRule(
            css,
            ".profile-result__report > .profile-result__section--why {");

        Assert.Contains("margin-top:", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("border", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("background", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("font-weight", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("animation", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("gradient", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("transform", rule, StringComparison.Ordinal);

        // L2 >= L3 invariant: the recurring-result cards and the Why card
        // share one card treatment, declared together in the same grouped
        // rule. Nothing later may raise Why above them.
        string sharedCard = RequireCssRule(
            css,
            ".profile-first-read__patterns > .profile-result__section,");

        Assert.Contains(
            ".profile-result__report > .profile-result__section--why",
            sharedCard,
            StringComparison.Ordinal);
        Assert.Contains("var(--border-subtle)", sharedCard, StringComparison.Ordinal);

        // Meaning keeps the dominant treatment, so L1 still leads.
        string meaningRule = RequireCssRule(
            css,
            ".profile-first-read__primary > .profile-result__meaning {");

        Assert.Contains("var(--accent-primary)", meaningRule, StringComparison.Ordinal);
    }

    [Fact]
    public void Starting_point_relationship_localizer_never_reads_the_evidence_trace()
    {
        // Behavioural tests prove today's output; this guards the method body
        // so a later edit cannot quietly pull the Representative Support
        // Evidence Trace into a first-read surface (p10 §8.6.1b).
        string source = ReadWebFile("Localization", "ProfileWebPresentationLocalizer.cs");
        int start = source.IndexOf(
            "public string GetStartingPointPatternRelationship(",
            StringComparison.Ordinal);

        Assert.True(start >= 0, "The Starting Point relationship method was not found.");

        int next = source.IndexOf("    public ", start + 1, StringComparison.Ordinal);
        string body = next > start ? source[start..next] : source[start..];

        string[] forbidden =
        [
            "SupportingEvidenceReferences",
            "SupportingObservationKeys",
            "EvidenceKey",
            "AnalysisReference",
            "IndependentSupportingRepositories",
            "SupportingRepositories",
            ".Take(",
            ".Skip(",
            ".OrderBy(",
            ".Where(",
            ".Reverse("
        ];

        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, body, StringComparison.Ordinal);
        }

        // Only the two identity fields participate.
        Assert.Contains("support.PatternFamily", body, StringComparison.Ordinal);
        Assert.Contains("support.PatternKey", body, StringComparison.Ordinal);
    }

    private static int CountOccurrences(string content, string value)
    {
        int count = 0;
        int index = content.IndexOf(value, StringComparison.Ordinal);

        while (index >= 0)
        {
            count++;
            index = content.IndexOf(value, index + value.Length, StringComparison.Ordinal);
        }

        return count;
    }

    private static string RequireCssRule(string css, string selector)
    {
        int start = css.IndexOf(selector, StringComparison.Ordinal);

        Assert.True(start >= 0, $"Expected CSS rule was not found: {selector}");

        return css[start..css.IndexOf('}', start)];
    }

    private static readonly Regex GeneratedContentPattern = new(
        @"(?<![\w-])content:\s*([^;]+);",
        RegexOptions.CultureInvariant);

    private static readonly Regex RazorCommentPattern = new(
        @"@\*.*?\*@",
        RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static string StripRazorComments(string content) =>
        RazorCommentPattern.Replace(content, string.Empty);

    private static int RequireSingleCall(
        string content,
        string partialName,
        string? modelExpression = null)
    {
        string modelPattern = modelExpression is null
            ? @"[^)]+"
            : Regex.Escape(modelExpression);

        Regex regex = new(
            $@"@await\s+Html\.PartialAsync\(\s*""{Regex.Escape(partialName)}""\s*,\s*{modelPattern}\s*\)",
            RegexOptions.CultureInvariant);

        MatchCollection matches = regex.Matches(content);

        Assert.True(
            matches.Count == 1,
            $"Expected exactly one call to '{partialName}'"
            + (modelExpression is null ? string.Empty : $" with model '{modelExpression}'")
            + $", found {matches.Count}.");

        return matches[0].Index;
    }

    private static string ReadWebFile(params string[] relativeSegments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(directory.FullName, "GitHubProof.slnx");

            if (File.Exists(solutionPath))
            {
                string path = Path.Combine(
                    [directory.FullName, "src", "GitHubProof.Web", .. relativeSegments]);

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Expected Web file was not found: {path}");
                }

                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate the repository root (GitHubProof.slnx) from "
            + AppContext.BaseDirectory);
    }

    private static string ReadWebCssFile(string fileName)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(directory.FullName, "GitHubProof.slnx");

            if (File.Exists(solutionPath))
            {
                string path = Path.Combine(
                    directory.FullName,
                    "src",
                    "GitHubProof.Web",
                    "wwwroot",
                    "css",
                    fileName);

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Expected Web CSS file was not found: {path}");
                }

                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate the repository root (GitHubProof.slnx) from "
            + AppContext.BaseDirectory);
    }
}
