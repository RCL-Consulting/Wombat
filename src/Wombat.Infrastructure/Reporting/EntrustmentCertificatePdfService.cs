using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Reporting;

internal sealed class EntrustmentCertificatePdfService : IEntrustmentCertificatePdfService
{
    // F-5-3 / T078: fixed metadata so the certificate is byte-for-byte reproducible from its data.
    private static readonly QuestPDF.Infrastructure.DocumentMetadata DeterministicMetadata = new()
    {
        Title = "Statement of Awarded Responsibility",
        Author = "Wombat",
        Subject = "Entrustment decision",
        Creator = "Wombat",
        Producer = "Wombat",
        CreationDate = PortfolioPdfService.DeterministicTimestamp,
        ModifiedDate = PortfolioPdfService.DeterministicTimestamp
    };

    private readonly IApplicationDbContext _dbContext;

    public EntrustmentCertificatePdfService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EntrustmentCertificateResult> GenerateAsync(EntrustmentCertificateRequest request, CancellationToken cancellationToken)
    {
        var data = await LoadCertificateDataAsync(request.DecisionId, cancellationToken)
            ?? throw new InvalidOperationException("The entrustment decision could not be found.");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(50);
                page.MarginVertical(45);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Header().Element(e => ComposeHeader(e, data));
                page.Content().Element(e => ComposeContent(e, data));
                page.Footer().Element(e => ComposeFooter(e, data));
            });
        }).WithMetadata(DeterministicMetadata);

        var pdfBytes = document.GeneratePdf();
        var hash = Convert.ToHexStringLower(SHA256.HashData(pdfBytes));
        var fileName = $"star-certificate-{data.EpaCode}-{data.DecisionId}-{hash[..8]}.pdf";

        return new EntrustmentCertificateResult(pdfBytes, fileName, hash);
    }

    private static void ComposeHeader(IContainer container, CertificateData data)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text(t => t.Span(data.InstitutionName).Bold().FontSize(14).FontColor(Colors.Grey.Darken3));
            column.Item().Height(6);
            column.Item().AlignCenter().Text(t => t.Span("Statement of Awarded Responsibility").Bold().FontSize(22).FontColor(Colors.Blue.Darken3));
            column.Item().Height(4);
            column.Item().AlignCenter().Text(t => t.Span("Entrustment Decision").FontSize(11).FontColor(Colors.Grey.Darken1));
            column.Item().Height(10);
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(IContainer container, CertificateData data)
    {
        container.Column(column =>
        {
            column.Spacing(14);

            column.Item().PaddingTop(18).Column(c =>
            {
                c.Item().AlignCenter().Text(t =>
                {
                    t.Span("This certifies that ").FontSize(11);
                    t.Span(data.TraineeName).Bold().FontSize(13);
                });
                c.Item().Height(6);
                c.Item().AlignCenter().Text(t => t.Span("is authorised to perform the following EPA").FontSize(11));
            });

            column.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(14).Column(c =>
            {
                c.Item().Text(t =>
                {
                    t.Span(data.EpaCode).Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                    t.Span("  ").FontSize(11);
                    t.Span(data.EpaTitle).Bold().FontSize(13);
                });
                c.Item().Height(8);
                c.Item().Text(t =>
                {
                    t.Span("Authorised level: ").FontSize(10);
                    t.Span($"{data.AuthorisedLevelOrder}. {data.AuthorisedLevelLabel}").Bold().FontSize(11);
                });
                if (!string.IsNullOrWhiteSpace(data.AuthorisedLevelDescription))
                {
                    c.Item().Height(4);
                    c.Item().Text(t => t.Span(data.AuthorisedLevelDescription!).FontSize(9).Italic().FontColor(Colors.Grey.Darken1));
                }
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(t => t.Span("Issued on").FontSize(9).FontColor(Colors.Grey.Darken1));
                    c.Item().Text(t => t.Span(data.IssuedOn.ToString("d MMMM yyyy")).Bold().FontSize(11));
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(t => t.Span("Expires on").FontSize(9).FontColor(Colors.Grey.Darken1));
                    c.Item().Text(t => t.Span(data.ExpiresOn?.ToString("d MMMM yyyy") ?? "No expiry").Bold().FontSize(11));
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(t => t.Span("Status").FontSize(9).FontColor(Colors.Grey.Darken1));
                    c.Item().Text(t =>
                    {
                        t.Span(data.Status.ToString()).Bold().FontSize(11).FontColor(data.Status switch
                        {
                            EntrustmentDecisionStatus.Active => Colors.Green.Darken2,
                            EntrustmentDecisionStatus.Revoked => Colors.Red.Darken2,
                            EntrustmentDecisionStatus.Superseded => Colors.Orange.Darken2,
                            EntrustmentDecisionStatus.Expired => Colors.Grey.Darken1,
                            _ => Colors.Grey.Darken3
                        });
                    });
                });
            });

            column.Item().Column(c =>
            {
                c.Item().Text(t => t.Span("Rationale").Bold().FontSize(10).FontColor(Colors.Grey.Darken2));
                c.Item().Height(4);
                c.Item().Text(t => t.Span(data.Rationale).FontSize(10));
            });

            if (data.EvidenceLinks.Count > 0)
            {
                column.Item().Column(c =>
                {
                    c.Item().Text(t => t.Span("Evidence summary").Bold().FontSize(10).FontColor(Colors.Grey.Darken2));
                    c.Item().Height(4);
                    foreach (var link in data.EvidenceLinks)
                    {
                        c.Item().Text(t =>
                        {
                            t.Span("• ").FontSize(9);
                            t.Span(link.SourceLabel).Bold().FontSize(9);
                            if (!string.IsNullOrWhiteSpace(link.Summary))
                            {
                                t.Span($" — {link.Summary}").FontSize(9).FontColor(Colors.Grey.Darken1);
                            }
                        });
                    }
                });
            }

            column.Item().PaddingTop(18).Column(c =>
            {
                c.Item().Text(t => t.Span("Issued by").Bold().FontSize(10).FontColor(Colors.Grey.Darken2));
                c.Item().Height(4);
                c.Item().Text(t => t.Span($"Panel: {data.PanelName}").FontSize(10));
                c.Item().Text(t => t.Span($"Committee review: #{data.CommitteeReviewId}").FontSize(10));
                c.Item().Text(t => t.Span($"Chair: {data.ChairName}").FontSize(10));
                c.Item().Height(24);
                c.Item().LineHorizontal(0.7f).LineColor(Colors.Grey.Darken1);
                c.Item().Text(t => t.Span("Chair signature on file").FontSize(8).Italic().FontColor(Colors.Grey.Darken1));
            });

            if (data.Status == EntrustmentDecisionStatus.Revoked)
            {
                column.Item().Border(1).BorderColor(Colors.Red.Darken2).Background(Colors.Red.Lighten5).Padding(10).Column(c =>
                {
                    c.Item().Text(t => t.Span("REVOKED").Bold().FontSize(11).FontColor(Colors.Red.Darken2));
                    c.Item().Text(t => t.Span($"Revoked on {data.RevokedOn:d MMMM yyyy} by {data.RevokedByUserId}").FontSize(9));
                    if (!string.IsNullOrWhiteSpace(data.RevocationReason))
                    {
                        c.Item().Text(t => t.Span($"Reason: {data.RevocationReason}").FontSize(9));
                    }
                });
            }
        });
    }

    private static void ComposeFooter(IContainer container, CertificateData data)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
            column.Item().Height(4);
            column.Item().AlignCenter().Text(t =>
            {
                t.Span("Decision #").FontSize(8).FontColor(Colors.Grey.Darken1);
                t.Span(data.DecisionId.ToString()).FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private async Task<CertificateData?> LoadCertificateDataAsync(int decisionId, CancellationToken cancellationToken)
    {
        var decision = await _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Include(d => d.Epa)
                .ThenInclude(epa => epa.SubSpeciality)
                    .ThenInclude(sub => sub.Speciality)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.IssuedByCommitteeReview)
                .ThenInclude(review => review.Panel)
            .Include(d => d.EvidenceLinks)
            .SingleOrDefaultAsync(d => d.Id == decisionId, cancellationToken);

        if (decision is null)
        {
            return null;
        }

        var chair = await _dbContext.Set<WombatIdentityUser>()
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == decision.IssuedByChairUserId, cancellationToken);
        var trainee = await _dbContext.Set<WombatIdentityUser>()
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == decision.TraineeUserId, cancellationToken);

        // The EPA is national now (T091); the issuing institution is where the trainee trained.
        var traineeProfile = await _dbContext.Set<Domain.Identity.TraineeProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == decision.TraineeUserId, cancellationToken);
        var institution = traineeProfile is not null
            ? await _dbContext.Set<Domain.Institutions.Institution>()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == traineeProfile.InstitutionId, cancellationToken)
            : null;

        return new CertificateData(
            DecisionId: decision.Id,
            TraineeName: FormatName(trainee) ?? decision.TraineeUserId,
            EpaCode: decision.Epa.Code,
            EpaTitle: decision.Epa.Title,
            AuthorisedLevelLabel: decision.AuthorisedLevel.Label,
            AuthorisedLevelOrder: decision.AuthorisedLevel.Order,
            AuthorisedLevelDescription: decision.AuthorisedLevel.Description,
            IssuedOn: decision.IssuedOn,
            ExpiresOn: decision.ExpiresOn,
            Status: decision.Status,
            Rationale: decision.Rationale,
            PanelName: decision.IssuedByCommitteeReview.Panel.Name,
            CommitteeReviewId: decision.IssuedByCommitteeReviewId,
            ChairName: FormatName(chair) ?? decision.IssuedByChairUserId,
            InstitutionName: institution?.Name ?? string.Empty,
            EvidenceLinks: decision.EvidenceLinks
                .OrderBy(link => link.SourceType)
                .ThenBy(link => link.Id)
                .Select(link => new CertificateEvidence(link.SourceLabel, link.Summary))
                .ToList(),
            RevokedOn: decision.RevokedOn,
            RevokedByUserId: decision.RevokedByUserId,
            RevocationReason: decision.RevocationReason);
    }

    private static string? FormatName(WombatIdentityUser? user)
    {
        if (user is null)
        {
            return null;
        }

        var composed = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(composed) ? null : composed;
    }
}

internal sealed record CertificateData(
    int DecisionId,
    string TraineeName,
    string EpaCode,
    string EpaTitle,
    string AuthorisedLevelLabel,
    int AuthorisedLevelOrder,
    string? AuthorisedLevelDescription,
    DateOnly IssuedOn,
    DateOnly? ExpiresOn,
    EntrustmentDecisionStatus Status,
    string Rationale,
    string PanelName,
    int CommitteeReviewId,
    string ChairName,
    string InstitutionName,
    List<CertificateEvidence> EvidenceLinks,
    DateTime? RevokedOn,
    string? RevokedByUserId,
    string? RevocationReason);

internal sealed record CertificateEvidence(string SourceLabel, string Summary);
