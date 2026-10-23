using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Assessors;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T376 (flow 12, lane T): the user page's Assessor training section (boards T03–T07, S10, Q18). Record training or Edit
/// training opens the note panel (F11, M9, M10) asking the status, a completion date only for a finished status, and the
/// qualifications, never an institution or a speciality (M1). A refusal is said under its field and marks its inputs, not
/// the fieldset (check Left 4); the saved result is said in the section and focused, its first sentence bold.
/// </summary>
public sealed class AssessorTrainingSectionTests : TestContext
{
    private const string FocusById = "wombat.focusById";
    private const string FocusReference = "Blazor._internal.domWrapper.focus";

    private readonly FakeSender _sender = new();
    private int _acted;

    public AssessorTrainingSectionTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("mbatha@kgk.test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"), new Claim(WombatClaimTypes.InstitutionId, "1"));
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
    }

    [Fact]
    public void T03_OpenedAtTheAddress_ThePanelIsOpen_ItsFirstRadioFocused_AndItAsksNoScope()
    {
        var cut = Render("zulu", "Thandi Zulu", open: true);

        var trigger = cut.Find("#training-trigger");
        trigger.TextContent.Trim().Should().Be("Record training");
        trigger.GetAttribute("aria-label").Should().Be("Record training for Thandi Zulu");
        trigger.GetAttribute("aria-expanded").Should().Be("true");
        trigger.GetAttribute("aria-controls").Should().Be("training-panel");
        var panel = cut.Find("#training-panel");
        panel.ClassList.Should().Contain(["detail-card", "note-panel", "note-panel--form"]);
        panel.HasAttribute("role").Should().BeFalse("M10: the panel is not a group, its fieldset is");
        cut.FindAll("#training-panel input[type=radio] + label").Select(label => label.TextContent)
            .Should().Equal("Not started", "In training", "Provisional (awaiting sign-off)", "Trained");
        cut.FindAll("#training-panel select").Should().BeEmpty("M1: no institution or speciality is asked");
        cut.FindAll("#training-completed").Should().BeEmpty("no finished status is chosen");
        FocusedIds().Should().Equal("training-status-0");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void WithARecord_TheSectionReadsItOut_AndOffersEditTraining_Closed()
    {
        _sender.Record = new AssessorTrainingDto(3, AssessorTrainingStatus.Trained, new DateOnly(2018, 10, 20), "MBChB, MMed (Paed)");

        var cut = Render("zulu", "Thandi Zulu");

        cut.FindAll("dl.detail-list dt").Select(dt => dt.TextContent).Should().Equal("Training status", "Training completed", "Qualifications");
        cut.FindAll("dl.detail-list dd.readonly-value").Select(dd => dd.TextContent).Should().Equal("Trained", "2018-10-20", "MBChB, MMed (Paed)");
        var trigger = cut.Find("#training-trigger");
        trigger.TextContent.Trim().Should().Be("Edit training");
        trigger.GetAttribute("aria-label").Should().Be("Edit training for Thandi Zulu");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
        trigger.HasAttribute("aria-controls").Should().BeFalse("M10: aria-controls only while the panel exists");
        cut.FindAll("#training-panel").Should().BeEmpty();
        FocusedIds().Should().BeEmpty("only ?section=training takes the focus");
    }

    [Fact]
    public void EditTraining_OpensThePanel_FilledFromTheRecord_AndSaysThePageActed()
    {
        _sender.Record = new AssessorTrainingDto(3, AssessorTrainingStatus.Provisional, new DateOnly(2026, 9, 20), "FCPaed");
        var cut = Render("khumalo", "Fatma Khumalo");

        cut.Find("#training-trigger").Click();

        cut.Find("#training-status-2").HasAttribute("checked").Should().BeTrue();
        cut.Find("#training-completed").GetAttribute("value").Should().Be("2026-09-20");
        cut.Find("#training-qualifications").GetAttribute("value").Should().Be("FCPaed");
        _acted.Should().Be(1, "opening is an action: the page clears its own result");
        FocusedIds().Should().Equal("training-status-0");
    }

    [Fact]
    public void T05_AnEmptySave_RefusesBothFields_OnTheirInputs_AndFocusesTheFirstRadio_SendingNothing()
    {
        var cut = Render("khumalo", "Fatma Khumalo", open: true);

        Button(cut, "Save training").Click();

        var group = cut.Find("#training-status");
        group.ClassList.Should().Contain(["choice-group", "is-invalid"]);
        group.HasAttribute("aria-invalid").Should().BeFalse("check Left 4: not a supported attribute on a group");
        cut.FindAll("#training-panel input[type=radio]").Should().OnlyContain(radio =>
            radio.GetAttribute("aria-invalid") == "true"
            && radio.GetAttribute("aria-describedby") == "training-status-error training-status-help");
        cut.Find("#training-status-error").TextContent.Should().Be("Choose the training status.");
        var qualifications = cut.Find("#training-qualifications");
        qualifications.GetAttribute("aria-invalid").Should().Be("true");
        qualifications.GetAttribute("aria-describedby").Should().Be("training-qualifications-error");
        qualifications.ClassList.Should().Contain("input-validation-error");
        cut.Find("#training-qualifications-error").TextContent.Should().Be("Type the qualifications, as they should read.");
        cut.FindAll(".validation-summary-errors, #refusal-summary").Should().BeEmpty("S22: no summary for a form's own fields");
        FocusedIds().Last().Should().Be("training-status-0");
        _sender.Saves.Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void T06_T07_AFinishedStatusAsksItsDate_AndTheSave_IsSaidInTheSection_FocusedAndReadOut()
    {
        var cut = Render("khumalo", "Fatma Khumalo", open: true);

        cut.Find("#training-status-2").Change(AssessorTrainingStatus.Provisional.ToString());
        cut.Find("#training-completed").Change("2026-09-20");
        cut.Find("#training-qualifications").Change("MBChB, MMed (Paed), FC Paed (SA)");
        cut.Find("label[for=training-completed]").TextContent.Should().Be("Training completed");
        Button(cut, "Save training").Click();

        cut.WaitForAssertion(() => cut.FindAll("#training-panel").Should().BeEmpty());
        _sender.Saves.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            UserId = "khumalo",
            TrainingStatus = AssessorTrainingStatus.Provisional,
            TrainingCompletedOn = (DateOnly?)new DateOnly(2026, 9, 20),
            Qualifications = "MBChB, MMed (Paed), FC Paed (SA)"
        });
        var result = cut.Find("#training-result .alert-success");
        result.TextContent.Should().Be("Training saved for Fatma Khumalo: Provisional (awaiting sign-off), completed 2026-09-20.");
        result.QuerySelector("strong")!.TextContent.Should().Be("Training saved for Fatma Khumalo:", "only its first sentence is bold");
        result.GetAttribute("role").Should().Be("status");
        var region = cut.FindComponent<ActionResult>();
        cut.WaitForAssertion(() => JSInterop.Invocations.Where(call => call.Identifier == FocusReference)
            .Should().ContainSingle().Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(region.Instance.Element.Id));
        cut.FindAll("dl.detail-list dd").Select(dd => dd.TextContent)
            .Should().Equal("Provisional (awaiting sign-off)", "2026-09-20", "MBChB, MMed (Paed), FC Paed (SA)");
        cut.Find("#training-trigger").TextContent.Trim().Should().Be("Edit training");
    }

    [Fact]
    public void P9_ThePagesOwnAction_ClearsTheSectionsResult_OneResultOnThePageAtATime()
    {
        // Save training, then Add a role… (or Lock out…) on the page: the page bumps PageResultVersion, and the section's
        // "Training saved for" goes, so its result never stands beside the page's (T376 build review, P9).
        var cut = Render("khumalo", "Fatma Khumalo", open: true);
        cut.Find("#training-status-3").Change(AssessorTrainingStatus.Trained.ToString());
        cut.Find("#training-qualifications").Change("MBChB");
        Button(cut, "Save training").Click();
        cut.WaitForAssertion(() => cut.FindAll("#training-result .alert-success").Should().ContainSingle());

        cut.SetParametersAndRender(parameters => parameters.Add(section => section.PageResultVersion, 1));

        cut.FindAll("#training-result .alert").Should().BeEmpty();
        cut.FindAll("dl.detail-list dd").Should().NotBeEmpty("only the result goes; the record stays");
    }

    [Fact]
    public void AStatusThatRecordsNoDate_DropsADateTypedForAnother_AndBothasNoDateSaveSaysSo()
    {
        var cut = Render("botha", "Sarah Botha", open: true);
        cut.Find("#training-status-3").Change(AssessorTrainingStatus.Trained.ToString());
        cut.Find("#training-completed").Change("2020-01-01");
        cut.Find("#training-status-1").Change(AssessorTrainingStatus.InTraining.ToString());
        cut.FindAll("#training-completed").Should().BeEmpty();
        cut.Find("#training-qualifications").Change("MBChB");
        Button(cut, "Save training").Click();

        cut.WaitForAssertion(() => _sender.Saves.Should().ContainSingle().Which.TrainingCompletedOn.Should().BeNull());

        // Botha's no-date save (check Left 4: a Spec line with no board).
        var botha = Render("botha", "Sarah Botha", open: true);
        botha.Find("#training-status-3").Change(AssessorTrainingStatus.Trained.ToString());
        botha.Find("#training-qualifications").Change("MBChB");
        Button(botha, "Save training").Click();
        botha.WaitForAssertion(() => botha.Find("#training-result .alert-success").TextContent
            .Should().Be("Training saved for Sarah Botha: Trained, no date."));
    }

    [Fact]
    public void Cancel_ClosesThePanel_ReturnsTheFocusToItsTrigger_AndSaysThePageActed()
    {
        var cut = Render("zulu", "Thandi Zulu", open: true);

        Button(cut, "Cancel").ClassList.Should().Contain("btn-quiet", "F11: a quiet Cancel");
        Button(cut, "Cancel").Click();

        cut.FindAll("#training-panel").Should().BeEmpty();
        cut.Find("#training-trigger").GetAttribute("aria-expanded").Should().Be("false");
        FocusedIds().Last().Should().Be("training-trigger");
        _acted.Should().Be(1);
    }

    [Fact]
    public void ARefusalForAReason_IsWordedFromTheReason_InTheSection()
    {
        _sender.SaveRefusal = new AssessorTrainingRefusedException(AssessorTrainingRefusal.NotAnAssessor, "audit words");
        var cut = Render("zulu", "Thandi Zulu", open: true);
        cut.Find("#training-status-3").Change(AssessorTrainingStatus.Trained.ToString());
        cut.Find("#training-qualifications").Change("MBChB");

        Button(cut, "Save training").Click();

        cut.WaitForAssertion(() => cut.Find("#training-result .alert-danger").TextContent
            .Should().Be("Thandi Zulu does not hold the Assessor role now, so the training was not saved."));
        cut.Markup.Should().NotContain("audit words");
    }

    [Fact]
    public void S10_WithoutAssessor_NothingShows_UnlessOpened_WhenTheAbsenceIsSaidOnce_AndFocused()
    {
        var closed = Render("patel", "Mohammed Patel", holdsAssessor: false);
        closed.Markup.Trim().Should().BeEmpty("Q18: hidden, kept");

        var opened = Render("patel", "Mohammed Patel", holdsAssessor: false, open: true);
        opened.Find("#training-absent").TextContent.Should().Be(
            "Mohammed Patel does not hold the Assessor role, so has no assessor training here.");
        FocusedIds().Should().Equal("training-absent");
        _sender.Reads.Should().Be(0, "nothing is read for a person without the role");
    }

    [Fact]
    public void Q18_GivenTheRoleAgain_TheSectionShowsTheKeptRecord()
    {
        _sender.Record = new AssessorTrainingDto(3, AssessorTrainingStatus.Trained, null, "MBChB");
        var cut = Render("zulu", "Thandi Zulu", holdsAssessor: false);

        cut.SetParametersAndRender(parameters => parameters.Add(section => section.HoldsAssessor, true));

        cut.WaitForAssertion(() => cut.FindAll("dl.detail-list dd").Select(dd => dd.TextContent).First().Should().Be("Trained"));
    }

    [Fact]
    public void OnTheCallersOwnPage_TheRecordIsReadOut_WithNoWayToChangeIt()
    {
        _sender.Record = new AssessorTrainingDto(3, AssessorTrainingStatus.Trained, null, "MBChB");

        var cut = Render("mbatha", "Nolwazi Mbatha", open: true);

        cut.FindAll("#training-trigger").Should().BeEmpty();
        cut.FindAll("#training-panel").Should().BeEmpty();
        var note = cut.Find(".alert-info");
        note.TextContent.Should().Be(AssessorTrainingWords.OwnTraining);
        note.HasAttribute("role").Should().BeFalse("a standing notice");
    }

    [Fact]
    public void AFailedRead_IsAFixedSentence_WithTryAgain()
    {
        _sender.ReadFailure = new InvalidOperationException("Npgsql exploded");

        var cut = Render("zulu", "Thandi Zulu", wait: false);
        cut.WaitForState(() => cut.FindAll(".alert-danger").Count > 0);

        cut.Find(".alert-danger").TextContent.Should().Contain("Could not load the assessor training.").And.NotContain("Npgsql");
        cut.FindAll("button").Should().Contain(button => button.TextContent.Trim() == "Try again");
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<AssessorTrainingSection> Render(
        string userId, string name, bool holdsAssessor = true, bool open = false, bool wait = true)
    {
        var cut = RenderComponent<AssessorTrainingSection>(parameters => parameters
            .Add(section => section.UserId, userId)
            .Add(section => section.PersonName, name)
            .Add(section => section.HoldsAssessor, holdsAssessor)
            .Add(section => section.Open, open)
            .Add(section => section.OnActed, () => _acted++));
        if (wait && holdsAssessor)
        {
            cut.WaitForState(() => cut.FindAll("#training-trigger, .alert-info").Count > 0);
        }

        return cut;
    }

    private IReadOnlyList<string> FocusedIds()
        => JSInterop.Invocations.Where(call => call.Identifier == FocusById).Select(call => (string)call.Arguments[0]!).ToList();

    private static AngleSharp.Dom.IElement Button(IRenderedFragment cut, string text)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == text);

    private sealed class FakeSender : IScopedSender
    {
        public AssessorTrainingDto? Record { get; set; }
        public Exception? ReadFailure { get; set; }
        public Exception? SaveRefusal { get; set; }
        public int Reads { get; private set; }
        public List<CreateOrUpdateAssessorProfileCommand> Saves { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetAssessorTrainingQuery:
                    Reads++;
                    return ReadFailure is not null
                        ? Task.FromException<TResponse>(ReadFailure)
                        : Task.FromResult((TResponse)(object?)Record!);
                case CreateOrUpdateAssessorProfileCommand save:
                    Saves.Add(save);
                    if (SaveRefusal is not null)
                    {
                        return Task.FromException<TResponse>(SaveRefusal);
                    }

                    return Task.FromResult((TResponse)(object)new AssessorProfileDto(
                        9, save.UserId, "x@kgk.test", "X", "Y", save.Qualifications.Trim(), 1, "KGK",
                        save.TrainingStatus,
                        AssessorTrainingWords.HasCompletion(save.TrainingStatus) ? save.TrainingCompletedOn : null));
                default:
                    throw new NotSupportedException(request.GetType().Name);
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
