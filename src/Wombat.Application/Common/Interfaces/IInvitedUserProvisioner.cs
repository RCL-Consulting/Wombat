namespace Wombat.Application.Common.Interfaces;

public sealed record ProvisionedInvitationUser(string UserId, string AssignedRole);

public interface IInvitedUserProvisioner
{
    Task<ProvisionedInvitationUser> ProvisionAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string targetRole,
        int institutionId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken = default);
}
