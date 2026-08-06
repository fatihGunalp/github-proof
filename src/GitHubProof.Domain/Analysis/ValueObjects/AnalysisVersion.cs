namespace GitHubProof.Domain.Analysis.ValueObjects;

public sealed record AnalysisVersion
{
    public AnalysisVersion(
        string analyzerVersion,
        string ruleSetVersion,
        int reportSchemaVersion)
    {
        AnalyzerVersion = RequireValue(
            analyzerVersion,
            nameof(analyzerVersion));

        RuleSetVersion = RequireValue(
            ruleSetVersion,
            nameof(ruleSetVersion));

        if (reportSchemaVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reportSchemaVersion),
                reportSchemaVersion,
                "Report schema version must be greater than zero.");
        }

        ReportSchemaVersion = reportSchemaVersion;
    }

    public string AnalyzerVersion { get; }

    public string RuleSetVersion { get; }

    public int ReportSchemaVersion { get; }

    public override string ToString()
    {
        return
            $"Analyzer={AnalyzerVersion}; " +
            $"Rules={RuleSetVersion}; " +
            $"Schema={ReportSchemaVersion}";
    }

    private static string RequireValue(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be null, empty, or whitespace.",
                parameterName);
        }

        return value.Trim();
    }
}
