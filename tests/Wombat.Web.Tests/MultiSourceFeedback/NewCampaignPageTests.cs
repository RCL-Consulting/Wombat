using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// New MSF campaign as round 3 drew it (T369, C1–C8): four cards; the Questionnaire's fold outside the campaign's form
/// (S5); the required mark (M7); Opens on today on the South African calendar (T325); F2's floor; the refusal summary that
/// takes the focus, its lines links to their fields (M9); Create unavailable while no template exists (C1); and its reads
/// guarded (T329).
/// </summary>
public sealed class NewCampaignPageTests : WombatTestContext
{
    private const string Molefe = "lerato";

    private static readonly IReadOnlyList<ActivityCatalogueOption> FifteenEpas = Enumerable.Range(1, 15)
        .Select(number => new ActivityCatalogueOption((100 + number).ToString(), $"PAED-{number:000} — EPA {number}"))
        .ToList();

    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 10, 14, 8, 0, 0, TimeSpan.Zero));

    public NewCampaignPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(
            new Claim(ClaimTypes.NameIdentifier, "coordinator-1"),
            new Claim(WombatClaimTypes.InstitutionId, "1"));

        Services.AddSingleton<IActivityReferenceDataService>(new ReferenceData());
        Services.AddSingleton<TimeProvider>(_clock);
        JSInterop.SetupVoid(PageFocus.FocusByIdIdentifier, _ => true).SetVoidResult();
    }

    // ─── No template yet (C1, C2) ────────────────────────────────────────────

    [Fact]
    public void WithNoTemplate_TheFoldIsOpen_TheSelectSaysSo_AndCreateIsUnavailableWithItsReason()
    {
        var sender = new CreateSender { Templates = [] };
        var cut = Render(sender);

        var select = cut.Find("#msf-template-id");
        select.HasAttribute("disabled").Should().BeTrue();
        cut.FindAll("#msf-template-id option").Select(Text).Should().Equal("No template yet");
        Text(cut.Find("#msf-template-id-help")).Should().Be("Write the first one below; it is chosen here once it is added.");
        cut.Find("#msf-template-fold").HasAttribute("open").Should().BeTrue();

        var create = cut.Find("#msf-create-campaign");
        create.GetAttribute("type").Should().Be("button", "nothing can be sent while no questionnaire exists");
        create.GetAttribute("aria-disabled").Should().Be("true");
        create.GetAttribute("aria-describedby").Should().Be("msf-create-reason");
        Text(cut.Find("#msf-create-reason")).Should().Be("Create campaign: write a questionnaire first.");

        cut.Find("#campaign-form").Submit();
        sender.Commands.Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AddTemplate_IsAButtonOutsideTheCampaignsForm_ChoosesTheNewTemplate_AndItsResultTakesTheFocus()
    {
        var sender = new CreateSender { Templates = [] };
        var cut = Render(sender);

        cut.Find("#msf-template-name").Closest("form").Should().BeNull("S5: nothing in the fold posts with Create");
        cut.Find("#msf-add-template").GetAttribute("type").Should().Be("button");
        cut.Find("#msf-add-template").Click();

        var command = sender.Commands.OfType<CreateMsfTemplateCommand>().Should().ContainSingle().Subject;
        command.Name.Should().Be("Default MSF");
        command.Kind.Should().Be(MsfTemplateKind.Msf);

        cut.WaitForAssertion(() => Text(cut.Find("#msf-template-result .alert-success")).Should().Be(
            "Template created. Default MSF is chosen as this campaign's template."));
        cut.FindAll("#msf-template-id option").Select(Text).Should().Equal("Default MSF (Multi-source feedback)");
        Text(cut.Find("#msf-template-id-help")).Should().Be(
            "Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?");
        cut.Find("#msf-template-fold").HasAttribute("open").Should().BeFalse();
        cut.Find("#msf-create-campaign").GetAttribute("type").Should().Be("submit");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.TemplateResultRegion.Id));
    }

    [Fact]
    public void ATemplateWithNoName_IsRefusedUnderTheField_InWombatsWords_AndNothingIsSent()
    {
        var sender = new CreateSender { Templates = [] };
        var cut = Render(sender);

        cut.Find("#msf-template-name").Change(string.Empty);
        cut.Find("#msf-add-template").Click();

        sender.Commands.Should().BeEmpty();
        Text(cut.Find("#msf-template-name-error")).Should().Be("Enter the template's name.");
        cut.Find("#msf-template-name").GetAttribute("aria-invalid").Should().Be("true");
        JSInterop.VerifyInvoke(PageFocus.FocusByIdIdentifier).Arguments[0].Should().Be("msf-template-name");
    }

    // ─── The form (C3, M7, T325) ─────────────────────────────────────────────

    [Fact]
    public void EveryRequiredControl_CarriesTheMark_AndAriaRequired_AndTheLineSaysWhatTheMarkMeans()
    {
        var cut = Render(new CreateSender());

        var line = cut.FindAll(".msf-form-column > p.field-help").First();
        Text(line).Should().Be("* marks a field you must fill in.");

        foreach (var id in new[] { "msf-template-id", "msf-subject", "msf-opens", "msf-closes", "msf-min-responses",
                     "msf-min-category", "msf-min-category-count", "msf-template-name", "msf-template-scale", "msf-template-comment" })
        {
            var mark = cut.Find($"label[for={id}] .required-mark");
            mark.GetAttribute("aria-hidden").Should().Be("true", id);
            cut.Find($"#{id}").GetAttribute("aria-required").Should().Be("true", id);
        }

        // M7: the hidden word on the EPA group's legend only.
        cut.FindAll("label .visually-hidden").Should().BeEmpty();
        Text(cut.Find("#msf-epas legend .visually-hidden")).Should().Be("required");

        // S5: the Template select joins the campaign's form from outside it.
        cut.Find("#msf-template-id").GetAttribute("form").Should().Be("campaign-form");
        cut.Find("#campaign-form").HasAttribute("novalidate").Should().BeTrue();
        cut.FindAll("section.form-container h2").Select(heading => Text(heading))
            .Should().Equal(
                "Questionnaire Written once, used by every campaign that names it.",
                "Trainee and window",
                "Thresholds Nothing is shown of a respondent group that falls short, so no answer can be traced to its author.",
                "Evidence");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheWindow_OpensTodayOnTheSouthAfricanCalendar_AndRunsTwoWeeks()
    {
        // T325: 22:30 UTC on the 13th is already the 14th in South Africa.
        _clock.Now = new DateTimeOffset(2026, 10, 13, 22, 30, 0, TimeSpan.Zero);
        var cut = Render(new CreateSender());

        cut.Find("#msf-opens").GetAttribute("value").Should().Be("2026-10-14");
        cut.Find("#msf-closes").GetAttribute("value").Should().Be("2026-10-28");
        Text(cut.Find("#msf-opens-help")).Should().Be("Today, on the South African calendar.");
        Text(cut.Find("#msf-subject-help")).Should().Be("Current trainees you can run a campaign for.");
        cut.Find("#msf-min-responses").GetAttribute("value").Should().Be("8");
        cut.Find("#msf-min-category").GetAttribute("value").Should().Be("3");
        cut.Find("#msf-min-category-count").GetAttribute("value").Should().Be("2");
    }

    [Fact]
    public void TickingEpas_IsCounted_TwoOfFifteen_ThenFifteen()
    {
        var cut = Render(new CreateSender());
        ChooseMolefe(cut);

        Text(cut.Find("#msf-epas p[role=status]")).Should().Be("0 of 15 ticked");
        cut.Find("#msf-epa-110").Change(true);
        cut.Find("#msf-epa-112").Change(true);
        Text(cut.Find("#msf-epas p[role=status]")).Should().Be("2 of 15 ticked");

        foreach (var epa in FifteenEpas.Where(epa => epa.Value is not "110" and not "112"))
        {
            cut.Find($"#msf-epa-{epa.Value}").Change(true);
        }

        Text(cut.Find("#msf-epas p[role=status]")).Should().Be("15 of 15 ticked");
        cut.Find("#msf-epas .epa-checks").Should().NotBeNull();
    }

    [Fact]
    public void Creating_SendsWhatWasChosen_AndGoesToTheCampaignsPage()
    {
        var sender = new CreateSender();
        var cut = Render(sender);
        ChooseMolefe(cut);
        cut.Find("#msf-min-responses").Change("5");
        cut.Find("#msf-epa-110").Change(true);
        cut.Find("#msf-epa-112").Change(true);

        cut.Find("#campaign-form").Submit();

        var create = sender.Commands.OfType<CreateMsfCampaignCommand>().Should().ContainSingle().Subject;
        create.SubjectUserId.Should().Be(Molefe);
        create.TemplateId.Should().Be(1, "the one template is chosen without a pick (3.35)");
        create.OpensOn.Should().Be(new DateOnly(2026, 10, 14));
        create.ClosesOn.Should().Be(new DateOnly(2026, 10, 28));
        (create.MinimumResponses, create.MinimumCategoryResponses, create.MinimumRespondentCategories).Should().Be((5, 3, 2));
        create.EpaIds.Should().Equal(110, 112);
        Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/msf/campaigns/41");
    }

    // ─── Refused (C4, C8, M9) ────────────────────────────────────────────────

    [Fact]
    public void NoEpaTicked_IsRefusedByTheSummary_WhichTakesTheFocus_ItsLinkFocusingTheFirstBox()
    {
        var sender = new CreateSender();
        var cut = Render(sender);
        ChooseMolefe(cut);

        cut.Find("#campaign-form").Submit();

        sender.Commands.Should().BeEmpty();
        var summary = cut.Find("#msf-create-refusal");
        summary.ClassList.Should().Contain(["validation-summary-errors", "error-summary"]);
        summary.GetAttribute("role").Should().Be("alert");
        summary.GetAttribute("tabindex").Should().Be("-1");
        Text(summary.QuerySelector("p")!).Should().Be("Not created. Fix the field below and create the campaign again.");
        var link = summary.QuerySelector("li a")!;
        Text(link).Should().Be("Select at least one EPA this campaign is evidence for.");
        link.GetAttribute("href").Should().Be("#msf-epa-101", "the first box, never the legend");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.Summary.Id));

        var group = cut.Find("#msf-epas");
        group.ClassList.Should().Contain(["choice-group", "is-invalid"]);
        group.GetAttribute("aria-describedby").Should().Be("msf-epas-help msf-epas-error");
        Text(cut.Find("#msf-epas-error")).Should().Be("Select at least one EPA this campaign is evidence for.");

        link.Click();
        JSInterop.VerifyInvoke(PageFocus.FocusByIdIdentifier).Arguments[0].Should().Be("msf-epa-101");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ARespondentGroupMinimumUnderThree_IsRefused_InTheFieldsWords_AndTheSummaryNamesTheField()
    {
        var sender = new CreateSender();
        var cut = Render(sender);
        ChooseMolefe(cut);
        cut.Find("#msf-epa-110").Change(true);
        cut.Find("#msf-min-category").Change("2");

        cut.Find("#campaign-form").Submit();

        sender.Commands.Should().BeEmpty();
        var link = cut.Find("#msf-create-refusal li a");
        Text(link).Should().Be("Minimum responses in a respondent group: enter a number from 3 to 100.");
        link.GetAttribute("href").Should().Be("#msf-min-category");
        Text(cut.Find("#msf-min-category-error")).Should().Be("Enter a number from 3 to 100.");
        var field = cut.Find("#msf-min-category");
        field.GetAttribute("aria-invalid").Should().Be("true");
        field.GetAttribute("aria-describedby").Should().Be("msf-min-category-help msf-min-category-error");
        field.ClassList.Should().Contain("input-validation-error");
    }

    [Fact]
    public void AHandlersRefusal_IsTheSummarysText()
    {
        var sender = new CreateSender
        {
            Refusal = new MsfRefusedException("These EPAs are not on the trainee's curriculum and cannot be covered by this campaign: 110")
        };
        var cut = Render(sender);
        ChooseMolefe(cut);
        cut.Find("#msf-epa-110").Change(true);

        cut.Find("#campaign-form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#msf-create-refusal")).Should().Be(
            "Not created. These EPAs are not on the trainee's curriculum and cannot be covered by this campaign: 110"));
        Services.GetRequiredService<FakeNavigationManager>().Uri.Should().NotContain("/msf/campaigns/41");
    }

    // ─── Loading and a failed read (C6, C7; T329) ────────────────────────────

    [Fact]
    public void WhileTheTemplatesAndTraineesAreRead_ThePageSaysSo()
    {
        var sender = new CreateSender { HoldReads = true };
        var cut = Render(sender);

        Text(cut.Find("h1")).Should().Be("New MSF campaign");
        Text(cut.Find("p[role=status]")).Should().Be("Loading the templates and trainees.");
        cut.FindAll("#msf-subject, #msf-create-campaign").Should().BeEmpty();

        sender.ReleaseReads();
        cut.WaitForAssertion(() => cut.FindAll("#msf-subject").Should().ContainSingle(), AsyncWorkTimeout);
    }

    [Fact]
    public void AFailedRead_IsAnAlertWithTryAgain_TheCircuitStaysUp_AndTryAgainReadsBoth()
    {
        var sender = new CreateSender { ReadFailures = 1 };
        var cut = Render(sender);

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load what this campaign needs. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("pool exhausted");
        cut.FindAll("#msf-subject").Should().BeEmpty();

        cut.FindAll("button").Single(button => Text(button) == "Try again").Click();
        cut.WaitForAssertion(() => cut.FindAll("#msf-subject").Should().ContainSingle());
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<NewCampaign> Render(CreateSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<NewCampaign>();
    }

    private static void ChooseMolefe(IRenderedComponent<NewCampaign> cut)
    {
        cut.Find("#msf-subject").Change(Molefe);
        cut.WaitForState(() => cut.FindAll("#msf-epas input[type=checkbox]").Count == 15);
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private static TraineeProfileDto Trainee()
        => new(1, Molefe, "molefe@kgk.wombat.local", "Lerato", "Molefe", 2, "Paediatrics", "11.1", 3, "Paediatrics", 4,
            "General Paediatrics", new DateOnly(2023, 1, 1), new DateOnly(2027, 12, 31), true);

    private static MsfTemplateDto DefaultMsf(int id) => new(id, "Default MSF", null, false, true,
    [
        new MsfQuestionDto(1, 1, "Rates the trainee's overall professional performance.", MsfQuestionType.Scale, null, true),
        new MsfQuestionDto(2, 2, "What should the trainee keep doing or improve?", MsfQuestionType.LongText, null, false)
    ]);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class ReferenceData : StubActivityReferenceDataService
    {
        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetSubjectCurriculumEpaOptionsAsync(
            string subjectUserId, string? permittedToolKey = null, CancellationToken cancellationToken = default)
            => Task.FromResult(FifteenEpas);
    }

    private sealed class CreateSender : IScopedSender
    {
        private TaskCompletionSource? _held;
        private int _readFailures;

        public List<MsfTemplateDto> Templates { get; init; } = [DefaultMsf(1)];

        public List<object> Commands { get; } = [];

        public Exception? Refusal { get; init; }

        public bool HoldReads { get; init; }

        public int ReadFailures { init => _readFailures = value; }

        public void ReleaseReads() => (_held ?? throw new InvalidOperationException("No read was held.")).SetResult();

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListMsfTemplatesQuery:
                    if (HoldReads && _held is null)
                    {
                        _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                        await _held.Task;
                    }

                    if (_readFailures > 0)
                    {
                        _readFailures--;
                        throw new TimeoutException("pool exhausted");
                    }

                    return (TResponse)(object)(IReadOnlyList<MsfTemplateDto>)Templates.ToList();

                case ListMsfCampaignSubjectsQuery:
                    return (TResponse)(object)(IReadOnlyList<TraineeProfileDto>)[Trainee()];

                case CreateMsfTemplateCommand template:
                    Commands.Add(template);
                    var created = DefaultMsf(Templates.Count + 1) with { Name = template.Name, Kind = template.Kind };
                    Templates.Add(created);
                    return (TResponse)(object)created;

                case CreateMsfCampaignCommand create:
                    Commands.Add(create);
                    if (Refusal is not null)
                    {
                        throw Refusal;
                    }

                    return (TResponse)(object)new MsfCampaignSummaryDto(
                        41, create.SubjectUserId, "Default MSF", create.OpensOn, create.ClosesOn, create.MinimumResponses,
                        create.MinimumCategoryResponses, MsfCampaignState.Draft, 0, 0, null);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
