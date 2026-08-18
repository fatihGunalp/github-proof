using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis;

// The lexical zone of one source offset. Comment and String are kept apart so a
// consumer can tell a real string literal from a quote that merely happens to
// sit inside a comment.
internal enum SourceLexicalZone : byte
{
    Code = 0,
    Comment = 1,
    String = 2
}

// The result of one artefact scan.
public sealed class SourceLexicalScan
{
    internal SourceLexicalScan(SourceLexicalZone[] zones, bool isDesynchronized)
    {
        this.zones = zones;
        IsDesynchronized = isDesynchronized;
    }

    private readonly SourceLexicalZone[] zones;

    // docs/architecture/repository-dependency-source-reference-detection.md
    // §6.1: the artefact reached end of file while the scanner was still inside
    // an unterminated modeled string or block comment. Every observation from
    // such an artefact is discarded and detection becomes incomplete. A partial
    // or best-effort observation is never returned.
    public bool IsDesynchronized { get; }

    // True only when the offset lies in source code, outside every comment and
    // every string literal.
    public bool IsCodeAt(int offset) =>
        offset >= 0 && offset < zones.Length && zones[offset] == SourceLexicalZone.Code;

    // True only at the opening delimiter of a real string literal. A quote
    // inside a comment is never a string start, so a commented import entry can
    // never be mistaken for a real one.
    public bool IsStringStartAt(int offset)
    {
        if (offset < 0 || offset >= zones.Length || zones[offset] != SourceLexicalZone.String)
        {
            return false;
        }

        return offset == 0 || zones[offset - 1] != SourceLexicalZone.String;
    }
}

// Syntax-aware comment and string exclusion for the five supported ecosystems,
// as required by
// docs/architecture/repository-dependency-source-reference-detection.md §6.
//
// A line-level regular expression is deliberately not used: Session 072
// measured that a line-level detector matches import-shaped text inside doc
// comments, docstrings and string literals.
//
// The scanner never logs, returns or embeds source content. It produces only a
// per-offset lexical zone and a desynchronization flag.
public static class SourceLexicalScanner
{
    public static SourceLexicalScan Scan(string content, DependencyEcosystem ecosystem)
    {
        ArgumentNullException.ThrowIfNull(content);

        SourceLexicalZone[] zones = new SourceLexicalZone[content.Length];

        bool desynchronized = ecosystem switch
        {
            DependencyEcosystem.Go => ScanCLike(content, zones, GoRules),
            DependencyEcosystem.Rust => ScanCLike(content, zones, RustRules),
            DependencyEcosystem.DotNet => ScanCLike(content, zones, DotNetRules),
            DependencyEcosystem.JavaScript => ScanCLike(content, zones, JavaScriptRules),
            DependencyEcosystem.Python => ScanPython(content, zones),

            // Java, Php and Ruby are outside the supported surface and are never
            // scanned. Nothing is classified, so nothing can be observed.
            _ => false
        };

        return new SourceLexicalScan(zones, desynchronized);
    }

    private sealed record LexicalRules(
        bool NestedBlockComments,
        bool BacktickRawString,
        bool TemplateLiteral,
        bool RegexLiteral,
        bool VerbatimString,
        bool RawQuotedString,
        bool HashRawString,
        bool CharLiteralNeedsLookahead);

    // `/* */` does not nest in Go; backtick strings are raw.
    private static readonly LexicalRules GoRules = new(
        NestedBlockComments: false,
        BacktickRawString: true,
        TemplateLiteral: false,
        RegexLiteral: false,
        VerbatimString: false,
        RawQuotedString: false,
        HashRawString: false,
        CharLiteralNeedsLookahead: false);

    // Rust nests block comments and carries r#"..."# raw strings. A leading
    // quote may also be a lifetime, so char literals need lookahead.
    private static readonly LexicalRules RustRules = new(
        NestedBlockComments: true,
        BacktickRawString: false,
        TemplateLiteral: false,
        RegexLiteral: false,
        VerbatimString: false,
        RawQuotedString: false,
        HashRawString: true,
        CharLiteralNeedsLookahead: true);

    // C# carries @"..." verbatim strings and """ raw strings.
    private static readonly LexicalRules DotNetRules = new(
        NestedBlockComments: false,
        BacktickRawString: false,
        TemplateLiteral: false,
        RegexLiteral: false,
        VerbatimString: true,
        RawQuotedString: true,
        HashRawString: false,
        CharLiteralNeedsLookahead: false);

    // JavaScript carries template literals with interpolation and regular
    // expression literals.
    private static readonly LexicalRules JavaScriptRules = new(
        NestedBlockComments: false,
        BacktickRawString: false,
        TemplateLiteral: true,
        RegexLiteral: true,
        VerbatimString: false,
        RawQuotedString: false,
        HashRawString: false,
        CharLiteralNeedsLookahead: false);

    // Keywords after which a `/` starts a regular expression rather than a
    // division. Anything else word-like is value-like and implies division.
    private static readonly IReadOnlySet<string> JavaScriptRegexKeywords =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "return", "typeof", "instanceof", "in", "of", "new", "delete",
            "void", "throw", "case", "do", "else", "yield", "await"
        };

    private static bool ScanCLike(string content, SourceLexicalZone[] zones, LexicalRules rules)
    {
        int index = 0;
        int length = content.Length;
        bool valueLike = false;

        while (index < length)
        {
            char current = content[index];

            if (current == '/' && index + 1 < length && content[index + 1] == '/')
            {
                // A line comment terminates legitimately at end of file.
                index = Mark(zones, content, index, FindLineEnd(content, index),
                    SourceLexicalZone.Comment);

                continue;
            }

            if (current == '/' && index + 1 < length && content[index + 1] == '*')
            {
                int end = FindBlockCommentEnd(content, index, rules.NestedBlockComments);

                if (end < 0)
                {
                    Mark(zones, content, index, length, SourceLexicalZone.Comment);
                    return true;
                }

                index = Mark(zones, content, index, end, SourceLexicalZone.Comment);
                valueLike = false;
                continue;
            }

            if (rules.VerbatimString && current == '@' && index + 1 < length
                && content[index + 1] == '"')
            {
                int end = FindVerbatimStringEnd(content, index + 2);

                if (end < 0)
                {
                    Mark(zones, content, index, length, SourceLexicalZone.String);
                    return true;
                }

                index = Mark(zones, content, index, end, SourceLexicalZone.String);
                valueLike = true;
                continue;
            }

            if (rules.RawQuotedString && current == '"' && IsRepeated(content, index, '"', 3))
            {
                int end = FindRawQuotedStringEnd(content, index);

                if (end < 0)
                {
                    Mark(zones, content, index, length, SourceLexicalZone.String);
                    return true;
                }

                index = Mark(zones, content, index, end, SourceLexicalZone.String);
                valueLike = true;
                continue;
            }

            if (rules.HashRawString && IsHashRawStringStart(content, index, out int hashes))
            {
                int end = FindHashRawStringEnd(content, index, hashes);

                if (end < 0)
                {
                    Mark(zones, content, index, length, SourceLexicalZone.String);
                    return true;
                }

                index = Mark(zones, content, index, end, SourceLexicalZone.String);
                valueLike = true;
                continue;
            }

            if (rules.BacktickRawString && current == '`')
            {
                int end = FindRawDelimitedEnd(content, index + 1, '`');

                if (end < 0)
                {
                    Mark(zones, content, index, length, SourceLexicalZone.String);
                    return true;
                }

                index = Mark(zones, content, index, end, SourceLexicalZone.String);
                valueLike = true;
                continue;
            }

            if (rules.TemplateLiteral && current == '`')
            {
                // The literal text is a string; an interpolation hole is code.
                zones[index] = SourceLexicalZone.String;
                index++;

                if (!ScanTemplateLiteral(content, zones, rules, ref index))
                {
                    return true;
                }

                valueLike = true;
                continue;
            }

            if (current == '"')
            {
                int end = FindQuotedEnd(content, index + 1, '"');

                index = Mark(zones, content, index, end, SourceLexicalZone.String);
                valueLike = true;
                continue;
            }

            if (current == '\'')
            {
                if (rules.CharLiteralNeedsLookahead && !LooksLikeCharLiteral(content, index))
                {
                    // A Rust lifetime such as `'a` is code, not a literal.
                    index++;
                    valueLike = true;
                    continue;
                }

                int end = FindQuotedEnd(content, index + 1, '\'');

                index = Mark(zones, content, index, end, SourceLexicalZone.String);
                valueLike = true;
                continue;
            }

            if (rules.RegexLiteral && current == '/' && !valueLike)
            {
                int end = FindRegexEnd(content, index);

                if (end > 0)
                {
                    index = Mark(zones, content, index, end, SourceLexicalZone.String);
                    valueLike = true;
                    continue;
                }
            }

            if (!char.IsWhiteSpace(current))
            {
                valueLike = IsValueLike(content, index, rules, current);
            }

            index++;
        }

        return false;
    }

    private static bool IsValueLike(
        string content,
        int index,
        LexicalRules rules,
        char current)
    {
        if (char.IsLetterOrDigit(current) || current is '_' or '$')
        {
            if (!rules.RegexLiteral)
            {
                return true;
            }

            // A regex-allowing keyword is not value-like.
            int start = index;

            while (start > 0 && (char.IsLetterOrDigit(content[start - 1])
                || content[start - 1] is '_' or '$'))
            {
                start--;
            }

            int end = index;

            while (end + 1 < content.Length && (char.IsLetterOrDigit(content[end + 1])
                || content[end + 1] is '_' or '$'))
            {
                end++;
            }

            return !JavaScriptRegexKeywords.Contains(content[start..(end + 1)]);
        }

        return current is ')' or ']';
    }

    // Scans a JavaScript template literal from just after the opening backtick.
    // Returns false when end of file is reached while still inside it.
    private static bool ScanTemplateLiteral(
        string content,
        SourceLexicalZone[] zones,
        LexicalRules rules,
        ref int index)
    {
        int length = content.Length;

        while (index < length)
        {
            char current = content[index];

            if (current == '\\' && index + 1 < length)
            {
                zones[index] = SourceLexicalZone.String;
                zones[index + 1] = SourceLexicalZone.String;
                index += 2;
                continue;
            }

            if (current == '`')
            {
                zones[index] = SourceLexicalZone.String;
                index++;
                return true;
            }

            if (current == '$' && index + 1 < length && content[index + 1] == '{')
            {
                zones[index] = SourceLexicalZone.String;
                zones[index + 1] = SourceLexicalZone.String;
                index += 2;

                if (!ScanTemplateHole(content, zones, rules, ref index))
                {
                    return false;
                }

                continue;
            }

            zones[index] = SourceLexicalZone.String;
            index++;
        }

        return false;
    }

    // An interpolation hole is ordinary code, so it is scanned recursively and
    // left classified as code.
    private static bool ScanTemplateHole(
        string content,
        SourceLexicalZone[] zones,
        LexicalRules rules,
        ref int index)
    {
        int length = content.Length;
        int depth = 0;

        while (index < length)
        {
            char current = content[index];

            if (current == '}' && depth == 0)
            {
                zones[index] = SourceLexicalZone.String;
                index++;
                return true;
            }

            if (current == '{')
            {
                depth++;
                index++;
                continue;
            }

            if (current == '}')
            {
                depth--;
                index++;
                continue;
            }

            if (current == '`')
            {
                zones[index] = SourceLexicalZone.String;
                index++;

                if (!ScanTemplateLiteral(content, zones, rules, ref index))
                {
                    return false;
                }

                continue;
            }

            if (current is '"' or '\'')
            {
                int end = FindQuotedEnd(content, index + 1, current);

                index = Mark(zones, content, index, end, SourceLexicalZone.String);
                continue;
            }

            if (current == '/' && index + 1 < length && content[index + 1] == '/')
            {
                index = Mark(zones, content, index, FindLineEnd(content, index),
                    SourceLexicalZone.Comment);

                continue;
            }

            if (current == '/' && index + 1 < length && content[index + 1] == '*')
            {
                int end = FindBlockCommentEnd(content, index, nested: false);

                if (end < 0)
                {
                    Mark(zones, content, index, length, SourceLexicalZone.Comment);
                    return false;
                }

                index = Mark(zones, content, index, end, SourceLexicalZone.Comment);
                continue;
            }

            index++;
        }

        return false;
    }

    private static bool ScanPython(string content, SourceLexicalZone[] zones)
    {
        int index = 0;
        int length = content.Length;

        while (index < length)
        {
            char current = content[index];

            if (current == '#')
            {
                index = Mark(zones, content, index, FindLineEnd(content, index),
                    SourceLexicalZone.Comment);

                continue;
            }

            if (current is '"' or '\'')
            {
                bool raw = HasPythonRawPrefix(content, index);
                bool triple = IsRepeated(content, index, current, 3);

                if (triple)
                {
                    int tripleEnd = FindPythonTripleEnd(content, index + 3, current, raw);

                    if (tripleEnd < 0)
                    {
                        Mark(zones, content, index, length, SourceLexicalZone.String);
                        return true;
                    }

                    index = Mark(zones, content, index, tripleEnd, SourceLexicalZone.String);
                    continue;
                }

                // A single-quoted Python literal cannot span a newline. An
                // unterminated one ends at the newline exactly as in the C-like
                // scanner, so it can never swallow the rest of the artefact and
                // is not a desynchronization.
                int end = FindPythonSingleEnd(content, index + 1, current, raw);

                index = Mark(zones, content, index, end, SourceLexicalZone.String);
                continue;
            }

            index++;
        }

        return false;
    }

    private static bool HasPythonRawPrefix(string content, int quote)
    {
        int start = quote;

        while (start > 0 && char.IsLetter(content[start - 1]))
        {
            start--;
        }

        for (int index = start; index < quote; index++)
        {
            if (content[index] is 'r' or 'R')
            {
                return true;
            }
        }

        return false;
    }

    private static int FindPythonSingleEnd(string content, int start, char quote, bool raw)
    {
        for (int index = start; index < content.Length; index++)
        {
            char current = content[index];

            if (!raw && current == '\\')
            {
                index++;
                continue;
            }

            if (current == '\n')
            {
                return index;
            }

            if (current == quote)
            {
                return index + 1;
            }
        }

        return content.Length;
    }

    private static int FindPythonTripleEnd(string content, int start, char quote, bool raw)
    {
        for (int index = start; index < content.Length; index++)
        {
            char current = content[index];

            if (!raw && current == '\\')
            {
                index++;
                continue;
            }

            if (current == quote && IsRepeated(content, index, quote, 3))
            {
                return index + 3;
            }
        }

        return -1;
    }

    private static bool LooksLikeCharLiteral(string content, int index)
    {
        int limit = Math.Min(content.Length, index + 8);

        for (int scan = index + 1; scan < limit; scan++)
        {
            if (content[scan] == '\n')
            {
                return false;
            }

            if (content[scan] == '\\')
            {
                scan++;
                continue;
            }

            if (content[scan] == '\'')
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsHashRawStringStart(string content, int index, out int hashes)
    {
        hashes = 0;

        int scan = index;

        if (scan < content.Length && content[scan] is 'b')
        {
            scan++;
        }

        if (scan >= content.Length || content[scan] is not 'r')
        {
            return false;
        }

        // A preceding identifier character means this is part of a longer name.
        if (index > 0 && (char.IsLetterOrDigit(content[index - 1]) || content[index - 1] == '_'))
        {
            return false;
        }

        scan++;

        while (scan < content.Length && content[scan] == '#')
        {
            hashes++;
            scan++;
        }

        return scan < content.Length && content[scan] == '"';
    }

    private static int FindHashRawStringEnd(string content, int index, int hashes)
    {
        int scan = content.IndexOf('"', index) + 1;

        while (scan < content.Length)
        {
            int quote = content.IndexOf('"', scan);

            if (quote < 0)
            {
                return -1;
            }

            int closing = quote + 1;
            int matched = 0;

            while (closing < content.Length && matched < hashes && content[closing] == '#')
            {
                matched++;
                closing++;
            }

            if (matched == hashes)
            {
                return closing;
            }

            scan = quote + 1;
        }

        return -1;
    }

    private static int FindRawQuotedStringEnd(string content, int index)
    {
        int opening = 0;

        while (index + opening < content.Length && content[index + opening] == '"')
        {
            opening++;
        }

        int scan = index + opening;

        while (scan < content.Length)
        {
            int quote = content.IndexOf('"', scan);

            if (quote < 0)
            {
                return -1;
            }

            int run = 0;

            while (quote + run < content.Length && content[quote + run] == '"')
            {
                run++;
            }

            if (run >= opening)
            {
                return quote + run;
            }

            scan = quote + run;
        }

        return -1;
    }

    private static int FindVerbatimStringEnd(string content, int start)
    {
        for (int index = start; index < content.Length; index++)
        {
            if (content[index] != '"')
            {
                continue;
            }

            if (index + 1 < content.Length && content[index + 1] == '"')
            {
                index++;
                continue;
            }

            return index + 1;
        }

        return -1;
    }

    private static int FindRawDelimitedEnd(string content, int start, char delimiter)
    {
        int end = content.IndexOf(delimiter, start);

        return end < 0 ? -1 : end + 1;
    }

    // A single-line literal cannot span a newline. An unterminated one ends at
    // the newline so the scanner can never run away, and that is deliberately
    // not a desynchronization.
    private static int FindQuotedEnd(string content, int start, char quote)
    {
        for (int index = start; index < content.Length; index++)
        {
            char current = content[index];

            if (current == '\\')
            {
                index++;
                continue;
            }

            if (current == '\n')
            {
                return index;
            }

            if (current == quote)
            {
                return index + 1;
            }
        }

        return content.Length;
    }

    // A regular expression literal is bounded to one line: when no closing
    // delimiter is found before the newline the `/` was division after all.
    private static int FindRegexEnd(string content, int index)
    {
        bool inClass = false;

        for (int scan = index + 1; scan < content.Length; scan++)
        {
            char current = content[scan];

            if (current == '\\')
            {
                scan++;
                continue;
            }

            if (current == '\n')
            {
                return -1;
            }

            if (current == '[')
            {
                inClass = true;
                continue;
            }

            if (current == ']')
            {
                inClass = false;
                continue;
            }

            if (current == '/' && !inClass)
            {
                int end = scan + 1;

                while (end < content.Length && char.IsLetter(content[end]))
                {
                    end++;
                }

                return end;
            }
        }

        return -1;
    }

    private static int FindBlockCommentEnd(string content, int index, bool nested)
    {
        int depth = 1;
        int scan = index + 2;

        while (scan < content.Length)
        {
            if (nested && content[scan] == '/' && scan + 1 < content.Length
                && content[scan + 1] == '*')
            {
                depth++;
                scan += 2;
                continue;
            }

            if (content[scan] == '*' && scan + 1 < content.Length && content[scan + 1] == '/')
            {
                depth--;
                scan += 2;

                if (depth == 0)
                {
                    return scan;
                }

                continue;
            }

            scan++;
        }

        return -1;
    }

    private static int FindLineEnd(string content, int index)
    {
        int end = content.IndexOf('\n', index);

        return end < 0 ? content.Length : end;
    }

    private static bool IsRepeated(string content, int index, char value, int count)
    {
        if (index + count > content.Length)
        {
            return false;
        }

        for (int offset = 0; offset < count; offset++)
        {
            if (content[index + offset] != value)
            {
                return false;
            }
        }

        return true;
    }

    private static int Mark(
        SourceLexicalZone[] zones,
        string content,
        int start,
        int end,
        SourceLexicalZone zone)
    {
        int limit = Math.Min(end, content.Length);

        for (int index = start; index < limit; index++)
        {
            zones[index] = zone;
        }

        return limit > start ? limit : start + 1;
    }
}
