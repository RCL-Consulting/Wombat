using System.Security.Claims;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Users;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// Who may administer trainee profiles: admit, change, withdraw, complete, and read them on the Trainees pages. The same
/// callers who administer users (<see cref="UserAdministrationRules.MayAdministerUsers" />), so the two surfaces cannot
/// drift. (T290)
/// </summary>
public static class TraineeAdministrationRules
{
    /// <summary>
    /// The refusal of every Trainees read and command to a caller who holds the Trainee role beside a role that
    /// administers trainees, and what the Trainees pages say to them. Given before any profile or user is looked up, so it
    /// says nothing about an id. (T290)
    /// </summary>
    public const string TraineeAdministersNoTrainee =
        "You hold the Trainee role, so you cannot view or change trainee profiles, including your own.";

    /// <summary>The refusal to a caller who holds no role that administers trainees.</summary>
    public const string MayNotAdministerTrainees = "You do not have permission to administer trainees.";

    /// <summary>
    /// <b>A trainee first</b> (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185). Until T290 the Trainees surface
    /// asked nothing of it: a registrar who also held InstitutionalAdmin could edit a peer's programme, withdraw them, or
    /// mark their own programme complete, which took their own Trainee role away and with it every trainee-first refusal,
    /// as Remove role did before T278.
    /// </summary>
    public static bool MayAdministerTrainees(ClaimsPrincipal principal) => UserAdministrationRules.MayAdministerUsers(principal);

    /// <summary>Refuses, before anything is looked up, a caller who may not administer trainees. Every Trainees handler asks it first.</summary>
    public static void DemandTraineeAdministration(ClaimsPrincipal principal)
    {
        if (MayAdministerTrainees(principal))
        {
            return;
        }

        throw new UnauthorizedAccessException(
            UserAdministrationRules.TraineeNoteOnUserPages(principal) is null ? MayNotAdministerTrainees : TraineeAdministersNoTrainee);
    }

    /// <summary>
    /// What the Trainees pages say, in place of what they would list or offer, to someone who holds Trainee beside a role
    /// that administers trainees; null for anyone else.
    /// </summary>
    public static string? TraineeNoteOnTraineePages(ClaimsPrincipal principal)
        => UserAdministrationRules.TraineeNoteOnUserPages(principal) is null ? null : TraineeAdministersNoTrainee;
}
