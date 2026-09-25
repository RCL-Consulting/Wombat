using System.Globalization;

namespace Wombat.Domain.Epas;

/// <summary>How a schema field's <c>scale_key</c> names its entrustment scale (T253).</summary>
public enum ScaleBindingKind
{
    /// <summary>The scale's id, as digits: what the activity-type builder writes (<c>"7"</c>).</summary>
    Id,

    /// <summary>The scale's <see cref="EntrustmentScale.SeedKey" /> after <see cref="ScaleBinding.SeedKeyPrefix" />: what a
    /// seed writes (<c>"seed:cpsa:scale:v11.1"</c>).</summary>
    SeedKey,

    /// <summary>The scale's exact name: anything else. No seed and no builder writes one; older schemas did.</summary>
    Name
}

/// <summary>
/// What a schema field's <c>scale_key</c> says about the scale it is bound to: the one reading of that string (T253).
/// </summary>
/// <remarks>
/// <para>
/// A <c>scale_key</c> is one of three forms, told apart by the string alone, never by what the database holds:
/// <list type="bullet">
///   <item><c>seed:</c> followed by a seed key binds the scale whose <see cref="EntrustmentScale.SeedKey" /> is that key.
///   The seeds bind this way, so an administrator's rename of the College's ladder cannot unbind them: the seed key is
///   written once, by the seeder, and no command changes it.</item>
///   <item>Digits alone bind the scale with that id. The activity-type builder binds this way.</item>
///   <item>Anything else binds the scale with exactly that name, ordinal and after trimming.</item>
/// </list>
/// </para>
/// <para>
/// The forms cannot overlap, because a scale's name may be neither of the other two: the create and update validators
/// refuse a name <see cref="Parse" /> does not read as <see cref="ScaleBindingKind.Name" />. So a key binds at most one
/// scale, and "which scale does this key bind" and "does this key bind that scale" are the same question. The resolvers
/// (credit, the rung picker, every label) ask the first; the scale's delete and rename ask the second.
/// </para>
/// <para>
/// Why a prefix and not the bare seed key: a bare <c>cpsa:scale:v11.1</c> would be a name too, and which one won would be
/// a precedence rule every reader had to share. And why a string at all, rather than a second schema property: a
/// <c>scale_key</c> stays one string, so the schema DSL, its round trip and the builder carry it unchanged.
/// </para>
/// </remarks>
public sealed record ScaleBinding
{
    /// <summary>What starts a <c>scale_key</c> that binds by seed key.</summary>
    public const string SeedKeyPrefix = "seed:";

    private ScaleBinding(ScaleBindingKind kind, string key, int? scaleId, string? seedKey, string? name)
    {
        Kind = kind;
        Key = key;
        ScaleId = scaleId;
        SeedKey = seedKey;
        Name = name;
    }

    public ScaleBindingKind Kind { get; }

    /// <summary>The <c>scale_key</c> as written, trimmed.</summary>
    public string Key { get; }

    /// <summary>The id it binds, when <see cref="Kind" /> is <see cref="ScaleBindingKind.Id" />.</summary>
    public int? ScaleId { get; }

    /// <summary>The seed key it binds, without the prefix, when <see cref="Kind" /> is <see cref="ScaleBindingKind.SeedKey" />.</summary>
    public string? SeedKey { get; }

    /// <summary>The name it binds, when <see cref="Kind" /> is <see cref="ScaleBindingKind.Name" />.</summary>
    public string? Name { get; }

    /// <summary>The <c>scale_key</c> that binds the scale with this seed key.</summary>
    public static string ForSeedKey(string seedKey) => SeedKeyPrefix + seedKey;

    /// <summary>Reads a <c>scale_key</c>, or returns null when it is blank (a field bound to no scale).</summary>
    public static ScaleBinding? Parse(string? scaleKey)
    {
        if (string.IsNullOrWhiteSpace(scaleKey))
        {
            return null;
        }

        var key = scaleKey.Trim();

        if (key.StartsWith(SeedKeyPrefix, StringComparison.Ordinal))
        {
            return new ScaleBinding(ScaleBindingKind.SeedKey, key, null, key[SeedKeyPrefix.Length..].Trim(), null);
        }

        // Digits only: no sign, no separators, no surrounding space (trimmed above). That is exactly what the builder
        // writes (an int's ToString()), and it leaves "-1" or "1,000" to be names, which the validators then allow.
        if (int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
        {
            return new ScaleBinding(ScaleBindingKind.Id, key, id, null, null);
        }

        return new ScaleBinding(ScaleBindingKind.Name, key, null, null, key);
    }

    /// <summary>
    /// True when a scale's name could not be bound by name, because <see cref="Parse" /> reads it as an id or a seed key.
    /// The create and update validators refuse such a name.
    /// </summary>
    public static bool IsReservedName(string? name)
        => Parse(name) is { Kind: not ScaleBindingKind.Name };

    /// <summary>True when this binding names the scale with this id, seed key and name.</summary>
    public bool Binds(int scaleId, string? scaleSeedKey, string scaleName) => Kind switch
    {
        ScaleBindingKind.Id => ScaleId == scaleId,
        ScaleBindingKind.SeedKey => scaleSeedKey is not null && string.Equals(SeedKey, scaleSeedKey, StringComparison.Ordinal),
        _ => string.Equals(Name, scaleName, StringComparison.Ordinal)
    };

    /// <summary>True when this binding names <paramref name="scale" />.</summary>
    public bool Binds(EntrustmentScale scale) => Binds(scale.Id, scale.SeedKey, scale.Name);
}
