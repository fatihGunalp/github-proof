using System.Globalization;
using System.Text;

namespace GitHubProof.Domain.Profile.ValueObjects;

// Builds the canonical token stream that snapshot identity is derived from.
//
// Every value is written with its field name, a type tag and an explicit
// length, so no two different semantic contents can produce the same stream.
// All formatting is culture-invariant, so locale never participates.
public sealed class SnapshotSemanticWriter
{
    private readonly StringBuilder buffer = new();

    public SnapshotSemanticWriter WriteAbsent(string field) =>
        Append(field, "absent", string.Empty);

    public SnapshotSemanticWriter Write(string field, string? value) =>
        value is null
            ? WriteAbsent(field)
            : Append(field, "text", value);

    public SnapshotSemanticWriter Write(string field, bool value) =>
        Append(field, "bool", value ? "true" : "false");

    public SnapshotSemanticWriter Write(string field, int? value) =>
        value is null
            ? WriteAbsent(field)
            : Append(
                field,
                "int",
                value.Value.ToString(CultureInfo.InvariantCulture));

    // Repository facts such as creation or last-push time are semantic content.
    // Observation time is not, and is excluded by its writer rather than here.
    public SnapshotSemanticWriter Write(string field, DateTimeOffset? value) =>
        value is null
            ? WriteAbsent(field)
            : Append(
                field,
                "instant",
                value.Value.ToUniversalTime().ToString(
                    "yyyy-MM-ddTHH:mm:ss.fffffffZ",
                    CultureInfo.InvariantCulture));

    // The enum name is the semantic identity, so renumbering members cannot
    // silently change identity.
    public SnapshotSemanticWriter Write<TEnum>(string field, TEnum value)
        where TEnum : struct, Enum =>
        Append(field, "enum", value.ToString());

    public SnapshotSemanticWriter Write(string field, Uri? value) =>
        value is null
            ? WriteAbsent(field)
            : Append(field, "uri", value.AbsoluteUri);

    public SnapshotSemanticWriter WriteComponent(
        string field,
        ISnapshotSemanticComponent? component)
    {
        if (component is null)
        {
            return WriteAbsent(field);
        }

        Append(field, "component", string.Empty);
        component.WriteSemanticContent(this);

        return Append(field, "component-end", string.Empty);
    }

    // The caller supplies the canonical order. Enumeration order that is
    // semantically irrelevant must be normalized before it reaches this
    // writer, never here.
    public SnapshotSemanticWriter WriteComponents(
        string field,
        IEnumerable<ISnapshotSemanticComponent> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        int index = 0;

        foreach (ISnapshotSemanticComponent component in components)
        {
            WriteComponent(
                $"{field}[{index.ToString(CultureInfo.InvariantCulture)}]",
                component);
            index++;
        }

        return Append(
            $"{field}.count",
            "int",
            index.ToString(CultureInfo.InvariantCulture));
    }

    public SnapshotSemanticWriter WriteValues(
        string field,
        IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        int index = 0;

        foreach (string value in values)
        {
            Write($"{field}[{index.ToString(CultureInfo.InvariantCulture)}]", value);
            index++;
        }

        return Append(
            $"{field}.count",
            "int",
            index.ToString(CultureInfo.InvariantCulture));
    }

    internal string Build() => buffer.ToString();

    private SnapshotSemanticWriter Append(
        string field,
        string tag,
        string payload)
    {
        buffer
            .Append(field.Length.ToString(CultureInfo.InvariantCulture))
            .Append(':')
            .Append(field)
            .Append('|')
            .Append(tag)
            .Append('|')
            .Append(payload.Length.ToString(CultureInfo.InvariantCulture))
            .Append(':')
            .Append(payload)
            .Append(';');

        return this;
    }
}
