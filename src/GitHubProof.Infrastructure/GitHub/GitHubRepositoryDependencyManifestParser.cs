using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Infrastructure.GitHub;

// Format normalization only. The parser records what a manifest declares and
// never classifies, groups or interprets a dependency.
internal static class GitHubRepositoryDependencyManifestParser
{
    private const string NpmAliasPrefix = "npm:";

    private static readonly Regex GradleCoordinate = new(
        "[\"']([A-Za-z0-9_.\\-]+:[A-Za-z0-9_.\\-]+(?::[^\"']+)?)[\"']",
        RegexOptions.CultureInvariant);

    private static readonly Regex GemDeclaration = new(
        "^\\s*gem\\s+[\"']([^\"']+)[\"']\\s*(?:,\\s*[\"']([^\"']+)[\"'])?",
        RegexOptions.CultureInvariant | RegexOptions.Multiline);

    private static readonly Regex GemspecDeclaration = new(
        "add(?:_runtime|_development)?_dependency\\s*\\(?\\s*[\"']([^\"']+)[\"']\\s*(?:,\\s*[\"']([^\"']+)[\"'])?",
        RegexOptions.CultureInvariant);

    public static DependencyEcosystem? MatchEcosystem(string path)
    {
        string fileName = path[(path.LastIndexOf('/') + 1)..];

        if (HasExtension(fileName, ".csproj")
            || HasExtension(fileName, ".fsproj")
            || HasExtension(fileName, ".vbproj")
            || IsNamed(fileName, "Directory.Packages.props"))
        {
            return DependencyEcosystem.DotNet;
        }

        if (IsNamed(fileName, "pom.xml")
            || IsNamed(fileName, "build.gradle")
            || IsNamed(fileName, "build.gradle.kts"))
        {
            return DependencyEcosystem.Java;
        }

        if (IsNamed(fileName, "package.json"))
        {
            return DependencyEcosystem.JavaScript;
        }

        if (IsNamed(fileName, "pyproject.toml")
            || IsNamed(fileName, "requirements.txt")
            || (fileName.StartsWith("requirements-", StringComparison.OrdinalIgnoreCase)
                && HasExtension(fileName, ".txt")))
        {
            return DependencyEcosystem.Python;
        }

        if (IsNamed(fileName, "go.mod"))
        {
            return DependencyEcosystem.Go;
        }

        if (IsNamed(fileName, "Cargo.toml"))
        {
            return DependencyEcosystem.Rust;
        }

        if (IsNamed(fileName, "composer.json"))
        {
            return DependencyEcosystem.Php;
        }

        return IsNamed(fileName, "Gemfile") || HasExtension(fileName, ".gemspec")
            ? DependencyEcosystem.Ruby
            : null;
    }

    // Unparseable or unrecognized content yields no declaration and never fails
    // the analysis.
    public static IReadOnlyList<RepositoryDependencyDeclaration> Parse(
        DependencyEcosystem ecosystem,
        string path,
        string content)
    {
        string fileName = path[(path.LastIndexOf('/') + 1)..];

        try
        {
            return ecosystem switch
            {
                DependencyEcosystem.DotNet => ParseMsBuild(path, content),
                DependencyEcosystem.Java => IsNamed(fileName, "pom.xml")
                    ? ParseMaven(path, content)
                    : ParseGradle(path, content),
                DependencyEcosystem.JavaScript => ParseJson(
                    path,
                    content,
                    DependencyEcosystem.JavaScript,
                    ["dependencies", "devDependencies"]),
                DependencyEcosystem.Php => ParseJson(
                    path,
                    content,
                    DependencyEcosystem.Php,
                    ["require", "require-dev"]),
                DependencyEcosystem.Python => IsNamed(fileName, "pyproject.toml")
                    ? ParsePyProject(path, content)
                    : ParseRequirements(path, content),
                DependencyEcosystem.Go => ParseGoModule(path, content),
                DependencyEcosystem.Rust => ParseCargo(path, content),
                DependencyEcosystem.Ruby => IsNamed(fileName, "Gemfile")
                    ? ParseGemfile(path, content)
                    : ParseGemspec(path, content),
                _ => []
            };
        }
        catch (Exception exception) when (
            exception is System.Xml.XmlException
                or JsonException
                or FormatException
                or ArgumentException
                or InvalidOperationException
                or RegexMatchTimeoutException)
        {
            return [];
        }
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParseMsBuild(
        string path,
        string content)
    {
        List<RepositoryDependencyDeclaration> declarations = [];

        foreach (XElement element in XDocument.Parse(content).Descendants())
        {
            if (!string.Equals(element.Name.LocalName, "PackageReference", StringComparison.Ordinal)
                && !string.Equals(element.Name.LocalName, "PackageVersion", StringComparison.Ordinal))
            {
                continue;
            }

            string? packageId = FindAttribute(element, "Include")
                ?? FindAttribute(element, "Update");
            string? version = FindAttribute(element, "Version")
                ?? FindChild(element, "Version");

            Add(declarations, DependencyEcosystem.DotNet, path, packageId, version);
        }

        return declarations;
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParseMaven(
        string path,
        string content)
    {
        List<RepositoryDependencyDeclaration> declarations = [];

        foreach (XElement element in XDocument.Parse(content).Descendants())
        {
            if (!string.Equals(element.Name.LocalName, "dependency", StringComparison.Ordinal))
            {
                continue;
            }

            string? group = FindChild(element, "groupId");
            string? artifact = FindChild(element, "artifactId");

            if (string.IsNullOrWhiteSpace(group) || string.IsNullOrWhiteSpace(artifact))
            {
                continue;
            }

            Add(
                declarations,
                DependencyEcosystem.Java,
                path,
                $"{group.Trim()}:{artifact.Trim()}",
                FindChild(element, "version"));
        }

        return declarations;
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParseGradle(
        string path,
        string content)
    {
        List<RepositoryDependencyDeclaration> declarations = [];

        foreach (string line in ReadLines(content))
        {
            foreach (Match match in GradleCoordinate.Matches(StripComment(line)))
            {
                string[] segments = match.Groups[1].Value.Split(':');

                if (segments.Length is < 2 or > 3)
                {
                    continue;
                }

                Add(
                    declarations,
                    DependencyEcosystem.Java,
                    path,
                    $"{segments[0]}:{segments[1]}",
                    segments.Length == 3 ? segments[2] : null);
            }
        }

        return declarations;
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParseJson(
        string path,
        string content,
        DependencyEcosystem ecosystem,
        IReadOnlyList<string> sections)
    {
        List<RepositoryDependencyDeclaration> declarations = [];
        using JsonDocument document = JsonDocument.Parse(content);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return declarations;
        }

        foreach (string section in sections)
        {
            if (!document.RootElement.TryGetProperty(section, out JsonElement element)
                || element.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (JsonProperty dependency in element.EnumerateObject())
            {
                string? declaredVersion = dependency.Value.ValueKind == JsonValueKind.String
                    ? dependency.Value.GetString()
                    : null;

                // An npm alias declares the referenced package inside the value;
                // the manifest key is only the local name. An alias that cannot
                // be resolved deterministically is omitted rather than recorded
                // under a false identity.
                if (ecosystem == DependencyEcosystem.JavaScript
                    && declaredVersion is not null
                    && declaredVersion.StartsWith(NpmAliasPrefix, StringComparison.Ordinal))
                {
                    if (TryReadNpmAlias(
                            declaredVersion,
                            out string? aliasPackageId,
                            out string? aliasVersion))
                    {
                        Add(declarations, ecosystem, path, aliasPackageId, aliasVersion);
                    }

                    continue;
                }

                Add(declarations, ecosystem, path, dependency.Name, declaredVersion);
            }
        }

        return declarations;
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParsePyProject(
        string path,
        string content)
    {
        List<RepositoryDependencyDeclaration> declarations = [];

        foreach (TomlEntry entry in TomlSubsetReader.Read(content))
        {
            if (string.Equals(entry.Section, "project", StringComparison.Ordinal)
                && string.Equals(entry.Key, "dependencies", StringComparison.Ordinal))
            {
                foreach (string requirement in entry.ArrayValues)
                {
                    AddRequirement(declarations, path, requirement);
                }

                continue;
            }

            if (string.Equals(
                    entry.Section,
                    "tool.poetry.dependencies",
                    StringComparison.Ordinal))
            {
                Add(declarations, DependencyEcosystem.Python, path, entry.Key, entry.Value);
            }
        }

        return declarations;
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParseRequirements(
        string path,
        string content)
    {
        List<RepositoryDependencyDeclaration> declarations = [];

        foreach (string line in ReadLines(content))
        {
            string requirement = StripComment(line).Trim();

            if (requirement.Length == 0 || requirement.StartsWith('-'))
            {
                continue;
            }

            AddRequirement(declarations, path, requirement);
        }

        return declarations;
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParseGoModule(
        string path,
        string content)
    {
        List<RepositoryDependencyDeclaration> declarations = [];
        bool inBlock = false;

        foreach (string rawLine in ReadLines(content))
        {
            string line = StripComment(rawLine).Trim();

            if (line.Length == 0)
            {
                continue;
            }

            if (inBlock)
            {
                if (line.StartsWith(')'))
                {
                    inBlock = false;
                    continue;
                }

                AddGoRequirement(declarations, path, line);
                continue;
            }

            if (!line.StartsWith("require", StringComparison.Ordinal))
            {
                continue;
            }

            string remainder = line["require".Length..].Trim();

            if (remainder.StartsWith('('))
            {
                inBlock = true;
                continue;
            }

            AddGoRequirement(declarations, path, remainder);
        }

        return declarations;
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParseCargo(
        string path,
        string content)
    {
        List<RepositoryDependencyDeclaration> declarations = [];

        foreach (TomlEntry entry in TomlSubsetReader.Read(content))
        {
            if (entry.Section is not ("dependencies"
                or "dev-dependencies"
                or "build-dependencies"))
            {
                continue;
            }

            // A renamed dependency declares the referenced crate in `package`;
            // the table key is only the local name. When the rename is present
            // but unreadable, the declaration is omitted rather than recorded
            // under a false identity.
            if (entry.HasPackageOverride)
            {
                if (!string.IsNullOrWhiteSpace(entry.PackageOverride))
                {
                    Add(
                        declarations,
                        DependencyEcosystem.Rust,
                        path,
                        entry.PackageOverride,
                        entry.Value);
                }

                continue;
            }

            Add(declarations, DependencyEcosystem.Rust, path, entry.Key, entry.Value);
        }

        return declarations;
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParseGemfile(
        string path,
        string content)
    {
        List<RepositoryDependencyDeclaration> declarations = [];

        foreach (Match match in GemDeclaration.Matches(content))
        {
            Add(
                declarations,
                DependencyEcosystem.Ruby,
                path,
                match.Groups[1].Value,
                match.Groups[2].Success ? match.Groups[2].Value : null);
        }

        return declarations;
    }

    private static IReadOnlyList<RepositoryDependencyDeclaration> ParseGemspec(
        string path,
        string content)
    {
        List<RepositoryDependencyDeclaration> declarations = [];

        foreach (Match match in GemspecDeclaration.Matches(content))
        {
            Add(
                declarations,
                DependencyEcosystem.Ruby,
                path,
                match.Groups[1].Value,
                match.Groups[2].Success ? match.Groups[2].Value : null);
        }

        return declarations;
    }

    private static void AddGoRequirement(
        List<RepositoryDependencyDeclaration> declarations,
        string path,
        string line)
    {
        string[] segments = line.Split(
            [' ', '\t'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0)
        {
            return;
        }

        Add(
            declarations,
            DependencyEcosystem.Go,
            path,
            segments[0],
            segments.Length > 1 ? segments[1] : null);
    }

    // PEP 508: identity is the leading requirement name; extras and environment
    // markers are not part of the declared version literal.
    private static void AddRequirement(
        List<RepositoryDependencyDeclaration> declarations,
        string path,
        string requirement)
    {
        string value = requirement.Trim();
        int nameLength = 0;

        while (nameLength < value.Length
            && (char.IsLetterOrDigit(value[nameLength])
                || value[nameLength] is '.' or '_' or '-'))
        {
            nameLength++;
        }

        if (nameLength == 0)
        {
            return;
        }

        string name = value[..nameLength];
        string remainder = value[nameLength..].Trim();

        if (remainder.StartsWith('['))
        {
            int close = remainder.IndexOf(']');
            remainder = close < 0 ? string.Empty : remainder[(close + 1)..].Trim();
        }

        int marker = remainder.IndexOf(';');

        if (marker >= 0)
        {
            remainder = remainder[..marker].Trim();
        }

        Add(declarations, DependencyEcosystem.Python, path, name, remainder);
    }

    // npm alias syntax is `npm:<name>[@<range>]`, where the name may be scoped.
    // The last `@` after the first character separates the range, so a scoped
    // name keeps its leading `@`.
    private static bool TryReadNpmAlias(
        string value,
        out string? packageId,
        out string? declaredVersion)
    {
        packageId = null;
        declaredVersion = null;

        string remainder = value[NpmAliasPrefix.Length..].Trim();

        if (remainder.Length == 0)
        {
            return false;
        }

        int separator = remainder.LastIndexOf('@');

        if (separator > 0)
        {
            packageId = remainder[..separator].Trim();
            string range = remainder[(separator + 1)..].Trim();
            declaredVersion = range.Length == 0 ? null : range;
        }
        else
        {
            packageId = remainder;
        }

        if (string.IsNullOrWhiteSpace(packageId)
            || string.Equals(packageId, "@", StringComparison.Ordinal)
            || packageId.EndsWith('/'))
        {
            packageId = null;
            declaredVersion = null;
            return false;
        }

        return true;
    }

    private static void Add(
        List<RepositoryDependencyDeclaration> declarations,
        DependencyEcosystem ecosystem,
        string path,
        string? packageId,
        string? declaredVersion)
    {
        if (string.IsNullOrWhiteSpace(packageId))
        {
            return;
        }

        declarations.Add(new RepositoryDependencyDeclaration(
            ecosystem,
            path,
            packageId.Trim(),
            declaredVersion));
    }

    private static string? FindAttribute(XElement element, string name) =>
        element.Attributes()
            .FirstOrDefault(attribute => string.Equals(
                attribute.Name.LocalName,
                name,
                StringComparison.Ordinal))?
            .Value;

    private static string? FindChild(XElement element, string name) =>
        element.Elements()
            .FirstOrDefault(child => string.Equals(
                child.Name.LocalName,
                name,
                StringComparison.Ordinal))?
            .Value;

    private static IEnumerable<string> ReadLines(string content) =>
        content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

    private static string StripComment(string line)
    {
        int comment = line.IndexOfAny(['#']);

        if (comment >= 0)
        {
            return line[..comment];
        }

        int inline = line.IndexOf("//", StringComparison.Ordinal);

        return inline >= 0 ? line[..inline] : line;
    }

    private static bool IsNamed(string fileName, string expected) =>
        string.Equals(fileName, expected, StringComparison.OrdinalIgnoreCase);

    private static bool HasExtension(string fileName, string extension) =>
        fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
}

internal sealed record TomlEntry(
    string Section,
    string Key,
    string? Value,
    IReadOnlyList<string> ArrayValues,
    bool HasPackageOverride = false,
    string? PackageOverride = null);

// Bounded TOML subset: section headers, string values, inline tables read for
// their version member, and single or multi line string arrays. Anything else
// is ignored rather than guessed.
internal static class TomlSubsetReader
{
    public static IReadOnlyList<TomlEntry> Read(string content)
    {
        List<TomlEntry> entries = [];
        string section = string.Empty;
        string? arrayKey = null;
        List<string> arrayValues = [];

        foreach (string rawLine in content.Split(['\r', '\n']))
        {
            string line = StripComment(rawLine).Trim();

            if (line.Length == 0)
            {
                continue;
            }

            if (arrayKey is not null)
            {
                arrayValues.AddRange(ReadQuotedValues(line));

                if (line.Contains(']', StringComparison.Ordinal))
                {
                    entries.Add(new TomlEntry(section, arrayKey, null, arrayValues));
                    arrayKey = null;
                    arrayValues = [];
                }

                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                section = line.Trim('[', ']').Trim();
                continue;
            }

            int separator = line.IndexOf('=', StringComparison.Ordinal);

            if (separator <= 0)
            {
                continue;
            }

            string key = line[..separator].Trim().Trim('"', '\'');
            string value = line[(separator + 1)..].Trim();

            if (key.Length == 0)
            {
                continue;
            }

            if (value.StartsWith('['))
            {
                List<string> values = [.. ReadQuotedValues(value)];

                if (value.Contains(']', StringComparison.Ordinal))
                {
                    entries.Add(new TomlEntry(section, key, null, values));
                    continue;
                }

                arrayKey = key;
                arrayValues = values;
                continue;
            }

            if (!value.StartsWith('{'))
            {
                entries.Add(new TomlEntry(
                    section,
                    key,
                    ReadQuotedValues(value).FirstOrDefault(),
                    []));
                continue;
            }

            IReadOnlyList<(string Key, string Value)> members = ReadInlineTable(value);

            entries.Add(new TomlEntry(
                section,
                key,
                FindMember(members, "version"),
                [],
                members.Any(member => string.Equals(
                    member.Key,
                    "package",
                    StringComparison.Ordinal)),
                FindMember(members, "package")));
        }

        return entries;
    }

    private static string? FindMember(
        IReadOnlyList<(string Key, string Value)> members,
        string name)
    {
        foreach ((string key, string value) in members)
        {
            if (string.Equals(key, name, StringComparison.Ordinal))
            {
                return ReadQuotedValues(value).FirstOrDefault();
            }
        }

        return null;
    }

    // Splits an inline table on top-level commas so a member value containing a
    // comma, bracket or quote cannot be mistaken for another member.
    private static IReadOnlyList<(string Key, string Value)> ReadInlineTable(string value)
    {
        string body = value.Trim();
        body = body[1..];

        int close = body.LastIndexOf('}');

        if (close >= 0)
        {
            body = body[..close];
        }

        List<(string Key, string Value)> members = [];
        int depth = 0;
        bool inside = false;
        char quote = '"';
        int start = 0;

        for (int index = 0; index <= body.Length; index++)
        {
            if (index == body.Length)
            {
                AddMember(members, body[start..]);
                break;
            }

            char current = body[index];

            if (inside)
            {
                if (current == quote)
                {
                    inside = false;
                }

                continue;
            }

            switch (current)
            {
                case '"':
                case '\'':
                    inside = true;
                    quote = current;
                    break;

                case '[':
                case '{':
                    depth++;
                    break;

                case ']':
                case '}':
                    depth--;
                    break;

                case ',' when depth == 0:
                    AddMember(members, body[start..index]);
                    start = index + 1;
                    break;
            }
        }

        return members;
    }

    private static void AddMember(
        List<(string Key, string Value)> members,
        string member)
    {
        int separator = member.IndexOf('=', StringComparison.Ordinal);

        if (separator <= 0)
        {
            return;
        }

        members.Add((
            member[..separator].Trim().Trim('"', '\''),
            member[(separator + 1)..].Trim()));
    }

    private static IEnumerable<string> ReadQuotedValues(string line)
    {
        bool inside = false;
        char quote = '"';
        int start = 0;

        for (int index = 0; index < line.Length; index++)
        {
            char current = line[index];

            if (!inside && current is '"' or '\'')
            {
                inside = true;
                quote = current;
                start = index + 1;
                continue;
            }

            if (inside && current == quote)
            {
                inside = false;
                yield return line[start..index];
            }
        }
    }

    private static string StripComment(string line)
    {
        bool inside = false;
        char quote = '"';

        for (int index = 0; index < line.Length; index++)
        {
            char current = line[index];

            if (!inside && current is '"' or '\'')
            {
                inside = true;
                quote = current;
                continue;
            }

            if (inside && current == quote)
            {
                inside = false;
                continue;
            }

            if (!inside && current == '#')
            {
                return line[..index];
            }
        }

        return line;
    }
}
