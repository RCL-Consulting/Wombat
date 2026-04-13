namespace Wombat.Domain.Reporting;

public sealed class PortfolioExport
{
    public int Id { get; set; }
    public string TraineeUserId { get; set; } = string.Empty;
    public string ExportedByUserId { get; set; } = string.Empty;
    public DateTime ExportedOn { get; set; }
    public DateOnly? FilterFromDate { get; set; }
    public DateOnly? FilterToDate { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
