using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Activities;
using Wombat.Domain.Audit;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Domain.Reporting;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.DataRights;

internal sealed class AccessReportBuilder : IAccessReportBuilder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPortfolioPdfService _pdfService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AccessReportBuilder(ApplicationDbContext dbContext, IPortfolioPdfService pdfService)
    {
        _dbContext = dbContext;
        _pdfService = pdfService;
    }

    public async Task<AccessExportResult> BuildAsync(string userId, CancellationToken cancellationToken)
    {
        var report = new AccessReport { SchemaVersion = 1, GeneratedOn = DateTime.UtcNow };

        // Profile
        var user = await _dbContext.Users
            .Include(u => u.SpecialityScopes)
            .Include(u => u.SubSpecialityScopes)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is not null)
        {
            report.Profile = new ProfileSection
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                InstitutionId = user.InstitutionId,
                SpecialityIds = user.SpecialityScopes.Select(s => s.SpecialityId).ToArray(),
                SubSpecialityIds = user.SubSpecialityScopes.Select(s => s.SubSpecialityId).ToArray()
            };
        }

        // Activities
        report.Activities = await _dbContext.Set<Activity>()
            .Where(a => a.SubjectUserId == userId || a.CreatedByUserId == userId)
            .Select(a => new ActivitySection
            {
                Id = a.Id,
                ActivityTypeId = a.ActivityTypeId,
                SubjectUserId = a.SubjectUserId,
                CreatedByUserId = a.CreatedByUserId,
                CurrentState = a.CurrentState,
                DataJson = a.DataJson,
                CreatedOn = a.CreatedOn,
                UpdatedOn = a.UpdatedOn
            })
            .ToListAsync(cancellationToken);

        // Committee reviews
        report.CommitteeReviews = await _dbContext.Set<CommitteeReview>()
            .Where(r => r.TraineeUserId == userId)
            .Include(r => r.Decisions)
            .Select(r => new CommitteeReviewSection
            {
                Id = r.Id,
                TraineeUserId = r.TraineeUserId,
                PanelId = r.PanelId,
                State = r.State.ToString(),
                ReviewPeriodFrom = r.ReviewPeriodFrom,
                ReviewPeriodTo = r.ReviewPeriodTo,
                ScheduledOn = r.ScheduledOn,
                DecisionCount = r.Decisions.Count
            })
            .ToListAsync(cancellationToken);

        // MSF campaigns (subject only — the user's feedback about them)
        report.MsfCampaigns = await _dbContext.Set<MsfCampaign>()
            .Where(c => c.SubjectUserId == userId)
            .Select(c => new MsfCampaignSection
            {
                Id = c.Id,
                TemplateId = c.TemplateId,
                State = c.State.ToString(),
                OpensOn = c.OpensOn,
                ClosesOn = c.ClosesOn
            })
            .ToListAsync(cancellationToken);

        // Curriculum progress
        report.CurriculumProgress = await _dbContext.Set<CurriculumItemProgress>()
            .Where(p => p.TraineeUserId == userId)
            .Select(p => new CurriculumProgressSection
            {
                CurriculumItemId = p.CurriculumItemId,
                CountsSoFar = p.CountsSoFar,
                MinimumLevelReachedCount = p.MinimumLevelReachedCount,
                LastUpdated = p.LastUpdated
            })
            .ToListAsync(cancellationToken);

        // Audit entries where user is the subject
        report.AuditEntries = await _dbContext.Set<AuditEntry>()
            .Where(e => e.ActorUserId == userId)
            .OrderByDescending(e => e.OccurredAt)
            .Take(500)
            .Select(e => new AuditEntrySection
            {
                Id = e.Id,
                OccurredAt = e.OccurredAt,
                Category = e.Category.ToString(),
                Action = e.Action,
                SubjectType = e.SubjectType,
                SubjectId = e.SubjectId,
                Success = e.Success
            })
            .ToListAsync(cancellationToken);

        // Portfolio exports
        report.PortfolioExports = await _dbContext.Set<PortfolioExport>()
            .Where(e => e.TraineeUserId == userId)
            .Select(e => new PortfolioExportSection
            {
                Id = e.Id,
                ExportedOn = e.ExportedOn,
                FileName = e.FileName,
                ContentHash = e.ContentHash
            })
            .ToListAsync(cancellationToken);

        // Generate JSON
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(report, JsonOptions);

        // Generate PDF via portfolio service
        byte[]? pdfBytes = null;
        try
        {
            var pdfResult = await _pdfService.GenerateAsync(
                new PortfolioExportRequest(userId, null, null),
                cancellationToken);
            pdfBytes = pdfResult.PdfBytes;
        }
        catch
        {
            // PDF generation is best-effort for access reports;
            // the JSON is the authoritative machine-readable export.
        }

        // Bundle into ZIP
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var jsonEntry = archive.CreateEntry("data-export.json", CompressionLevel.Optimal);
            using (var jsonStream = jsonEntry.Open())
            {
                jsonStream.Write(jsonBytes);
            }

            if (pdfBytes is not null)
            {
                var pdfEntry = archive.CreateEntry("portfolio-summary.pdf", CompressionLevel.Optimal);
                using var pdfStream = pdfEntry.Open();
                pdfStream.Write(pdfBytes);
            }
        }

        var zipBytes = memoryStream.ToArray();
        var fileName = $"data-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";

        return new AccessExportResult(zipBytes, fileName);
    }

    // --- Internal DTOs for the JSON export ---

    private sealed class AccessReport
    {
        public int SchemaVersion { get; set; }
        public DateTime GeneratedOn { get; set; }
        public ProfileSection? Profile { get; set; }
        public List<ActivitySection> Activities { get; set; } = [];
        public List<CommitteeReviewSection> CommitteeReviews { get; set; } = [];
        public List<MsfCampaignSection> MsfCampaigns { get; set; } = [];
        public List<CurriculumProgressSection> CurriculumProgress { get; set; } = [];
        public List<AuditEntrySection> AuditEntries { get; set; } = [];
        public List<PortfolioExportSection> PortfolioExports { get; set; } = [];
    }

    private sealed class ProfileSection
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int? InstitutionId { get; set; }
        public int[] SpecialityIds { get; set; } = [];
        public int[] SubSpecialityIds { get; set; } = [];
    }

    private sealed class ActivitySection
    {
        public int Id { get; set; }
        public int ActivityTypeId { get; set; }
        public string SubjectUserId { get; set; } = string.Empty;
        public string CreatedByUserId { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string DataJson { get; set; } = "{}";
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    private sealed class CommitteeReviewSection
    {
        public int Id { get; set; }
        public string TraineeUserId { get; set; } = string.Empty;
        public int PanelId { get; set; }
        public string State { get; set; } = string.Empty;
        public DateOnly ReviewPeriodFrom { get; set; }
        public DateOnly ReviewPeriodTo { get; set; }
        public DateOnly ScheduledOn { get; set; }
        public int DecisionCount { get; set; }
    }

    private sealed class MsfCampaignSection
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string State { get; set; } = string.Empty;
        public DateOnly OpensOn { get; set; }
        public DateOnly ClosesOn { get; set; }
    }

    private sealed class CurriculumProgressSection
    {
        public int CurriculumItemId { get; set; }
        public int CountsSoFar { get; set; }
        public int MinimumLevelReachedCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    private sealed class AuditEntrySection
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? SubjectType { get; set; }
        public Guid? SubjectId { get; set; }
        public bool Success { get; set; }
    }

    private sealed class PortfolioExportSection
    {
        public int Id { get; set; }
        public DateTime ExportedOn { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentHash { get; set; } = string.Empty;
    }
}
