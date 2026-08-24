using Xunit;
using System.Text.RegularExpressions;

namespace GitHubProof.Web.Tests;

public sealed class CssArchitectureTests
{
    private static readonly Regex RawColorPattern = new(
        @"#[0-9a-fA-F]{3,8}\b|rgba?\(|hsla?\(",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Fact]
    public void Consumer_stylesheets_do_not_use_raw_colors()
    {
        string cssDirectory = GetCssDirectory();

        string[] consumerFiles = Directory
            .EnumerateFiles(cssDirectory, "*.css", SearchOption.TopDirectoryOnly)
            .Where(path => !string.Equals(
                Path.GetFileName(path),
                "tokens.css",
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(consumerFiles);

        foreach (string file in consumerFiles)
        {
            string content = File.ReadAllText(file);

            Assert.False(
                RawColorPattern.IsMatch(content),
                $"Raw color value found outside tokens.css: {Path.GetFileName(file)}");
        }
    }

    [Fact]
    public void Consumer_stylesheets_do_not_depend_on_primitive_color_tokens()
    {
        string cssDirectory = GetCssDirectory();

        string[] consumerFiles = Directory
            .EnumerateFiles(
                cssDirectory,
                "*.css",
                SearchOption.TopDirectoryOnly)
            .Where(path => !string.Equals(
                Path.GetFileName(path),
                "tokens.css",
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(consumerFiles);

        foreach (string file in consumerFiles)
        {
            string content = File.ReadAllText(file);

            Assert.DoesNotContain(
                "--primitive-",
                content,
                StringComparison.Ordinal);
        }
    }
    [Fact]
    public void Component_stylesheet_uses_semantic_interaction_tokens()
    {
        string componentPath = Path.Combine(
            GetCssDirectory(),
            "components.css");

        string content = File.ReadAllText(componentPath);

        string[] requiredTokens =
        [
            "--accent-primary",
            "--accent-primary-hover",
            "--fg-primary",
            "--fg-secondary",
            "--surface-default",
            "--surface-hover",
            "--border-default",
            "--border-focus",
            "--status-missing-fg",
            "--status-missing-border"
        ];

        foreach (string token in requiredTokens)
        {
            Assert.Contains(
                token,
                content,
                StringComparison.Ordinal);
        }
    }

    // Session 116. --fg-tertiary carries semantic content across both product
    // surfaces — Repository Proof evidence sources, score readouts and category
    // status, and Profile row status, Supported+empty copy and identity
    // context — at 13px or smaller, so WCAG AA 1.4.3 normal-text 4.5:1 applies.
    // In light theme it was mapped to neutral-500 and measured 4.016-4.467
    // against every reachable light background, i.e. it failed everywhere, not
    // only on the two surfaces Session 115 happened to sample. Nothing in the
    // suite protected contrast, so a value could silently regress again.
    //
    // Backgrounds are resolved through the semantic token graph rather than
    // named primitives, so remapping a surface token cannot leave this guard
    // asserting a stale colour.
    [Fact]
    public void Tertiary_text_token_meets_AA_contrast_on_every_reachable_background()
    {
        string tokens = ReadTokens();
        Dictionary<string, string> primitives = ReadPrimitives(tokens);

        // Surfaces that current --fg-tertiary consumers actually sit on.
        // Deliberately bounded, and status surfaces are deliberately absent:
        // every .status-badge--* variant overrides --status-badge-icon together
        // with --status-badge-bg, so --fg-tertiary only ever renders on the
        // base badge background (--surface-subtle).
        string[] surfaces =
        [
            "--bg-canvas",
            "--surface-default",
            "--surface-raised",
            "--surface-subtle"
        ];

        foreach (bool dark in new[] { false, true })
        {
            string theme = dark ? "dark" : "light";
            string foreground = ResolveSemantic(tokens, "--fg-tertiary", primitives, dark);

            foreach (string surface in surfaces)
            {
                string background = ResolveSemantic(tokens, surface, primitives, dark);
                double contrast = ContrastRatio(foreground, background);

                Assert.True(
                    contrast >= 4.5,
                    $"--fg-tertiary ({theme}) {foreground} on {surface} {background} "
                    + $"is {contrast:0.000}:1, below the WCAG AA 4.5:1 required for "
                    + "the semantic text that uses it.");
            }
        }
    }

    // The correction must not be achieved by collapsing tertiary into
    // secondary. This asserts only that the two light values stay distinct —
    // no visual-distance policy is implied, because no canonical design-system
    // contract establishes one.
    [Fact]
    public void Tertiary_text_token_is_not_collapsed_into_secondary_in_light_theme()
    {
        string tokens = ReadTokens();
        Dictionary<string, string> primitives = ReadPrimitives(tokens);

        string tertiary = ResolveSemantic(tokens, "--fg-tertiary", primitives, dark: false);
        string secondary = ResolveSemantic(tokens, "--fg-secondary", primitives, dark: false);

        Assert.True(
            !string.Equals(tertiary, secondary, StringComparison.OrdinalIgnoreCase),
            $"Light --fg-tertiary resolves to {tertiary}, the same value as "
            + "--fg-secondary, so the two text levels have collapsed into one.");
    }

    // Session 117. --fg-muted is the dimmest shared text role. Four consumers
    // were audited individually; two of them are non-exempt semantic text and
    // therefore bind WCAG AA 1.4.3 at 4.5:1:
    //
    //   components.css  .input::placeholder          16px placeholder text,
    //                                                on --surface-default
    //   report.css      .report-disclaimer__version  13px policy-version string,
    //                                                on --bg-canvas
    //
    // The other two are exempt and deliberately impose no contrast requirement
    // here, so that a future value choice is not over-constrained by decoration:
    //
    //   report.css  .report-notes li::before   CSS-generated "—" list marker.
    //               Pure decoration: <ul>/<li> already carries list semantics to
    //               assistive technology and removing the dash loses nothing.
    //   report.css  .report-identity__metadata-icon / __url-external
    //               Four aria-hidden glyphs, each redundant with the visible text
    //               beside it (URL, default branch, primary language). WCAG 1.4.11
    //               covers graphics REQUIRED to understand content; these are not.
    //
    // The exemption is based on CONSUMER SEMANTICS, not on background: the four
    // aria-hidden icons render on --surface-subtle, while the decorative "—"
    // marker renders on --bg-canvas. --surface-subtle is absent from the
    // required set below because no non-exempt consumer renders there;
    // --bg-canvas stays REQUIRED because .report-disclaimer__version is
    // semantic text on it. Exempt consumers inherit the corrected value as a
    // legitimate side effect and impose no requirement of their own.
    [Fact]
    public void Muted_text_token_meets_AA_contrast_for_its_non_exempt_text_consumers()
    {
        string tokens = ReadTokens();
        Dictionary<string, string> primitives = ReadPrimitives(tokens);

        // Backgrounds reachable by a non-exempt --fg-muted text consumer.
        // .repository-input__field[readonly] moves the field onto
        // --surface-subtle, but readonly is only set on submit, when the field
        // is required and non-empty, so no placeholder renders in that state.
        string[] surfaces = ["--surface-default", "--bg-canvas"];

        foreach (bool dark in new[] { false, true })
        {
            string theme = dark ? "dark" : "light";
            string foreground = ResolveSemantic(tokens, "--fg-muted", primitives, dark);

            foreach (string surface in surfaces)
            {
                string background = ResolveSemantic(tokens, surface, primitives, dark);
                double contrast = ContrastRatio(foreground, background);

                Assert.True(
                    contrast >= 4.5,
                    $"--fg-muted ({theme}) {foreground} on {surface} {background} "
                    + $"is {contrast:0.000}:1, below the WCAG AA 4.5:1 required by "
                    + "its non-exempt text consumers (.input::placeholder and "
                    + ".report-disclaimer__version).");
            }
        }
    }

    // Every --fg-muted declaration must be one of the three known theme paths,
    // and the two dark paths must agree. ResolveSemantic already fails on a
    // dark disagreement; this additionally proves no declaration escaped the
    // light/dark attribution altogether.
    [Fact]
    public void Muted_text_token_declares_exactly_the_three_known_theme_paths()
    {
        string tokens = ReadTokens();
        Dictionary<string, string> primitives = ReadPrimitives(tokens);

        int declarations = Regex.Matches(
            tokens,
            @"--fg-muted:\s*var\(--primitive-[a-z0-9-]+\);",
            RegexOptions.CultureInvariant).Count;

        Assert.True(
            declarations == 3,
            $"Expected exactly three --fg-muted declarations (:root, "
            + $"[data-theme=\"dark\"] and the prefers-color-scheme fallback), "
            + $"found {declarations}.");

        // Resolves only if both dark declarations agree.
        string dark = ResolveSemantic(tokens, "--fg-muted", primitives, dark: true);
        string light = ResolveSemantic(tokens, "--fg-muted", primitives, dark: false);

        Assert.NotEqual(light, dark);
    }

    // Muted must stay LESS prominent than tertiary. An equality check alone was
    // insufficient: independent review showed that mapping muted to a value
    // MORE prominent than tertiary — inverting the hierarchy outright — passed
    // every guard. The invariant is directional and is expressed against the
    // same semantic background, so it needs no invented minimum visual
    // distance: muted must clear AA and still contrast less than tertiary.
    // Equality is caught as a side effect, since equal values are not "less".
    [Fact]
    public void Muted_text_token_stays_less_prominent_than_tertiary()
    {
        string tokens = ReadTokens();
        Dictionary<string, string> primitives = ReadPrimitives(tokens);

        string[] surfaces = ["--surface-default", "--bg-canvas"];

        foreach (bool dark in new[] { false, true })
        {
            string theme = dark ? "dark" : "light";
            string muted = ResolveSemantic(tokens, "--fg-muted", primitives, dark);
            string tertiary = ResolveSemantic(tokens, "--fg-tertiary", primitives, dark);

            foreach (string surface in surfaces)
            {
                string background = ResolveSemantic(tokens, surface, primitives, dark);
                double mutedContrast = ContrastRatio(muted, background);
                double tertiaryContrast = ContrastRatio(tertiary, background);

                Assert.True(
                    mutedContrast < tertiaryContrast,
                    $"{theme} --fg-muted {muted} contrasts {mutedContrast:0.000}:1 "
                    + $"on {surface} {background}, which is not less than "
                    + $"--fg-tertiary {tertiary} at {tertiaryContrast:0.000}:1. "
                    + "Muted must remain the less prominent of the two text levels.");

                Assert.True(
                    mutedContrast >= 4.5,
                    $"{theme} --fg-muted {muted} on {surface} {background} is "
                    + $"{mutedContrast:0.000}:1, below WCAG AA 4.5:1.");
            }
        }
    }

    private static string ReadTokens() =>
        File.ReadAllText(Path.Combine(GetCssDirectory(), "tokens.css"));

    private static Dictionary<string, string> ReadPrimitives(string tokens)
    {
        Dictionary<string, string> primitives = new(StringComparer.Ordinal);

        foreach (Match match in Regex.Matches(
            tokens,
            @"--primitive-([a-z0-9-]+):\s*(#[0-9a-fA-F]{6});",
            RegexOptions.CultureInvariant))
        {
            primitives[match.Groups[1].Value] = match.Groups[2].Value;
        }

        Assert.NotEmpty(primitives);

        return primitives;
    }

    // tokens.css declares each semantic token in three places: the bare :root
    // light block, the [data-theme="dark"] override that the theme toggle
    // drives, and the @media (prefers-color-scheme: dark) fallback for visitors
    // who never touched the toggle. Every declaration is attributed to its
    // enclosing block instead of relying on first/last match order, so a
    // regression in the primary dark override cannot slip past unread.
    private static string ResolveSemantic(
        string tokens,
        string name,
        Dictionary<string, string> primitives,
        bool dark)
    {
        (int Index, string Theme)[] blocks =
        [
            (tokens.IndexOf(":root {", StringComparison.Ordinal), "light"),
            (tokens.IndexOf("[data-theme=\"dark\"] {", StringComparison.Ordinal), "dark"),
            (tokens.IndexOf("@media (prefers-color-scheme: dark)", StringComparison.Ordinal), "dark")
        ];

        foreach ((int index, string theme) in blocks)
        {
            Assert.True(index >= 0, $"tokens.css is missing an expected {theme} block.");
        }

        List<string> resolved = [];
        string wanted = dark ? "dark" : "light";

        foreach (Match match in Regex.Matches(
            tokens,
            Regex.Escape(name) + @":\s*var\(--primitive-([a-z0-9-]+)\);",
            RegexOptions.CultureInvariant))
        {
            // The declaration belongs to the nearest block opened before it.
            string theme = blocks
                .Where(block => block.Index < match.Index)
                .OrderByDescending(block => block.Index)
                .First()
                .Theme;

            if (theme != wanted)
            {
                continue;
            }

            string primitive = match.Groups[1].Value;

            Assert.True(
                primitives.ContainsKey(primitive),
                $"{name} resolves to unknown primitive --primitive-{primitive}.");

            resolved.Add(primitives[primitive]);
        }

        Assert.True(
            resolved.Count > 0,
            $"tokens.css declares no {wanted} mapping for {name}.");

        // Both dark declarations describe the same theme, so they must agree.
        Assert.True(
            resolved.Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1,
            $"{name} has conflicting {wanted} mappings: "
            + string.Join(", ", resolved));

        return resolved[0];
    }

    private static double ContrastRatio(string foreground, string background)
    {
        double a = RelativeLuminance(foreground);
        double b = RelativeLuminance(background);

        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    private static double RelativeLuminance(string hex)
    {
        string value = hex.TrimStart('#');

        double Channel(int offset)
        {
            double channel = Convert.ToInt32(value.Substring(offset, 2), 16) / 255.0;

            return channel <= 0.03928
                ? channel / 12.92
                : Math.Pow((channel + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(0)) + (0.7152 * Channel(2)) + (0.0722 * Channel(4));
    }

    private static string GetCssDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(
                directory.FullName,
                "GitHubProof.slnx");

            if (File.Exists(solutionPath))
            {
                string cssDirectory = Path.Combine(
                    directory.FullName,
                    "src",
                    "GitHubProof.Web",
                    "wwwroot",
                    "css");

                if (!Directory.Exists(cssDirectory))
                {
                    throw new DirectoryNotFoundException(
                        $"Web CSS directory was not found: {cssDirectory}");
                }

                return cssDirectory;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repository root containing GitHubProof.slnx.");
    }
}