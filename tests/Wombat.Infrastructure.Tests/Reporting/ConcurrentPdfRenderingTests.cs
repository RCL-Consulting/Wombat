using System.Collections.Concurrent;
using System.IO.Compression;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Reporting;
using Wombat.Tests.Shared;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// T200: PDFs exported at the same time each keep their whole text layer, because <see cref="QuestPdfRenderer" />
/// renders one at a time in the process.
/// </summary>
/// <remarks>
/// <para>
/// Two QuestPDF renders that overlapped could each come out with fonts whose ToUnicode map sent every glyph but one to
/// U+0000: a PDF that prints correctly, reads as nothing, and hashes differently from the same data rendered alone. It
/// surfaced in the T169 review as a determinism test failing about once in 25 suite runs.
/// </para>
/// <para>
/// <b>Deliberately outside <see cref="QuestPdfRenderingCollection" />.</b> That collection runs its classes one at a
/// time, which is exactly what would hide the defect. Here every round starts the portfolio export, the entrustment
/// certificate and the data-subject access report together on the thread pool, through the production services, and
/// only the production gate stands between them. Nothing else in this assembly renders outside that collection, and the
/// collection never runs alongside another, so nothing but these exports is rendering while they run.
/// </para>
/// <para>
/// <b>How many.</b> With the gate removed, every run of this test failed: sixteen of sixteen, eleven at 30 rounds while it
/// was sized and five at 50. The five had 11 to 55 bad PDFs out of 450, in 8 to 27 of the 50 rounds, and the worst read
/// 838 of its 864 characters as U+0000. At the lowest of those rates (8 rounds in 50), the chance that all
/// <see cref="Rounds" /> rounds come out clean is about (42/50)^50, under 1 in 5,000. With the gate, the 450 exports
/// take about five seconds. <c>PdfRenderingTests</c> (Architecture) catches a render that bypasses the gate outright.
/// </para>
/// </remarks>
public sealed class ConcurrentPdfRenderingTests
{
    private const int Rounds = 50;

    /// <summary>Three of each export per round, all started together.</summary>
    private const int ExportsPerKind = 3;

    private const string Trainee = "trainee-1";

    static ConcurrentPdfRenderingTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    private enum Export
    {
        Portfolio,
        Certificate,
        AccessReport
    }

    [Fact]
    public async Task ExportsRenderedAtTheSameTime_EachKeepTheirWholeTextLayer()
    {
        var database = Guid.NewGuid().ToString();
        Seed(database);

        // Rendered one at a time, before anything overlaps: what each export must read as, and be byte for byte.
        var portfolio = await PdfOf(Export.Portfolio, database);
        var certificate = await PdfOf(Export.Certificate, database);
        var accessReport = await PdfOf(Export.AccessReport, database);

        var portfolioText = Text(portfolio);
        portfolioText.Should().ContainAll(
            "Portfolio Export", "Lerato Molefe", "Statements of Awarded Responsibility (STARs)", "PAED-001", "Acute admission");
        var certificateText = Text(certificate);
        certificateText.Should().ContainAll(
            "Statement of Awarded Responsibility", "This certifies that", "Lerato Molefe", "Chair: Thandi Nkosi");
        foreach (var reference in new[] { portfolioText, certificateText })
        {
            reference.Should().NotContain("\0", "a glyph mapped to U+0000 reads as nothing");
            reference.Should().NotContain("\uFFFD", "every glyph shown has a ToUnicode entry");
        }

        // The access report's PDF is the subject's own portfolio, rendered by the same service.
        accessReport.Should().Equal(portfolio);

        var expected = new Dictionary<Export, (byte[] Bytes, string Text)>
        {
            [Export.Portfolio] = (portfolio, portfolioText),
            [Export.Certificate] = (certificate, certificateText),
            [Export.AccessReport] = (portfolio, portfolioText)
        };

        var failures = new List<string>();
        for (var round = 0; round < Rounds; round++)
        {
            var go = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var exports = Enum.GetValues<Export>()
                .SelectMany(kind => Enumerable.Repeat(kind, ExportsPerKind))
                .Select(kind => Task.Run(async () =>
                {
                    await go.Task;
                    return (Kind: kind, Pdf: await PdfOf(kind, database));
                }))
                .ToArray();
            go.SetResult();

            foreach (var (kind, pdf) in await Task.WhenAll(exports))
            {
                var text = Text(pdf);
                if (text != expected[kind].Text)
                {
                    failures.Add(
                        $"round {round}, {kind}: {text.Count(c => c == '\0')} of {text.Length} characters read as U+0000, " +
                        $"{text.Count(c => c == '\uFFFD')} have no mapping");
                }
                else if (!pdf.AsSpan().SequenceEqual(expected[kind].Bytes))
                {
                    failures.Add($"round {round}, {kind}: the text reads right but the bytes differ (T078's content hash)");
                }
            }
        }

        failures.Should().BeEmpty(
            "each of {0} exports started together ({1} rounds) must read, and hash, as the same export rendered alone; " +
            "{2} did not:{3}",
            Rounds * ExportsPerKind * Enum.GetValues<Export>().Length,
            Rounds,
            failures.Count,
            Environment.NewLine + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public async Task ARenderThatThrows_ReleasesTheGateForTheNext()
    {
        // A render that failed and kept the gate would hang every later export in the process.
        var failing = Document.Create(_ => throw new InvalidOperationException("A section failed to compose."));

        var render = () => QuestPdfRenderer.GeneratePdfAsync(failing, CancellationToken.None);

        await render.Should().ThrowAsync<Exception>();

        var next = Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A5);
            page.Content().Text("Rendered after a failure");
        }));

        var pdf = await QuestPdfRenderer.GeneratePdfAsync(next, CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(10));

        Text(pdf).Should().Contain("Rendered after a failure");
    }

    [Fact]
    public async Task AnExportWhoseCallerHasGone_IsNotRendered()
    {
        // The wait for the gate is the part of an export that can be cancelled: a caller who has gone away stops queueing.
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        // QuestPDF composes a document only when it renders it, so a composed document is a rendered one.
        var composed = 0;
        var document = Document.Create(container =>
        {
            Interlocked.Increment(ref composed);
            container.Page(page => page.Content().Text("Never rendered"));
        });
        var render = () => QuestPdfRenderer.GeneratePdfAsync(document, cancelled.Token);

        await render.Should().ThrowAsync<OperationCanceledException>();
        composed.Should().Be(0, "a caller who has gone away is refused before the render, not after it");

        // The guard: the same document, for a caller still there, is composed, so the count above could have moved.
        await QuestPdfRenderer.GeneratePdfAsync(document, CancellationToken.None);
        composed.Should().BePositive("guard: rendering the document composes it");
    }

    [Fact]
    public async Task ARenderThatQueued_DoesNotHoldTheGateWhileItsCallerIsBusy()
    {
        // On Blazor Server a caller's context is its circuit's dispatcher. A render that queued for the gate and then
        // resumed on that dispatcher would hold the gate, which every export in the process waits on, until that one
        // circuit got round to it. The busy caller here never gets round to it unless the test runs its queue.
        using var firstComposing = new ManualResetEventSlim();
        using var releaseFirst = new ManualResetEventSlim();
        var first = Task.Run(() => QuestPdfRenderer.GeneratePdfAsync(
            Document.Create(container =>
            {
                firstComposing.Set();
                releaseFirst.Wait(TimeSpan.FromSeconds(30));
                container.Page(page => page.Content().Text("Rendered first"));
            }),
            CancellationToken.None));
        firstComposing.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue("the first render holds the gate while it composes");

        var busyCaller = new BusySynchronizationContext();
        var second = StartedOn(busyCaller, () => QuestPdfRenderer.GeneratePdfAsync(
            Document.Create(container => container.Page(page => page.Content().Text("Rendered second"))),
            CancellationToken.None));

        try
        {
            second.IsCompleted.Should().BeFalse("guard: the second render queued behind the first");

            releaseFirst.Set();
            Text(await first.WaitAsync(TimeSpan.FromSeconds(10))).Should().Contain("Rendered first");

            var finished = await Task.WhenAny(second, Task.Delay(TimeSpan.FromSeconds(10)));
            finished.Should().BeSameAs(
                second,
                "a render that has the gate runs without waiting for its caller's context; {0} continuation(s) were " +
                "left waiting for the busy caller",
                busyCaller.Waiting);
            Text(await second).Should().Contain("Rendered second");
        }
        finally
        {
            releaseFirst.Set();

            // A regression leaves the second render waiting here, holding the gate: run it, so the gate is free again.
            busyCaller.RunWaiting();
        }
    }

    private static string Text(byte[] pdf) => string.Join("\f", PdfTextLayer.Pages(pdf));

    /// <summary>Starts <paramref name="start" /> with <paramref name="context" /> as the caller's context.</summary>
    private static T StartedOn<T>(SynchronizationContext context, Func<T> start)
    {
        var previous = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(context);
        try
        {
            return start();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }
    }

    /// <summary>A caller's context that is busy: what is posted to it waits until <see cref="RunWaiting" />.</summary>
    private sealed class BusySynchronizationContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _waiting = new();

        public int Waiting => _waiting.Count;

        public override void Post(SendOrPostCallback d, object? state) => _waiting.Enqueue((d, state));

        public void RunWaiting()
        {
            while (_waiting.TryDequeue(out var work))
            {
                work.Callback(work.State);
            }
        }
    }

    // ─── The three exports, each with its own context, as three requests would have ───────────────────────────────

    /// <remarks>Bounded, so a gate that is never released fails this class instead of hanging the run.</remarks>
    private static Task<byte[]> PdfOf(Export kind, string database)
        => ExportAsync(kind, database).WaitAsync(TimeSpan.FromMinutes(1));

    private static async Task<byte[]> ExportAsync(Export kind, string database)
    {
        await using var db = Db(database);
        switch (kind)
        {
            case Export.Portfolio:
                return (await Portfolio(db).GenerateAsync(
                    new PortfolioExportRequest(Trainee, null, null, SubjectPrincipal()),
                    CancellationToken.None)).PdfBytes;
            case Export.Certificate:
                var decisionId = await db.Set<EntrustmentDecision>().Select(decision => decision.Id).SingleAsync();
                return (await new EntrustmentCertificatePdfService(db).GenerateAsync(
                    new EntrustmentCertificateRequest(decisionId),
                    CancellationToken.None)).PdfBytes;
            default:
                var report = await new AccessReportBuilder(db, Portfolio(db)).BuildAsync(Trainee, CancellationToken.None);
                using (var zip = new ZipArchive(new MemoryStream(report.ZipBytes), ZipArchiveMode.Read))
                {
                    var entry = zip.GetEntry("portfolio-summary.pdf")
                        ?? throw new InvalidOperationException("The access report carried no PDF: its render threw.");
                    await using var pdf = entry.Open();
                    using var bytes = new MemoryStream();
                    await pdf.CopyToAsync(bytes);
                    return bytes.ToArray();
                }
        }
    }

    /// <summary>The per-EPA section prints "today" for an open-ended export (T169), so the clock is pinned.</summary>
    private static PortfolioPdfService Portfolio(ApplicationDbContext db)
        => new(db, new MsfAggregationService(), new FixedClock(new DateTimeOffset(2026, 9, 23, 8, 0, 0, TimeSpan.Zero)));

    private static ClaimsPrincipal SubjectPrincipal()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Trainee)], "test"));

    private static ApplicationDbContext Db(string database)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(database).Options);

    private static void Seed(string database)
    {
        using var db = Db(database);
        db.Set<WombatIdentityUser>().Add(new WombatIdentityUser { Id = Trainee, FirstName = "Lerato", LastName = "Molefe" });
        db.Set<WombatIdentityUser>().Add(new WombatIdentityUser { Id = "chair-1", FirstName = "Thandi", LastName = "Nkosi" });
        db.Set<Institution>().Add(new Institution { Id = 1, Name = "Host Academic Hospital", ShortCode = "HOST" });
        db.Set<Speciality>().Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });
        db.Set<Curriculum>().Add(new Curriculum { Id = 1, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "11.1" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = Trainee,
            InstitutionId = 1,
            CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 2, Name = "Paed General Entrustment Scale" });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 9, ScaleId = 2, Order = 4, Label = "Unsupervised" });
        db.Set<Epa>().Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true });
        db.Set<DecisionPanel>().Add(new DecisionPanel { Id = 1, Name = "Paediatrics CCC", InstitutionId = 1 });
        db.Set<CommitteeReview>().Add(new CommitteeReview { Id = 1, TraineeUserId = Trainee, PanelId = 1 });
        db.Set<EntrustmentDecision>().Add(EntrustmentDecision.Issue(
            Trainee, epaId: 1, authorisedLevelId: 9, issuedOn: new DateOnly(2026, 6, 18), expiresOn: null,
            committeeReviewId: 1, chairUserId: "chair-1", rationale: "Target met.",
            evidenceLinks: StarEvidence.One()));
        db.SaveChanges();
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
