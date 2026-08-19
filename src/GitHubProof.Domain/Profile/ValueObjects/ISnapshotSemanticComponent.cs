namespace GitHubProof.Domain.Profile.ValueObjects;

// Implemented by every type that owns part of the canonical normalized profile
// snapshot semantic representation. Each type describes its own content, so
// snapshot identity is never derived from a hand-maintained field inventory
// kept somewhere else.
public interface ISnapshotSemanticComponent
{
    void WriteSemanticContent(SnapshotSemanticWriter writer);
}
