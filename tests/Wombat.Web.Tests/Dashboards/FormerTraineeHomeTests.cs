using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.FormerTrainee;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Web.Components.Pages;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// T379 (flow 13, lane R; R3-H01–H07; Q1, Q23, F6, S8, S9): a former trainee's Home is one emphasised card, "Your training
/// record", under DashboardFrame: the end and its day, the record kept read-only, then the record's four pages, each its
/// page's title as the link with a line under it. Until T379 it was one sentence and "Open My progress →".
/// </summary>
public sealed class FormerTraineeHomeTests : WombatTestContext
{
    private const string FailureText = "Npgsql could not reach the database (a fake failure).";

    [Fact]
    public void AGraduatesCard_LeadsWithTheEnd_ThenTheRecordsPages_EachWithItsLine()
    {
        // R3-H01: Lerato Molefe after graduating.
        var cut = RenderHome(new Sender(Record(completed: true, year: 4, new RecordStarsDto(true, 15))));

        cut.FindAll(".page-subtitle").Should().BeEmpty("no role: no subtitle (flow 01)");
        cut.FindAll(".header-container .btn").Should().BeEmpty("no header action");
        var card = cut.Find(".dashboard-grid > section.detail-card.detail-card--emphasis");
        Text(card.QuerySelector("h2")!).Should().Be("Your training record");
        var body = card.QuerySelector(".record-card-body")!;
        body.QuerySelectorAll(":scope > p").Select(Text).Should().Equal(
            "You completed your programme on 2026-10-26.",
            "Your record is kept, read-only: no target applies to you any more.");
        body.QuerySelector("p")!.ClassList.Should().Contain("ended-line");

        Rows(cut).Should().Equal(
            ("My progress", "/portfolio/progress", "Every period of your programme, to training year 4, and your standing against Annexure A."),
            ("My authorisations", "/portfolio/authorisations", "Your 15 STARs in force, each with its certificate to download."),
            ("My activities", "/activities/mine", "Every activity you filed or that was filed about you, read-only."),
            ("Export portfolio", "/portfolio/export", "A PDF of your record. Anyone holding it can check it on Verify portfolio export."));
        foreach (var link in cut.FindAll(".record-links a"))
        {
            cut.Find($"#{link.GetAttribute("aria-describedby")}").ClassList.Should().Contain("record-link-why", "each link is described by its line");
        }

        cut.Markup.Should().NotContain("&rarr;").And.NotContain("→").And.NotContain("You hold no role at the moment");
    }

    [Fact]
    public void AWithdrawalWithNoStar_ReadsTheEnd_AndTheStarsRowAsText_WithNoLink()
    {
        // R3-H02, F6: Pieter du Plessis, no STAR decision of any kind.
        var cut = RenderHome(new Sender(Record(completed: false, year: 2, new RecordStarsDto(false, 0))));

        Text(cut.Find(".ended-line")).Should().Be("Your programme ended on 2026-10-24.");
        var stars = cut.FindAll(".record-links li").ToArray()[1];
        stars.QuerySelector("a").Should().BeNull();
        Text(stars.QuerySelector(".record-link-plain")!).Should().Be("STARs");
        Text(stars.QuerySelector(".record-link-none")!).Should().Be("No STAR was issued to you, so there is no certificate to download.");
        cut.FindAll("a[href='/portfolio/authorisations']").Should().BeEmpty();
        Rows(cut)[0].Line.Should().Be("Every period of your programme, to training year 2, and your standing against Annexure A.");
    }

    [Theory]
    [InlineData(0, "None of your STARs is in force now; My authorisations lists them.")]
    [InlineData(1, "Your 1 STAR in force, with its certificate to download.")]
    public void AStarIssued_ButFewOrNoneInForce_IsSaidSo(int inForce, string line)
    {
        // S8's PROPOSED sentence (drawn on no board) for a STAR issued but none in force; the singular for one.
        var cut = RenderHome(new Sender(Record(completed: true, year: 4, new RecordStarsDto(true, inForce))));

        Rows(cut)[1].Should().Be(("My authorisations", "/portfolio/authorisations", line));
    }

    [Fact]
    public void WhileTheRecordIsRead_TheCardIsItsTitleAndASkeleton_AndOffersNothing()
    {
        // R3-H05.
        var cut = RenderHome(new Sender(answer: null) { Hang = true });

        cut.Find("p[role=status]").TextContent.Should().Be(DashboardFrame.LoadingWords);
        Text(cut.Find(".dashboard-grid h2")).Should().Be("Your training record");
        cut.FindAll(".dashboard-grid a").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsHomesFixedWords_WithTryAgain_NeverTheExceptionsText()
    {
        // R3-H06: flow 01's words; Try again reads again and the card comes.
        var sender = new Sender(Record(completed: true, year: 4, new RecordStarsDto(true, 15))) { Fail = true };
        var cut = RenderHome(sender);

        cut.Find(".alert-danger").TextContent.Should().Contain("Could not load your Home.");
        cut.Markup.Should().NotContain(FailureText);

        sender.Fail = false;
        cut.Find(".alert-danger button").Click();

        cut.WaitForAssertion(() => cut.FindAll(".record-links li").Should().HaveCount(4));
    }

    [Fact]
    public void ARecordHolderWithNoEndedProgrammeToShow_IsShownNoRoleAssigned()
    {
        // The read answers nothing (a running profile, no profile, an end Wombat never dated): flow 01's card (R3-H04).
        var cut = RenderHome(new Sender(answer: null));

        Text(cut.Find(".dashboard-grid h2")).Should().Be("No role assigned");
        cut.FindAll(".dashboard-grid a").Should().BeEmpty();
    }

    [Fact]
    public void NeitherRoleNorRecord_IsNoRoleAssigned_AndReadsNothing()
    {
        // R3-H04: no claim, so the card is not drawn and nothing is read.
        var sender = new Sender(answer: null);
        var cut = RenderHome(sender, withRecord: false);

        Text(cut.Find(".dashboard-grid")).Should().Contain("No role assigned");
        sender.Count.Should().Be(0);
    }

    // ---- rendering ----

    private IRenderedComponent<Home> RenderHome(Sender sender, bool withRecord = true)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("molefe@kgk.wombat.local");
        Claim[] claims = withRecord
            ? [new Claim(ClaimTypes.NameIdentifier, "molefe"), new Claim(WombatClaimTypes.TraineeRecord, "true")]
            : [new Claim(ClaimTypes.NameIdentifier, "molefe")];
        auth.SetClaims(claims);
        Services.AddSingleton<IScopedSender>(sender);
        Services.AddSingleton(TimeProvider.System);

        return RenderComponent<Home>(parameters => parameters.AddCascadingValue(ActingRole.None));
    }

    private static FormerTraineeHomeDto Record(bool completed, int year, RecordStarsDto stars)
        => new(completed ? new DateOnly(2026, 10, 26) : new DateOnly(2026, 10, 24), completed, year, stars);

    /// <summary>Each linked row of the card: the link's words, its href and its line.</summary>
    private static List<(string Label, string? Href, string Line)> Rows(IRenderedFragment cut)
        => cut.FindAll(".record-links li")
            .Where(row => row.QuerySelector("a") is not null)
            .Select(row => (Text(row.QuerySelector("a")!), row.QuerySelector("a")!.GetAttribute("href"), Text(row.QuerySelector(".record-link-why")!)))
            .ToList();

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Answers the record's read, or hangs, or fails, as the test sets it; counts the reads.</summary>
    private sealed class Sender(FormerTraineeHomeDto? answer) : IScopedSender
    {
        public bool Hang { get; init; }

        public bool Fail { get; set; }

        public int Count { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Count++;
            request.Should().BeOfType<GetFormerTraineeHomeQuery>();
            if (Hang)
            {
                return new TaskCompletionSource<TResponse>().Task;
            }

            return Fail
                ? Task.FromException<TResponse>(new InvalidOperationException(FailureText))
                : Task.FromResult((TResponse)(object)answer!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
