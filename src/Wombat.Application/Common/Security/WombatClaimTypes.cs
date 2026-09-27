namespace Wombat.Application.Common.Security;

public static class WombatClaimTypes
{
    public const string CollegeId = "college_id";
    public const string InstitutionId = "institution_id";
    public const string SpecialityId = "speciality_id";
    public const string SubSpecialityId = "sub_speciality_id";

    /// <summary>
    /// Issued at sign-in to someone who holds a trainee profile, current or ended (T252). Completing a programme removes
    /// the Trainee role (there is no alumnus role), so this is what still admits a graduate to their own record: the
    /// progress page (<c>TraineeOrFormerTrainee</c>) and its nav link. It lends nothing over anyone else: every read it
    /// admits to is the caller's own.
    /// </summary>
    public const string TraineeRecord = "trainee_record";

    /// <summary>
    /// The person's name as the shell shows it: "FirstName LastName", or their email when the account has no name (the
    /// bootstrap administrator, an erased account). Issued at sign-in so the shell reads no database: the error page draws
    /// the shell too, and must not fail with the database (T335, flow 01, review S7). Read it with
    /// <c>ClaimsPrincipal.GetDisplayName()</c>.
    /// </summary>
    public const string DisplayName = "display_name";

    /// <summary>
    /// The acting role stored with the account (<c>WombatIdentityUser.ActingRole</c>), as sign-in found it: a role key, or
    /// no claim when none is stored. What is stored, not what is shown: the web's <c>ActingRoleResolver</c> honours it
    /// only while the person holds that role, and otherwise falls back to the role precedence (T335, flow 01, D1).
    /// </summary>
    public const string ActingRole = "acting_role";
}
