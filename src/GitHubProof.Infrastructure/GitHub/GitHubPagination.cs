namespace GitHubProof.Infrastructure.GitHub;

internal static class GitHubPagination
{
    public static Uri? FindNextPage(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Link", out IEnumerable<string>? links))
        {
            return null;
        }

        foreach (string link in links.SelectMany(value => value.Split(',')))
        {
            if (!link.Contains("rel=\"next\"", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int start = link.IndexOf('<');
            int end = link.IndexOf('>');

            if (start < 0 || end <= start + 1)
            {
                throw new ArgumentException("GitHub pagination link is malformed.", nameof(response));
            }

            if (!Uri.TryCreate(link[(start + 1)..end], UriKind.Absolute, out Uri? nextPage))
            {
                throw new ArgumentException("GitHub pagination link is invalid.", nameof(response));
            }

            return nextPage;
        }

        return null;
    }
}
