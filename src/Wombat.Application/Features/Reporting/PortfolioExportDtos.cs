namespace Wombat.Application.Features.Reporting;

/// <param name="Principal">
/// The caller, so the PDF's CONTENTS obey the same gate that authorised the export. (T101)
/// </param>
/// <remarks>
/// Without this the export gate and the export body disagreed: access was decided from the trainee's
/// current <c>TraineeProfile</c> while the activities query filtered on subject and dates alone. So a
/// Coordinator at the trainee's NEW institution received, in one PDF, assessments stamped to the old
/// one — rows <c>/activities/{id}</c> refuses them individually — and activities filed before the
/// subject had a profile at all were bundled in although no overseer may open them. Passing the
/// principal down makes the bundle exactly the union of what the caller may read one by one.
/// </remarks>
public sealed record PortfolioExportRequest(
    string TraineeUserId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    System.Security.Claims.ClaimsPrincipal Principal);

public sealed record PortfolioExportResult(
    byte[] PdfBytes,
    string FileName,
    string ContentHash);

/// <summary>
/// What Verify says of a hash (T379, flow 13, S10): its first generation and who made it, how many times the same file was
/// exported and the last time, and the first export's trainee, filter and file. People are ids, never names: Verify is
/// open to anyone holding the file.
/// </summary>
public sealed record PortfolioExportRecordDto(
    string TraineeUserId,
    string FirstExportedByUserId,
    DateTime FirstExportedOn,
    DateTime LastExportedOn,
    int Count,
    DateOnly? FilterFromDate,
    DateOnly? FilterToDate,
    string ContentHash,
    string FileName);
