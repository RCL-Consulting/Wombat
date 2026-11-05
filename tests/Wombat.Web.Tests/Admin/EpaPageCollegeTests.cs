using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Epas;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T394 (flow 16, lane D): an EPA's page in the College's words, as R3-E05–E14 and E20 draw it. A national EPA's form is
/// subtitled by its College and discipline; the College admin's create names her College, and the picker's help says whose
/// sub-specialities it offers (N16). EV2 (Q21): one standing line saying how far a change reaches, for a caller who may
/// change a national EPA, and none for a reader or an institution's own EPA (P3); the pause dialog opens with the reach
/// (Q2, R3-E07). Save says it will ask while Active is unticked on an EPA stored active (N20); the form opens with the
/// required line (M4).
/// </summary>
public sealed class EpaPageCollegeTests : TestContext
{
    private const string Cpsa = "College of Paediatricians of South Africa";
    private const string Paediatrics = "Paediatrics / Paediatrics";
    private const string Kgk = "Kgosi Kgari Teaching Hospital";

    private static readonly EpaReachDto Reach = new(1, [Kgk], 3);

    public EpaPageCollegeTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("anton@test");
        auth.SetRoles(WombatRoles.CollegeAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "college-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ANationalEpasForm_IsSubtitledByItsCollegeAndDiscipline_AndItsPickerSaysWhoseItOffers()
    {
        var cut = Render(CollegePage(National(Reach)), 7);

        Text(cut.Find("h1")).Should().Be("PAED-006 — Managing long-term health conditions (LTHCs)");
        Text(cut.Find(".page-subtitle")).Should().Be("College of Paediatricians of South Africa · Paediatrics / Paediatrics");
        Text(cut.Find("#epa-sub-speciality-help")).Should().Be("College of Paediatricians of South Africa's sub-specialities.");
        cut.Find("#epa-sub-speciality").GetAttribute("aria-describedby").Should().Contain("epa-sub-speciality-help");
    }

    [Fact]
    public void EV2_TheStandingLine_SaysHowFarAChangeReaches_UnderTheResultRegion_NoLiveRegion()
    {
        var cut = Render(CollegePage(National(Reach)), 7);

        var line = cut.Find(".alert-info");
        line.HasAttribute("role").Should().BeFalse("a standing notice is no live region");
        Text(line).Should().Be(
            "PAED-006 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it. Its code, title and description are read at once wherever it is named.");
        line.PreviousElementSibling!.ClassList.Should().Contain("action-result", "under the result region (R3-E05)");
    }

    [Fact]
    public void EV2_OnANationalEpaNoAdoptedCurriculumHolds_SaysNoRegistrarIsMeasuredAgainstIt()
    {
        var cut = Render(CollegePage(National(EpaReachDto.None) with { Code = "PAED-016" }), 18);

        Text(cut.Find(".alert-info")).Should().Be(
            "PAED-016 is on no curriculum an institution has adopted, so no registrar is measured against it yet. Its code, title and description are read at once wherever it is named.");
    }

    [Fact]
    public void AnOwnEpa_CarriesNoReachLine_AndItsDialogNoReach()
    {
        // P3: an institution's own EPA's page stays flow 15's, though § 0 reads its reach.
        var own = Render(new EpaPageDto(Own(), [new EpaDisciplineDto(2, Paediatrics)], true, Kgk, true), 17);

        own.FindAll(".alert-info").Should().BeEmpty();
        Text(own.Find("dialog")).Should().NotContain("registrars in training");
        Text(own.Find(".page-subtitle")).Should().Be("Define the EPA code, scope, and descriptive guidance.");
    }

    [Fact]
    public void AReader_CarriesNoReachLine()
    {
        // A reader is told no reach: it names institutions (§ 0 fills none for her).
        var reader = Render(CollegePage(National(reach: null) with { MayChange = false }), 13);

        reader.FindAll(".alert-info").Should().BeEmpty();
    }

    [Fact]
    public void ThePauseDialog_OpensWithTheReach_ThenTheBuiltBody()
    {
        var cut = Render(CollegePage(National(Reach)), 7);

        var paragraphs = cut.FindAll("dialog .dialog-body p").Select(Text).ToList();
        paragraphs.Should().Equal(
            "PAED-006 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training are measured against it.",
            "It can no longer be chosen for a new activity, and it stops being a target on running programmes' progress pages and dashboards, for every institution that has adopted it. A programme that has ended keeps it on its record, marked no longer in use. The progress it has earned is kept. An activity completed against it while it is inactive counts towards nothing until it is reactivated. You can reactivate it on this page.");
        Text(cut.Find("dialog h2")).Should().Be("Deactivate PAED-006?");
        var describedBy = cut.Find("dialog").GetAttribute("aria-describedby");
        cut.Find("dialog .dialog-body").Id.Should().Be(describedBy, "both paragraphs describe the dialog");
    }

    [Fact]
    public void Save_SaysItWillAsk_WhileActiveIsUntickedOnAnEpaStoredActive()
    {
        var cut = Render(CollegePage(National(Reach)), 7);
        var save = () => cut.FindAll("form button[type=submit]").Single();

        save().HasAttribute("aria-haspopup").Should().BeFalse();
        cut.Find("#epa-active").Change(false);
        save().GetAttribute("aria-haspopup").Should().Be("dialog", "N20: it opens the deactivation's dialog");
        cut.Find("#epa-active").Change(true);
        save().HasAttribute("aria-haspopup").Should().BeFalse("ticking it back asks nothing");
    }

    [Fact]
    public void AReactivatingSave_KeepsTheReachLine()
    {
        var inactive = National(Reach) with { IsActive = false };
        var sender = new Sender { Page = CollegePage(inactive), Saved = inactive with { IsActive = true, Reach = null }, Credited = 1 };
        var cut = Render(sender, 7);

        cut.Find("#epa-active").Change(true);
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be(
            "EPA reactivated. 1 activity completed against it while it was inactive now counts towards progress."));
        Text(cut.Find(".alert-info")).Should().StartWith("PAED-006 is on 1 curriculum adopted by 1 institution");
    }

    [Fact]
    public void TheForm_OpensWithTheRequiredLine_AndItsStarredControlsSaySo()
    {
        var cut = Render(CollegePage(National(Reach)), 7);

        Text(cut.Find("form > p.field-help")).Should().Be("* marks a field you must fill in.");
        foreach (var id in new[] { "epa-sub-speciality", "epa-code", "epa-title", "epa-category" })
        {
            cut.Find($"#{id}").GetAttribute("aria-required").Should().Be("true", id);
        }

        cut.Find("#epa-description").HasAttribute("aria-required").Should().BeFalse();
    }

    [Fact]
    public void TheCollegesCreate_NamesHerCollege_AndCarriesNoReachLine()
    {
        var cut = Render(CollegePage(epa: null), id: null);

        Text(cut.Find("h1")).Should().Be("Create EPA");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "A national EPA of College of Paediatricians of South Africa. It reaches a registrar once a curriculum an institution has adopted holds it.");
        Text(cut.Find("#epa-sub-speciality-help")).Should().Be("College of Paediatricians of South Africa's sub-specialities.");
        cut.FindAll(".alert-info").Should().BeEmpty("no EPA yet (R3-E10)");
    }

    [Fact]
    public void TheAdministratorsForm_SaysHerPickerIsEveryCollege_AndHerCreateKeepsTheBuiltSubtitle()
    {
        var page = new EpaPageDto(National(Reach), [new EpaDisciplineDto(9, "Neurology / Adult Neurology"), new EpaDisciplineDto(2, Paediatrics)], false, null, true)
        {
            ReadsEveryCollege = true
        };
        var cut = Render(page, 7);

        Text(cut.Find(".page-subtitle")).Should().Be("College of Paediatricians of South Africa · Paediatrics / Paediatrics");
        Text(cut.Find("#epa-sub-speciality-help")).Should().Be("Every College's sub-specialities.");
    }

    [Fact]
    public void TheAdministratorsCreate_KeepsTheBuiltSubtitle_HerPickerEveryCollege()
    {
        var page = new EpaPageDto(null, [new EpaDisciplineDto(2, Paediatrics)], false, null, true) { ReadsEveryCollege = true };
        var create = Render(page, id: null);

        Text(create.Find(".page-subtitle")).Should().Be("Define the EPA code, scope, and descriptive guidance.");
        Text(create.Find("#epa-sub-speciality-help")).Should().Be("Every College's sub-specialities.");
    }

    private IRenderedComponent<EpaEdit> Render(EpaPageDto page, int? id) => Render(new Sender { Page = page }, id);

    private IRenderedComponent<EpaEdit> Render(Sender sender, int? id)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<EpaEdit>(parameters => parameters.Add(component => component.Id, id));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private static EpaPageDto CollegePage(EpaDto? epa)
        => new(epa, [new EpaDisciplineDto(3, "Paediatrics / Neonatology"), new EpaDisciplineDto(2, Paediatrics)], false, null, true)
        {
            CollegeName = Cpsa
        };

    private static EpaDto National(EpaReachDto? reach)
        => new EpaDto(7, 2, "Paediatrics", Cpsa, "PAED-006", "Managing long-term health conditions (LTHCs)", null, null,
            EpaCategory.Core, true, DateTime.UnixEpoch)
        {
            MayChange = true,
            DisciplineName = Paediatrics,
            Reach = reach
        };

    private static EpaDto Own()
        => new EpaDto(17, 2, "Paediatrics", Cpsa, "KGK-001", "Running a paediatric outreach clinic at a district hospital", null, null,
            EpaCategory.Elective, true, DateTime.UnixEpoch)
        {
            OwningInstitutionId = 3,
            OwningInstitutionName = Kgk,
            MayChange = true,
            DisciplineName = Paediatrics,
            Reach = new EpaReachDto(1, [Kgk], 2)
        };

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        public EpaPageDto? Page { get; init; }
        public EpaDto? Saved { get; init; }
        public int Credited { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request switch
            {
                GetEpaPageQuery => Task.FromResult((TResponse)(object)Page!),
                UpdateEpaCommand => Task.FromResult((TResponse)(object)new UpdateEpaResult(Saved ?? Page!.Epa!, Credited)),
                _ => throw new NotSupportedException(request.GetType().Name)
            };

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
