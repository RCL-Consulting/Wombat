namespace Wombat.Domain.Identity;

public static class WombatRoles
{
    public const string Administrator = "Administrator";
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
        InstitutionalAdmin,
        SpecialityAdmin,
        SubSpecialityAdmin,
        Coordinator,
        CommitteeMember,
        Assessor,
        Trainee,
        PendingTrainee
    ];
}
