namespace Wombat.Domain.Invitations;

public sealed class Invitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;

    /// <summary>
    /// The institution this invitation scopes the invitee to. Null only for a
    /// <c>CollegeAdmin</c> invitation, which is scoped to a <see cref="CollegeId"/> instead. (T093)
    /// </summary>
    public int? InstitutionId { get; set; }

    /// <summary>
    /// The national College this invitation scopes the invitee to. Set only for a
    /// <c>CollegeAdmin</c> invitation; null for every institution-scoped role. (T093)
    /// </summary>
    public int? CollegeId { get; set; }

    public int? SpecialityId { get; set; }
    public int? SubSpecialityId { get; set; }
    public string IssuedByUserId { get; set; } = string.Empty;
    public DateTime IssuedOn { get; set; }
    public DateOnly ExpiresOn { get; set; }
    public DateTime? UsedOn { get; set; }
    public DateTime? RevokedOn { get; set; }
}
