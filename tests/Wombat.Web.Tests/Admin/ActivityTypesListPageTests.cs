using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, lane B): the activity-type list as R2-A01–A12, R3-A09-nocreate, R3-A12, R3-A13 and R3-C08-list draw it.
/// Whose each type is in words (Q7), the caller's own first (Q11, N13), the search applied with Show and Clear and its
/// count line (Q7), the no-match state apart from the empty catalogue (T326), the subtitle by role (Q12, T11, N12), a
/// system-managed row's release line and View (Q9, T334), "Not published" for "v0", the draft's day, and loading and a
/// failed read in fixed words (T329).
/// </summary>
public sealed class ActivityTypesListPageTests : TestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";

    private readonly TestAuthorizationContext _auth;

    public ActivityTypesListPageTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("nolwazi@test");
        _auth.SetRoles(WombatRoles.InstitutionalAdmin);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "inst-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void EachRow_SaysWhoseItIs_ItsVersion_ItsDraft_AndItsStatus_InWords()
    {
        var cut = Render(new ActivityTypeAdminListDto(
        [
            Row(23, "kgk_teaching_log", "KGK Teaching Session Log", ActivityScope.Institution, Kgk, canWrite: true,
                owner: new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Institution, Kgk), version: 0,
                draftOn: new DateTime(2026, 11, 4, 22, 30, 0, DateTimeKind.Utc)),
            Row(11, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityScope.Speciality, "Paediatrics", canWrite: false,
                owner: new ActivityTypeOwnerDto(ActivityTypeOwnerKind.College, "CPSA"))
        ], CanCreate: true));

        cut.FindAll("thead th").Select(Text).Should().Equal("Name", "Set by", "Published", "Draft", "Status", "Actions");
        Text(cut.Find("caption")).Should().Be("Activity types you can read or change");

        var own = RowOf(cut, 23);
        Text(own.QuerySelector("th[scope=row]")!).Should().Be("KGK Teaching Session Logkgk_teaching_log");
        own.QuerySelector("th[scope=row] .person-cell code")!.TextContent.Should().Be("kgk_teaching_log", "the key under the name, in monospace");
        Text(own.QuerySelector("td[data-label='Set by']")!).Should().Be($"{Kgk}Institution");
        Text(own.QuerySelector("td[data-label=Published]")!).Should().Be("Not published", "never \"v0\"");
        Text(own.QuerySelector("td[data-label=Draft]")!).Should().Be("Saved 2026-11-05", "22:30 UTC is already the 5th in South Africa");
        Text(own.QuerySelector("td[data-label=Status]")!).Should().Be("Active");

        var college = RowOf(cut, 11);
        Text(college.QuerySelector("td[data-label='Set by']")!).Should().Be("CPSASpeciality · Paediatrics");
        Text(college.QuerySelector("td[data-label=Published]")!).Should().Be("Version 1");
        Text(college.QuerySelector("td[data-label=Draft]")!).Should().Be("None");

        cut.FindAll(".badge").Should().BeEmpty("words, never a badge, for an owner or a status (F8)");
        cut.Markup.Should().NotContain("v0");
    }

    [Fact]
    public void TheList_StacksWide_InsideTheActivityTypesPage()
    {
        var cut = Render(List(Row(11, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityScope.Speciality, "Paediatrics", canWrite: false)));

        cut.Find("div.people-page.activity-types-page").Should().NotBeNull();
        cut.Find("table").ClassList.Should().Contain(["clinic-table", "clinic-table--stack", "clinic-table--stack-wide"]);
        cut.FindAll("tbody th[scope=row]").Should().HaveCount(1);
        cut.FindAll("tbody td").Where(cell => !cell.ClassList.Contains("col-actions"))
            .Should().OnlyContain(cell => cell.HasAttribute("data-label"));
    }

    [Fact]
    public void ASystemManagedRow_CarriesItsReleaseLine_AndOffersView()
    {
        var cut = Render(List(
            Row(30, "msf_cpsa", "Multi-Source Feedback (Paediatrics)", ActivityScope.Speciality, "Paediatrics", canWrite: false,
                owner: new ActivityTypeOwnerDto(ActivityTypeOwnerKind.College, "CPSA"), writtenBy: "the MSF release")));

        var row = RowOf(cut, 30);
        var spans = row.QuerySelectorAll("th[scope=row] .person-cell > span").Select(Text).ToList();
        spans.Should().Equal("Written by the MSF release.", "msf_cpsa");
        var view = row.QuerySelector("a.btn")!;
        Text(view).Should().Be("View");
        view.GetAttribute("aria-label").Should().Be("View Multi-Source Feedback (Paediatrics)");
        view.GetAttribute("href").Should().Be("/admin/activity-types/30");
    }

    [Fact]
    public void AnInactiveType_ReadsInactive()
    {
        var cut = Render(List(Row(23, "kgk_teaching_log", "KGK Teaching Session Log", ActivityScope.Institution, Kgk, canWrite: true, isActive: false)));

        Text(RowOf(cut, 23).QuerySelector("td[data-label=Status]")!).Should().Be("Inactive");
    }

    [Fact]
    public void TheCallersOwnTypes_ComeFirst_ThenTheRest_EachByName()
    {
        var cut = Render(List(
            Row(1, "dops", "DOPS", ActivityScope.Speciality, "General Medicine", canWrite: false),
            Row(2, "direct_observation_cpsa", "Direct Observation (Paediatrics)", ActivityScope.Speciality, "Paediatrics", canWrite: false),
            Row(3, "kgk_teaching_log", "KGK Teaching Session Log", ActivityScope.Institution, Kgk, canWrite: true),
            Row(4, "acat", "ACAT", ActivityScope.Speciality, "General Medicine", canWrite: false),
            Row(5, "cbd", "Case-based Discussion", ActivityScope.Speciality, "General Medicine", canWrite: false)));

        Names(cut).Should().Equal(
            "KGK Teaching Session Log", "ACAT", "Case-based Discussion", "Direct Observation (Paediatrics)", "DOPS");
    }

    [Fact]
    public void TheAdministratorsList_IsByNameAlone()
    {
        _auth.SetRoles(WombatRoles.Administrator);
        var cut = Render(List(
            Row(1, "msf_cpsa", "Multi-Source Feedback (Paediatrics)", ActivityScope.Speciality, "Paediatrics", canWrite: false, writtenBy: "the MSF release"),
            Row(2, "acat", "ACAT", ActivityScope.Speciality, "General Medicine", canWrite: true),
            Row(3, "cpsa_case_presentation_log", "CPSA Case Presentation Log", ActivityScope.Speciality, "Paediatrics", canWrite: true),
            Row(4, "cbd", "Case-based Discussion", ActivityScope.Speciality, "General Medicine", canWrite: true)));

        Names(cut).Should().Equal("ACAT", "Case-based Discussion", "CPSA Case Presentation Log", "Multi-Source Feedback (Paediatrics)");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "The forms registrars file and assessors complete. You can change every type but the two that Wombat's releases write.");
    }

    [Fact]
    public void AnInstitutionsSubtitle_SaysWhatIsHers()
    {
        var cut = Render(List(Row(1, "acat", "ACAT", ActivityScope.Speciality, "General Medicine", canWrite: false)));

        Text(cut.Find(".page-subtitle")).Should().Be(
            "The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.");
    }

    [Fact]
    public void ACollegeAdminsSubtitle_NamesTheCollege()
    {
        _auth.SetRoles(WombatRoles.CollegeAdmin);
        var cut = Render(List(Row(1, "acat", "ACAT", ActivityScope.Speciality, "General Medicine", canWrite: false))
            with { CallerCollegeName = "College of Paediatricians of South Africa" });

        Text(cut.Find(".page-subtitle")).Should().Be(
            "The forms registrars file and assessors complete. You can change the types College of Paediatricians of South Africa sets; the others you can read.");
    }

    /// <summary>N12 (R3-C08-list): a College with no speciality has no type for its admin to change, and no New activity type.</summary>
    [Fact]
    public void ACollegeWithNoSpeciality_SaysSo_AndOffersNoNewType()
    {
        _auth.SetRoles(WombatRoles.CollegeAdmin);
        var cut = Render(new ActivityTypeAdminListDto([Row(1, "acat", "ACAT", ActivityScope.Speciality, "General Medicine", canWrite: false)], CanCreate: false)
        {
            CallerCollegeName = "College of Surgeons",
            CallerCollegeHasSpeciality = false
        });

        Text(cut.Find(".page-subtitle")).Should().Be(
            "The forms registrars file and assessors complete. College of Surgeons has no speciality yet, so there are no types for you to change.");
        cut.FindAll("a").Should().NotContain(link => Text(link) == "New activity type");
        cut.FindAll("tbody tr").Should().HaveCount(1, "the College still reads every published type");
    }

    [Fact]
    public void NewActivityType_IsOfferedWithItsPlus_ToACallerWhoMayCreate()
    {
        var cut = Render(List(Row(1, "acat", "ACAT", ActivityScope.Speciality, "General Medicine", canWrite: false)));

        var create = cut.FindAll(".header-container a.btn-primary").Should().ContainSingle().Subject;
        Text(create).Should().Be("New activity type");
        create.GetAttribute("href").Should().Be("/admin/activity-types/new");
        create.QuerySelector("svg").Should().NotBeNull();
    }

    [Fact]
    public void TheCountLine_SaysHowManyTypes_WithItsSingular()
    {
        var cut = Render(List(Row(1, "acat", "ACAT", ActivityScope.Speciality, "General Medicine", canWrite: false)));

        var count = cut.Find("p#types-count");
        count.GetAttribute("role").Should().Be("status");
        Text(count).Should().Be("1 activity type.");
        cut.FindAll("button").Should().NotContain(button => Text(button) == "Clear", "Clear comes once a search is applied");
    }

    [Fact]
    public void Show_AppliesTheSearchToNameAndKey_AndFocusesTheCountLine_ThenClearBringsBackEveryRow()
    {
        var cut = Render(Cpsa22());

        cut.Find("input#types-q").Change("cpsa");
        cut.FindAll("tbody tr").Should().HaveCount(22, "nothing is applied until Show");
        cut.Find("form[role=search]").Submit();

        cut.FindAll("tbody tr").Should().HaveCount(12);
        Text(cut.Find("p#types-count")).Should().Be("12 of 22 activity types match “cpsa”.");
        Text(cut.Find("caption")).Should().Be("Activity types matching “cpsa”");
        Names(cut).Should().Contain("Mini-CEX (Paediatrics)", "matched by key: mini_cex_cpsa");
        Focused().Should().Be("types-count");

        cut.FindAll("button").Single(button => Text(button) == "Clear").Click();

        cut.FindAll("tbody tr").Should().HaveCount(22);
        Text(cut.Find("p#types-count")).Should().Be("22 activity types.");
        cut.Find("input#types-q").GetAttribute("value").Should().BeNullOrEmpty();
        Focused().Should().Be("types-q");
    }

    [Fact]
    public void ASearchMatchingNothing_SaysSo_ApartFromTheEmptyCatalogue_AndClearTheSearchBringsBackEveryRow()
    {
        var cut = Render(Cpsa22());

        cut.Find("input#types-q").Change("zzz");
        cut.Find("form[role=search]").Submit();

        cut.FindAll("table").Should().BeEmpty();
        cut.FindAll("p#types-count").Should().BeEmpty();
        var state = cut.Find("#types-no-match");
        Text(state.QuerySelector(".state-panel-title")!).Should().Be("No activity types match \"zzz\".");
        state.QuerySelector(".state-panel-title")!.GetAttribute("role").Should().Be("status");
        Text(state.QuerySelector(".state-panel-copy")!).Should().Be("The search is kept above. Clear it to see all 22.");
        cut.Find("input#types-q").GetAttribute("value").Should().Be("zzz", "the search is kept above");
        cut.Markup.Should().NotContain("Create the first activity type");
        Focused().Should().Be("types-no-match");

        cut.FindAll("button").Single(button => Text(button) == "Clear the search").Click();

        cut.FindAll("tbody tr").Should().HaveCount(22);
        Focused().Should().Be("types-q");
    }

    [Fact]
    public void TheEmptyCatalogue_ToACreator_OffersTheFirstType()
    {
        var cut = Render(new ActivityTypeAdminListDto([], CanCreate: true));

        Text(cut.Find(".state-panel-title")).Should().Be("No activity types");
        Text(cut.Find(".state-panel-copy")).Should().Be("Create the first activity type to start the builder.");
        Text(cut.Find(".detail-card--empty a.btn-primary")).Should().Be("New activity type");
        cut.FindAll("form[role=search]").Should().BeEmpty("nothing to search");
    }

    [Fact]
    public void TheEmptyCatalogue_ToAReader_SaysNothingIsOpen()
    {
        var cut = Render(new ActivityTypeAdminListDto([], CanCreate: false));

        Text(cut.Find(".state-panel-copy")).Should().Be("No activity type is open to you yet.");
        cut.FindAll("a").Should().NotContain(link => Text(link) == "New activity type");
    }

    [Fact]
    public void WhileLoading_TheLoadingLine_AndNoAction()
    {
        var pending = new TaskCompletionSource<ActivityTypeAdminListDto>();
        Services.AddSingleton<IScopedSender>(new FakeSender(() => pending.Task));
        var cut = RenderComponent<ActivityTypesList>();

        var line = cut.Find("p.loading-line");
        line.GetAttribute("role").Should().Be("status");
        Text(line).Should().Be("Loading the activity types.");
        cut.FindAll("a.btn").Should().BeEmpty("no action is offered until the read is in");
        cut.FindAll("form[role=search]").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_SaysSoInFixedWords_AndTryAgainReadsAgain()
    {
        var calls = 0;
        Services.AddSingleton<IScopedSender>(new FakeSender(() => ++calls == 1
            ? Task.FromException<ActivityTypeAdminListDto>(new InvalidOperationException("Npgsql: connection refused at 10.0.0.1"))
            : Task.FromResult(List(Row(1, "acat", "ACAT", ActivityScope.Speciality, "General Medicine", canWrite: false)))));
        var cut = RenderComponent<ActivityTypesList>();

        var alert = cut.WaitForElement(".action-result .alert-danger");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load the activity types. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql").And.NotContain("connection refused");
        cut.FindAll(".detail-card--empty").Should().BeEmpty("no empty state under the failure");
        cut.FindAll(".header-container a").Should().BeEmpty();

        cut.FindAll("button").Single(button => Text(button) == "Try again").Click();

        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);
        JSInterop.Invocations.Should().Contain(call => call.Identifier == PageFocus.FocusHeadingIdentifier, "the h1 after a Try again that read");
    }

    [Fact]
    public void TwoTypesOfOneName_AreToldApartByKey_InTheirActionsNames()
    {
        var cut = Render(List(
            Row(1, "log_a", "Teaching log", ActivityScope.Institution, Kgk, canWrite: true),
            Row(2, "log_b", "Teaching log", ActivityScope.Institution, Kgk, canWrite: true)));

        cut.FindAll("tbody a.btn").Select(link => link.GetAttribute("aria-label"))
            .Should().Equal("Edit Teaching log (log_a)", "Edit Teaching log (log_b)");
    }

    // ---- helpers ----

    private IRenderedComponent<ActivityTypesList> Render(ActivityTypeAdminListDto list)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(() => Task.FromResult(list)));
        var cut = RenderComponent<ActivityTypesList>();
        cut.WaitForState(() => cut.FindAll("p.loading-line").Count == 0);
        return cut;
    }

    private string? Focused()
        => JSInterop.Invocations.LastOrDefault(call => call.Identifier == PageFocus.FocusByIdIdentifier).Arguments?.FirstOrDefault() as string;

    private static ActivityTypeAdminListDto List(params ActivityTypeAdminListItemDto[] rows) => new(rows, CanCreate: true);

    /// <summary>Prof Mbatha's 22 before her own: twelve of CPSA (each key ending _cpsa) and ten of the Demo College.</summary>
    private static ActivityTypeAdminListDto Cpsa22()
    {
        var rows = Enumerable.Range(1, 12)
            .Select(index => Row(index, $"type_{index:00}_cpsa", index == 1 ? "Mini-CEX (Paediatrics)" : $"Paediatric type {index:00}",
                ActivityScope.Speciality, "Paediatrics", canWrite: false))
            .Concat(Enumerable.Range(13, 10)
                .Select(index => Row(index, $"demo_{index}", $"Demo type {index}", ActivityScope.Speciality, "General Medicine", canWrite: false)))
            .ToArray();
        return List(rows);
    }

    private static IElement RowOf(IRenderedFragment cut, int id)
        => cut.FindAll("tbody tr").Single(row => row.QuerySelector($"a[href='/admin/activity-types/{id}']") is not null);

    private static List<string> Names(IRenderedFragment cut)
        => cut.FindAll("tbody th[scope=row] .person-cell")
            .Select(cell => cell.ChildNodes.First(node => node.NodeType == NodeType.Text).TextContent.Trim())
            .ToList();

    private static string Text(IElement element) => element.TextContent.Trim();

    private static ActivityTypeAdminListItemDto Row(
        int id,
        string key,
        string name,
        ActivityScope scope,
        string? target,
        bool canWrite,
        ActivityTypeOwnerDto? owner = null,
        int version = 1,
        DateTime? draftOn = null,
        bool isActive = true,
        string? writtenBy = null)
        => new(id, key, name, null, scope, 7, version, isActive, draftOn is not null, draftOn, canWrite, target)
        {
            Owner = owner,
            SystemManaged = writtenBy is not null,
            WrittenBy = writtenBy
        };

    private sealed class FakeSender(Func<Task<ActivityTypeAdminListDto>> list) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is ListActivityTypesAdminQuery
                ? As<TResponse>(list())
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        private static async Task<TResponse> As<TResponse>(Task<ActivityTypeAdminListDto> read) => (TResponse)(object)await read;

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
