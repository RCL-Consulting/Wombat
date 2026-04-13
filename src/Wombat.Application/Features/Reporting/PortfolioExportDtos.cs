namespace Wombat.Application.Features.Reporting;

public sealed record PortfolioExportRequest(
    string TraineeUserId,
    DateOnly? FromDate,
    DateOnly? ToDate);

public sealed record PortfolioExportResult(
    byte[] PdfBytes,
    string FileName,
    string ContentHash);

public sealed record PortfolioExportRecordDto(
    int Id,
    string TraineeUserId,
    string ExportedByUserId,
    DateTime ExportedOn,
    DateOnly? FilterFromDate,
    DateOnly? FilterToDate,
    string ContentHash,
    string FileName);
