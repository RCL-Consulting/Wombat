using Microsoft.AspNetCore.Identity;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.DataRights;

internal sealed class ObjectionFlagService : IObjectionFlagUpdater, IObjectionFlagReader
{
    private readonly UserManager<WombatIdentityUser> _userManager;

    public ObjectionFlagService(UserManager<WombatIdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task UpdateAsync(
        string userId,
        bool optOutOfOptionalProcessing,
        bool optOutOfDigestEmails,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.OptOutOfOptionalProcessing = optOutOfOptionalProcessing;
        user.OptOutOfDigestEmails = optOutOfDigestEmails;

        await _userManager.UpdateAsync(user);
    }

    public async Task<ObjectionFlagsDto> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        return new ObjectionFlagsDto(
            user.OptOutOfOptionalProcessing,
            user.OptOutOfDigestEmails);
    }
}
