namespace GitHubProof.Web.Localization;

// Web-owned, localized presentation copy for the bounded Meaning surface.
// It carries no report, evidence, repository or analysis identity.
public sealed class ProfileMeaningPresentation
{
    public ProfileMeaningPresentation(
        string technicalNarrative,
        string practiceNarrative,
        string provenance)
    {
        TechnicalNarrative = RequireText(technicalNarrative, nameof(technicalNarrative));
        PracticeNarrative = RequireText(practiceNarrative, nameof(practiceNarrative));
        Provenance = RequireText(provenance, nameof(provenance));
    }

    public string TechnicalNarrative { get; }

    public string PracticeNarrative { get; }

    public string Provenance { get; }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Meaning presentation text cannot be blank.", parameterName);
        }

        return value;
    }
}
