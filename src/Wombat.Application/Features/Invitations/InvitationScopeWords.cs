using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Invitations;

/// <summary>
/// An invitation's role and scope in the pages' words (T373, flow 11, Q13, S12; build-contracts § 0): Register's card ("as
/// Coordinator at Kgosi Kgari Teaching Hospital"), the invitation's mail, and the lists' scope lines ("Paediatrics /
/// Paediatrics"). One place, so the card, the mail and the rows cannot drift. The role is its label
/// (<see cref="WombatRoleLabels" />), never its key: until T373 the mail and the card said "as InstitutionalAdmin".
/// </summary>
public static class InvitationScopeWords
{
    /// <summary>
    /// "Paediatrics / Paediatrics": the speciality, then " / " and its sub-speciality when there is one; null when the
    /// invitation names no speciality (an Institutional admin, an unscoped Coordinator or Committee member, a College admin).
    /// </summary>
    public static string? Disciplines(string? specialityName, string? subSpecialityName)
    {
        if (string.IsNullOrWhiteSpace(specialityName))
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(subSpecialityName)
            ? specialityName.Trim()
            : $"{specialityName.Trim()} / {subSpecialityName.Trim()}";
    }

    /// <summary>
    /// "Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital": the role's label, then "in ‹disciplines›"
    /// only when the invitation has one (<see cref="Disciplines" />), then "at ‹institution›", or "at ‹College›" for a
    /// College admin (R3-0-steps 2.8). The caller writes "as" before it: "This invitation is for ‹address›, as …", "You are
    /// invited to Wombat as …".
    /// </summary>
    public static string RoleAndScope(
        string targetRole,
        string? institutionName,
        string? collegeName,
        string? specialityName,
        string? subSpecialityName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetRole);

        var words = WombatRoleLabels.For(targetRole);
        if (Disciplines(specialityName, subSpecialityName) is { } disciplines)
        {
            words += $" in {disciplines}";
        }

        var place = string.IsNullOrWhiteSpace(institutionName) ? collegeName : institutionName;
        if (!string.IsNullOrWhiteSpace(place))
        {
            words += $" at {place.Trim()}";
        }

        return words;
    }
}
