using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// T147: every label on the MSF pages names a control that exists. The campaign editor's EPA checkboxes sat
/// in a <c>FormField</c>, whose <c>&lt;label for="msf-epas"&gt;</c> named no element: each checkbox has its own
/// id. The group is now a fieldset named by its legend (DESIGN.md § Form system). The report page is checked
/// too, so the next field added to either page is held to the same rule.
/// </summary>
public sealed class MsfPageLabelTests : TestContext
{
    private const string TraineeUserId = "trainee-1";

    private static readonly IReadOnlyList<ActivityCatalogueOption> TraineeEpas =
    [
        new("101", "PAED-001 Resuscitate a critically ill child"),
        new("102", "PAED-002 Manage a child with a chronic condition"),
        new("103", "PAED-003 Lead a ward round")
    ];

    public MsfPageLabelTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));

        Services.AddSingleton<IScopedSender>(new FakeSender());
        Services.AddSingleton<IActivityReferenceDataService>(new ReferenceData());
    }

    // ---- The campaign editor ----

    [Fact]
    public void CreatingACampaign_BeforeATraineeIsChosen_EveryLabelNamesARealControl()
    {
        var cut = RenderComponent<CampaignEdit>();

        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void CreatingACampaign_WithTheTraineesEpasListed_EveryLabelNamesARealControl()
    {
        var cut = RenderWithTraineeChosen();

        // The state the defect lived in: a checkbox per EPA, under a heading that named nothing.
        cut.FindAll("input[type=checkbox]").Should().HaveCount(TraineeEpas.Count);
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheEpaCheckboxes_AreAFieldsetNamedByItsLegend_NotAFormFieldLabel()
    {
        var cut = RenderWithTraineeChosen();

        var fieldset = cut.FindAll("fieldset").Should().ContainSingle().Subject;

        // The legend names the group, and says it is required the way FormField says it of a single field.
        var legend = fieldset.QuerySelector("legend")!;
        legend.TextContent.Should().Contain("Evidence for these EPAs");
        legend.QuerySelector(".visually-hidden")!.TextContent.Trim().Should().Be("required");

        // Every checkbox is in the group's grid, each with a label naming it and only it.
        var checks = fieldset.QuerySelectorAll(".check-grid > .form-check").ToList();
        checks.Select(check => check.QuerySelector("input[type=checkbox]")!.Id)
            .Should().Equal(TraineeEpas.Select(epa => $"msf-epa-{epa.Value}"));
        checks.Select(check => check.QuerySelector("label")!.TextContent.Trim())
            .Should().Equal(TraineeEpas.Select(epa => epa.Label));

        // The help text is announced with the group, not left as a paragraph a screen reader may skip.
        var help = cut.Find($"#{fieldset.GetAttribute("aria-describedby")}");
        help.TextContent.Should().Contain("separate evidence record");
        fieldset.Contains(help).Should().BeTrue();

        // And the old label is gone: nothing else claims to name the group.
        cut.FindAll("label").Should().NotContain(label => label.TextContent.Contains("Evidence for these EPAs"));
    }

    [Fact]
    public void AnExistingCampaign_EveryLabelNamesARealControl()
    {
        var cut = RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, 5));

        cut.Find("#msf-respondent-email");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    // ---- The report ----

    [Fact]
    public void TheCampaignReport_EveryLabelNamesARealControl()
    {
        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, 5));
        cut.WaitForState(() => cut.FindAll("#msf-narrative").Count > 0);

        // Both of its fields render: the level select only when the scale has rungs to offer.
        cut.Find("#msf-level");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    // ---- helpers ----

    private IRenderedComponent<CampaignEdit> RenderWithTraineeChosen()
    {
        var cut = RenderComponent<CampaignEdit>();

        cut.Find("#msf-subject").Change(TraineeUserId);
        cut.WaitForState(() => cut.FindAll("input[type=checkbox]").Count > 0);

        return cut;
    }

    private static TraineeProfileDto Trainee()
        => new(1, TraineeUserId, "trainee@test", "Thandi", "Mokoena", 2, "Paediatrics", "11.1", 3, "Paediatrics", 4,
            "General Paediatrics", new DateOnly(2026, 1, 1), new DateOnly(2029, 12, 31), true);

    private static MsfCampaignAggregateReportDto Report()
        => new(5, TraineeUserId, "Default MSF", MsfCampaignState.UnderReview, 8, 3, 9, null, true, [], 2, 2,
            [new MsfCoveredEpaDto(101, "PAED-001", "Resuscitate a critically ill child", false)], null, null);

    private sealed class ReferenceData : StubActivityReferenceDataService
    {
        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetSubjectCurriculumEpaOptionsAsync(
            string subjectUserId, string? permittedToolKey = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(subjectUserId == TraineeUserId ? TraineeEpas : []);

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetRatedLevelOptionsForActivityTypeAsync(
            string activityTypeKey, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([new("1", "1 - Observe"), new("2", "2 - Direct supervision")]);
    }

    private sealed class FakeSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                ListMsfTemplatesQuery => (IReadOnlyList<MsfTemplateDto>)[new MsfTemplateDto(1, "Default MSF", null, false, true, [])],
                ListTraineesForSpecialityQuery => (IReadOnlyList<TraineeProfileDto>)[Trainee()],
                GetCampaignAggregateReportQuery => Report(),
                GetMsfCampaignSetupQuery setup => new MsfCampaignSetupDto(
                    setup.CampaignId, "Default MSF", MsfTemplateKind.Msf, MsfCampaignState.Draft,
                    [MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Nurse]),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
