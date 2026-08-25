using System.Text.Json.Nodes;

namespace GitHubProof.Infrastructure.GitHub.Profile;

// One read-only GraphQL operation to send over the Profile transport.
//
// S1 introduces no production repository query: this type carries whatever the
// caller supplies, and the only caller in this session is a test. Variables are
// a JsonObject rather than an arbitrary object graph so the body is built
// without reflection-based serialization and stays exactly what the caller
// wrote.
public sealed record GitHubGraphQlRequest
{
    public GitHubGraphQlRequest(string query, JsonObject? variables = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException(
                "A GraphQL request requires a query.",
                nameof(query));
        }

        Query = query;
        Variables = variables;
    }

    public string Query { get; }

    public JsonObject? Variables { get; }
}
