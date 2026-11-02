using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Epas;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T392 (flow 15, lane C): the EPA list as R2-C01–C03, R3-C09 and C15–C17 draw it. Whose is what in words (Q3, F8), Edit
/// only where the query's "may change" says so (T291 item 1, S4), Create EPA only when it can be completed (Q12), the
/// Administrator's pager (P5), and a failed read in fixed words (T329).
/// </summary>
public sealed class EpasListPageTests : TestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";

    public EpasListPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("nolwazi@test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "inst-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TheInstitutionsList_MarksItsOwnEpa_AndOffersEditOnItAlone()
    {
        var cut = Render(new EpaListDto([Own(17, "KGK-001", "Running a paediatric outreach clinic at a district hospital"), National(1, "PAED-001", "Providing paediatric emergency care to children")],
            EpaListReader.Institution, true, Kgk));

        Text(cut.Find(".page-subtitle")).Should().Be(
            "The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change.");
        Text(cut.Find("caption")).Should().Be("EPAs your institution reads");
        cut.FindAll("thead th").Select(Text).Should().Equal("EPA", "College", "Sub-speciality", "Status", "Actions");

        var own = cut.FindAll("tbody tr").First();
        own.QuerySelector("th[scope=row] a")!.GetAttribute("href").Should().Be("/admin/epas/17");
        Text(own.QuerySelector("th[scope=row] a")!).Should().Be("KGK-001 — Running a paediatric outreach clinic at a district hospital");
        Text(own.QuerySelector("th[scope=row] .muted.text-sm")!).Should().Be("Your institution's own EPA");
        own.QuerySelector("td div.actions-cell a")!.GetAttribute("aria-label")
            .Should().Be("Edit KGK-001 — Running a paediatric outreach clinic at a district hospital");

        var national = cut.FindAll("tbody tr").ElementAt(1);
        national.QuerySelector("th[scope=row] .muted").Should().BeNull("a national EPA carries no owner mark");
        national.QuerySelector("td div.actions-cell a").Should().BeNull("UpdateEpa refuses her a national EPA");
        Text(national.QuerySelector("td div.actions-cell span.muted")!).Should().Be("Set by the College.");
        Text(national.QuerySelectorAll("td").ElementAt(1)).Should().Be("Paediatrics / Paediatrics");
        cut.FindAll(".badge").Should().BeEmpty("words, never a badge, on the admin lists (F8)");

        var create = cut.FindAll(".header-container a.btn-primary").Should().ContainSingle().Subject;
        Text(create).Should().Be("Create EPA");
        create.QuerySelector("svg").Should().NotBeNull("Create EPA keeps its plus (round-3-check decision 4)");
    }

    [Fact]
    public void AnInactiveEpa_ReadsInactive_InWords()
    {
        var cut = Render(new EpaListDto([National(13, "PAED-012", "Communicating", isActive: false)], EpaListReader.Institution, true, Kgk));

        Text(cut.FindAll("tbody tr td").ElementAt(2)).Should().Be("Inactive");
    }

    [Fact]
    public void NothingAdopted_OffersNoCreate_AndSaysWhy_WithAWayToCurricula()
    {
        var cut = Render(new EpaListDto([], EpaListReader.Institution, false, Kgk));

        cut.FindAll(".header-container a.btn-primary").Should().BeEmpty("Create EPA could not be completed (Q12)");
        Text(cut.Find(".state-panel-title")).Should().Be("No EPAs yet");
        Text(cut.Find(".state-panel-copy")).Should().Be(
            "Your institution has not adopted a curriculum yet, so there is no discipline to add an EPA to. Adopt one under Curricula.");
        var go = cut.Find(".detail-card--empty a");
        Text(go).Should().Be("Go to Curricula");
        go.GetAttribute("href").Should().Be("/admin/curricula");
    }

    [Fact]
    public void TheAdministratorsList_NamesEachOwner_TellsTwoOwnEpasOfOneCodeApart_AndPagesAtTwenty()
    {
        var epas = new List<EpaDto>
        {
            Own(17, "KGK-001", "Outreach") with { NamesOwner = true },
            Own(40, "KGK-001", "Outreach", "Groote Schuur Hospital") with { NamesOwner = true }
        };
        epas.AddRange(Enumerable.Range(1, 38).Select(index => National(100 + index, $"PAED-{index:000}", $"EPA {index}", mayChange: true)));
        var cut = Render(new EpaListDto(epas, EpaListReader.Administrator, true, null));

        Text(cut.Find(".page-subtitle")).Should().Be("Every College's EPAs, and every institution's own.");
        Text(cut.Find("caption")).Should().Be("Every EPA");
        cut.FindAll("tbody tr").Should().HaveCount(20, "the Administrator's list pages at 20 (P5)");
        var marks = cut.FindAll("tbody th[scope=row] .muted.text-sm").Select(Text).ToList();
        marks.Should().Equal("Kgosi Kgari Teaching Hospital's own EPA", "Groote Schuur Hospital's own EPA");
        var edits = cut.FindAll("tbody a[aria-label^='Edit KGK-001']").Select(edit => edit.GetAttribute("aria-label")).ToList();
        edits.Should().OnlyHaveUniqueItems();
        edits[0].Should().Be("Edit KGK-001 — Outreach (Kgosi Kgari Teaching Hospital's own EPA)");

        var pager = cut.Find("nav.pager");
        pager.GetAttribute("aria-label").Should().Be("Pages of EPAs");
        Text(cut.Find(".pager-page-size-label")).Should().Be("EPAs per page");

        cut.FindAll(".pager button").Single(button => Text(button) == "Next").Click();
        cut.FindAll("tbody tr").Should().HaveCount(20);
        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier.Contains("focus", StringComparison.OrdinalIgnoreCase),
            "a page turn moves the focus to the list's heading");
    }

    [Fact]
    public void AShortList_ShowsNoPager()
    {
        var cut = Render(new EpaListDto([National(1, "PAED-001", "One")], EpaListReader.Institution, true, Kgk));

        cut.FindAll(".pager").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsFixedWords_WithTryAgain_NeverTheExceptionsText_AndNoCreate()
    {
        var sender = new Sender { Failure = new InvalidOperationException("Npgsql exploded") };
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<EpasList>();

        cut.WaitForAssertion(() => Text(cut.Find(".alert.alert-danger .alert-row-text"))
            .Should().Be("Could not load the EPAs. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Npgsql");
        cut.FindAll(".header-container a.btn-primary").Should().BeEmpty("no action is offered until the read is in");

        sender.Failure = null;
        sender.List = new EpaListDto([National(1, "PAED-001", "One")], EpaListReader.Institution, true, Kgk);
        cut.FindAll("button").Single(button => Text(button) == "Try again").Click();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().ContainSingle());
    }

    [Fact]
    public void WhileItIsRead_ItSaysSo_AndOffersNothing()
    {
        var sender = new Sender { Held = new TaskCompletionSource<EpaListDto>() };
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<EpasList>();

        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading the EPAs.");
        cut.FindAll(".header-container a.btn-primary").Should().BeEmpty();
    }

    private IRenderedComponent<EpasList> Render(EpaListDto list)
    {
        Services.AddSingleton<IScopedSender>(new Sender { List = list });
        var cut = RenderComponent<EpasList>();
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private static EpaDto National(int id, string code, string title, bool isActive = true, bool mayChange = false)
        => new EpaDto(id, 2, "Paediatrics", "College of Paediatricians of South Africa", code, title, null, null, EpaCategory.Core, isActive, DateTime.UnixEpoch)
        {
            MayChange = mayChange,
            DisciplineName = "Paediatrics / Paediatrics"
        };

    private static EpaDto Own(int id, string code, string title, string owner = Kgk)
        => National(id, code, title, mayChange: true) with { OwningInstitutionId = owner.Length, OwningInstitutionName = owner };

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        public EpaListDto? List { get; set; }
        public Exception? Failure { get; set; }
        public TaskCompletionSource<EpaListDto>? Held { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is not GetEpaListQuery)
            {
                throw new NotSupportedException(request.GetType().Name);
            }

            if (Failure is not null)
            {
                return Task.FromException<TResponse>(Failure);
            }

            return Held is not null
                ? Held.Task.ContinueWith(task => (TResponse)(object)task.Result, cancellationToken)
                : Task.FromResult((TResponse)(object)List!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
