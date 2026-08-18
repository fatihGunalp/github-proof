using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Domain.Analysis.Models;

// One qualifying import-declaration observed in one collected source artefact.
// It carries the canonical minimum locked by
// docs/architecture/repository-dependency-source-reference-detection.md §14:
// path, form kind and 1-based line. It deliberately carries no snippet, no
// source substring, no content hash, no column or byte offset, no confidence
// and no count.
public sealed record DependencySourceReferenceLocation
{
    public DependencySourceReferenceLocation(
        string sourcePath,
        SourceReferenceForm form,
        int line)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                nameof(sourcePath));
        }

        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(
                nameof(form),
                form,
                "Source reference form is not defined.");
        }

        if (line < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(line),
                line,
                "Line numbers are 1-based.");
        }

        SourcePath = sourcePath.Trim();
        Form = form;
        Line = line;
    }

    public string SourcePath { get; }

    public SourceReferenceForm Form { get; }

    public int Line { get; }
}
