using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Institutions.Queries.GetCollegeTree;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T394 (flow 16, lane A, Q1): the College's tree as R3-A02, A04, A07, A12, A13b, A14b, A16b, A22 and R2-A10, A18a, A19a,
/// A20a draw it. One h2 section a speciality, its id the tree's section id; its sub-specialities its rows with the ladder
/// each uses; Edit and Create sub-speciality named for their speciality (M5, M8); no Back to colleges (T291); the tokens'
/// results focused and the token dropped (S13); College unavailable for what the caller cannot open (Q7); N4's names.
/// </summary>
public sealed class CollegeTreePageTests : TestContext
{
    private const string Cpsa = "College of Paediatricians of South Africa";

    public CollegeTreePageTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void TheTree_IsOneSectionASpeciality_ItsSubSpecialitiesItsRows_WithTheLadderEachUses()
    {
        var cut = Render(new Sender { Tree = Tree() });

        Text(cut.Find("h1")).Should().Be("Specialities");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "College of Paediatricians of South Africa (CPSA): its specialities, their sub-specialities and the ladder each uses.");
        cut.Find(".people-page").ClassList.Should().Contain("college-tree-page");

        var section = cut.Find("section.user-section");
        section.GetAttribute("aria-labelledby").Should().Be("speciality-2");
        cut.Find("h2#speciality-2").TextContent.Should().Be("Paediatrics");
        Text(section.QuerySelector("p.muted")!).Should().Be("Specialist training in Paediatrics. · Active");
        Text(section.QuerySelector("caption")!).Should().Be("Sub-specialities of Paediatrics");
        section.QuerySelectorAll("thead th").Select(Text).Should().Equal(
            "Sub-speciality", "Description", "Default scale", "Status", "Actions");

        var rows = section.QuerySelectorAll("tbody tr").ToList();
        rows.Select(row => Text(row.QuerySelector("th[scope=row]")!)).Should().Equal("Neonatology", "Paediatrics");
        rows[1].QuerySelectorAll("td[data-label]").Select(Text).Should().Equal(
            "General paediatric specialist training programme.", "CPSA Paediatric Entrustment Scale v11.1", "Active");
        rows[0].QuerySelectorAll("td[data-label]").Select(Text).Should().Equal(
            "Certificate training in Neonatology.", "CPSA Paediatric Entrustment Scale v11.1", "Inactive");
        cut.FindAll(".clinic-table").Should().OnlyContain(table => table.ClassList.Contains("clinic-table--stack-wide"));
    }

    [Fact]
    public void EachSectionsActions_AndEachRowsEdit_AreNamedForTheirRecord_AndLeadToIt()
    {
        var cut = Render(new Sender { Tree = Tree() });

        var head = cut.Find(".form-section-head");
        head.QuerySelectorAll("a").Select(link => (link.GetAttribute("aria-label"), link.GetAttribute("href"))).Should().Equal(
            ("Edit the speciality Paediatrics", "/admin/colleges/2/specialities/2"),
            ("Create sub-speciality of Paediatrics", "/admin/specialities/2/sub-specialities/new"));
        cut.FindAll("tbody a").Select(link => (link.GetAttribute("aria-label"), link.GetAttribute("href"))).Should().Equal(
            ("Edit the sub-speciality Neonatology", "/admin/specialities/2/sub-specialities/4"),
            ("Edit the sub-speciality Paediatrics", "/admin/specialities/2/sub-specialities/2"));

        var create = cut.Find(".header-container .actions-cell a");
        Text(create).Should().Be("Create speciality");
        create.GetAttribute("href").Should().Be("/admin/colleges/2/specialities/new");
        cut.FindAll("a").Should().NotContain(link => Text(link) == "Back to colleges", "T291: the trail is the way back");
        cut.FindAll("a").Should().NotContain(link => Text(link) == "Sub-specialities", "Q1: the sub-specialities are the section's rows");
    }

    [Fact]
    public void ASpecialityWithNoSubSpeciality_SaysSo_InPlaceOfItsTable_AndASectionWithNoDescription_ShowsItsStatusAlone()
    {
        var tree = new CollegeTreeDto(3, "College of Neurologists of South Africa", "CNSA",
            [new CollegeTreeSpecialityDto(3, "Neurology", null, true, [])]);
        var cut = Render(new Sender { Tree = tree }, collegeId: 3);

        cut.FindAll("table").Should().BeEmpty();
        cut.FindAll("section.user-section p.muted").Select(Text).Should().Equal(
            "Active", "No sub-specialities yet. Create the first sub-speciality of Neurology.");
    }

    [Fact]
    public void ACollegeWithNoSpeciality_IsTheEmptyState_WithCreateSpecialityInTheHeader()
    {
        var cut = Render(new Sender { Tree = new CollegeTreeDto(3, "College of Neurologists of South Africa", "CNSA", []) }, collegeId: 3);

        Text(cut.Find(".detail-card--empty .state-panel-title")).Should().Be("No specialities yet");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            "Create the first speciality for College of Neurologists of South Africa.");
        Text(cut.Find(".header-container .actions-cell a")).Should().Be("Create speciality");
    }

    [Fact]
    public void AColumnNoRowFills_IsNotDrawn()
    {
        var tree = new CollegeTreeDto(3, "College of Neurologists of South Africa", "CNSA",
            [new CollegeTreeSpecialityDto(3, "Neurology", null, true, [new CollegeTreeSubSpecialityDto(3, 3, "Adult Neurology", null, true, null, null)])]);
        var cut = Render(new Sender { Tree = tree }, collegeId: 3);

        cut.FindAll("thead th").Select(Text).Should().Equal("Sub-speciality", "Default scale", "Status", "Actions");
        cut.FindAll("tbody td[data-label]").Select(Text).Should().Equal("No default", "Active");
    }

    [Fact]
    public void TwoSubSpecialitiesSharingAName_AreEachNamedWithTheirSpeciality()
    {
        // N4: within one speciality a name is unique; across two, the row's name adds its speciality.
        var tree = new CollegeTreeDto(2, Cpsa, "CPSA",
        [
            new CollegeTreeSpecialityDto(2, "Paediatrics", null, true, [new CollegeTreeSubSpecialityDto(4, 2, "Neonatology", null, true, null, null)]),
            new CollegeTreeSpecialityDto(5, "Surgery", null, true, [new CollegeTreeSubSpecialityDto(6, 5, "Neonatology", null, true, null, null), new CollegeTreeSubSpecialityDto(7, 5, "Trauma", null, true, null, null)])
        ]);
        var cut = Render(new Sender { Tree = tree });

        cut.FindAll("tbody th[scope=row]").Select(Text).Should().Equal("Neonatology (Paediatrics)", "Neonatology (Surgery)", "Trauma");
        cut.FindAll("tbody a").Select(link => link.GetAttribute("aria-label")).Should().Equal(
            "Edit the sub-speciality Neonatology (Paediatrics)", "Edit the sub-speciality Neonatology (Surgery)", "Edit the sub-speciality Trauma");
    }

    [Theory]
    [InlineData("?saved=speciality:2", "Speciality saved.")]
    [InlineData("?created=speciality:2", "Speciality Paediatrics created.")]
    [InlineData("?created=sub:4", "Sub-speciality Neonatology created.")]
    [InlineData("?saved=sub:2", "Sub-speciality saved.")]
    public void ArrivingByAToken_WritesItsResult_FocusesIt_AndDropsTheToken(string query, string result)
    {
        var cut = Render(new Sender { Tree = Tree() }, query: query);
        var navigation = Services.GetRequiredService<NavigationManager>();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-success")).Should().Be(result));
        cut.Find(".action-result .alert strong").TextContent.Should().Be(result, "its first sentence is bold");
        cut.FindAll(".action-result").Should().ContainSingle("S3: one result region");
        cut.WaitForAssertion(() => navigation.Uri.Should().EndWith("/admin/colleges/2/specialities"));
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus"));
    }

    [Theory]
    [InlineData("?created=sub:99")]
    [InlineData("?saved=speciality:99")]
    [InlineData("?created=sub:abc")]
    public void AStaleOrForeignToken_WritesNothing_AndFocusesNothing_ButIsDropped(string query)
    {
        var cut = Render(new Sender { Tree = Tree() }, query: query);
        var navigation = Services.GetRequiredService<NavigationManager>();

        cut.WaitForAssertion(() => navigation.Uri.Should().EndWith("/admin/colleges/2/specialities"));
        cut.FindAll(".alert").Should().BeEmpty();
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Theory]
    [InlineData(WombatRoles.CollegeAdmin, "Go to Specialities", "/admin/specialities")]
    [InlineData(WombatRoles.Administrator, "Go to Colleges", "/admin/colleges")]
    public void ACollegeTheCallerCannotOpen_IsCollegeUnavailable_NeverAnEmptyListOfferingCreate(string role, string go, string href)
    {
        var cut = Render(new Sender { Tree = null }, role: role, collegeId: 1);

        Text(cut.Find("h1")).Should().Be("College unavailable");
        Text(cut.Find(".detail-card--empty div.state-panel-copy")).Should().Be("This College does not exist, or you cannot open it.");
        // T394 fix pass (pages S4): every Unavailable is flow 15's markup, its link in .form-actions with no icon.
        var link = cut.Find(".detail-card--empty .form-actions a");
        link.QuerySelector("svg").Should().BeNull();
        Text(link).Should().Be(go);
        link.GetAttribute("href").Should().Be(href);
        cut.FindAll(".header-container a").Should().BeEmpty("no Create speciality on a College it cannot open");
        cut.Markup.Should().NotContain("No specialities yet");
    }

    [Fact]
    public void WhileTheTreeIsRead_TheHeaderIsThere_OneLoadingLine_AndNoAction()
    {
        var held = new TaskCompletionSource<CollegeTreeDto?>();
        var cut = RenderPending(new Sender { HeldTree = held });

        Text(cut.Find("h1")).Should().Be("Specialities");
        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading the specialities.");
        cut.FindAll("a.btn, button").Should().BeEmpty("Create speciality waits for the read");
        held.SetResult(Tree());
    }

    [Fact]
    public void AFailedRead_IsTheFixedSentence_WithTryAgain_NoEmptyState_NoExceptionText()
    {
        var cut = Render(new Sender { TreeFailure = new InvalidOperationException("Npgsql timeout 42") });

        Text(cut.Find(".alert.alert-danger .alert-row-text")).Should().Be(
            "Could not load the specialities. Nothing has changed. Try again, or come back in a few minutes.");
        Text(cut.Find(".alert.alert-danger button")).Should().Be("Try again");
        cut.Markup.Should().NotContain("Npgsql").And.NotContain("No specialities yet");
    }

    [Fact]
    public void TheHeavyTree_KeepsATwoThousandCharacterDescription_WholeInItsLineAndItsCell()
    {
        // Decision 4: the cap, not R3-A22's 2,051.
        var description = string.Concat(Enumerable.Repeat("Specialist training in a long speciality. ", 50))[..2000];
        description.Length.Should().Be(2000);
        var subs = Enumerable.Range(1, 6)
            .Select(index => new CollegeTreeSubSpecialityDto(10 + index, 2, $"Sub-speciality {index}", "A description.", true, 1, "CPSA Paediatric Entrustment Scale v11.1"))
            .ToList();
        var tree = new CollegeTreeDto(2, Cpsa, "CPSA",
        [
            new CollegeTreeSpecialityDto(2, "Paediatrics", "Specialist training in Paediatrics.", true, subs),
            new CollegeTreeSpecialityDto(8, "Second speciality", description, true, [new CollegeTreeSubSpecialityDto(30, 8, "Its sub-speciality", description, true, null, null)])
        ]);
        var cut = Render(new Sender { Tree = tree });

        var sections = cut.FindAll("section.user-section").ToList();
        sections.Should().HaveCount(2);
        sections.First().QuerySelectorAll("tbody tr").Should().HaveCount(6);
        Text(sections.Last().QuerySelector("p.muted")!).Should().Be($"{description.Trim()} · Active");
        cut.FindAll("td.col-wrap[data-label=Description]").Last().TextContent.Should().Be(description);
    }

    private IRenderedComponent<SpecialitiesList> Render(
        Sender sender, string role = WombatRoles.CollegeAdmin, int collegeId = 2, string? query = null)
    {
        var cut = RenderPending(sender, role, collegeId, query);
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private IRenderedComponent<SpecialitiesList> RenderPending(
        Sender sender, string role = WombatRoles.CollegeAdmin, int collegeId = 2, string? query = null)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{role}@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, $"{role}-1"));
        Services.AddSingleton<IScopedSender>(sender);
        if (query is not null)
        {
            Services.GetRequiredService<NavigationManager>().NavigateTo($"/admin/colleges/{collegeId}/specialities{query}");
        }

        var acting = ActingRoleResolver.Resolve(role, [role]);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "Test"));
        return RenderComponent<SpecialitiesList>(parameters => parameters
            .Add(page => page.CollegeId, collegeId)
            .AddCascadingValue(acting)
            .AddCascadingValue(NavItems.For(acting, user))
            .AddCascadingValue(new RouteData(typeof(SpecialitiesList), new Dictionary<string, object?> { ["CollegeId"] = collegeId })));
    }

    /// <summary>CPSA's tree at 6.12: Paediatrics with Neonatology (inactive, R3-A16b) and Paediatrics.</summary>
    private static CollegeTreeDto Tree() => new(2, Cpsa, "CPSA",
    [
        new CollegeTreeSpecialityDto(2, "Paediatrics", "Specialist training in Paediatrics.", true,
        [
            new CollegeTreeSubSpecialityDto(4, 2, "Neonatology", "Certificate training in Neonatology.", false, 1, "CPSA Paediatric Entrustment Scale v11.1"),
            new CollegeTreeSubSpecialityDto(2, 2, "Paediatrics", "General paediatric specialist training programme.", true, 1, "CPSA Paediatric Entrustment Scale v11.1")
        ])
    ]);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        public CollegeTreeDto? Tree { get; init; }
        public TaskCompletionSource<CollegeTreeDto?>? HeldTree { get; init; }
        public Exception? TreeFailure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request switch
            {
                GetCollegeTreeQuery when TreeFailure is not null => Task.FromException<TResponse>(TreeFailure),
                GetCollegeTreeQuery when HeldTree is not null => HeldTree.Task.ContinueWith(task => (TResponse)(object)task.Result!, cancellationToken),
                GetCollegeTreeQuery => Task.FromResult((TResponse)(object?)Tree!),
                _ => throw new NotSupportedException(request.GetType().Name),
            };

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
