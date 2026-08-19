using System.Security.Cryptography;
using System.Text;

namespace GitHubProof.Domain.Profile.ValueObjects;

// Deterministic identity of one canonical normalized profile snapshot.
//
// It answers "what semantic snapshot was observed", never "when was it
// observed": observation time under any field name is excluded by the
// components that describe their own content.
//
// Its purpose is deterministic input provenance. It is not cryptographic
// attestation and not legal proof; the digest is only a stable, collision-
// resistant encoding of the canonical token stream.
public sealed record SnapshotIdentity
{
    private const string Algorithm = "sha256";

    private SnapshotIdentity(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static SnapshotIdentity FromSemanticContent(
        ISnapshotSemanticComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        SnapshotSemanticWriter writer = new();
        component.WriteSemanticContent(writer);

        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(writer.Build()));

        return new SnapshotIdentity(
            $"{Algorithm}:{Convert.ToHexStringLower(digest)}");
    }

    public override string ToString() => Value;
}
