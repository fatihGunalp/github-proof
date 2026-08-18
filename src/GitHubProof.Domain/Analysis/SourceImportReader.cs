using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis;

// One qualifying import declaration read from a collected source artefact.
// Identity is the raw source-visible reference identity; it is resolved against
// declarations separately and never rewritten back into a declaration.
public sealed record SourceImportCandidate(
    SourceReferenceForm Form,
    string Identity,
    int Line);

// Reads exactly the qualifying import-declaration forms locked by
// docs/architecture/repository-dependency-source-reference-detection.md §5 and
// §7. Every structural token must lie in code according to the supplied lexical
// scan, so a form inside a comment or a string literal is never read.
//
// The reader is hand-written rather than regular-expression driven: repository
// source is untrusted input, and the contract forbids both clock dependence and
// a line-level pattern that can see inside comments and strings.
public static class SourceImportReader
{
    public static IReadOnlyList<SourceImportCandidate> Read(
        string content,
        DependencyEcosystem ecosystem,
        SourceLexicalScan scan)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(scan);

        List<SourceImportCandidate> candidates = [];
        LineIndex lines = new(content);

        switch (ecosystem)
        {
            case DependencyEcosystem.Go:
                ReadGo(content, scan, lines, candidates);
                break;

            case DependencyEcosystem.JavaScript:
                ReadJavaScript(content, scan, lines, candidates);
                break;

            case DependencyEcosystem.Python:
                ReadPython(content, scan, lines, candidates);
                break;

            case DependencyEcosystem.Rust:
                ReadRust(content, scan, lines, candidates);
                break;

            case DependencyEcosystem.DotNet:
                ReadDotNet(content, scan, lines, candidates);
                break;

            default:
                // Java, Php and Ruby are outside the supported V1 surface.
                break;
        }

        return candidates;
    }

    // ---------------------------------------------------------------- Go ----

    private static void ReadGo(
        string content,
        SourceLexicalScan scan,
        LineIndex lines,
        List<SourceImportCandidate> candidates)
    {
        int index = 0;

        while (TryFindKeyword(content, scan, "import", index, out int keyword))
        {
            int cursor = SkipInlineWhitespace(content, keyword + "import".Length);

            if (cursor < content.Length && content[cursor] == '(')
            {
                index = ReadGoBlock(content, scan, lines, candidates, cursor + 1);
                continue;
            }

            cursor = SkipGoImportAlias(content, cursor);

            if (scan.IsStringStartAt(cursor)
                && TryReadStringLiteral(content, cursor, out string? path, out int end)
                && IsUsableIdentity(path))
            {
                candidates.Add(new SourceImportCandidate(
                    SourceReferenceForm.Import,
                    path,
                    lines.LineAt(keyword)));

                index = end;
                continue;
            }

            index = keyword + "import".Length;
        }
    }

    private static int ReadGoBlock(
        string content,
        SourceLexicalScan scan,
        LineIndex lines,
        List<SourceImportCandidate> candidates,
        int start)
    {
        int cursor = start;

        while (cursor < content.Length)
        {
            cursor = SkipWhitespace(content, cursor);

            if (cursor >= content.Length)
            {
                return cursor;
            }

            if (content[cursor] == ')')
            {
                return cursor + 1;
            }

            // A block entry begins either at a code token — an alias — or at the
            // opening delimiter of a real string literal. A quote that merely
            // sits inside a comment is not a string start, so a commented-out
            // entry is skipped rather than read.
            if (!scan.IsCodeAt(cursor) && !scan.IsStringStartAt(cursor))
            {
                cursor = NextLineStart(content, cursor);
                continue;
            }

            int entry = cursor;
            cursor = SkipGoImportAlias(content, cursor);

            if (scan.IsStringStartAt(cursor)
                && TryReadStringLiteral(content, cursor, out string? path, out int end)
                && IsUsableIdentity(path))
            {
                candidates.Add(new SourceImportCandidate(
                    SourceReferenceForm.Import,
                    path,
                    lines.LineAt(entry)));

                cursor = end;
                continue;
            }

            cursor = NextLineStart(content, cursor);
        }

        return cursor;
    }

    private static int SkipGoImportAlias(string content, int index)
    {
        int cursor = index;

        if (cursor < content.Length && (content[cursor] == '_' || content[cursor] == '.'
            || char.IsLetter(content[cursor])))
        {
            int scan = cursor;

            while (scan < content.Length && (char.IsLetterOrDigit(content[scan])
                || content[scan] is '_' or '.'))
            {
                scan++;
            }

            int afterAlias = SkipInlineWhitespace(content, scan);

            if (afterAlias > scan && afterAlias < content.Length && content[afterAlias] == '"')
            {
                return afterAlias;
            }
        }

        return cursor;
    }

    // -------------------------------------------------------- JavaScript ----

    private static void ReadJavaScript(
        string content,
        SourceLexicalScan scan,
        LineIndex lines,
        List<SourceImportCandidate> candidates)
    {
        for (int index = 0; index < content.Length; index++)
        {
            if (!scan.IsCodeAt(index) || !IsIdentifierStart(content, index))
            {
                continue;
            }

            int end = IdentifierEnd(content, index);
            string word = content[index..end];

            switch (word)
            {
                case "import":
                    ReadJavaScriptImport(content, scan, lines, candidates, index, end);
                    break;

                case "export":
                    ReadJavaScriptFrom(
                        content,
                        scan,
                        lines,
                        candidates,
                        index,
                        end,
                        SourceReferenceForm.ExportFrom);
                    break;

                case "require":
                    ReadJavaScriptCall(
                        content,
                        lines,
                        candidates,
                        index,
                        end,
                        SourceReferenceForm.RequireCall);
                    break;
            }

            index = end - 1;
        }
    }

    private static void ReadJavaScriptImport(
        string content,
        SourceLexicalScan scan,
        LineIndex lines,
        List<SourceImportCandidate> candidates,
        int keyword,
        int keywordEnd)
    {
        int cursor = SkipInlineWhitespace(content, keywordEnd);

        // `import("spec")` is the dynamic form and obeys the literal-only rule.
        if (cursor < content.Length && content[cursor] == '(')
        {
            ReadJavaScriptCall(
                content,
                lines,
                candidates,
                keyword,
                keywordEnd,
                SourceReferenceForm.DynamicImportCall);

            return;
        }

        // `import "spec"` is the bare side-effect form.
        if (TryReadStringLiteral(content, cursor, out string? bare, out _))
        {
            AddJavaScriptSpecifier(candidates, lines, keyword, bare, SourceReferenceForm.Import);
            return;
        }

        ReadJavaScriptFrom(
            content,
            scan,
            lines,
            candidates,
            keyword,
            keywordEnd,
            SourceReferenceForm.Import);
    }

    // Scans forward from an `import` or `export` keyword for the `from` clause.
    // The window is bounded and the statement terminates at a code semicolon, so
    // the scan is linear and cannot run away on untrusted input.
    private static void ReadJavaScriptFrom(
        string content,
        SourceLexicalScan scan,
        LineIndex lines,
        List<SourceImportCandidate> candidates,
        int keyword,
        int keywordEnd,
        SourceReferenceForm form)
    {
        const int Window = 1024;
        int limit = Math.Min(content.Length, keywordEnd + Window);

        for (int cursor = keywordEnd; cursor < limit; cursor++)
        {
            if (!scan.IsCodeAt(cursor))
            {
                continue;
            }

            if (content[cursor] == ';')
            {
                return;
            }

            if (!IsIdentifierStart(content, cursor))
            {
                continue;
            }

            int end = IdentifierEnd(content, cursor);

            if (!content[cursor..end].Equals("from", StringComparison.Ordinal))
            {
                cursor = end - 1;
                continue;
            }

            int specifier = SkipInlineWhitespace(content, end);

            if (TryReadStringLiteral(content, specifier, out string? value, out _))
            {
                AddJavaScriptSpecifier(candidates, lines, keyword, value, form);
            }

            return;
        }
    }

    // §7: the parenthesised argument must be exactly one string literal and
    // nothing else. A concatenated, templated, computed or multi-argument call
    // yields no candidate.
    private static void ReadJavaScriptCall(
        string content,
        LineIndex lines,
        List<SourceImportCandidate> candidates,
        int keyword,
        int keywordEnd,
        SourceReferenceForm form)
    {
        int cursor = SkipInlineWhitespace(content, keywordEnd);

        if (cursor >= content.Length || content[cursor] != '(')
        {
            return;
        }

        cursor = SkipWhitespace(content, cursor + 1);

        if (!TryReadStringLiteral(content, cursor, out string? value, out int end))
        {
            return;
        }

        cursor = SkipWhitespace(content, end);

        if (cursor >= content.Length || content[cursor] != ')')
        {
            return;
        }

        AddJavaScriptSpecifier(candidates, lines, keyword, value, form);
    }

    private static void AddJavaScriptSpecifier(
        List<SourceImportCandidate> candidates,
        LineIndex lines,
        int keyword,
        string specifier,
        SourceReferenceForm form)
    {
        // §5: a relative specifier is a local module reference and is never a
        // package reference.
        if (!IsUsableIdentity(specifier)
            || specifier.StartsWith('.')
            || specifier.StartsWith('/'))
        {
            return;
        }

        candidates.Add(new SourceImportCandidate(form, specifier, lines.LineAt(keyword)));
    }

    // ------------------------------------------------------------ Python ----

    private static void ReadPython(
        string content,
        SourceLexicalScan scan,
        LineIndex lines,
        List<SourceImportCandidate> candidates)
    {
        foreach (int start in lines.StatementStarts(content, scan))
        {
            int end = IdentifierEnd(content, start);
            string word = content[start..end];

            if (word.Equals("import", StringComparison.Ordinal))
            {
                foreach (string module in ReadPythonImportList(content, end))
                {
                    candidates.Add(new SourceImportCandidate(
                        SourceReferenceForm.Import,
                        module,
                        lines.LineAt(start)));
                }

                continue;
            }

            if (!word.Equals("from", StringComparison.Ordinal))
            {
                continue;
            }

            int cursor = SkipInlineWhitespace(content, end);
            int module0 = cursor;

            while (cursor < content.Length && (char.IsLetterOrDigit(content[cursor])
                || content[cursor] is '_' or '.'))
            {
                cursor++;
            }

            if (cursor == module0)
            {
                continue;
            }

            int afterModule = SkipInlineWhitespace(content, cursor);
            int keywordEnd = IdentifierEnd(content, afterModule);

            if (!content[afterModule..keywordEnd].Equals("import", StringComparison.Ordinal))
            {
                continue;
            }

            string dotted = content[module0..cursor];

            // `from . import x` and `from .pkg import x` are relative.
            if (!dotted.StartsWith('.') && IsUsableIdentity(dotted))
            {
                candidates.Add(new SourceImportCandidate(
                    SourceReferenceForm.ImportFrom,
                    TopLevelModule(dotted),
                    lines.LineAt(start)));
            }
        }
    }

    private static IEnumerable<string> ReadPythonImportList(string content, int index)
    {
        int cursor = index;

        while (cursor < content.Length)
        {
            cursor = SkipInlineWhitespace(content, cursor);
            int start = cursor;

            while (cursor < content.Length && (char.IsLetterOrDigit(content[cursor])
                || content[cursor] is '_' or '.'))
            {
                cursor++;
            }

            if (cursor == start)
            {
                yield break;
            }

            string module = content[start..cursor];

            if (!module.StartsWith('.') && IsUsableIdentity(module))
            {
                yield return TopLevelModule(module);
            }

            cursor = SkipInlineWhitespace(content, cursor);

            // `import a as b` stops the alias from being read as a module.
            int aliasEnd = IdentifierEnd(content, cursor);

            if (aliasEnd > cursor
                && content[cursor..aliasEnd].Equals("as", StringComparison.Ordinal))
            {
                cursor = SkipInlineWhitespace(content, aliasEnd);
                cursor = IdentifierEnd(content, cursor);
                cursor = SkipInlineWhitespace(content, cursor);
            }

            if (cursor >= content.Length || content[cursor] != ',')
            {
                yield break;
            }

            cursor++;
        }
    }

    // -------------------------------------------------------------- Rust ----

    private static void ReadRust(
        string content,
        SourceLexicalScan scan,
        LineIndex lines,
        List<SourceImportCandidate> candidates)
    {
        foreach (int start in lines.StatementStarts(content, scan))
        {
            int cursor = start;
            int end = IdentifierEnd(content, cursor);
            string word = content[cursor..end];

            if (word.Equals("pub", StringComparison.Ordinal))
            {
                cursor = SkipInlineWhitespace(content, end);

                if (cursor < content.Length && content[cursor] == '(')
                {
                    int close = content.IndexOf(')', cursor);

                    if (close < 0)
                    {
                        continue;
                    }

                    cursor = SkipInlineWhitespace(content, close + 1);
                }

                end = IdentifierEnd(content, cursor);
                word = content[cursor..end];
            }

            SourceReferenceForm form;

            if (word.Equals("use", StringComparison.Ordinal))
            {
                form = SourceReferenceForm.Use;
                cursor = SkipInlineWhitespace(content, end);
            }
            else if (word.Equals("extern", StringComparison.Ordinal))
            {
                cursor = SkipInlineWhitespace(content, end);
                int crateEnd = IdentifierEnd(content, cursor);

                if (!content[cursor..crateEnd].Equals("crate", StringComparison.Ordinal))
                {
                    continue;
                }

                form = SourceReferenceForm.ExternCrate;
                cursor = SkipInlineWhitespace(content, crateEnd);
            }
            else
            {
                continue;
            }

            // A leading `::` names the crate root explicitly.
            if (cursor + 1 < content.Length && content[cursor] == ':' && content[cursor + 1] == ':')
            {
                cursor += 2;
            }

            int segmentEnd = IdentifierEnd(content, cursor);

            if (segmentEnd == cursor)
            {
                continue;
            }

            string segment = content[cursor..segmentEnd];

            if (IsUsableIdentity(segment))
            {
                candidates.Add(new SourceImportCandidate(form, segment, lines.LineAt(start)));
            }
        }
    }

    // ------------------------------------------------------------ DotNet ----

    private static void ReadDotNet(
        string content,
        SourceLexicalScan scan,
        LineIndex lines,
        List<SourceImportCandidate> candidates)
    {
        foreach (int start in lines.StatementStarts(content, scan))
        {
            int cursor = start;
            int end = IdentifierEnd(content, cursor);
            string word = content[cursor..end];

            if (word.Equals("global", StringComparison.Ordinal))
            {
                cursor = SkipInlineWhitespace(content, end);
                end = IdentifierEnd(content, cursor);
                word = content[cursor..end];
            }

            if (!word.Equals("using", StringComparison.Ordinal))
            {
                continue;
            }

            cursor = SkipInlineWhitespace(content, end);
            int next = IdentifierEnd(content, cursor);

            if (next > cursor && content[cursor..next].Equals("static", StringComparison.Ordinal))
            {
                cursor = SkipInlineWhitespace(content, next);
            }

            int nameStart = cursor;

            while (cursor < content.Length && (char.IsLetterOrDigit(content[cursor])
                || content[cursor] is '_' or '.'))
            {
                cursor++;
            }

            if (cursor == nameStart)
            {
                continue;
            }

            string name = content[nameStart..cursor];
            int afterName = SkipInlineWhitespace(content, cursor);

            // `using Alias = Ns;` names the referenced namespace on the right.
            if (afterName < content.Length && content[afterName] == '='
                && (afterName + 1 >= content.Length || content[afterName + 1] != '='))
            {
                int rightStart = SkipInlineWhitespace(content, afterName + 1);
                int right = rightStart;

                while (right < content.Length && (char.IsLetterOrDigit(content[right])
                    || content[right] is '_' or '.'))
                {
                    right++;
                }

                if (right == rightStart)
                {
                    continue;
                }

                name = content[rightStart..right];
                afterName = SkipInlineWhitespace(content, right);
            }

            // A `using` statement or declaration such as `using var x = ...` is
            // not a namespace directive; only a directive ends in `;` here.
            if (afterName >= content.Length || content[afterName] != ';')
            {
                continue;
            }

            if (IsUsableIdentity(name))
            {
                candidates.Add(new SourceImportCandidate(
                    SourceReferenceForm.Using,
                    name,
                    lines.LineAt(start)));
            }
        }
    }

    // ----------------------------------------------------------- helpers ----

    private static bool TryFindKeyword(
        string content,
        SourceLexicalScan scan,
        string keyword,
        int from,
        out int index)
    {
        for (int cursor = Math.Max(from, 0); cursor < content.Length; cursor++)
        {
            if (!scan.IsCodeAt(cursor) || !IsIdentifierStart(content, cursor))
            {
                continue;
            }

            int end = IdentifierEnd(content, cursor);

            if (content[cursor..end].Equals(keyword, StringComparison.Ordinal))
            {
                index = cursor;
                return true;
            }

            cursor = end - 1;
        }

        index = -1;
        return false;
    }

    private static bool TryReadStringLiteral(
        string content,
        int index,
        out string value,
        out int end)
    {
        value = string.Empty;
        end = index;

        if (index >= content.Length || content[index] is not ('"' or '\''))
        {
            return false;
        }

        char quote = content[index];
        System.Text.StringBuilder literal = new();

        for (int cursor = index + 1; cursor < content.Length; cursor++)
        {
            char current = content[cursor];

            if (current == '\\' && cursor + 1 < content.Length)
            {
                literal.Append(content[cursor + 1]);
                cursor++;
                continue;
            }

            if (current == '\n')
            {
                return false;
            }

            if (current == quote)
            {
                value = literal.ToString();
                end = cursor + 1;
                return true;
            }

            literal.Append(current);
        }

        return false;
    }

    // §5: the Python reference identity is the top-level module segment.
    private static string TopLevelModule(string dotted)
    {
        int dot = dotted.IndexOf('.');

        return dot < 0 ? dotted : dotted[..dot];
    }

    private static bool IsUsableIdentity(string? identity) =>
        !string.IsNullOrWhiteSpace(identity);

    private static bool IsIdentifierStart(string content, int index)
    {
        if (index >= content.Length)
        {
            return false;
        }

        char current = content[index];

        if (!char.IsLetter(current) && current is not ('_' or '$'))
        {
            return false;
        }

        return index == 0 || !(char.IsLetterOrDigit(content[index - 1])
            || content[index - 1] is '_' or '$');
    }

    private static int IdentifierEnd(string content, int index)
    {
        int cursor = index;

        while (cursor < content.Length && (char.IsLetterOrDigit(content[cursor])
            || content[cursor] is '_' or '$'))
        {
            cursor++;
        }

        return cursor;
    }

    private static int SkipInlineWhitespace(string content, int index)
    {
        int cursor = index;

        while (cursor < content.Length && content[cursor] is ' ' or '\t' or '\r')
        {
            cursor++;
        }

        return cursor;
    }

    private static int SkipWhitespace(string content, int index)
    {
        int cursor = index;

        while (cursor < content.Length && char.IsWhiteSpace(content[cursor]))
        {
            cursor++;
        }

        return cursor;
    }

    private static int NextLineStart(string content, int index)
    {
        int newline = content.IndexOf('\n', index);

        return newline < 0 ? content.Length : newline + 1;
    }

    // Line numbers are deterministic because
    // repository-source-evidence-collection.md §16 preserves line endings and
    // strips only a single leading BOM.
    private sealed class LineIndex
    {
        private readonly List<int> starts = [0];

        public LineIndex(string content)
        {
            for (int index = 0; index < content.Length; index++)
            {
                if (content[index] == '\n')
                {
                    starts.Add(index + 1);
                }
            }
        }

        public int LineAt(int offset)
        {
            int low = 0;
            int high = starts.Count - 1;

            while (low < high)
            {
                int middle = (low + high + 1) / 2;

                if (starts[middle] <= offset)
                {
                    low = middle;
                }
                else
                {
                    high = middle - 1;
                }
            }

            return low + 1;
        }

        // The first code offset of each line, which is where a line-anchored
        // statement keyword must begin.
        public IEnumerable<int> StatementStarts(string content, SourceLexicalScan scan)
        {
            foreach (int start in starts)
            {
                int cursor = start;

                while (cursor < content.Length && content[cursor] is ' ' or '\t')
                {
                    cursor++;
                }

                if (cursor < content.Length && scan.IsCodeAt(cursor)
                    && IsIdentifierStart(content, cursor))
                {
                    yield return cursor;
                }
            }
        }
    }
}
