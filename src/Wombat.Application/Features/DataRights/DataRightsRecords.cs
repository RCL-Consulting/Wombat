namespace Wombat.Application.Features.DataRights;

/// <summary>
/// The records a correction names (T387, Q26): a short fixed list, each a key stored in
/// <c>DataRightsRectification.TargetType</c>, and the label every page reads it by ("Account › Last name"). One place, so
/// the request page, My data rights' answer and the command cannot name a record differently.
/// </summary>
/// <remarks>
/// <para>
/// Every record is the requester's own, so a key names no id. Other carries the record's name typed by the person
/// recording it, after <see cref="OtherPrefix" />, and the column holds 200 characters, so the name holds at most
/// <see cref="OtherNameMaxLength" />.
/// </para>
/// <para>
/// A name that comes from an institutional sign-in is recorded as Other (F6): the radios for the two names are not
/// offered, since the institution, not Wombat, corrects it.
/// </para>
/// </remarks>
public static class DataRightsRecords
{
    public const string AccountFirstName = "Account.FirstName";
    public const string AccountLastName = "Account.LastName";
    public const string ProgrammeStart = "TraineeProfile.ProgrammeStart";
    public const string OtherPrefix = "Other:";

    /// <summary>The column's 200 characters less <see cref="OtherPrefix" />'s six.</summary>
    public const int OtherNameMaxLength = 194;

    /// <summary>The three fixed records, in the order the Correction section offers them (Other follows them).</summary>
    public static IReadOnlyList<string> Fixed { get; } = [AccountFirstName, AccountLastName, ProgrammeStart];

    /// <summary>Other's key for a record named by hand: "Other:Home address".</summary>
    public static string Other(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return OtherPrefix + name.Trim();
    }

    /// <summary>A fixed record's key, or Other's with a name of 1 to <see cref="OtherNameMaxLength" /> characters.</summary>
    public static bool IsValid(string? targetType)
    {
        if (string.IsNullOrWhiteSpace(targetType))
        {
            return false;
        }

        if (Fixed.Contains(targetType, StringComparer.Ordinal))
        {
            return true;
        }

        return OtherName(targetType) is { Length: > 0 and <= OtherNameMaxLength } name && name.Trim().Length == name.Length;
    }

    /// <summary>Other's name, or null for a fixed record's key.</summary>
    public static string? OtherName(string targetType)
        => targetType.StartsWith(OtherPrefix, StringComparison.Ordinal) ? targetType[OtherPrefix.Length..] : null;

    /// <summary>
    /// The record as a person reads it: "Account › First name", "Account › Last name", "Trainee profile › Programme start",
    /// "Other › Home address". An unknown key reads as itself, so nothing stored is ever hidden.
    /// </summary>
    public static string Label(string targetType) => targetType switch
    {
        AccountFirstName => "Account › First name",
        AccountLastName => "Account › Last name",
        ProgrammeStart => "Trainee profile › Programme start",
        _ when OtherName(targetType) is { } name => $"Other › {name}",
        _ => targetType
    };
}
