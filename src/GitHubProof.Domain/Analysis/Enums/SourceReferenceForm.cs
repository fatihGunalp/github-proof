namespace GitHubProof.Domain.Analysis.Enums;

// The qualifying import-declaration forms locked by
// docs/architecture/repository-dependency-source-reference-detection.md §5.
// A form kind records only which syntax was observed. It never implies usage,
// execution, produced work or capability.
public enum SourceReferenceForm
{
    // Go `import "path"` and `import alias "path"`, JavaScript
    // `import ... from "spec"` and `import "spec"`, Python `import a.b`.
    Import = 1,

    // Python `from a.b import x`.
    ImportFrom = 2,

    // JavaScript `export ... from "spec"`.
    ExportFrom = 3,

    // JavaScript `require("spec")` under §7.
    RequireCall = 4,

    // JavaScript `import("spec")` under §7.
    DynamicImportCall = 5,

    // Rust `use crate_ident::...` and `use crate_ident;`.
    Use = 6,

    // Rust `extern crate crate_ident`.
    ExternCrate = 7,

    // DotNet `using Ns;`, `global using Ns;`, `using static Ns;`,
    // `using Alias = Ns;`.
    Using = 8
}
