namespace Wombat.Domain.Identity;

public static class WombatRoles
{
    public const string Administrator = "Administrator";
    public const string CollegeAdmin = "CollegeAdmin";
    public const string InstitutionalAdmin = "InstitutionalAdmin";
    public const string SpecialityAdmin = "SpecialityAdmin";
    public const string SubSpecialityAdmin = "SubSpecialityAdmin";
    public const string Coordinator = "Coordinator";
    public const string CommitteeMember = "CommitteeMember";
    public const string Assessor = "Assessor";
    public const string Trainee = "Trainee";
    public const string PendingTrainee = "PendingTrainee";

    public static readonly IReadOnlyList<string> All =
    [
        Administrator,
        CollegeAdmin,
        InstitutionalAdmin,
        SpecialityAdmin,
        SubSpecialityAdmin,
        Coordinator,
        CommitteeMember,
        Assessor,
        Trainee,
        PendingTrainee
    ];

    /// <summary>
    /// The roles a <c>user</c> field may require its nominee to hold (T102): the roles whose authority is not bounded by a
    /// speciality. The nominee directory matches the activity's institution only and reads no one's speciality rows.
    /// InstitutionalAdmin, Coordinator and CommitteeMember oversee the whole institution; Assessor and Trainee oversee
    /// nothing and act only where a field names them, so naming one from another discipline at the same institution is
    /// accepted by design (D23). Excluded: the two national roles, which carry no institution; PendingTrainee, admitted to
    /// nothing; and the speciality and sub-speciality admins, whose authority everywhere else is their speciality, which
    /// the directory does not match, so a field requiring one would admit another speciality's admin.
    /// </summary>
    public static readonly IReadOnlyList<string> Nominable =
    [
        InstitutionalAdmin,
        Coordinator,
        CommitteeMember,
        Assessor,
        Trainee
    ];
}
