using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Assessors;
using Wombat.Application.Features.Institutions.Queries.GetMyInstitutionName;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Assessors;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T376 (flow 12, lane T): Assessors lists every holder of the Assessor role in the reader's scope, with the training
/// recorded on each person's page (Q2; boards T01–T14). Each row's Record training or Edit training opens that page at
/// <c>?section=training</c>; the list records nothing, so it has no create action. The subtitle names her institution only
/// once it is read (S9); a failed read is a fixed sentence (T329); the pager shows only above one page (F9).
/// </summary>
public sealed class AssessorsListTests : TestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private readonly TestAuthorizationContext _auth;
    private readonly FakeSender _sender = new();

    public AssessorsListTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
        _auth = this.AddTestAuthorization();
        SignIn(WombatRoles.InstitutionalAdmin);
    }

    [Fact]
    public void T02_FiveHoldersWithNoRecord_ReadNotRecorded_AndEachOffersRecordTraining_OnTheirPage()
    {
        _sender.List = Page(Row("botha", "Sarah", "Botha"), Row("khumalo", "Fatma", "Khumalo"), Row("naidoo", "David", "Naidoo"),
            Row("patel", "Mohammed", "Patel"), Row("zulu", "Thandi", "Zulu"));

        var cut = Render();

        cut.Find(".page-subtitle").TextContent.Should().Be(
            $"Everyone at {Kgk} who holds the Assessor role, with the assessor training recorded for each.");
        cut.Find("h2.list-section-title").TextContent.Trim().Should().Be($"Assessors at {Kgk} 5");
        cut.Find("caption").TextContent.Should().Be(
            $"At {Kgk}, by last name: each holder of the Assessor role, and the training recorded on their page.");
        cut.Find("table").ClassList.Should().Contain("clinic-table--stack-wide");
        cut.FindAll("thead th").Select(th => th.TextContent.Trim())
            .Should().Equal("Name", "Scope", "Training status", "Training completed", "Actions");

        var botha = cut.FindAll("tbody tr").ToList()[0];
        var header = botha.QuerySelector("th[scope=row]")!;
        header.ClassList.Should().NotContain("col-wrap", "check Left 3: no .col-wrap on the row header");
        header.QuerySelector(".person-cell")!.TextContent.Should().Be("Sarah Bothabotha@kgk.wombat.local");
        header.InnerHtml.Should().Contain("botha@<wbr>kgk.wombat.local");
        Cell(botha, "Scope").Should().Be("Paediatrics / Paediatrics");
        Cell(botha, "Training status").Should().Be("Not recorded");
        Cell(botha, "Training completed").Should().Be("—");
        var action = botha.QuerySelector("a.btn")!;
        action.TextContent.Trim().Should().Be("Record training");
        action.GetAttribute("aria-label").Should().Be("Record training for Sarah Botha");
        action.GetAttribute("href").Should().Be("/admin/users/botha?section=training");

        cut.FindAll(".pager").Should().BeEmpty("F9: five holders fit one page");
        cut.Markup.Should().NotContain("Create assessor profile", "Q2: training is recorded on the person's page");
    }

    [Fact]
    public void T08_RecordedTraining_ReadsItsStatusAndDate_AndOffersEditTraining()
    {
        _sender.List = Page(
            Row("botha", "Sarah", "Botha", 1, AssessorTrainingStatus.Trained, null),
            Row("khumalo", "Fatma", "Khumalo", 2, AssessorTrainingStatus.Provisional, new DateOnly(2026, 9, 20)),
            Row("patel", "Mohammed", "Patel", 3, AssessorTrainingStatus.InTraining, null),
            Row("smit", "Pieter", "Smit", 4, AssessorTrainingStatus.NotStarted, null));

        var cut = Render();
        var rows = cut.FindAll("tbody tr").ToList();

        (Cell(rows[0], "Training status"), Cell(rows[0], "Training completed")).Should().Be(("Trained", "Not recorded"));
        (Cell(rows[1], "Training status"), Cell(rows[1], "Training completed"))
            .Should().Be(("Provisional (awaiting sign-off)", "2026-09-20"));
        Cell(rows[2], "Training status").Should().Be("In training");
        Cell(rows[3], "Training status").Should().Be("Not started", "the nit: Not started is a record");
        rows[1].QuerySelector("a.btn")!.GetAttribute("aria-label").Should().Be("Edit training for Fatma Khumalo");
        rows[1].QuerySelector("a.btn")!.TextContent.Trim().Should().Be("Edit training");
        rows[1].QuerySelector("td[data-label='Training completed']")!.ClassList.Should().Contain("inv-when",
            "a date never splits on its hyphens");
    }

    [Fact]
    public void T01_NobodyHoldsTheRole_IsTheEmptyCard_WithNoTable()
    {
        _sender.List = new AssessorListDto([], 0, 1, 20);

        var cut = Render();

        cut.Find(".detail-card--empty .state-panel-title").TextContent.Should().Be("Nobody holds the Assessor role yet");
        cut.Find(".detail-card--empty .state-panel-copy").TextContent.Should().StartWith(
            "An assessor's training record holds their training status");
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void T11_ThirtyHolders_ShowTheirFirstTwenty_ThePager_AndATurnFocusesTheHeading()
    {
        _sender.List = new AssessorListDto(Enumerable.Range(1, 20).Select(i => Row($"a{i}", "Assessor", $"N{i:00}")).ToArray(), 30, 1, 20);

        var cut = Render();

        var pager = cut.Find("nav.pager");
        pager.GetAttribute("aria-label").Should().Be("Pages of assessors");
        pager.QuerySelector(".pager-info")!.TextContent.Should().Be("Showing 1–20 of 30");
        cut.Find("h2.list-section-title").TextContent.Trim().Should().Be($"Assessors at {Kgk} 30");

        _sender.List = new AssessorListDto(Enumerable.Range(21, 10).Select(i => Row($"a{i}", "Assessor", $"N{i:00}")).ToArray(), 30, 2, 20);
        var focusBefore = FocusCalls();
        cut.FindAll(".pager button").Single(button => button.TextContent.Trim() == "Next").Click();

        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(10));
        _sender.Lists.Last().Page.Should().Be(2);
        cut.WaitForAssertion(() => FocusCalls().Should().BeGreaterThan(focusBefore, "F9: a turn focuses the list's heading"));
        cut.Find("h2.list-section-title").GetAttribute("tabindex").Should().Be("-1");
    }

    [Fact]
    public void P6_ARowsPerPageChange_ReadsInPlace_AndLeavesTheFocusOnTheSelect()
    {
        // As on Users (T350 build review, A1): a size change re-reads in place, the select is never taken out of the page,
        // and nothing moves the focus off it (T376 build review, P6). Only a page turn focuses the heading.
        _sender.List = new AssessorListDto(Enumerable.Range(1, 20).Select(i => Row($"a{i}", "Assessor", $"N{i:00}")).ToArray(), 60, 1, 20);
        var cut = Render();

        _sender.List = new AssessorListDto(Enumerable.Range(1, 50).Select(i => Row($"a{i}", "Assessor", $"N{i:00}")).ToArray(), 60, 1, 50);
        var focusBefore = FocusCalls();
        cut.Find("nav.pager select").Change("50");

        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(50));
        _sender.Lists.Last().PageSize.Should().Be(50);
        cut.FindAll("p.loading-line").Should().BeEmpty("the list is read in place, so the select stays where the person left it");
        FocusCalls().Should().Be(focusBefore, "the focus stays on the select");
    }

    [Fact]
    public void T12_WhileLoading_TheSubtitleHasNoName_AndTheLoadingLineIsAStatus()
    {
        _sender.Pending = true;

        var cut = RenderComponent<AssessorsList>();

        cut.Find(".page-subtitle").TextContent.Should().Be(AssessorsText.SubtitleWithoutName);
        var line = cut.Find("p.loading-line");
        line.TextContent.Should().Be("Loading the assessors.");
        line.GetAttribute("role").Should().Be("status");
        cut.Find(".people-page").GetAttribute("aria-busy").Should().Be("true");
    }

    [Fact]
    public void T13_AFailedRead_IsAFixedSentence_WithTryAgain_NeverTheExceptionsText()
    {
        _sender.Failure = new InvalidOperationException("Npgsql: connection refused at 10.0.0.1");

        var cut = RenderComponent<AssessorsList>();
        cut.WaitForState(() => cut.FindAll(".alert-danger").Count > 0);

        cut.Find(".alert-danger").TextContent.Should().Contain(AssessorsText.LoadFailed).And.NotContain("Npgsql");
        cut.FindAll("button").Should().Contain(button => button.TextContent.Trim() == "Try again");
        cut.Find(".page-subtitle").TextContent.Should().Be(AssessorsText.SubtitleWithoutName, "S9");
    }

    [Fact]
    public void T14_TheAdministrator_ReadsEveryInstitution_WithAnInstitutionColumn()
    {
        SignIn(WombatRoles.Administrator);
        _sender.List = Page(Row("botha", "Sarah", "Botha"), Row("demo", "Demo", "Assessor") with { InstitutionName = "Demo Institution" },
            Row("nowhere", "Ned", "Nobody") with { InstitutionName = null });

        var cut = Render();

        cut.Find(".page-subtitle").TextContent.Should().Be(
            "Everyone who holds the Assessor role, at every institution, with the assessor training recorded for each.");
        cut.Find("h2.list-section-title").TextContent.Trim().Should().Be("Assessors at every institution 3");
        cut.Find("caption").TextContent.Should().Be("Every institution, by last name: each holder of the Assessor role and their training.");
        cut.FindAll("thead th").Select(th => th.TextContent.Trim()).Should().Contain("Institution");
        Cell(cut.FindAll("tbody tr").ToList()[1], "Institution").Should().Be("Demo Institution");
        Cell(cut.FindAll("tbody tr").ToList()[2], "Institution").Should().Be(PersonScopeWords.NoInstitution);
        _sender.AskedName.Should().BeFalse("the Administrator's words name no institution");
    }

    [Fact]
    public void S8_ATraineeWhoAdministers_ReadsTheNote_AndNoSubtitle_AndNothingIsRead()
    {
        _auth.SetRoles(WombatRoles.InstitutionalAdmin, WombatRoles.Trainee);

        var cut = RenderComponent<AssessorsList>();

        var note = cut.Find(".alert");
        note.TextContent.Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
        note.HasAttribute("role").Should().BeFalse("S23: a standing notice, not a live region");
        cut.FindAll(".page-subtitle").Should().BeEmpty();
        _sender.Lists.Should().BeEmpty();
        _sender.AskedName.Should().BeFalse();
    }

    [Fact]
    public void EveryIdTheTableNames_Exists()
    {
        _sender.List = new AssessorListDto(Enumerable.Range(1, 20).Select(i => Row($"a{i}", "Assessor", $"N{i:00}")).ToArray(), 25, 1, 20);

        var cut = Render();

        IdReferences.Broken(cut).Should().BeEmpty();
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<AssessorsList> Render()
    {
        var cut = RenderComponent<AssessorsList>();
        cut.WaitForState(() => cut.FindAll("table, .detail-card--empty").Count > 0);
        return cut;
    }

    private int FocusCalls() => JSInterop.Invocations.Count(invocation => invocation.Identifier == FocusIdentifier);

    private void SignIn(params string[] roles)
    {
        _auth.SetAuthorized("mbatha@kgk.test");
        _auth.SetRoles(roles);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"), new Claim(WombatClaimTypes.InstitutionId, "1"));
    }

    private static string Cell(IElement row, string label)
        => row.QuerySelector($"td[data-label='{label}']")!.TextContent.Trim();

    private static AssessorListDto Page(params AssessorRowDto[] rows) => new(rows, rows.Length, 1, 20);

    private static AssessorRowDto Row(
        string id, string first, string last, int? profileId = null, AssessorTrainingStatus? status = null, DateOnly? completedOn = null)
        => new(id, $"{id}@kgk.wombat.local", first, last, Kgk, "Paediatrics / Paediatrics", profileId, status, completedOn);

    private sealed class FakeSender : IScopedSender
    {
        public AssessorListDto List { get; set; } = new([], 0, 1, 20);
        public Exception? Failure { get; set; }
        public bool Pending { get; set; }
        public bool AskedName { get; private set; }
        public List<ListAssessorsQuery> Lists { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (Pending)
            {
                return new TaskCompletionSource<TResponse>().Task;
            }

            switch (request)
            {
                case GetMyInstitutionNameQuery:
                    AskedName = true;
                    return Task.FromResult((TResponse)(object)Kgk);
                case ListAssessorsQuery list:
                    Lists.Add(list);
                    return Failure is not null ? Task.FromException<TResponse>(Failure) : Task.FromResult((TResponse)(object)List);
                default:
                    throw new NotSupportedException(request.GetType().Name);
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
