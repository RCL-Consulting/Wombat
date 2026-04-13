namespace Wombat.Domain.Institutions;

public sealed class InstitutionBrand
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string? LogoBase64 { get; set; }
    public string? PrimaryColorHex { get; set; }
    public string? SecondaryColorHex { get; set; }

    public Institution Institution { get; set; } = null!;
}
