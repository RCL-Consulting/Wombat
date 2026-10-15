using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Activities;

/// <summary>
/// Who may file an activity: only a registrar in a programme that is running, filing about themselves (D55, T153). The
/// new-activity page admits only the Trainee role, which admission grants with the profile and the end of a programme
/// takes away (D54); the create itself refuses a subject with no active trainee profile, which the role travels with, so
/// a circuit whose claims predate the end is refused too.
/// </summary>
/// <remarks>
/// Until D55 the page asked only that the caller was signed in and the create checked no role or profile. A graduate, a
/// withdrawn registrar or an assessor could type the address and file about themselves into an assessor's inbox, and an
/// encounter dated on or before a graduate's last day credited their archived profile after the committee had decided on
/// it (F-5.22b). A pending trainee files nothing either: before admission there is no curriculum to count towards.
/// </remarks>
public static class ActivityFilingRules
{
    /// <summary>The role the new-activity page admits.</summary>
    public const string FilingRole = WombatRoles.Trainee;

    /// <summary>The refusal of a create whose subject has no programme running.</summary>
    public const string NoCurrentProgramme =
        "Only a registrar whose programme is running can file an activity. Yours has not started or has ended, so an " +
        "activity would count towards nothing.";
}
