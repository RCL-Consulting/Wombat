using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Reporting;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// The STAR certificate's text reads back as it was written, punctuation included: the "•" that opens each evidence
/// line and the "—" before its summary, and the en dash, section sign and quotation marks a rationale or a rung's
/// description may hold. (T212)
/// </summary>
/// <remarks>
/// <para>
/// T212 reported that "•" and "—" drew correctly but extracted as U+FFFD with pdftotext and PyMuPDF. Measured on this
/// renderer (QuestPDF 2026.2.4, Windows), they do not. Every font the certificate embeds is a Lato subset (Type0,
/// Identity-H) that holds the bullet and the em dash (glyphs 334 and 332 of Lato Regular), and its <c>/ToUnicode</c> map
/// sends them to U+2022 and U+2014. PyMuPDF reads both back (code points checked, not printed), and so does
/// <c>pdftotext -enc UTF-8</c>. The U+FFFD came from reading the output through a console that is not UTF-8. Python's
/// standard output on Windows is cp1252, which writes the two as the bytes 0x95 and 0x97. pdftotext writes Latin-1 by
/// default, where "•" becomes 0xB7. A UTF-8 terminal shows each of those bytes as U+FFFD. They were the only characters
/// affected because they are the only ones in the certificate outside ASCII.
/// </para>
/// <para>
/// So this pins what is right rather than fixing what was wrong. The certificate's text, read through each font's
/// ToUnicode map as <see cref="PdfTextLayer" /> reads it, holds each line exactly. And no glyph is drawn from a font other
/// than the embedded Lato: a character Lato lacked would be drawn from whatever font the host offers (DirectWrite on
/// Windows, FreeType on the Linux server), which is where a certificate's text could differ from one machine to the next.
/// </para>
/// </remarks>
[Collection(QuestPdfRenderingCollection.Name)]
public sealed partial class EntrustmentCertificateTextLayerTests
{
    private const string Summary =
        "State: completed; EPA PAED-001; Clinical case analysis; rated 3a; encounter 2026-02-10; updated 2026-02-10 09:00 UTC.";

    private const string LevelDescription = "Supervision on request – indirect (§ 3.2): “ready” when it’s asked";

    private const string Rationale = "Target met — consistent across the window.";

    static EntrustmentCertificateTextLayerTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public async Task TheCertificatesText_ReadsBackWhole_BulletAndDashesIncluded()
    {
        var pdf = await CertificateAsync();

        var page = PdfTextLayer.Pages(pdf).Single();
        page.Should().NotContain("�", "every glyph drawn maps to its character");
        page.Should().NotContain("\0", "no glyph maps to U+0000 (T200)");

        // Each span is its own text block and a long summary wraps, so the reader's lines are joined and their spacing
        // collapsed; every character is still the one the font's map gives.
        var text = Whitespace().Replace(page, " ");
        text.Should().Contain($"• Clinical Case Analysis (Paediatrics) #19 — {Summary}");
        text.Should().Contain("• Annual MSF #50 Issued by", "a line with no summary has no dash");
        text.Should().Contain(LevelDescription);
        text.Should().Contain(Rationale);
    }

    [Fact]
    public async Task EveryGlyph_IsDrawnFromTheEmbeddedLato()
    {
        var pdf = await CertificateAsync();

        var fonts = BaseFont().Matches(Encoding.Latin1.GetString(pdf)).Select(match => match.Groups[1].Value).Distinct().ToArray();

        fonts.Should().NotBeEmpty("guard: the fonts are read");
        fonts.Should().OnlyContain(
            font => LatoSubset().IsMatch(font),
            "a character Lato lacked would be drawn from a font the host offers, which differs between machines; found {0}",
            string.Join(", ", fonts));
    }

    private static async Task<byte[]> CertificateAsync()
    {
        var database = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(database).Options);
        db.Set<WombatIdentityUser>().Add(new WombatIdentityUser { Id = "trainee-1", FirstName = "Lerato", LastName = "Molefe" });
        db.Set<WombatIdentityUser>().Add(new WombatIdentityUser { Id = "chair-1", FirstName = "Thandi", LastName = "Nkosi" });
        db.Set<Institution>().Add(new Institution { Id = 1, Name = "Host Academic Hospital", ShortCode = "HOST" });
        db.Set<Speciality>().Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = 1,
            CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 2, Name = "CPSA ladder" });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 9, ScaleId = 2, Order = 3, Label = "3a", Description = LevelDescription });
        db.Set<Epa>().Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true });
        db.Set<DecisionPanel>().Add(new DecisionPanel { Id = 1, Name = "Paediatrics CCC", InstitutionId = 1 });
        db.Set<CommitteeReview>().Add(new CommitteeReview { AcademicYear = 2026, Semester = 1, Id = 1, TraineeUserId = "trainee-1", PanelId = 1 });
        db.Set<EntrustmentDecision>().Add(EntrustmentDecision.Issue(
            "trainee-1", epaId: 1, authorisedLevelId: 9, issuedOn: new DateOnly(2026, 6, 18), expiresOn: null,
            committeeReviewId: 1, chairUserId: "chair-1", rationale: Rationale,
            evidenceLinks:
            [
                // The summary as StartCommitteeReview writes an activity's; a campaign's line with no summary has no dash.
                EntrustmentEvidenceLink.FromSnapshot(new CommitteeEvidence
                {
                    Id = 1,
                    SourceType = CommitteeEvidenceSourceType.Activity,
                    ActivityId = 19,
                    SourceLabel = "Clinical Case Analysis (Paediatrics) #19",
                    Summary = Summary
                }),
                EntrustmentEvidenceLink.FromSnapshot(new CommitteeEvidence
                {
                    Id = 2,
                    SourceType = CommitteeEvidenceSourceType.MsfCampaign,
                    MsfCampaignId = 50,
                    SourceLabel = "Annual MSF #50",
                    Summary = string.Empty
                })
            ]));
        await db.SaveChangesAsync();

        var decisionId = await db.Set<EntrustmentDecision>().Select(decision => decision.Id).SingleAsync();
        var result = await new EntrustmentCertificatePdfService(db).GenerateAsync(
            new EntrustmentCertificateRequest(decisionId),
            CancellationToken.None);
        return result.PdfBytes;
    }

    // Font dictionaries are plain objects in what QuestPDF writes (PdfTextLayer refuses object streams), so the names read
    // straight from the bytes.
    [GeneratedRegex(@"/BaseFont\s*/([^\s/<>\[\]()]+)")]
    private static partial Regex BaseFont();

    [GeneratedRegex(@"^[A-Z]{6}\+Lato-(Regular|Bold|Italic|BoldItalic)$")]
    private static partial Regex LatoSubset();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
