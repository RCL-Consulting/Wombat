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

    public async Task<ReminderEmailsSaved> UpdateAsync(
        string userId,
        bool optOutOfDigestEmails,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        // T387 (N3): the result says whether this save changed anything, against the stored state, not the page's.
        var changed = user.OptOutOfDigestEmails != optOutOfDigestEmails;
        if (changed)
        {
            user.OptOutOfDigestEmails = optOutOfDigestEmails;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"The reminder preference could not be saved ({string.Join(", ", result.Errors.Select(error => error.Code))}).");
            }
        }

        return new ReminderEmailsSaved(optOutOfDigestEmails, changed);
    }

    public async Task<ObjectionFlagsDto> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        return new ObjectionFlagsDto(user.OptOutOfDigestEmails);
    }
}
