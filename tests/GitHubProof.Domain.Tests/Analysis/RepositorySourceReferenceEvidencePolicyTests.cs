using System.Reflection;
using GitHubProof.Domain.Analysis;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using Xunit;

namespace GitHubProof.Domain.Tests.Analysis;

public sealed class RepositorySourceReferenceEvidencePolicyTests
{
    private static readonly DateTimeOffset CapturedAt =
        DateTimeOffset.Parse("2026-08-18T09:00:00Z");

    private static readonly AnalysisVersion Version =
        new("test-analyzer", "test-rules", 1);

    // ------------------------------------------------- supported ecosystems --

    [Fact]
    public void Compose_GoImport_ProducesObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Go, "go.mod", "github.com/gin-gonic/gin")],
            [SourceFile("main.go", "package main\n\nimport \"github.com/gin-gonic/gin\"\n")]);

        DependencySourceReferenceEvidence reference = Assert.Single(evidence.References);

        Assert.Equal(DependencyEcosystem.Go, reference.Ecosystem);
        Assert.Equal("github.com/gin-gonic/gin", reference.ResolvedReferenceIdentity);

        DependencySourceReferenceLocation location = Assert.Single(reference.Locations);

        Assert.Equal("main.go", location.SourcePath);
        Assert.Equal(SourceReferenceForm.Import, location.Form);
        Assert.Equal(3, location.Line);
    }

    [Fact]
    public void Compose_JavaScriptImport_ProducesObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.JavaScript, "package.json", "axios")],
            [SourceFile("src/client.js", "import axios from 'axios';\n")]);

        DependencySourceReferenceEvidence reference = Assert.Single(evidence.References);

        Assert.Equal("axios", reference.ResolvedReferenceIdentity);
        Assert.Equal(SourceReferenceForm.Import, Assert.Single(reference.Locations).Form);
    }

    [Fact]
    public void Compose_PythonImport_ProducesObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")],
            [SourceFile("app.py", "import requests\n")]);

        Assert.Equal("requests", Assert.Single(evidence.References).ResolvedReferenceIdentity);
    }

    [Fact]
    public void Compose_RustUse_ProducesObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Rust, "Cargo.toml", "serde")],
            [SourceFile("src/lib.rs", "use serde::Serialize;\n")]);

        DependencySourceReferenceEvidence reference = Assert.Single(evidence.References);

        Assert.Equal("serde", reference.ResolvedReferenceIdentity);
        Assert.Equal(SourceReferenceForm.Use, Assert.Single(reference.Locations).Form);
    }

    [Fact]
    public void Compose_DotNetUsing_ProducesObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper")],
            [SourceFile("Repository.cs", "using Dapper;\n")]);

        DependencySourceReferenceEvidence reference = Assert.Single(evidence.References);

        Assert.Equal("Dapper", reference.ResolvedReferenceIdentity);
        Assert.Equal(SourceReferenceForm.Using, Assert.Single(reference.Locations).Form);
    }

    // ------------------------------------------------------ qualifying forms --

    [Fact]
    public void Compose_GoImportBlockAndAlias_AreBothRead()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(DependencyEcosystem.Go, "go.mod", "gorm.io/gorm"),
                Declaration(DependencyEcosystem.Go, "go.mod", "github.com/lib/pq")
            ],
            [
                SourceFile(
                    "db.go",
                    "package db\n\nimport (\n\t\"gorm.io/gorm\"\n\t_ \"github.com/lib/pq\"\n)\n")
            ]);

        Assert.Equal(
            new[] { "github.com/lib/pq", "gorm.io/gorm" },
            evidence.References.Select(reference => reference.ResolvedReferenceIdentity));
    }

    [Theory]
    [InlineData("import 'axios';\n", SourceReferenceForm.Import)]
    [InlineData("import axios from 'axios';\n", SourceReferenceForm.Import)]
    [InlineData("export { get } from 'axios';\n", SourceReferenceForm.ExportFrom)]
    [InlineData("const a = require('axios');\n", SourceReferenceForm.RequireCall)]
    [InlineData("const a = await import('axios');\n", SourceReferenceForm.DynamicImportCall)]
    public void Compose_JavaScriptQualifyingForms_AreRead(string content, SourceReferenceForm form)
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.JavaScript, "package.json", "axios")],
            [SourceFile("src/index.js", content)]);

        Assert.Equal(form, Assert.Single(Assert.Single(evidence.References).Locations).Form);
    }

    [Fact]
    public void Compose_PythonFromImport_IsRead()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "sqlalchemy")],
            [SourceFile("model.py", "from sqlalchemy.orm import Session\n")]);

        Assert.Equal(
            SourceReferenceForm.ImportFrom,
            Assert.Single(Assert.Single(evidence.References).Locations).Form);
    }

    [Fact]
    public void Compose_RustExternCrate_IsRead()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Rust, "Cargo.toml", "serde")],
            [SourceFile("src/main.rs", "extern crate serde;\n")]);

        Assert.Equal(
            SourceReferenceForm.ExternCrate,
            Assert.Single(Assert.Single(evidence.References).Locations).Form);
    }

    [Theory]
    [InlineData("global using Dapper;\n")]
    [InlineData("using static Dapper;\n")]
    [InlineData("using Sql = Dapper;\n")]
    public void Compose_DotNetUsingVariants_AreRead(string content)
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper")],
            [SourceFile("Data.cs", content)]);

        Assert.Equal("Dapper", Assert.Single(evidence.References).ResolvedReferenceIdentity);
    }

    [Fact]
    public void Compose_DotNetUsingStatement_IsNotADirective()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper")],
            [SourceFile("Data.cs", "using var connection = Dapper.Create();\n")]);

        Assert.Empty(evidence.References);
    }

    // -------------------------------------------------- lexical exclusion ----

    [Theory]
    [InlineData("Repository.cs", "// using Dapper;\n")]
    [InlineData("Repository.cs", "/* using Dapper; */\n")]
    [InlineData("Repository.cs", "/// <summary>using Dapper;</summary>\n")]
    public void Compose_DotNetCommentedDirective_ProducesNoObservation(
        string path,
        string content)
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper")],
            [SourceFile(path, content)]);

        Assert.Empty(evidence.References);
    }

    [Fact]
    public void Compose_GoImportBlockBlockComment_ProducesNoObservation()
    {
        const string Content =
            "package main\n\nimport (\n\t/*\n\t\"github.com/fake/c\"\n\t*/\n\t\"os\"\n)\n";

        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(DependencyEcosystem.Go, "go.mod", "github.com/fake/c"),
                Declaration(DependencyEcosystem.Go, "go.mod", "os")
            ],
            [SourceFile("main.go", Content)]);

        DependencySourceReferenceEvidence reference = Assert.Single(evidence.References);

        Assert.Equal("os", reference.ResolvedReferenceIdentity);
        Assert.Equal(7, Assert.Single(reference.Locations).Line);
    }

    [Fact]
    public void Compose_GoImportBlockLineComment_ProducesNoObservation()
    {
        const string Content =
            "package main\n\nimport (\n\t// \"github.com/fake/c\"\n\t\"os\"\n)\n";

        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(DependencyEcosystem.Go, "go.mod", "github.com/fake/c"),
                Declaration(DependencyEcosystem.Go, "go.mod", "os")
            ],
            [SourceFile("main.go", Content)]);

        Assert.Equal("os", Assert.Single(evidence.References).ResolvedReferenceIdentity);
    }

    [Fact]
    public void Compose_PythonUnterminatedSingleQuote_DoesNotSwallowLaterLines()
    {
        // The literal ends at the newline exactly as in the C-like scanner, so
        // it neither hides the rest of the artefact nor marks it unreliable.
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")],
            [SourceFile("app.py", "value = 'oops\nimport requests\n")]);

        Assert.Equal(2, Assert.Single(Assert.Single(evidence.References).Locations).Line);
        Assert.False(evidence.HasUnreliableSourceArtefact);
        Assert.True(evidence.IsDetectionSurfaceComplete);
    }

    [Fact]
    public void Compose_PythonUnterminatedTripleQuote_IsUnreliable()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")],
            [SourceFile("app.py", "import requests\n\"\"\"never closed\n")]);

        Assert.Empty(evidence.References);
        Assert.True(evidence.HasUnreliableSourceArtefact);
        Assert.False(evidence.IsDetectionSurfaceComplete);
    }

    [Fact]
    public void Compose_RustDocCommentImport_ProducesNoObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Rust, "Cargo.toml", "walkdir")],
            [SourceFile("src/lib.rs", "/// use walkdir::WalkDir;\nfn main() {}\n")]);

        Assert.Empty(evidence.References);
    }

    [Fact]
    public void Compose_PythonDocstringImport_ProducesNoObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "pytest")],
            [SourceFile("test_thing.py", "\"\"\"\nimport pytest\n\"\"\"\nvalue = 1\n")]);

        Assert.Empty(evidence.References);
    }

    [Fact]
    public void Compose_PythonTripleStringDoesNotHideLaterImport()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")],
            [SourceFile("app.py", "\"\"\"\nimport fake_module\n\"\"\"\nimport requests\n")]);

        Assert.Equal(4, Assert.Single(Assert.Single(evidence.References).Locations).Line);
    }

    [Fact]
    public void Compose_JavaScriptTemplateLiteralImport_ProducesNoObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.JavaScript, "package.json", "axios")],
            [SourceFile("src/doc.js", "const sample = `\nimport axios from 'axios';\n`;\n")]);

        Assert.Empty(evidence.References);
    }

    [Fact]
    public void Compose_GoRawStringImport_ProducesNoObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Go, "go.mod", "github.com/evil/pkg")],
            [SourceFile("doc.go", "package doc\n\nvar sample = `import \"github.com/evil/pkg\"`\n")]);

        Assert.Empty(evidence.References);
    }

    [Fact]
    public void Compose_RustRawStringAndNestedBlockComment_ProduceNoObservation()
    {
        const string Content =
            "/* outer /* inner */ still a comment\nuse fake_crate::Thing;\n*/\n"
            + "const S: &str = r#\"use other_crate::Thing;\"#;\n"
            + "use serde::Serialize;\n";

        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(DependencyEcosystem.Rust, "Cargo.toml", "fake-crate"),
                Declaration(DependencyEcosystem.Rust, "Cargo.toml", "other-crate"),
                Declaration(DependencyEcosystem.Rust, "Cargo.toml", "serde")
            ],
            [SourceFile("src/lib.rs", Content)]);

        DependencySourceReferenceEvidence reference = Assert.Single(evidence.References);

        Assert.Equal("serde", reference.ResolvedReferenceIdentity);
        Assert.Equal(5, Assert.Single(reference.Locations).Line);
    }

    [Fact]
    public void Compose_DotNetVerbatimAndRawStrings_ProduceNoObservation()
    {
        const string Content =
            "var a = @\"using Fake.Namespace;\";\n"
            + "var b = \"\"\"\nusing Other.Namespace;\n\"\"\";\n"
            + "using Dapper;\n";

        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(DependencyEcosystem.DotNet, "App.csproj", "Fake.Namespace"),
                Declaration(DependencyEcosystem.DotNet, "App.csproj", "Other.Namespace"),
                Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper")
            ],
            [SourceFile("Data.cs", Content)]);

        Assert.Equal("Dapper", Assert.Single(evidence.References).ResolvedReferenceIdentity);
    }

    [Fact]
    public void Compose_JavaScriptRegexLiteral_DoesNotDesynchroniseLaterImport()
    {
        const string Content =
            "const quoted = /\"/;\nconst client = require('axios');\n";

        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.JavaScript, "package.json", "axios")],
            [SourceFile("src/index.js", Content)]);

        Assert.Equal(2, Assert.Single(Assert.Single(evidence.References).Locations).Line);
    }

    // ------------------------------------------------- desynchronization ----

    [Fact]
    public void Compose_UnterminatedBlockComment_DiscardsEveryObservationFromArtefact()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper")],
            [SourceFile("Data.cs", "using Dapper;\n/* never closed\n")]);

        Assert.Empty(evidence.References);
        Assert.True(evidence.HasUnreliableSourceArtefact);
        Assert.False(evidence.IsDetectionSurfaceComplete);
    }

    [Fact]
    public void Compose_UnreliableArtefact_DoesNotSuppressOtherArtefacts()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper")],
            [
                SourceFile("Broken.cs", "using Dapper;\n/* never closed\n"),
                SourceFile("Working.cs", "using Dapper;\n")
            ]);

        DependencySourceReferenceEvidence reference = Assert.Single(evidence.References);

        Assert.Equal("Working.cs", Assert.Single(reference.Locations).SourcePath);
        Assert.True(evidence.HasUnreliableSourceArtefact);
    }

    // ------------------------------------------------ literal-only calls ----

    [Theory]
    [InlineData("const a = require('axios' + suffix);\n")]
    [InlineData("const a = await import(`axios`);\n")]
    [InlineData("const a = await import(specifier);\n")]
    [InlineData("const a = await import('axios', options);\n")]
    public void Compose_NonLiteralCallArgument_ProducesNoObservation(string content)
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.JavaScript, "package.json", "axios")],
            [SourceFile("src/index.js", content)]);

        Assert.Empty(evidence.References);
    }

    [Theory]
    [InlineData("import x from './local';\n")]
    [InlineData("const a = require('./local');\n")]
    [InlineData("const a = await import('/abs/local');\n")]
    public void Compose_RelativeSpecifier_ProducesNoObservation(string content)
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.JavaScript, "package.json", "local")],
            [SourceFile("src/index.js", content)]);

        Assert.Empty(evidence.References);
    }

    // ------------------------------------------------ identity resolution ----

    [Theory]
    [InlineData("@nestjs/core", "import { Module } from '@nestjs/core';\n")]
    [InlineData("@nestjs/core", "import { Module } from '@nestjs/core/errors';\n")]
    [InlineData("next", "import Link from 'next/link';\n")]
    public void Compose_ScopedAndSubpathSpecifiers_ResolveToPackageHead(
        string packageId,
        string content)
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.JavaScript, "package.json", packageId)],
            [SourceFile("src/index.ts", content)]);

        Assert.Equal(packageId, Assert.Single(evidence.References).ResolvedReferenceIdentity);
    }

    [Fact]
    public void Compose_NestedGoModules_LongestModulePathWins()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(DependencyEcosystem.Go, "go.mod", "github.com/prometheus/client_golang"),
                Declaration(
                    DependencyEcosystem.Go,
                    "exp/go.mod",
                    "github.com/prometheus/client_golang/exp")
            ],
            [
                SourceFile(
                    "main.go",
                    "package main\n\nimport \"github.com/prometheus/client_golang/exp/api\"\n")
            ]);

        DependencySourceReferenceEvidence reference = Assert.Single(evidence.References);

        Assert.Equal(
            "github.com/prometheus/client_golang/exp",
            reference.Declaration.PackageId);
    }

    [Fact]
    public void Compose_RustHyphenatedCrate_NormalisesToUnderscore()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Rust, "Cargo.toml", "rust-decimal")],
            [SourceFile("src/lib.rs", "use rust_decimal::Decimal;\n")]);

        Assert.Equal("rust-decimal", Assert.Single(evidence.References).Declaration.PackageId);
    }

    [Theory]
    [InlineData("Flask", "import flask\n")]
    [InlineData("python.dateutil", "import python_dateutil\n")]
    [InlineData("typing-extensions", "import typing_extensions\n")]
    public void Compose_PythonDistributionNormalisation_Matches(string packageId, string content)
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", packageId)],
            [SourceFile("app.py", content)]);

        Assert.Equal(packageId, Assert.Single(evidence.References).Declaration.PackageId);
    }

    [Fact]
    public void Compose_PythonUnderivableModuleName_ProducesNoObservation()
    {
        // V1 ships no identity registry, so scikit-learn -> sklearn is a
        // deliberate, contract-recorded false negative.
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "scikit-learn")],
            [SourceFile("model.py", "import sklearn\n")]);

        Assert.Empty(evidence.References);
    }

    [Fact]
    public void Compose_DotNetOwnSubnamespace_Matches()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.DotNet, "App.csproj", "Microsoft.EntityFrameworkCore")],
            [SourceFile("Context.cs", "using Microsoft.EntityFrameworkCore.Metadata;\n")]);

        Assert.Single(evidence.References);
    }

    [Fact]
    public void Compose_DotNetReversePrefix_IsNeverMatched()
    {
        // The namespace is a prefix of the package id. Reverse-prefix matching is
        // prohibited, so this produces nothing rather than a wrong attribution.
        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(
                    DependencyEcosystem.DotNet,
                    "App.csproj",
                    "Microsoft.Extensions.DependencyInjection.Abstractions")
            ],
            [SourceFile("Startup.cs", "using Microsoft.Extensions.DependencyInjection;\n")]);

        Assert.Empty(evidence.References);
    }

    // ------------------------------------------------------------ ambiguity --

    [Fact]
    public void Compose_IdentityAcceptedByTwoDeclarations_ProducesNoObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(DependencyEcosystem.JavaScript, "app/package.json", "axios"),
                Declaration(DependencyEcosystem.JavaScript, "web/package.json", "axios")
            ],
            [SourceFile("src/index.js", "import axios from 'axios';\n")]);

        Assert.Empty(evidence.References);
    }

    // ------------------------------------------------- unsupported surface --

    [Theory]
    [InlineData(DependencyEcosystem.Java, "Main.java", "import okhttp3.OkHttpClient;\n", "com.squareup.okhttp3:okhttp")]
    [InlineData(DependencyEcosystem.Php, "src/App.php", "use GuzzleHttp\\Client;\n", "guzzlehttp/guzzle")]
    [InlineData(DependencyEcosystem.Ruby, "app.rb", "require 'sinatra'\n", "sinatra")]
    public void Compose_UnsupportedEcosystem_ProducesNoObservation(
        DependencyEcosystem ecosystem,
        string path,
        string content,
        string packageId)
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(ecosystem, "manifest", packageId)],
            [SourceFile(path, content)]);

        Assert.Empty(evidence.References);
        Assert.False(evidence.HasUnreliableSourceArtefact);
    }

    // ------------------------------------ structurally unreferenceable set --

    [Fact]
    public void Compose_TypesOnlyPackage_IsNeverMatched()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.JavaScript, "package.json", "@types/node")],
            [SourceFile("src/index.ts", "import fs from '@types/node';\n")]);

        Assert.Empty(evidence.References);
    }

    [Fact]
    public void Compose_MsBuildPlaceholder_IsNeverMatched()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(DependencyEcosystem.DotNet, "App.csproj", "$(SomePackage)"),
                Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper")
            ],
            [SourceFile("Data.cs", "using Dapper;\n")]);

        Assert.Equal("Dapper", Assert.Single(evidence.References).Declaration.PackageId);
    }

    // ------------------------------------------------ Go indirect regression --

    [Fact]
    public void Compose_GoDeclarationWithoutIndirectMarker_IsTreatedAsOrdinary()
    {
        // Session 073 Step 0: the declaration surface preserves no `// indirect`
        // marker, so an indirect requirement is indistinguishable from a direct
        // one and a positively matching import produces the ordinary syntactic
        // observation.
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Go, "go.mod", "golang.org/x/sys")],
            [SourceFile("sys.go", "package sys\n\nimport \"golang.org/x/sys/unix\"\n")]);

        Assert.Equal("golang.org/x/sys", Assert.Single(evidence.References).Declaration.PackageId);
    }

    [Fact]
    public void RepositoryDependencyDeclaration_ExposesNoIndirectOrScopeMarker()
    {
        // Regression guard: no future code may assume an indirect marker exists.
        string[] properties = typeof(RepositoryDependencyDeclaration)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "DeclaredVersion", "Ecosystem", "ManifestPath", "PackageId" },
            properties);
    }

    // ------------------------------------------------ payload and ordering --

    [Fact]
    public void Compose_RepeatedReferences_CollapseIntoOneObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")],
            [
                SourceFile("a.py", "import requests\nimport requests\n"),
                SourceFile("b.py", "import requests\n")
            ]);

        DependencySourceReferenceEvidence reference = Assert.Single(evidence.References);

        Assert.Equal(
            new[] { ("a.py", 1), ("a.py", 2), ("b.py", 1) },
            reference.Locations.Select(location => (location.SourcePath, location.Line)));
    }

    [Fact]
    public void Compose_DuplicateLocations_Collapse()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")],
            [SourceFile("a.py", "import requests, requests\n")]);

        Assert.Single(Assert.Single(evidence.References).Locations);
    }

    [Fact]
    public void Compose_Observations_AreOrderedByEcosystemThenDeclarationIdentity()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [
                Declaration(DependencyEcosystem.Python, "requirements.txt", "requests"),
                Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper"),
                Declaration(DependencyEcosystem.Go, "go.mod", "gorm.io/gorm"),
                Declaration(DependencyEcosystem.DotNet, "App.csproj", "Npgsql")
            ],
            [
                SourceFile("app.py", "import requests\n"),
                SourceFile("Data.cs", "using Dapper;\nusing Npgsql;\n"),
                SourceFile("db.go", "package db\n\nimport \"gorm.io/gorm\"\n")
            ]);

        Assert.Equal(
            new[] { "Dapper", "Npgsql", "gorm.io/gorm", "requests" },
            evidence.References.Select(reference => reference.Declaration.PackageId));

        Assert.Equal(
            new[]
            {
                DependencyEcosystem.DotNet,
                DependencyEcosystem.DotNet,
                DependencyEcosystem.Go,
                DependencyEcosystem.Python
            },
            evidence.References.Select(reference => reference.Ecosystem));
    }

    [Fact]
    public void Compose_IsDeterministicAcrossInputOrder()
    {
        RepositoryDependencyDeclaration[] declarations =
        [
            Declaration(DependencyEcosystem.DotNet, "App.csproj", "Dapper"),
            Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")
        ];

        RepositorySourceFile[] files =
        [
            SourceFile("Data.cs", "using Dapper;\n"),
            SourceFile("app.py", "import requests\n")
        ];

        RepositorySourceReferenceEvidence first = Compose(declarations, files);
        RepositorySourceReferenceEvidence second = Compose(
            declarations.Reverse().ToArray(),
            files.Reverse().ToArray());

        Assert.Equal(
            first.References.Select(reference => reference.Declaration.PackageId),
            second.References.Select(reference => reference.Declaration.PackageId));
    }

    [Fact]
    public void SourceLocation_CarriesOnlyPathFormAndLine()
    {
        string[] properties = typeof(DependencySourceReferenceLocation)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "Form", "Line", "SourcePath" }, properties);
    }

    [Fact]
    public void Result_ExposesNoUnreferencedCountOrCoverageSurface()
    {
        HashSet<string> forbidden = new(StringComparer.OrdinalIgnoreCase)
        {
            "unreferenced", "count", "counts", "ratio", "coverage", "score",
            "weight", "confidence", "snippet", "hash", "content", "percentage"
        };

        IEnumerable<string> words = typeof(RepositorySourceReferenceEvidence)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Concat(typeof(DependencySourceReferenceEvidence)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .SelectMany(property => System.Text.RegularExpressions.Regex
                .Split(property.Name, "(?<!^)(?=[A-Z])"));

        Assert.DoesNotContain(words, forbidden.Contains);
    }

    // ------------------------------------------ availability and completeness --

    [Fact]
    public void Compose_SourceFilesUnavailable_IsUndetermined()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")],
            [],
            includeSourceFilesAvailability: false);

        Assert.True(evidence.IsDetectionUndetermined);
        Assert.False(evidence.IsDetectionSurfaceComplete);
        Assert.Empty(evidence.References);
    }

    [Fact]
    public void Compose_DependencyManifestsUnavailable_IsUndetermined()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [],
            [SourceFile("app.py", "import requests\n")],
            includeManifestsAvailability: false);

        Assert.True(evidence.IsDetectionUndetermined);
    }

    [Fact]
    public void Compose_IncompleteSourceFiles_AreAvailableButIncomplete()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")],
            [SourceFile("app.py", "import requests\n")],
            areSourceFilesComplete: false);

        Assert.False(evidence.IsDetectionUndetermined);
        Assert.False(evidence.IsDetectionSurfaceComplete);
        Assert.Single(evidence.References);
    }

    [Fact]
    public void Compose_CompleteAndEmpty_IsSurfaceCompleteWithNoObservation()
    {
        RepositorySourceReferenceEvidence evidence = Compose(
            [Declaration(DependencyEcosystem.Python, "requirements.txt", "requests")],
            [SourceFile("app.py", "value = 1\n")]);

        Assert.Empty(evidence.References);
        Assert.True(evidence.IsDetectionSurfaceComplete);
    }

    [Fact]
    public void Compose_CarriesTheRuleVersionAndAnalysisReference()
    {
        RepositorySourceReferenceEvidence evidence = Compose([], []);

        Assert.Same(SourceReferenceRuleVersion.V1, evidence.RuleVersion);
        Assert.Equal("octocat", evidence.Analysis.Repository.Owner);
    }

    // ----------------------------------------------------------- helpers ----

    private static RepositorySourceReferenceEvidence Compose(
        IReadOnlyList<RepositoryDependencyDeclaration> declarations,
        IReadOnlyList<RepositorySourceFile> sourceFiles,
        bool areDeclarationsComplete = true,
        bool areSourceFilesComplete = true,
        bool includeManifestsAvailability = true,
        bool includeSourceFilesAvailability = true)
    {
        DataAvailability? manifests = includeManifestsAvailability
            ? DataAvailability.Available(RepositoryDataKeys.DependencyManifests, CapturedAt)
            : null;

        DataAvailability? sources = includeSourceFilesAvailability
            ? DataAvailability.Available(RepositoryDataKeys.SourceFiles, CapturedAt)
            : null;

        RepositorySnapshot snapshot = CreateSnapshot(
            declarations,
            sourceFiles,
            areDeclarationsComplete,
            areSourceFilesComplete,
            manifests,
            sources);

        return new RepositorySourceReferenceEvidencePolicy().Compose(CreateAnalysis(snapshot));
    }

    private static RepositoryDependencyDeclaration Declaration(
        DependencyEcosystem ecosystem,
        string manifestPath,
        string packageId,
        string? declaredVersion = null) =>
        new(ecosystem, manifestPath, packageId, declaredVersion);

    private static RepositorySourceFile SourceFile(string path, string content) =>
        new(path, content);

    private static RepositorySnapshot CreateSnapshot(
        IReadOnlyList<RepositoryDependencyDeclaration> declarations,
        IReadOnlyList<RepositorySourceFile> sourceFiles,
        bool areDeclarationsComplete,
        bool areSourceFilesComplete,
        DataAvailability? manifests,
        DataAvailability? sources)
    {
        List<DataAvailability> availability = [];

        if (manifests is not null)
        {
            availability.Add(manifests);
        }

        if (sources is not null)
        {
            availability.Add(sources);
        }

        return new RepositorySnapshot(
            new RepositoryIdentity(
                "octocat",
                "hello-world",
                new Uri("https://github.com/octocat/hello-world"),
                "main",
                isPublic: true,
                isArchived: false,
                isFork: false,
                isTemplate: false,
                primaryLanguage: "C#"),
            CapturedAt,
            headCommitSha: "abc123",
            metadata: null,
            readme: null,
            fileTree: null,
            commits: null,
            issues: null,
            pullRequests: null,
            releases: null,
            workflows: null,
            topics: null,
            license: null,
            languages: null,
            contributors: null,
            availability.Count == 0 ? null : availability,
            collectionWarnings: null,
            tags: null,
            licenseFile: null,
            documentationMarkdown: null,
            new RepositoryCollection<RepositoryDependencyDeclaration>(
                declarations,
                isComplete: areDeclarationsComplete),
            new RepositoryCollection<RepositorySourceFile>(
                sourceFiles,
                isComplete: areSourceFilesComplete));
    }

    private static RepositoryAnalysis CreateAnalysis(RepositorySnapshot snapshot) =>
        new(
            Guid.Parse("07300000-0000-0000-0000-000000000001"),
            snapshot.Repository,
            snapshot,
            AnalysisStatus.Partial,
            Version,
            CapturedAt,
            CapturedAt,
            signalResults: null,
            categoryResults: null,
            new ScoreBreakdown(categoryBreakdowns: null),
            strengths: null,
            criticalGaps: null,
            new ImprovementPlan([], CapturedAt, Version.RuleSetVersion),
            limitationKeys: null,
            warnings: null);
}
