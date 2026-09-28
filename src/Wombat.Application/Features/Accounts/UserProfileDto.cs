using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Accounts;

/// <summary>
/// The signed-in person's own account, as My account shows it (T185; T339, flow 02).
/// </summary>
/// <param name="Roles">The roles held, by their keys.</param>
public sealed record UserProfileDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles)
{
    /// <summary>
    /// The account's institution by name; null for an account with none, the platform Administrator's or a College
    /// admin's, whose Account card has no Institution row at all (T339, C4).
    /// </summary>
    public string? InstitutionName { get; init; }

    /// <summary>
    /// A Wombat password signs the account in (<see cref="AccountSignInMethods.HasLocalPassword" />), so My account offers
    /// Change password; an account that signs in only through its institution has none to change (T286, T339).
    /// </summary>
    public bool HasLocalPassword { get; init; }

    /// <summary>The account's institutional sign-ins, each by its provider's name (T339; T286's list).</summary>
    public IReadOnlyList<InstitutionalSignIn> InstitutionalSignIns { get; init; } = [];

    /// <summary>
    /// The roles held, by label ("Pending trainee", never "PendingTrainee"), in the order <see cref="WombatRoles.All" />
    /// lists them; a key that names no role last, as it is (<see cref="WombatRoleLabels.For" />). The page reads these, so
    /// it names no Domain type (T339, flow 02; T190).
    /// </summary>
    public IReadOnlyList<string> RoleLabels
        => WombatRoles.All.Where(Roles.Contains)
            .Concat(Roles.Where(role => !WombatRoles.All.Contains(role)).Order(StringComparer.Ordinal))
            .Select(WombatRoleLabels.For)
            .ToList();
}
