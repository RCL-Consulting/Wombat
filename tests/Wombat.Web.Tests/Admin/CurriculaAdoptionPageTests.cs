using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Adoptions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetMyInstitutionName;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Adoptions;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T392 (flow 15, lane A; boards R3-A01–A15, R2-A12): Curricula is where an institution adopts (Q1). Its versions are rows
/// with their state for the institution in words (Q10), the open versions it has not actively adopted are offered (Q7),
/// a first adoption of a discipline asks nothing and any other confirms first (Q6), the result takes the focus at the
/// page's top, a refusal is the typed one in the open section, and nothing prints an exception's text (T329).
/// </summary>
public sealed class CurriculaAdoptionPageTests : TestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";
    private const string Secret = "Npgsql: connection refused at 10.0.0.4";

    private readonly FakeSender _sender = new();
    private readonly TestAuthorizationContext _auth;

    public CurriculaAdoptionPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
        _auth = this.AddTestAuthorization();
    }

    // ---- The Institutional admin ----

    [Fact]
    public void NothingAdopted_TheEmptySection_AndEachOpenVersionAdoptedWithoutAQuestion()
    {
        SignInInstitutionalAdmin();
        _sender.Adoptable = [Open(3, "11.1", AdoptionRelation.None)];

        var cut = Render();

        cut.Find("h1").TextContent.Should().Be("Curricula");
        cut.Find(".page-subtitle").TextContent.Should().Be(
            $"The College curricula {Kgk} trains against, and the versions open to it.");
        Headings(cut, "h2").Should().Equal("Curricula you train against", "Open for adoption");
        cut.Find(".detail-card--empty .state-panel-title").TextContent.Should().Be("No curricula yet");
        cut.Find(".detail-card--empty .state-panel-copy").TextContent.Should().Be(
            "Your institution has not adopted a curriculum yet. Adopt one below before admitting trainees.");
        cut.Find("caption").TextContent.Should().Be("College curriculum versions open for adoption");
        cut.Find("tbody th[scope=row] .muted.text-sm").TextContent.Should().Be(
            "Your institution has adopted no Paediatrics / Paediatrics version yet.");

        var adopt = AdoptButton(cut);
        adopt.TextContent.Trim().Should().Be("Adopt");
        adopt.GetAttribute("aria-label").Should().Be("Adopt Paediatric EPA Curriculum (11.1)");
        adopt.HasAttribute("aria-haspopup").Should().BeFalse("a first adoption supersedes nothing, so asks nothing (Q6)");
        cut.FindAll(".header-container .actions-cell").Should().BeEmpty("she creates no curriculum");
    }

    [Fact]
    public void AFirstAdoption_IsSentAtOnce_ItsResultTakesTheFocusAtThePagesTop_AndTheListIsReadAgain()
    {
        SignInInstitutionalAdmin();
        _sender.Adoptable = [Open(3, "11.1", AdoptionRelation.None)];
        var cut = Render();
        _sender.Curricula = [Adopted(3, "11.1")];
        _sender.Adoptable = [];
        _sender.Adoption = Adoption("11.1");

        AdoptButton(cut).Click();

        cut.WaitForAssertion(() => cut.Find(".alert-success").TextContent.Should().Be(
            "Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it."));
        cut.Find(".alert-success strong").TextContent.Should().Be("Paediatric EPA Curriculum (11.1) adopted.");
        _sender.Adopted.Should().Equal((4, 3));
        cut.Find(".curricula-page > .action-result").TextContent.Trim().Should().StartWith("Paediatric EPA Curriculum (11.1) adopted.");
        var region = cut.FindComponents<ActionResult>()[0];
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>().Which.Id.Should().Be(region.Instance.Element.Id));

        cut.Find("tbody td[data-label='For your institution']").TextContent.Trim().Should().Be("Adopted on 2026-10-30");
        cut.Find("tbody td[data-label='Items']").TextContent.Should().Be("15 national · 0 your own");
        Headings(cut, "h2").Should().Contain("Open for adoption");
        cut.Find(".curricula-section p.muted + p.muted").TextContent.Should().Be("No other College curriculum version is open for adoption.");
    }

    [Fact]
    public void AnAdoptedVersion_ReadsItsStateInWords_WithItsRegistrarsAndItemCounts()
    {
        SignInInstitutionalAdmin();
        _sender.Curricula =
        [
            Adopted(4, "11.2", national: 16),
            Superseded(3, "11.1", registrars: 3, own: 1),
            Superseded(5, "10.9", registrars: 1),
            Superseded(6, "10.8", registrars: 0)
        ];

        var cut = Render();

        var rows = cut.FindAll("tbody tr").ToList();
        rows[0].QuerySelector("th")!.TextContent.Should().Be("Paediatric EPA Curriculum (11.2)");
        rows[0].QuerySelector("td[data-label='Items']")!.TextContent.Should().Be("16 national · 0 your own");
        rows[1].QuerySelector("td[data-label='Items']")!.TextContent.Should().Be("15 national · 1 your own");
        rows[1].QuerySelector("td[data-label='For your institution']")!.TextContent.Trim()
            .Should().StartWith("Superseded on 2026-10-30");
        rows[1].QuerySelector("td[data-label='For your institution'] .muted.text-sm")!.TextContent.Should().Be("3 registrars still on it");
        rows[2].QuerySelector("td[data-label='For your institution'] .muted.text-sm")!.TextContent.Should().Be("1 registrar still on it");
        rows[3].QuerySelector("td[data-label='For your institution'] .muted")!.Should().BeNull("no registrar is still on it");
        rows[1].QuerySelector("a")!.GetAttribute("href").Should().Be("/admin/curricula/3/items");
        rows[1].QuerySelector("a")!.GetAttribute("aria-label").Should().Be("Items of Paediatric EPA Curriculum (11.1)");
        cut.Find("table").ClassList.Should().Contain("clinic-table--stack-wide");
        cut.FindAll("thead th").Select(header => header.TextContent.Trim()).Should().NotContain("Status",
            "the College's flag is not hers to read here (Q10)");
    }

    [Fact]
    public void ASupersedingAdoption_AsksFirst_NamingTheInstitution_TheRegistrarsAndTheOwnItemsNotCarried()
    {
        SignInInstitutionalAdmin();
        _sender.Curricula = [Adopted(3, "11.1", own: 1)];
        _sender.Adoptable = [Open(4, "11.2", AdoptionRelation.Newer, registrars: 3, ownItems: ["KGK-001"])];

        var cut = Render();

        cut.Find("tbody th[scope=row] .muted.text-sm").TextContent.Should().Be("Newer than the version your institution has adopted (11.1).");
        var adopt = AdoptButton(cut);
        adopt.TextContent.Trim().Should().Be("Adopt…");
        adopt.GetAttribute("aria-haspopup").Should().Be("dialog");
        adopt.GetAttribute("aria-label").Should().Be("Adopt Paediatric EPA Curriculum (11.2)…");

        adopt.Click();

        _sender.Adopted.Should().BeEmpty("nothing is sent before the dialog's confirm");
        JSInterop.VerifyInvoke("wombatDialog.showModal");
        var dialog = cut.Find("dialog");
        dialog.QuerySelector("h2")!.TextContent.Should().Be($"Adopt Paediatric EPA Curriculum (11.2) for {Kgk}?");
        dialog.QuerySelectorAll(".dialog-body p").Select(paragraph => paragraph.TextContent).Should().Equal(
            $"It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at {Kgk}.",
            "The 3 registrars on 11.1 stay on it until each is moved on their profile.",
            "Your institution's own item on 11.1, KGK-001, is not carried over: add it again on 11.2.");
        var buttons = dialog.QuerySelectorAll("button").Select(button => button.TextContent.Trim()).ToList();
        buttons.Should().Equal("Cancel", "Adopt 11.2");
        dialog.QuerySelector("button")!.HasAttribute("autofocus").Should().BeTrue("the dialog opens on Cancel");
        dialog.QuerySelectorAll("button")[1].ClassList.Should().Contain("btn-primary").And.NotContain("btn-danger");
    }

    // S1 (R3-A08): going back counts one registrar in the singular, and has no own-items line when none is left behind.
    [Fact]
    public void GoingBack_WithOneRegistrar_SaysItInTheSingular_AndLeavesTheOwnItemsLineOut()
    {
        SignInInstitutionalAdmin();
        _sender.Curricula = [Adopted(4, "11.2")];
        _sender.Adoptable = [Open(3, "11.1", AdoptionRelation.Older, adoptedVersion: "11.2", registrars: 1)];

        var cut = Render();
        cut.Find("tbody th[scope=row] .muted.text-sm").TextContent.Should().Be("Older than the version your institution has adopted (11.2).");
        AdoptButton(cut).Click();

        cut.FindAll("dialog .dialog-body p").Select(paragraph => paragraph.TextContent).Should().Equal(
            $"It supersedes Paediatric EPA Curriculum (11.2) for every new admission to Paediatrics / Paediatrics at {Kgk}.",
            "The registrar on 11.2 stays on it until moved on their profile.");
        cut.Find("dialog h2").TextContent.Should().Be($"Adopt Paediatric EPA Curriculum (11.1) for {Kgk}?");
    }

    [Fact]
    public void NoRegistrarOnTheSupersededVersion_LeavesThatLineOut()
    {
        SignInInstitutionalAdmin();
        _sender.Adoptable = [Open(4, "11.2", AdoptionRelation.Newer, registrars: 0)];

        var cut = Render();
        AdoptButton(cut).Click();

        cut.FindAll("dialog .dialog-body p").Should().ContainSingle();
    }

    [Fact]
    public void TheDialogsConfirm_Adopts_AndTheResultSaysWhatWasSuperseded_InTheSingularToo()
    {
        SignInInstitutionalAdmin();
        _sender.Adoptable = [Open(4, "11.2", AdoptionRelation.Newer, registrars: 3)];
        var cut = Render();
        AdoptButton(cut).Click();
        _sender.Adoption = Adoption("11.2") with { SupersededName = "Paediatric EPA Curriculum", SupersededVersion = "11.1", SupersededRegistrarCount = 3 };

        cut.FindAll("dialog button").ToList()[1].Click();

        cut.WaitForAssertion(() => cut.Find(".alert-success").TextContent.Should().Be(
            "Paediatric EPA Curriculum (11.2) adopted. New admissions to Paediatrics / Paediatrics join it. " +
            "Paediatric EPA Curriculum (11.1) is superseded; its 3 registrars stay on it until each is moved."));
        _sender.Adopted.Should().Equal((4, 4));

        var one = CurriculaWords.Adopted(Adoption("11.2") with { SupersededName = "Paediatric EPA Curriculum", SupersededVersion = "11.1", SupersededRegistrarCount = 1 }, null);
        $"{one.Lead} {one.Tail}".Should().EndWith("Paediatric EPA Curriculum (11.1) is superseded; its registrar stays on it until moved.");
        var none = CurriculaWords.Adopted(Adoption("11.2") with { SupersededName = "Paediatric EPA Curriculum", SupersededVersion = "11.1" }, null);
        $"{none.Lead} {none.Tail}".Should().EndWith("Paediatric EPA Curriculum (11.1) is superseded.");
    }

    // T301 (R3-A07): the held-back refusal is the typed one, in the open section, focused, over the list read again.
    [Fact]
    public void AHeldBackVersion_IsRefusedInTheOpenSection_Focused_AndTheListIsReadAgain()
    {
        SignInInstitutionalAdmin();
        _sender.Curricula = [Adopted(3, "11.1")];
        _sender.Adoptable = [Open(7, "1.0", AdoptionRelation.None), Open(8, "2026.1", AdoptionRelation.None)];
        var cut = Render();
        _sender.Refusal = new AdoptionRefusedException(AdoptionRefusal.HeldBack, Secret);
        _sender.Adoptable = [Open(8, "2026.1", AdoptionRelation.None)];

        AdoptButton(cut).Click();

        cut.WaitForAssertion(() => cut.Find(".alert-danger").TextContent.Should().Be(
            "This curriculum version is not open for adoption. Its College has held it back, so nothing was adopted."));
        cut.Find(".alert-danger").GetAttribute("role").Should().Be("alert");
        cut.Markup.Should().NotContain(Secret, "the page maps the reason, never the message (T329)");
        var section = cut.Find(".alert-danger").Closest(".curricula-section")!;
        section.QuerySelector("h2")!.TextContent.Should().Be("Open for adoption");
        cut.FindAll(".alert-success").Should().BeEmpty();
        cut.FindAll("tbody button").Should().ContainSingle("the list was read again: the held-back version is gone");
        var sectionResult = cut.FindComponents<ActionResult>()[1];
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>().Which.Id.Should().Be(sectionResult.Instance.Element.Id));
    }

    [Fact]
    public void AnyOtherFailure_IsTheFixedSentence_NeverTheExceptionsText()
    {
        SignInInstitutionalAdmin();
        _sender.Adoptable = [Open(3, "11.1", AdoptionRelation.None)];
        var cut = Render();
        _sender.Refusal = new InvalidOperationException(Secret);

        AdoptButton(cut).Click();

        cut.WaitForAssertion(() => cut.Find(".alert-danger").TextContent.Should().Be(
            "The curriculum could not be adopted. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain(Secret);
    }

    [Fact]
    public void WhileTheListLoads_OneLoadingLine_AndNothingOffered()
    {
        SignInInstitutionalAdmin();
        _sender.HoldLists = new TaskCompletionSource();

        var cut = RenderComponent<CurriculaList>();

        cut.FindAll(".loading-line").Should().ContainSingle().Which.TextContent.Should().Be("Loading the curricula.");
        cut.Find(".loading-line").GetAttribute("role").Should().Be("status");
        cut.FindAll("button").Where(button => button.Closest("dialog") is null).Should().BeEmpty("no Adopt until the read is in");
        Headings(cut, "h2").Should().Equal("Curricula you train against", "Open for adoption");
        _sender.HoldLists.SetResult();
    }

    [Fact]
    public void AFailedRead_IsFixedWordsWithTryAgain_NoEmptyStateUnderIt_AndTheH1TakesTheFocusOnceItHasRead()
    {
        SignInInstitutionalAdmin();
        _sender.ListFailure = new InvalidOperationException(Secret);

        var cut = Render(wait: false);
        cut.WaitForAssertion(() => cut.Find(".alert-danger"));

        cut.Find(".alert-danger .alert-row-text").TextContent.Should().Be(
            "Could not load the curricula. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain(Secret);
        cut.FindAll(".detail-card--empty").Should().BeEmpty();
        Headings(cut, "h2").Should().BeEmpty("no Open for adoption under a failed read (A15)");

        _sender.ListFailure = null;
        cut.Find(".alert-row button").Click();

        cut.WaitForAssertion(() => Headings(cut, "h2").Should().HaveCount(2));
        cut.WaitForAssertion(() => JSInterop.VerifyInvoke(PageFocus.FocusHeadingIdentifier));
    }

    // T239: one action a row named for its row, no two alike: two versions of one name and version in two disciplines are
    // told apart by their sub-speciality.
    [Fact]
    public void TwoOpenVersionsAlike_AreToldApartByTheirSubSpeciality()
    {
        SignInInstitutionalAdmin();
        _sender.Adoptable =
        [
            Open(3, "1.0", AdoptionRelation.None),
            Open(4, "1.0", AdoptionRelation.None) with { SubSpecialityName = "Neonatology" }
        ];

        var cut = Render();

        cut.FindAll("tbody button").Select(button => button.GetAttribute("aria-label")).Should().Equal(
            "Adopt Paediatric EPA Curriculum (1.0), Paediatrics",
            "Adopt Paediatric EPA Curriculum (1.0), Neonatology");
    }

    // ---- The College admin (R3-A12) ----

    [Fact]
    public void TheCollegeAdmin_ReadsHerCollegesVersions_WithTheirStatus_AndNoAdoption()
    {
        SignIn(WombatRoles.CollegeAdmin, ["AdministratorOrCollegeAdmin"], new Claim(WombatClaimTypes.CollegeId, "5"));
        _sender.Curricula = [College(4, "11.2", isOpen: true), College(9, "1.0", isOpen: false)];

        var cut = Render();

        cut.Find(".page-subtitle").TextContent.Should().Be("Manage curriculum versions and the sub-speciality they apply to.");
        Headings(cut, "h2").Should().BeEmpty();
        cut.Find("caption").TextContent.Should().Be("College of Paediatricians of South Africa's curriculum versions");
        cut.FindAll("thead th").Select(header => header.TextContent.Trim()).Should().Equal(
            "Curriculum", "Discipline", "Effective", "Items", "Status", "Actions");
        cut.FindAll("td[data-label='Status']").Select(cell => cell.TextContent).Should().Equal("Open", "Held back");
        cut.Find(".header-container a.btn-primary").TextContent.Trim().Should().Be("Create curriculum");
        cut.Find(".header-container a.btn-primary svg").Should().NotBeNull("the plus stays (round-3-check decision 4)");
        cut.FindAll("button").Should().BeEmpty();
        _sender.Requests.OfType<GetAdoptableCurriculaQuery>().Should().BeEmpty();
    }

    // ---- The Administrator (R3-A09–A11) ----

    [Fact]
    public void TheAdministrator_ReadsAllCurricula_AndChoosesAnInstitutionBeforeAnythingIsOffered()
    {
        SignInAdministrator();
        _sender.Curricula = [College(4, "11.2", isOpen: true, items: 17), College(9, "1.0", isOpen: false)];

        var cut = Render();

        cut.Find(".page-subtitle").TextContent.Should().Be("Every College's curricula; and, for one institution, what it trains against and may adopt.");
        Headings(cut, "h2").Should().Equal("All curricula", "An institution's curricula");
        cut.Find("caption").TextContent.Should().Be(
            "Every College's curriculum versions, with the College's status. Items counts each institution's own items too.");
        cut.FindAll("td[data-label='Status']").Select(cell => cell.TextContent).Should().Equal("Open", "Held back");
        cut.FindAll("td[data-label='Items']").ToList()[0].TextContent.Should().Be("17");
        cut.Find("label[for=cur-institution]").TextContent.Should().Be("Institution");
        cut.Find("#cur-institution option").TextContent.Should().Be("— Select an institution —");
        cut.Find(".search-container button[type=submit]").TextContent.Trim().Should().Be("Show");
        cut.Find(".curricula-section p.muted").TextContent.Should().Be(
            "Choose an institution, then Show, to see what it trains against and adopt for it.");
        cut.FindAll("tbody button").Should().BeEmpty();
        cut.FindAll(".header-container .actions-cell a").Select(link => link.TextContent.Trim())
            .Should().Equal("Curriculum progress", "Create curriculum");
    }

    [Fact]
    public void TheAdministrator_Shows_AnInstitutionsTwoSections_NamedForIt_AndAdoptsForIt()
    {
        SignInAdministrator();
        _sender.Curricula = [College(4, "11.2", isOpen: true)];
        var cut = Render();
        _sender.Curricula = [Adopted(3, "11.1", own: 1, institution: Kgk)];
        _sender.Adoptable = [Open(4, "11.2", AdoptionRelation.Newer, registrars: 3, ownItems: ["KGK-001"])];

        cut.Find("#cur-institution").Change("4");
        cut.Find(".search-container").Submit();

        cut.WaitForAssertion(() => Headings(cut, "h3").Should().Equal(
            $"Curricula {Kgk} trains against", $"Open to {Kgk}"));
        _sender.Requests.OfType<GetCurriculaListQuery>().Last().InstitutionId.Should().Be(4);
        _sender.Requests.OfType<GetAdoptableCurriculaQuery>().Last().InstitutionId.Should().Be(4);
        JSInterop.VerifyInvoke(PageFocus.FocusByIdIdentifier).Arguments[0].Should().Be("cur-inst-h");

        var chosen = cut.Find("#cur-inst-h").Closest(".curricula-section")!;
        chosen.QuerySelector("caption")!.TextContent.Should().Be($"The curricula {Kgk} trains against, with the state of each");
        chosen.QuerySelector("td[data-label='For this institution']").Should().NotBeNull();
        chosen.QuerySelector("td[data-label='Items']")!.TextContent.Should().Be($"15 national · 1 {Kgk}'s own");
        chosen.QuerySelector("a")!.GetAttribute("aria-label").Should().Be($"Items of Paediatric EPA Curriculum (11.1) for {Kgk}");
        cut.FindAll("tbody th[scope=row] .muted.text-sm").Single().TextContent.Should().Be(
            $"Newer than the version {Kgk} has adopted (11.1).");
        var adopt = AdoptButton(cut);
        adopt.GetAttribute("aria-label").Should().Be($"Adopt Paediatric EPA Curriculum (11.2) for {Kgk}…");

        adopt.Click();
        cut.Find("dialog h2").TextContent.Should().Be($"Adopt Paediatric EPA Curriculum (11.2) for {Kgk}?");
        cut.FindAll("dialog .dialog-body p").Last().TextContent.Should().Be(
            $"{Kgk}'s own item on 11.1, KGK-001, is not carried over: add it again on 11.2.");
        _sender.Adoption = Adoption("11.2");
        cut.FindAll("dialog button").ToList()[1].Click();

        cut.WaitForAssertion(() => _sender.Adopted.Should().Equal((4, 4)));
        cut.WaitForAssertion(() => cut.Find(".alert-success").TextContent.Should().Be(
            $"Paediatric EPA Curriculum (11.2) adopted. New admissions to Paediatrics / Paediatrics at {Kgk} join it."));
        _sender.Requests.OfType<GetCurriculaListQuery>().Last().InstitutionId.Should().Be(4, "an adoption re-reads with the institution kept (P4)");
    }

    // ---- The build review's fixes (T392: C4, P1, P8, P9) ----

    [Fact]
    public void TheAdoptCarriesTheVersionTheDialogNamedAsSuperseded_AndAStaleOneIsRefused_TheListReadAgain()
    {
        // C4: the dialog is the consent (Q6); the command refuses if what it named is no longer the active adoption.
        SignInInstitutionalAdmin();
        _sender.Adoptable = [Open(4, "11.2", AdoptionRelation.Newer, registrars: 3)];
        var cut = Render();
        _sender.Refusal = new AdoptionRefusedException(AdoptionRefusal.Stale, "Another adoption was made since this page was read, so nothing was adopted.");
        var reads = _sender.Requests.OfType<GetAdoptableCurriculaQuery>().Count();

        AdoptButton(cut).Click();
        cut.FindAll("dialog button").ToList()[1].Click();

        cut.WaitForAssertion(() => cut.Find(".alert-danger").TextContent.Should().Be(
            "Another adoption was made since this page was read, so nothing was adopted. The list below has been read again."));
        _sender.Requests.OfType<AdoptCurriculumCommand>().Single().ExpectedSupersededCurriculumId.Should().Be(99);
        _sender.Requests.OfType<GetAdoptableCurriculaQuery>().Count().Should().Be(reads + 1);
    }

    [Fact]
    public void AFirstAdoption_ExpectsToSupersedeNothing()
    {
        SignInInstitutionalAdmin();
        _sender.Adoptable = [Open(3, "11.1", AdoptionRelation.None)];
        var cut = Render();
        _sender.Adoption = Adoption("11.1");

        AdoptButton(cut).Click();

        cut.WaitForAssertion(() => _sender.Commands.Should().ContainSingle().Which.ExpectedSupersededCurriculumId.Should().BeNull());
    }

    [Fact]
    public void AnAdoptionMade_WhoseListCannotBeReadAgain_StaysAdopted_AndSaysTheListWasNotRead()
    {
        // P1: never "adopted" and "could not be adopted" together.
        SignInInstitutionalAdmin();
        _sender.Adoptable = [Open(3, "11.1", AdoptionRelation.None)];
        var cut = Render();
        _sender.Adoption = Adoption("11.1");
        _sender.ListFailureAfterAdopt = new InvalidOperationException(Secret);

        AdoptButton(cut).Click();

        cut.WaitForAssertion(() => cut.Find(".alert-success").TextContent.Should().Be(
            "Paediatric EPA Curriculum (11.1) adopted. New admissions to Paediatrics / Paediatrics join it. The list could not be read again: reload the page to see it."));
        cut.FindAll(".alert-danger").Should().BeEmpty();
        cut.Markup.Should().NotContain("10.0.0.4");
    }

    [Fact]
    public void Show_WithNoInstitutionChosen_IsRefusedUnderTheSelect_Focused_AndWhatIsShownStays()
    {
        // P8.
        SignInAdministrator();
        _sender.Curricula = [College(4, "11.2", isOpen: true)];
        var cut = Render();
        var reads = _sender.Requests.Count;

        cut.Find(".search-container").Submit();

        var refusal = cut.Find("#cur-institution-refusal");
        refusal.TextContent.Should().Be("Choose an institution.");
        refusal.GetAttribute("role").Should().Be("alert");
        cut.Find("#cur-institution").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#cur-institution").GetAttribute("aria-describedby").Should().Be("cur-institution-refusal");
        JSInterop.VerifyInvoke(PageFocus.FocusByIdIdentifier).Arguments[0].Should().Be("cur-institution");
        _sender.Requests.Count.Should().Be(reads, "nothing is read");
        cut.Find(".curricula-section p.muted").TextContent.Should().Be(
            "Choose an institution, then Show, to see what it trains against and adopt for it.");
    }

    [Fact]
    public void AFailedShow_OffersTryAgain_WhichReadsTheInstitutionAgain()
    {
        // P9: the S7 form.
        SignInAdministrator();
        _sender.Curricula = [College(4, "11.2", isOpen: true)];
        var cut = Render();
        _sender.ListFailure = new InvalidOperationException(Secret);

        cut.Find("#cur-institution").Change("4");
        cut.Find(".search-container").Submit();

        var alert = cut.Find("#cur-inst-error .alert-danger");
        alert.QuerySelector(".alert-row-text")!.TextContent.Should().Be(
            "Could not load the curricula. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("10.0.0.4");
        _sender.ListFailure = null;
        _sender.Curricula = [Adopted(3, "11.1")];

        alert.QuerySelector("button")!.Click();

        cut.WaitForAssertion(() => Headings(cut, "h3").Should().Equal($"Curricula {Kgk} trains against", $"Open to {Kgk}"));
    }

    // ---- The old address ----

    [Fact]
    public void CurriculumAdoptionsOldAddress_SendsToCurricula_ReplacingItself()
    {
        SignInInstitutionalAdmin();
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        navigation.NavigateTo("/admin/adoptions");

        var cut = RenderComponent<AdoptionsList>();

        navigation.Uri.Should().EndWith("/admin/curricula");
        navigation.History.First().Options.ReplaceHistoryEntry.Should().BeTrue();
        cut.Markup.Trim().Should().BeEmpty("it draws nothing");
        typeof(AdoptionsList).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Policy.Should().Be("AdministratorOrInstitutionalAdmin");
    }

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    public void TheOldAddress_LightsCurricula(string role)
    {
        var menu = NavItems.For(
            ActingRoleResolver.Resolve(role, [role]),
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test")));

        NavOwners.Lit(typeof(AdoptionsList), role, menu).Item.Should().Be(NavItems.Curricula);
    }

    // ---- helpers ----

    private void SignInInstitutionalAdmin()
        => SignIn(WombatRoles.InstitutionalAdmin, [], new Claim(WombatClaimTypes.InstitutionId, "4"));

    private void SignInAdministrator()
        => SignIn(WombatRoles.Administrator, ["AdministratorOrCollegeAdmin", "Administrator"]);

    private void SignIn(string role, string[] policies, params Claim[] claims)
    {
        _auth.SetAuthorized($"{role}@test");
        _auth.SetRoles(role);
        _auth.SetPolicies(policies);
        _auth.SetClaims([new Claim(ClaimTypes.NameIdentifier, $"{role}-1"), .. claims]);
    }

    private IRenderedComponent<CurriculaList> Render(bool wait = true)
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var cut = RenderComponent<CurriculaList>();
        if (wait)
        {
            cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        }

        return cut;
    }

    private static AngleSharp.Dom.IElement AdoptButton(IRenderedComponent<CurriculaList> cut)
        => cut.FindAll("tbody button").First();

    /// <summary>The page's headings of a level, leaving out the closed dialog's title.</summary>
    private static IReadOnlyList<string> Headings(IRenderedComponent<CurriculaList> cut, string level)
        => cut.FindAll(level).Where(heading => heading.Closest("dialog") is null).Select(heading => heading.TextContent).ToList();

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    private static AdoptableCurriculumDto Open(
        int id, string version, AdoptionRelation relation, string adoptedVersion = "11.1", int registrars = 0, string[]? ownItems = null)
        => new(id, 9, "College of Paediatricians of South Africa", "Paediatrics", "Paediatrics", "Paediatric EPA Curriculum", version)
        {
            EffectiveFrom = new DateOnly(2026, 10, 30),
            NationalItemCount = 16,
            Relation = relation,
            AdoptedCurriculumId = relation == AdoptionRelation.None ? null : 99,
            AdoptedName = relation == AdoptionRelation.None ? null : "Paediatric EPA Curriculum",
            AdoptedVersion = relation == AdoptionRelation.None ? null : adoptedVersion,
            AdoptedRegistrarCount = registrars,
            OwnItemsNotCarried = ownItems ?? []
        };

    private static CurriculumDto Curriculum(int id, string version, bool isOpen = true, int items = 0)
        => new(id, 2, 9, "Paediatrics", "Paediatrics", "College of Paediatricians of South Africa", "Paediatric EPA Curriculum", version,
            new DateOnly(2026, 1, 1), null, isOpen,
            Enumerable.Range(1, items).Select(item => new CurriculumItemDto(item, item, $"PAED-{item:000}", "EPA", 1,
                Wombat.Domain.Curricula.QuotaPeriod.AcademicYear, 3, 12, null, null, null, true, null, null, null, null, null, false)).ToList(),
            null);

    private static CurriculumDto College(int id, string version, bool isOpen, int items = 0)
        => Curriculum(id, version, isOpen, items) with { CanEditCurriculum = true };

    private static CurriculumDto Adopted(int id, string version, int national = 15, int own = 0, string institution = Kgk)
        => Curriculum(id, version) with
        {
            Institution = new CurriculumInstitutionStateDto(4, institution, true, new DateOnly(2026, 10, 30), null, null, null, 0, national, own, [])
        };

    private static CurriculumDto Superseded(int id, string version, int registrars, int own = 0)
        => Curriculum(id, version) with
        {
            Institution = new CurriculumInstitutionStateDto(4, Kgk, false, new DateOnly(2026, 1, 10), new DateOnly(2026, 10, 30),
                "Paediatric EPA Curriculum", "11.2", registrars, 15, own, [])
        };

    private static AdoptionDto Adoption(string version)
        => new(7, 4, 3, "Paediatric EPA Curriculum", version, 9, "Paediatrics", "Paediatrics", "College of Paediatricians of South Africa",
            new DateOnly(2026, 10, 30), true);

    private sealed class FakeSender : IScopedSender
    {
        public IReadOnlyList<CurriculumDto> Curricula { get; set; } = [];
        public IReadOnlyList<AdoptableCurriculumDto> Adoptable { get; set; } = [];
        public AdoptionDto? Adoption { get; set; }
        public Exception? Refusal { get; set; }
        public Exception? ListFailure { get; set; }
        public TaskCompletionSource? HoldLists { get; set; }
        public List<(int InstitutionId, int CurriculumId)> Adopted { get; } = [];
        public List<AdoptCurriculumCommand> Commands { get; } = [];
        public Exception? ListFailureAfterAdopt { get; set; }
        public List<object> Requests { get; } = [];

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            object? answer;
            switch (request)
            {
                case GetMyInstitutionNameQuery:
                    answer = Kgk;
                    break;
                case GetInstitutionsListQuery:
                    answer = new[] { new InstitutionDto(4, Kgk, "KGK", null, true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)) };
                    break;
                case GetCurriculaListQuery:
                    if (HoldLists is { } hold)
                    {
                        await hold.Task;
                    }

                    if (ListFailure is { } failure)
                    {
                        throw failure;
                    }

                    answer = Curricula;
                    break;
                case GetAdoptableCurriculaQuery:
                    answer = Adoptable;
                    break;
                case AdoptCurriculumCommand adopt:
                    if (Refusal is { } refusal)
                    {
                        throw refusal;
                    }

                    Adopted.Add((adopt.InstitutionId, adopt.CurriculumId));
                    Commands.Add(adopt);
                    if (ListFailureAfterAdopt is { } after)
                    {
                        ListFailure = after;
                    }

                    answer = Adoption;
                    break;
                default:
                    throw new NotSupportedException(request.GetType().Name);
            }

            return (TResponse)answer!;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
