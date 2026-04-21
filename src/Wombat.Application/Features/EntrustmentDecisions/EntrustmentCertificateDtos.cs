namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record EntrustmentCertificateRequest(int DecisionId);

public sealed record EntrustmentCertificateResult(
    byte[] PdfBytes,
    string FileName,
    string ContentHash);

public interface IEntrustmentCertificatePdfService
{
    Task<EntrustmentCertificateResult> GenerateAsync(EntrustmentCertificateRequest request, CancellationToken cancellationToken);
}
