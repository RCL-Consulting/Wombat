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
}
