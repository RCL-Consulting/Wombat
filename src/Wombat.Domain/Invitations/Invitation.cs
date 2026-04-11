namespace Wombat.Domain.Invitations;

public sealed class Invitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public int InstitutionId { get; set; }
    public int? SpecialityId { get; set; }
    public int? SubSpecialityId { get; set; }
    public string IssuedByUserId { get; set; } = string.Empty;
    public DateTime IssuedOn { get; set; }
    public DateOnly ExpiresOn { get; set; }
    public DateTime? UsedOn { get; set; }
    public DateTime? RevokedOn { get; set; }
}
