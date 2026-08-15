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