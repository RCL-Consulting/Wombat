using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T400 (flow 18, lane A; boards R2-I01–I06): the Administrator's institutions. Q5's subtitle; the list's own class, which
/// the 1150px stack reads; the name the row's header; "None" for no contact email; the status a badge; Created the South
/// African day; Edit named for its row; Create institution once the rows are in; the loading line and the house load error.
/// </summary>
public sealed class InstitutionsListPageTests : TestContext
{
    public InstitutionsListPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("devadmin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetPolicies("Administrator");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "devadmin"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void I02_TheRows_ByTheQuery_InTheBoardsColumns_AndWords()
    {
        var cut = Render(new Sender(
            new InstitutionDto(1, "Demo Institution", "DEMO", "admin@demo.local", true, new DateTime(2026, 11, 10, 6, 0, 0, DateTimeKind.Utc)),
            // Created at 22:30 UTC on the 10th: the 11th in South Africa (T24).
            new InstitutionDto(2, "Kgosi Kgari Teaching Hospital", "KGK", null, false, new DateTime(2026, 11, 10, 22, 30, 0, DateTimeKind.Utc))));

        Text(cut.Find("h1")).Should().Be("Institutions");
        Text(cut.Find(".page-subtitle")).Should().Be("The institutions that hold Wombat's people and records.");
        cut.Find("div.people-page.institutions-page").Should().NotBeNull("the 1150px stack reads the list's class (C1, S9)");
        Text(cut.Find("table caption")).Should().Be("Institutions");
        cut.Find("table").ClassList.Should().Contain("clinic-table--stack-wide");
        cut.FindAll("thead th").Select(Text).Should().Equal("Name", "Short code", "Contact email", "Status", "Created", "Actions");

        var rows = cut.FindAll("tbody tr").ToList();
        Text(rows[0].QuerySelector("th[scope=row]")!).Should().Be("Demo Institution");
        rows[0].QuerySelectorAll("td[data-label]").Select(cell => (cell.GetAttribute("data-label"), Text(cell))).Should().Equal(
            ("Short code", "DEMO"), ("Contact email", "admin@demo.local"), ("Status", "Active"), ("Created", "2026-11-10"));
        rows[0].QuerySelector("td[data-label=Status] .badge")!.ClassList.Should().Contain("badge-completed");
        rows[1].QuerySelectorAll("td[data-label]").Select(Text).Should().Equal("KGK", "None", "Inactive", "2026-11-11");
        rows[1].QuerySelector("td[data-label='Contact email'] .muted").Should().NotBeNull();
        rows[1].QuerySelector("td[data-label=Status] .badge")!.ClassList.Should().Contain("badge-withdrawn");

        var edit = rows[1].QuerySelector("a.btn")!;
        Text(edit).Should().Be("Edit");
        edit.GetAttribute("aria-label").Should().Be("Edit Kgosi Kgari Teaching Hospital");
        edit.GetAttribute("href").Should().Be("/admin/institutions/2");
        cut.FindAll(".actions-cell a").Select(link => (Text(link), link.GetAttribute("href")))
            .Should().Equal(("Create institution", "/admin/institutions/new"));
        cut.FindAll("[style], td.actions-cell, .pager").Should().BeEmpty("no inline style, no actions cell in a row, no pager");
    }

    [Fact]
    public void I04_Loading_SaysSo_AndOffersNoCreateUntilTheRowsAreIn()
    {
        Services.AddSingleton<IScopedSender>(new Sender());
        var cut = RenderComponent<InstitutionsList>();

        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading the institutions.");
        cut.FindAll(".actions-cell a").Should().BeEmpty("Create institution appears once the rows have loaded (R2-I04)");
    }

    [Fact]
    public void I05_AFailedRead_IsTheHouseSentence_WithTryAgain_NeverTheExceptions()
    {
        var cut = Render(new Sender { Failure = new InvalidOperationException("Npgsql: connection refused") });

        Text(cut.Find(".alert.alert-danger .alert-row-text")).Should().Be(
            "Could not load the institutions. Nothing has changed. Try again, or come back in a few minutes.");
        cut.FindAll(".alert-danger button").Select(Text).Should().Equal("Try again");
        cut.Markup.Should().NotContain("Npgsql");
        cut.FindAll("table, .actions-cell a").Should().BeEmpty("nothing under the alert");
    }

    private IRenderedComponent<InstitutionsList> Render(Sender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<InstitutionsList>();
        cut.WaitForState(() => cut.FindAll("table").Count > 0 || cut.FindAll(".alert-danger").Count > 0);
        return cut;
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>Answers the list, or fails, or (with no rows given and no failure) never answers.</summary>
    private sealed class Sender(params InstitutionDto[] rows) : IScopedSender
    {
        public Exception? Failure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            request.Should().BeOfType<GetInstitutionsListQuery>();
            if (Failure is not null)
            {
                return Task.FromException<TResponse>(Failure);
            }

            return rows.Length == 0
                ? new TaskCompletionSource<TResponse>().Task
                : Task.FromResult((TResponse)(object)(IReadOnlyList<InstitutionDto>)rows);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
