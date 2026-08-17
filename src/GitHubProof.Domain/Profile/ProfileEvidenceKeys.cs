namespace GitHubProof.Domain.Profile;

// Canonical observation key vocabulary for non-signal repository evidence.
// Signal observations use their canonical signal key unchanged.
public static class ProfileEvidenceKeys
{
    public const string LanguagesDataKey = "languages";
    public const string LanguageObservationPrefix = "language.";

    public static string CreateLanguageObservationKey(string language) =>
        LanguageObservationPrefix
        + ProfileValue.RequireValue(language, nameof(language));

    public static bool TryReadLanguage(
        string observationKey,
        out string language)
    {
        if (observationKey is not null
            && observationKey.StartsWith(
                LanguageObservationPrefix,
                StringComparison.Ordinal)
            && observationKey.Length > LanguageObservationPrefix.Length)
        {
            language = observationKey[LanguageObservationPrefix.Length..];
            return true;
        }

        language = string.Empty;
        return false;
    }
}
