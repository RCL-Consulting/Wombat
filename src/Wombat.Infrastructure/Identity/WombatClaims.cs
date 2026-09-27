using Wombat.Application.Common.Security;

namespace Wombat.Infrastructure.Identity;

public static class WombatClaims
{
    public const string CollegeId = WombatClaimTypes.CollegeId;
    public const string InstitutionId = WombatClaimTypes.InstitutionId;
    public const string SpecialityId = WombatClaimTypes.SpecialityId;
    public const string SubSpecialityId = WombatClaimTypes.SubSpecialityId;
    public const string TraineeRecord = WombatClaimTypes.TraineeRecord;
    public const string DisplayName = WombatClaimTypes.DisplayName;
    public const string ActingRole = WombatClaimTypes.ActingRole;
}
