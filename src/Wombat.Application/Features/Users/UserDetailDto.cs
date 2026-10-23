using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.Users;

/// <summary>
/// One person's page (T376, flow 12: the who line, Roles and scope, Sign-in and access). In a file of its own since T376
/// (build-contracts § 0), the user page lane's (P) to change.
/// </summary>
/// <remarks>
/// Q9: the page no longer lists the invitations addressed to the person's email, and the DTO no longer carries them: an
/// invitation to an address an account holds cannot be issued (T286) or accepted, and Invitations lists every one.
/// </remarks>
/// <param name="IsLockedOut">
/// The administrative lock (<see cref="UserIdentityDetails.IsDeactivated" />): an administrator's lock or an erasure, never
/// Identity's brute-force lockout that lifts itself after minutes (build-contracts § 1, the nit). Users' Status reads the
/// same field, so the two pages say "Locked out" of the same people.
/// </param>
public sealed record UserDetailDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    string? InstitutionName,
    IReadOnlyCollection<int> SpecialityIds,
    IReadOnlyCollection<int> SubSpecialityIds,
    IReadOnlyCollection<string> Roles,
    bool IsLockedOut)
{
    /// <summary>The person's one scope in words (S1, <see cref="PersonScopeWords" />): the who line's Scope.</summary>
    public string Scope { get; init; } = PersonScopeWords.AnySpeciality;

    /// <summary>The speciality names the person is scoped to, sorted as the scope prints them (Add role's read-outs, M8).</summary>
    public IReadOnlyList<string> SpecialityNames { get; init; } = [];

    /// <summary>The sub-speciality names the person is scoped to.</summary>
    public IReadOnlyList<string> SubSpecialityNames { get; init; } = [];

    /// <summary>A College admin's College (T093), which their scope reads as; null for everyone else.</summary>
    public int? CollegeId { get; init; }

    /// <summary>That College's name.</summary>
    public string? CollegeName { get; init; }

    /// <summary>
    /// For a person who holds no role, their latest ended programme (S2, Q13; <see cref="EndedProgrammes" />): the no-role
    /// sentence says how and when it ended. Null for anyone who holds a role, and for a person with no ended profile.
    /// </summary>
    public EndedProgrammeDto? EndedProgramme { get; init; }

    /// <summary>
    /// The account may use a Wombat password at all (<see cref="AccountSignInMethods.AllowsLocalPassword" />, F3). False for
    /// an account its institution's sign-in provisioned: Password says a password cannot be set, with no field.
    /// </summary>
    public bool AllowsLocalPassword { get; init; } = true;

    /// <summary>A Wombat password signs the account in (<see cref="AccountSignInMethods.HasLocalPassword" />).</summary>
    public bool HasLocalPassword { get; init; } = true;

    /// <summary>The account's institutional sign-ins, each by its provider's key and name (Q10, F7); no date is stored.</summary>
    public IReadOnlyList<InstitutionalSignIn> InstitutionalSignIns { get; init; } = [];

    /// <summary>"Thandi Zulu": the h1, the tab and every sentence that names the person.</summary>
    public string Name => $"{FirstName} {LastName}".Trim();
}
