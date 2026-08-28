using System.Globalization;
using System.Text;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Application.Analysis.Presentation;

public sealed class RepositoryReportMarkdownRenderer
{
    public string Render(RepositoryReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        ValidateReport(report);

        StringBuilder builder = new();

        AppendLine(builder, $"# {report.Identity.FullName}");
        AppendBlankLine(builder);

        foreach (string sectionKey in report.SectionOrder)
        {
            switch (sectionKey)
            {
                case RepositoryReportSectionKeys.Identity:
                    RenderIdentity(builder, report.Identity);
                    break;

                case RepositoryReportSectionKeys.Proves:
                    RenderHighlights(builder, report.Proves, includeRecommendation: false);
                    break;

                case RepositoryReportSectionKeys.Weakens:
                    RenderHighlights(builder, report.Weakens, includeRecommendation: true);
                    break;

                case RepositoryReportSectionKeys.DoFirst:
                    RenderActions(builder, report.DoFirst);
                    break;

                case RepositoryReportSectionKeys.Uncertainty:
                    RenderUncertainty(
                        builder,
                        report.Uncertainty,
                        report.Warnings,
                        report.Limitations);
                    break;

                case RepositoryReportSectionKeys.CategoriesAndScore:
                    RenderScoreSummary(builder, report.ScoreSummary);
                    break;

                case RepositoryReportSectionKeys.SignalDetails:
                    RenderSignalDetails(builder, report.SignalDetails);
                    break;

                case RepositoryReportSectionKeys.Disclaimer:
                    RenderDisclaimer(builder, report.Disclaimer);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported report section key '{sectionKey}'.");
            }
        }

        return builder.ToString();
    }

    private static void ValidateReport(RepositoryReport report)
    {
        if (!report.SectionOrder.SequenceEqual(
                RepositoryReportSectionKeys.CanonicalOrder,
                StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "RepositoryReport section order must match the canonical evidence-first order.",
                nameof(report));
        }

        if (!string.Equals(
                report.Identity.SectionKey,
                RepositoryReportSectionKeys.Identity,
                StringComparison.Ordinal)
            || !string.Equals(
                report.Proves.Key,
                RepositoryReportSectionKeys.Proves,
                StringComparison.Ordinal)
            || !string.Equals(
                report.Weakens.Key,
                RepositoryReportSectionKeys.Weakens,
                StringComparison.Ordinal)
            || !string.Equals(
                report.DoFirst.Key,
                RepositoryReportSectionKeys.DoFirst,
                StringComparison.Ordinal)
            || !string.Equals(
                report.Uncertainty.Key,
                RepositoryReportSectionKeys.Uncertainty,
                StringComparison.Ordinal)
            || !string.Equals(
                report.ScoreSummary.SectionKey,
                RepositoryReportSectionKeys.CategoriesAndScore,
                StringComparison.Ordinal)
            || !string.Equals(
                report.SignalDetails.Key,
                RepositoryReportSectionKeys.SignalDetails,
                StringComparison.Ordinal)
            || !string.Equals(
                report.Disclaimer.SectionKey,
                RepositoryReportSectionKeys.Disclaimer,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "RepositoryReport section keys do not match the canonical report contract.",
                nameof(report));
        }

        if (report.Proves.Items.Count > 3
            || report.Weakens.Items.Count > 3
            || report.DoFirst.Items.Count > 3)
        {
            throw new ArgumentException(
                "RepositoryReport cannot contain more than three strengths, gaps, or first actions.",
                nameof(report));
        }
    }

    private static void RenderIdentity(
        StringBuilder builder,
        RepositoryReportIdentity identity)
    {
        AppendSectionHeading(builder, identity.SectionTitle);

        AppendLine(builder, $"**{identity.FullName}**");
        AppendLine(builder, identity.Url.AbsoluteUri);
        AppendLine(builder, identity.DefaultBranch);

        if (!string.IsNullOrWhiteSpace(identity.HeadCommitSha))
        {
            AppendLine(builder, identity.HeadCommitSha);
        }

        if (!string.IsNullOrWhiteSpace(identity.PrimaryLanguage))
        {
            AppendLine(builder, identity.PrimaryLanguage);
        }

        AppendLine(builder, identity.AnalysisStatusLabel);
        AppendLine(
            builder,
            identity.CapturedAt.ToString("O", CultureInfo.InvariantCulture));

        AppendLine(
            builder,
            string.Join(
                " · ",
                identity.AnalyzerVersion,
                identity.RuleSetVersion,
                identity.ReportSchemaVersion.ToString(CultureInfo.InvariantCulture)));

        AppendBlankLine(builder);
    }

    private static void RenderHighlights(
        StringBuilder builder,
        RepositoryReportSection<RepositoryReportHighlight> section,
        bool includeRecommendation)
    {
        AppendSectionHeading(builder, section.Title);

        foreach (RepositoryReportHighlight item in section.Items)
        {
            AppendLine(builder, $"### {item.Title}");
            AppendBlankLine(builder);

            AppendLine(builder, item.Explanation);
            AppendBlankLine(builder);

            AppendLine(builder, item.WhyItMatters);
            AppendBlankLine(builder);

            if (includeRecommendation
                && !string.IsNullOrWhiteSpace(item.Recommendation))
            {
                AppendLine(builder, item.Recommendation);
                AppendBlankLine(builder);
            }

            AppendEvidence(builder, item.Evidence);
        }
    }

    private static void RenderActions(
        StringBuilder builder,
        RepositoryReportSection<RepositoryReportAction> section)
    {
        AppendSectionHeading(builder, section.Title);

        foreach (RepositoryReportAction action in section.Items)
        {
            AppendLine(builder, $"### {action.Priority}. {action.Title}");
            AppendBlankLine(builder);

            AppendLine(builder, action.Recommendation);
            AppendBlankLine(builder);

            AppendLine(builder, action.WhyItMatters);
            AppendBlankLine(builder);

            AppendLine(builder, action.EffortLabel);
            AppendBlankLine(builder);

            AppendEvidence(builder, action.Evidence);
        }
    }

    private static void RenderUncertainty(
        StringBuilder builder,
        RepositoryReportSection<RepositoryReportUncertainty> section,
        IReadOnlyList<RepositoryReportWarning> warnings,
        IReadOnlyList<RepositoryReportLimitation> limitations)
    {
        AppendSectionHeading(builder, section.Title);

        foreach (RepositoryReportUncertainty item in section.Items)
        {
            AppendLine(
                builder,
                $"### {item.Title} — {item.StatusLabel}");

            AppendBlankLine(builder);

            AppendLine(builder, item.Explanation);
            AppendBlankLine(builder);

            AppendEvidence(builder, item.Evidence);
        }

        foreach (RepositoryReportWarning warning in warnings)
        {
            AppendLine(builder, $"- {warning.Message}");
        }

        if (warnings.Count > 0)
        {
            AppendBlankLine(builder);
        }

        foreach (RepositoryReportLimitation limitation in limitations)
        {
            AppendLine(builder, $"- {limitation.Message}");
        }

        if (limitations.Count > 0)
        {
            AppendBlankLine(builder);
        }
    }

    private static void RenderScoreSummary(
        StringBuilder builder,
        RepositoryReportScoreSummary scoreSummary)
    {
        AppendSectionHeading(builder, scoreSummary.SectionTitle);

        foreach (RepositoryReportCategory category in scoreSummary.Categories)
        {
            AppendLine(builder, $"### {category.Label}");
            AppendBlankLine(builder);

            if (category.NormalizedScore is not null)
            {
                AppendLine(
                    builder,
                    $"{category.NormalizedScore.Value.ToString(CultureInfo.InvariantCulture)}/100 · {category.StatusLabel}");
            }
            else
            {
                AppendLine(builder, category.StatusLabel);
            }

            AppendLine(
                builder,
                $"{category.Score.ToString(CultureInfo.InvariantCulture)} / {category.MaximumScore.ToString(CultureInfo.InvariantCulture)}");

            AppendBlankLine(builder);
        }

        if (scoreSummary.NormalizedScore is not null)
        {
            AppendLine(
                builder,
                $"**{scoreSummary.NormalizedScore.Value.ToString(CultureInfo.InvariantCulture)}/100**");

            if (!string.IsNullOrWhiteSpace(scoreSummary.Label))
            {
                AppendLine(builder, scoreSummary.Label);
            }
        }
        else if (!string.IsNullOrWhiteSpace(scoreSummary.Label))
        {
            AppendLine(builder, scoreSummary.Label);
        }

        AppendBlankLine(builder);
    }

    private static void RenderSignalDetails(
        StringBuilder builder,
        RepositoryReportSection<SignalPresentation> section)
    {
        AppendSectionHeading(builder, section.Title);

        foreach (SignalPresentation signal in section.Items)
        {
            AppendLine(builder, $"### {signal.Title}");
            AppendBlankLine(builder);

            if (signal.Score is not null)
            {
                AppendLine(
                    builder,
                    $"{signal.StatusLabel} · {signal.Score.Value.ToString(CultureInfo.InvariantCulture)} / {signal.MaximumScore.ToString(CultureInfo.InvariantCulture)}");
            }
            else
            {
                AppendLine(builder, signal.StatusLabel);
            }

            AppendBlankLine(builder);

            AppendLine(builder, signal.Description);
            AppendBlankLine(builder);

            AppendLine(builder, signal.Explanation);
            AppendBlankLine(builder);

            AppendLine(builder, signal.WhyItMatters);
            AppendBlankLine(builder);

            if (!string.IsNullOrWhiteSpace(signal.Recommendation))
            {
                AppendLine(builder, signal.Recommendation);
                AppendBlankLine(builder);
            }

            foreach (string limitation in signal.Limitations)
            {
                AppendLine(builder, $"- {limitation}");
            }

            if (signal.Limitations.Count > 0)
            {
                AppendBlankLine(builder);
            }

            AppendEvidence(builder, signal.Evidence);
        }
    }

    private static void RenderDisclaimer(
        StringBuilder builder,
        RepositoryReportDisclaimer disclaimer)
    {
        AppendSectionHeading(builder, disclaimer.SectionTitle);

        AppendLine(builder, disclaimer.Text);
        AppendBlankLine(builder);

        AppendLine(
            builder,
            $"v{disclaimer.PolicyVersion.ToString(CultureInfo.InvariantCulture)}");
    }

    private static void AppendEvidence(
        StringBuilder builder,
        IReadOnlyList<EvidenceItem> evidenceItems)
    {
        foreach (EvidenceItem evidence in evidenceItems)
        {
            AppendLine(
                builder,
                $"- {evidence.Source}: {evidence.Value}");

            if (!string.IsNullOrWhiteSpace(evidence.Location))
            {
                AppendLine(builder, $"  {evidence.Location}");
            }

            AppendLine(
                builder,
                $"  {evidence.ObservedAt.ToString("O", CultureInfo.InvariantCulture)}");

            if (!string.IsNullOrWhiteSpace(evidence.Notes))
            {
                AppendLine(builder, $"  {evidence.Notes}");
            }
        }

        if (evidenceItems.Count > 0)
        {
            AppendBlankLine(builder);
        }
    }

    private static void AppendSectionHeading(
        StringBuilder builder,
        string title)
    {
        AppendLine(builder, $"## {title}");
        AppendBlankLine(builder);
    }

    private static void AppendLine(
        StringBuilder builder,
        string value) =>
        builder.Append(value).Append('\n');

    private static void AppendBlankLine(StringBuilder builder) =>
        builder.Append('\n');
}