using Wombat.Application.Features.Sso;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;

namespace Wombat.Web.Components.Pages.Admin.Sso;

/// <summary>
/// The SSO mappings page's words (T376, flow 12, boards R3-O01–O10), in one place so its tests hold the page to them. A
/// bracketed figure on a board (‹provider›, ‹group display name›) is read from the data.
/// </summary>
public static class SsoMappingWords
{
    /// <summary>Her subtitle (O01): her institution's groups, at each person's next sign-in.</summary>
    public const string Subtitle =
        "Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.";

    /// <summary>The Administrator's subtitle, as built (O02: the setting's name kept).</summary>
    public const string AdministratorSubtitle =
        "Map external identity provider groups to Wombat roles. Changes take effect on the next login for each user.";

    public const string NotSetUpHeading = "Institutional sign-in is not set up";

    /// <summary>O01's first sentence (T326), naming her institution.</summary>
    public static string NotSetUpFor(string institution)
        => $"Institutional sign-in is not set up for {institution}. A Wombat administrator sets it up; ask them if your " +
           "hospital signs staff in through its own system.";

    public const string NotSetUpUntilThen =
        "Until then, your people sign in with an email and a password, and there is nothing to change on this page.";

    public const string LoadingLine = "Loading the SSO mappings.";

    /// <summary>A failed read (O10; T329): fixed words, the failure logged.</summary>
    public const string LoadFailed =
        "Could not load the SSO mappings. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>An add or a delete that failed for a reason that is no refusal (T329).</summary>
    public const string ActionFailed =
        "Could not change the SSO mappings. Nothing has changed. Try again, or come back in a few minutes.";

    public const string AddHeading = "Add a mapping";

    public const string FormHelp =
        "* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.";

    public const string ChooseARole = "Choose a role";

    public const string RoleHelp =
        "Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.";

    public const string AnySpeciality = PersonScopeWords.AnySpeciality;

    // The page's own checks (T324: Wombat's words, never a property's name).
    public const string GroupIdMissing = "Type the group's id, as the provider sends it.";
    public const string GroupNameMissing = "Type the group's display name.";
    public const string RoleMissing = "Choose a role.";

    /// <summary>The built scope refusals (Spec 3), under the field that lacks it.</summary>
    public const string SpecialityAdminNeedsSpeciality = "Speciality administrators must be scoped to a speciality.";
    public const string NeedsSpecialityAndSubSpeciality = "The selected role requires speciality and sub-speciality scope.";

    public const string LiveHeading = "Current mappings";
    public const string GoneHeading = "Mappings left from a removed provider";
    public const string Empty = "No group gives a role yet. Use Add a mapping to give one.";
    public const string GoneRowNote = "Read only: its provider is no longer set up.";

    public const string DeleteTitle = "Delete this mapping?";
    public const string DeleteKeep = "Keep the mapping";
    public const string ScopeStays = "The scope it gave stays.";

    /// <summary>The Provider read-out's help when one provider serves the institution (O04).</summary>
    public static string OneProviderFor(string institution) => $"The one provider set up for {institution}.";

    /// <summary>The live list's caption (O05).</summary>
    public static string LiveCaption(string? institution)
        => institution is null
            ? "Each applies at a person's next sign-in, at its provider's institution."
            : $"At {institution}. Each applies at a person's next sign-in.";

    /// <summary>The gone list's caption (O03, S5).</summary>
    public static string GoneCaption(string? institution, bool administrator)
        => administrator
            ? "These name a provider no longer set up for their institution, so they give no new role; roles they already " +
              "gave remain. Delete one once it is no longer wanted."
            : $"Read only. These name a provider no longer set up for {institution ?? "your institution"}, so they give no " +
              "new role; roles they already gave remain. Only a Wombat administrator can delete them.";

    /// <summary>The help under the read-outs when only one speciality, sub-speciality or both are adopted (O04b).</summary>
    public static string OnlyOnesAdopted(string institution) => $"The only ones {institution} has adopted.";

    public static string OnlySpecialityAdopted(string institution) => $"The only speciality {institution} has adopted.";

    public static string OnlySubSpecialityAdopted(string institution, string speciality)
        => $"The only sub-speciality of {speciality} {institution} has adopted.";

    /// <summary>"Assessor", or nothing: the role in a person's words (Q14).</summary>
    public static string Role(SsoGroupMappingDto mapping) => WombatRoleLabels.For(mapping.WombatRole);

    /// <summary>
    /// What a mapping's members get beside the role: "Paediatrics / Paediatrics", "Paediatrics"; an Institutional admin
    /// mapping its institution; a role kept to no speciality "Any speciality" (S1's words).
    /// </summary>
    public static string Scope(SsoGroupMappingDto mapping)
    {
        if (mapping.WombatRole == WombatRoles.InstitutionalAdmin)
        {
            return mapping.InstitutionName ?? PersonScopeWords.AnySpeciality;
        }

        return PersonScopeWords.Disciplines(
                   mapping.SpecialityName is null ? [] : [mapping.SpecialityName],
                   mapping.SubSpecialityName is null ? [] : [mapping.SubSpecialityName])
               ?? PersonScopeWords.AnySpeciality;
    }

    /// <summary>O05's result after its bold "Mapping added.": "People in ‹group› get Assessor in Paediatrics / Paediatrics at their next sign-in."</summary>
    public static string AddedRest(string group, string role, string? scope)
        => scope is null
            ? $"People in {group} get {role} at their next sign-in."
            : $"People in {group} get {role} in {scope} at their next sign-in.";

    /// <summary>O08's result after its bold "Mapping deleted." (F8).</summary>
    public static string DeletedRest(string group, string role)
        => $"From each person's next sign-in, {group} no longer gives {role}.";

    /// <summary>The dialog's second sentence (F8, O07).</summary>
    public static string NoLongerGives(string group, string role)
        => $"From each person's next sign-in, belonging to {group} no longer gives {role}.";

    /// <summary>The dialog's danger button, naming its mapping (O07's nit).</summary>
    public static string DeleteConfirm(string group) => $"Delete the {group} mapping";
}
