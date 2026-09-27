using System.Collections;
using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft;
using Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Accounts;
using Wombat.Application.Features.Adoptions;
using Wombat.Application.Features.Assessors;
using Wombat.Application.Features.Colleges;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.DeactivateInstitution;
using Wombat.Application.Features.Institutions.Commands.DeactivateSpeciality;
using Wombat.Application.Features.Institutions.Commands.DeactivateSubSpeciality;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;
using Wombat.Application.Features.Institutions.Commands.UpdateSpeciality;
using Wombat.Application.Features.Institutions.Commands.UpdateSubSpeciality;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionById;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialityById;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesForSpeciality;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Invitations;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Reporting;
using Wombat.Application.Features.Scheduling;
using Wombat.Application.Features.Scheduling.Commands.DisableScheduledJob;
using Wombat.Application.Features.Scheduling.Commands.RunScheduledJobNow;
using Wombat.Application.Features.Scheduling.Queries.GetScheduledJobStatus;
using Wombat.Application.Features.Sso;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.AddRoleToUser;
using Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;
using Wombat.Application.Features.Users.Commands.ResetUserPassword;
using Wombat.Application.Features.Users.Commands.RevokePendingInvitationsForEmail;
using Wombat.Application.Features.Users.Commands.SetUserLockout;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Components.Pages.Admin.Adoptions;
using Wombat.Web.Components.Pages.Admin.Assessors;
using Wombat.Web.Components.Pages.Admin.Colleges;
using Wombat.Web.Components.Pages.Admin.CurriculumProgress;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Components.Pages.Admin.DataRights;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Components.Pages.Admin.Invitations;
using Wombat.Web.Components.Pages.Admin.Jobs;
using Wombat.Web.Components.Pages.Admin.Sso;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Pages.Profile;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;
using RebuildCommand = Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress.RebuildCurriculumProgressCommand;
using RebuildResult = Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress.RebuildCurriculumProgressResult;

namespace Wombat.Web.Tests.Accessibility;

/// <summary>
/// T234: a button keeps the keyboard focus while its own action runs, and once the action is done its result takes the
/// focus. A refusal leaves the focus where it was.
/// </summary>
/// <remarks>
/// <para>
/// Until T234 page after page disabled its own button while its action ran, so a keyboard user who pressed Save found
/// the focus on the page body: a browser drops the focus of a button it disables. Where the button survived, it came
/// back enabled with the focus nowhere; where the action took it away (a cleared choice, a rebuilt form, a row gone), the
/// focus stayed lost. A status that arrives on its own is often not read either. Now the pressed button stays enabled,
/// a second press sends nothing (the in-flight flag, T202), and a done action moves the focus to its result region
/// (<see cref="ActionResult" />, DESIGN.md § Button system).
/// </para>
/// <para>
/// Each page is a scenario: how to render it, which button is pressed and how, and which request the test holds open
/// until it releases or refuses it. That request is the window a second press falls into.
/// </para>
/// </remarks>
public sealed class ActionFocusTests : TestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private const string Refusal = "Refused for the test.";

    private static readonly DateTime Created = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly TestAuthorizationContext _auth;

    // How many focus calls a scenario's own setup made; FocusCalls() counts only those after (IgnoreFocusSoFar).
    private int _focusCallsBefore;

    public ActionFocusTests()
    {
        _auth = this.AddTestAuthorization();
        SignIn(WombatRoles.Administrator);

        // Dialogs and downloads call into the browser; neither is what these tests are about.
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public static TheoryData<string> AllScenarios()
    {
        var data = new TheoryData<string>();
        foreach (var name in Scenarios.Keys)
        {
            data.Add(name);
        }

        return data;
    }

    public static TheoryData<string> ScenariosThatStay()
    {
        var data = new TheoryData<string>();
        foreach (var (name, scenario) in Scenarios)
        {
            if (scenario.LeavesFor is null)
            {
                data.Add(name);
            }
        }

        return data;
    }

    public static TheoryData<string> ScenariosThatLeave()
    {
        var data = new TheoryData<string>();
        foreach (var (name, scenario) in Scenarios)
        {
            if (scenario.LeavesFor is not null)
            {
                data.Add(name);
            }
        }

        return data;
    }

    public static TheoryData<string> RefusableScenarios()
    {
        var data = new TheoryData<string>();
        foreach (var (name, scenario) in Scenarios)
        {
            if (scenario.CanBeRefused)
            {
                data.Add(name);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public void WhileTheActionRuns_ItsButtonStaysEnabled_AndASecondPressSendsNothing(string name)
    {
        var scenario = Scenarios[name];
        var hold = new Hold();
        var cut = scenario.Render(this, hold);
        scenario.Button(cut).HasAttribute("aria-disabled").Should().BeFalse("nothing is running yet");

        scenario.Press(cut);
        cut.WaitForAssertion(() => hold.Count.Should().Be(1));

        scenario.Button(cut).HasAttribute("disabled").Should().BeFalse(
            "the button has the focus, and a browser drops the focus of a button it disables, to the page (T234)");
        scenario.Button(cut).GetAttribute("aria-disabled").Should().Be("true",
            "a screen reader is told that a second press does nothing; a changed label on the focused button is often " +
            "not read (T234 review)");

        scenario.Press(cut);
        hold.Count.Should().Be(1, "a press while the action runs must send nothing");

        hold.Release();
        if (scenario.LeavesFor is { } destination)
        {
            cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith(destination));
        }
        else
        {
            cut.WaitForAssertion(() => FocusCalls().Should().NotBeEmpty());
        }

        hold.Count.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(ScenariosThatStay))]
    public void OnceTheActionIsDone_ItsResultTakesTheFocus(string name)
    {
        var scenario = Scenarios[name];
        var hold = new Hold();
        var cut = scenario.Render(this, hold);

        scenario.Press(cut);
        cut.WaitForAssertion(() => hold.Count.Should().Be(1));
        FocusCalls().Should().BeEmpty("nothing has answered yet");

        hold.Release();

        // bUnit on .NET 10 leaves an element's blazor:elementReference empty in the markup, so the focused reference is
        // matched to the region's own.
        var region = cut.FindComponent<ActionResult>();
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(region.Instance.Element.Id));
        region.Find(".action-result").TextContent.Trim().Should().NotBeEmpty("the focus reads the result out");
        region.Find(".action-result").GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(ScenariosThatLeave))]
    public void OnceAnActionThatLeavesThePageIsDone_ItMovesNoFocus_TheNextPageHasIt(string name)
    {
        var scenario = Scenarios[name];
        var hold = new Hold();
        var cut = scenario.Render(this, hold);

        scenario.Press(cut);
        cut.WaitForAssertion(() => hold.Count.Should().Be(1));

        hold.Release();

        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith(scenario.LeavesFor!));
        FocusCalls().Should().BeEmpty("the page it leaves for moves the focus to its heading (FocusOnNavigate)");
    }

    [Theory]
    [MemberData(nameof(RefusableScenarios))]
    public void OnceTheActionIsRefused_TheRefusalIsShown_AndTheFocusStaysWhereItWas(string name)
    {
        var scenario = Scenarios[name];
        var hold = new Hold();
        var cut = scenario.Render(this, hold);

        scenario.Press(cut);
        cut.WaitForAssertion(() => hold.Count.Should().Be(1));

        hold.Refuse();

        cut.WaitForAssertion(() => cut.Find(".alert.alert-danger").TextContent.Should().Contain(Refusal));
        cut.Find(".alert.alert-danger").GetAttribute("role").Should().Be("alert", "a refusal is read at once");

        if (scenario.RefusalTakesFocus)
        {
            cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle());
        }
        else
        {
            FocusCalls().Should().BeEmpty("the button that was pressed is still there, enabled, with the focus");
            scenario.Button(cut).HasAttribute("disabled").Should().BeFalse();
            scenario.Button(cut).HasAttribute("aria-disabled").Should().BeFalse("its action has answered");
        }
    }

    /// <summary>
    /// T234. A certificate's Download button was disabled, with every other, while it generated. A download says what it
    /// did itself, and a refusal is an alert, so the focus stays on the button pressed: only the others are disabled.
    /// </summary>
    [Fact]
    public void WhileACertificateIsGenerated_ItsButtonStaysEnabled_TheOthersAreNot_AndASecondPressSendsNothing()
    {
        SignIn(WombatRoles.Trainee);
        var hold = new Hold();
        var cut = Page<MyAuthorisations>(new Sender(
            hold,
            request => request is DownloadEntrustmentCertificateCommand,
            request => request switch
            {
                GetActiveDecisionsForTraineeQuery => new[] { Decision(1, "PAED-001"), Decision(2, "PAED-002") },
                DownloadEntrustmentCertificateCommand => new EntrustmentCertificateResult([1, 2, 3], "star.pdf", "hash-1"),
                _ => null
            }));

        DownloadButtons(cut)[0].Click();

        cut.WaitForAssertion(() => DownloadButtons(cut)[0].TextContent.Trim().Should().Be("Generating…"));
        DownloadButtons(cut)[0].HasAttribute("disabled").Should().BeFalse("it has the focus (T234)");
        DownloadButtons(cut)[0].GetAttribute("aria-disabled").Should().Be("true", "a second press does nothing (T234 review)");
        DownloadButtons(cut)[1].HasAttribute("disabled").Should().BeTrue();

        DownloadButtons(cut)[0].Click();
        hold.Count.Should().Be(1, "a press while the certificate is generated sends nothing");

        hold.Release();

        cut.WaitForAssertion(() => DownloadButtons(cut).Should().OnlyContain(button => !button.HasAttribute("disabled")));
        DownloadButtons(cut).Should().OnlyContain(button => !button.HasAttribute("aria-disabled"));
        FocusCalls().Should().BeEmpty("the focus never left the button");
        hold.Count.Should().Be(1);
    }

    // ---- T239 review: the names of repeated actions outside a table ----
    //
    // These pages' repeated buttons sit in cards and lists, not a table's rows, so the source scan
    // (Design/RowActionMarkupTests) does not see them. They are here for the fixtures: each page renders as its focus
    // scenarios do.

    /// <summary>
    /// T239. Every card's button read "Download certificate". At rest each is named by its EPA; while its certificate is
    /// generated, by what it then says, "Generating…", as a sighted user reads it; and named again once it is done.
    /// </summary>
    [Fact]
    public void EachCertificatesDownload_IsNamedByItsEpa_AndWhileItRuns_ByWhatItSays()
    {
        SignIn(WombatRoles.Trainee);
        var hold = new Hold();
        var cut = Page<MyAuthorisations>(new Sender(
            hold,
            request => request is DownloadEntrustmentCertificateCommand,
            request => request switch
            {
                GetActiveDecisionsForTraineeQuery => new[] { Decision(1, "PAED-001"), Decision(2, "PAED-002") },
                DownloadEntrustmentCertificateCommand => new EntrustmentCertificateResult([1, 2, 3], "star.pdf", "hash-1"),
                _ => null
            }));

        DownloadButtons(cut).Select(button => AccessibleNames.NameOf(cut, button))
            .Should().Equal("Download certificate for PAED-001", "Download certificate for PAED-002");

        DownloadButtons(cut)[0].Click();
        cut.WaitForAssertion(() => DownloadButtons(cut)[0].TextContent.Trim().Should().Be("Generating…"));
        DownloadButtons(cut).Select(button => AccessibleNames.NameOf(cut, button))
            .Should().Equal("Generating…", "Download certificate for PAED-002");

        hold.Release();
        cut.WaitForAssertion(() => DownloadButtons(cut).Select(button => AccessibleNames.NameOf(cut, button))
            .Should().Equal("Download certificate for PAED-001", "Download certificate for PAED-002"));
    }

    /// <summary>
    /// T239. Each of a user's roles had a "Remove" button, and a screen reader read "Remove" for every one. (Trainee has no
    /// Remove since T303: admission grants it and Mark complete takes it away, UserDetailTraineeRoleTests.)
    /// </summary>
    [Fact]
    public void EachRolesRemove_IsNamedByTheRoleItRemoves()
    {
        var cut = UserDetailPage(new Hold(), _ => false, User() with { Roles = [WombatRoles.Coordinator, WombatRoles.Assessor] });

        cut.FindAll("button").Where(button => button.TextContent.Trim() == "Remove")
            .Select(button => AccessibleNames.NameOf(cut, button))
            .Should().Equal("Remove the Coordinator role", "Remove the Assessor role");
    }

    /// <summary>T239. A review stages one decision an EPA, and each staged decision's Remove is named by it.</summary>
    [Fact]
    public void EachStagedDecisionsRemove_IsNamedByItsEpa()
    {
        var cut = ReviewPage(new Hold(), _ => false,
            Review(CommitteeReviewState.InProgress), Review(CommitteeReviewState.InProgress),
            [PendingStar(), PendingStar() with { Id = 10, EpaId = 2, EpaCode = "PAED-002", EpaTitle = "EPA 2" }]);

        cut.FindAll("button").Where(button => button.TextContent.Trim() == "Remove")
            .Select(button => AccessibleNames.NameOf(cut, button))
            .Should().Equal("Remove the staged decision on PAED-001", "Remove the staged decision on PAED-002");
    }

    /// <summary>
    /// T239. A trainee's own requests: Withdraw names the request, and Download names what it downloads, the request's
    /// export, not the request (T239 review).
    /// </summary>
    [Fact]
    public void EachOwnRequestsActions_NameTheRequest_AndDownloadNamesItsExport()
    {
        var withdrawable = new DataRightsRequestSummaryDto(
            RequestId, "Tia Trainee", Created, DataRightsRequestType.Access, DataRightsRequestStatus.Submitted);
        var cut = Page<DataRights>(new Sender(new Hold(), _ => false, request => request switch
        {
            GetObjectionFlagsQuery => new ObjectionFlagsDto(false, false),
            GetMyDataRightsRequestsQuery => new[]
            {
                withdrawable,
                withdrawable with
                {
                    Id = new Guid("6a1f0000-0000-0000-0000-000000000002"),
                    RequestedOn = Created.AddHours(3),
                    Type = DataRightsRequestType.Export,
                    Status = DataRightsRequestStatus.Completed
                }
            },
            _ => null
        }));
        cut.WaitForState(() => cut.FindAll("tbody a.btn").Count == 1);

        cut.FindAll("tbody button.btn, tbody a.btn").Select(control => AccessibleNames.NameOf(cut, control)).Should().Equal(
            "Withdraw the Access request made 2026-01-01 00:00",
            "Download the export of the Export request made 2026-01-01 03:00");
    }

    /// <summary>
    /// T234 review. A confirmed action runs with its dialog open and the focus on the dialog's confirm button, which, like
    /// every button whose own action runs, stays enabled, says it is unavailable, and sends nothing on a second press.
    /// Cancel would race the action, so it is disabled until the action answers.
    /// </summary>
    [Fact]
    public void WhileAConfirmedActionRuns_TheConfirmButtonStaysEnabled_SaysItIsUnavailable_AndASecondPressSendsNothing()
    {
        var hold = new Hold();
        var cancelled = 0;
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.Title, "Deactivate this EPA?")
            .Add(dialog => dialog.ConfirmLabel, "Deactivate")
            .Add(dialog => dialog.OnConfirm, () => hold.Next())
            .Add(dialog => dialog.OnCancel, () => cancelled++));

        IElement Confirm() => cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == "Deactivate");
        IElement Cancel() => cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == "Cancel");
        Confirm().HasAttribute("aria-disabled").Should().BeFalse();

        Confirm().Click();
        cut.WaitForAssertion(() => hold.Count.Should().Be(1));

        Confirm().HasAttribute("disabled").Should().BeFalse("it has the focus");
        Confirm().GetAttribute("aria-disabled").Should().Be("true");
        Cancel().HasAttribute("disabled").Should().BeTrue("closing the dialog would race the action");

        Confirm().Click();
        hold.Count.Should().Be(1, "a press while the action runs sends nothing");

        hold.Release();

        cut.WaitForAssertion(() => Confirm().HasAttribute("aria-disabled").Should().BeFalse());
        Cancel().HasAttribute("disabled").Should().BeFalse();
        cancelled.Should().Be(0);
        hold.Count.Should().Be(1);
    }

    private static IReadOnlyList<IElement> DownloadButtons(IRenderedFragment cut)
        => cut.FindAll(".detail-card button").ToList();

    private static EntrustmentDecisionDto Decision(int id, string epaCode)
        => new(id, "admin-1", 100 + id, epaCode, $"{epaCode} title", true, 3, "3a", 3, new DateOnly(2026, 7, 1), null, 30,
            "chair-1", "Consistent across the period.", EntrustmentDecisionStatus.Active, null, null, null, null, []);

    // ---- scenarios ----

    /// <param name="Render">Renders the page, ready to act, with its server answering from <paramref name="Render" />'s hold.</param>
    /// <param name="Button">The button the scenario presses.</param>
    /// <param name="Press">Presses it, filling in whatever the action needs first.</param>
    /// <param name="RefusalTakesFocus">A refusal leaves the button disabled, so its result takes the focus too.</param>
    /// <param name="CanBeRefused">The page reports a refusal of this request.</param>
    /// <param name="LeavesFor">Where a done action goes (a clone): the page it leaves for moves the focus, so it moves none.</param>
    private sealed record Scenario(
        Func<ActionFocusTests, Hold, IRenderedFragment> Render,
        Func<IRenderedFragment, IElement> Button,
        Action<IRenderedFragment> Press,
        bool RefusalTakesFocus = false,
        bool CanBeRefused = true,
        string? LeavesFor = null);

    private static readonly IReadOnlyDictionary<string, Scenario> Scenarios = new Dictionary<string, Scenario>
    {
        ["InstitutionEdit Save"] = new(
            (test, hold) => test.Page<InstitutionEdit>(
                new Sender(hold, request => request is UpdateInstitutionCommand, request => request switch
                {
                    GetInstitutionByIdQuery => Institution(),
                    UpdateInstitutionCommand => Institution(),
                    _ => null
                }),
                parameters => parameters.Add(page => page.Id, 4)),
            SubmitButton,
            Submit),

        // T302, T264: Deactivate is offered to those the Administrator policy admits, and asks first. The dialog's confirm
        // button has the focus while the deactivation runs, so it is the button held to the rule; the page's Deactivate,
        // which the deactivation removes, is disabled then, as every button that would race it is.
        ["InstitutionEdit Deactivate"] = new(
            (test, hold) =>
            {
                test._auth.SetPolicies("Administrator");
                return test.Page<InstitutionEdit>(
                    new Sender(hold, request => request is DeactivateInstitutionCommand, request => request switch
                    {
                        GetInstitutionByIdQuery => Institution(),
                        _ => null
                    }),
                    parameters => parameters.Add(page => page.Id, 4));
            },
            cut => cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == "Deactivate"),
            cut =>
            {
                if (Named(cut, "Deactivate") is { } opener && !opener.HasAttribute("disabled"))
                {
                    opener.Click();
                }

                cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == "Deactivate").Click();
            }),

        ["SpecialityEdit Save"] = new(
            (test, hold) => test.Page<SpecialityEdit>(
                new Sender(hold, request => request is UpdateSpecialityCommand, request => request switch
                {
                    GetCollegeByIdQuery => College(),
                    GetSpecialityByIdQuery => Speciality(),
                    UpdateSpecialityCommand => Speciality(),
                    _ => null
                }),
                parameters => parameters.Add(page => page.CollegeId, 1).Add(page => page.Id, 5)),
            SubmitButton,
            Submit),

        ["SubSpecialityEdit Save"] = new(
            (test, hold) => test.Page<SubSpecialityEdit>(
                new Sender(hold, request => request is UpdateSubSpecialityCommand, request => request switch
                {
                    GetSpecialityByIdQuery => Speciality(),
                    GetSubSpecialitiesForSpecialityQuery => new[] { SubSpeciality() },
                    UpdateSubSpecialityCommand => SubSpeciality(),
                    _ => null
                }),
                parameters => parameters.Add(page => page.SpecialityId, 5).Add(page => page.Id, 9)),
            SubmitButton,
            Submit),

        ["CollegeEdit Save"] = new(
            (test, hold) => test.Page<CollegeEdit>(
                new Sender(hold, request => request is UpdateCollegeCommand, request => request switch
                {
                    GetCollegeByIdQuery => College(),
                    UpdateCollegeCommand => College(),
                    _ => null
                }),
                parameters => parameters.Add(page => page.Id, 1)),
            SubmitButton,
            Submit),

        ["CollegeEdit Deactivate"] = new(
            (test, hold) => test.Page<CollegeEdit>(
                new Sender(hold, request => request is DeactivateCollegeCommand, request => request switch
                {
                    GetCollegeByIdQuery => College(),
                    _ => null
                }),
                parameters => parameters.Add(page => page.Id, 1)),
            cut => Named(cut, "Deactivate"),
            cut => Named(cut, "Deactivate").Click()),

        ["CurriculumEdit Save"] = new(
            (test, hold) => test.Page<CurriculumEdit>(
                new Sender(hold, request => request is UpdateCurriculumCommand, request => request switch
                {
                    GetCurriculumByIdQuery or UpdateCurriculumCommand => Curriculum(),
                    _ => null
                }),
                parameters => parameters.Add(page => page.Id, 2)),
            cut => cut.FindAll("form").First().QuerySelector("button[type=submit]")!,
            cut => cut.FindAll("form").First().Submit()),

        ["EntrustmentScaleEdit Save"] = new(
            (test, hold) => test.Page<EntrustmentScaleEdit>(
                new Sender(hold, request => request is UpdateEntrustmentScaleCommand, request => request switch
                {
                    GetEntrustmentScaleByIdQuery => Scale(),
                    // T253: a save answers with the scale and any rename warning.
                    UpdateEntrustmentScaleCommand => new UpdateEntrustmentScaleResult(Scale(), null),
                    _ => null
                }),
                parameters => parameters.Add(page => page.Id, 3)),
            cut => Named(cut, "Save", "Saving..."),
            cut => Named(cut, "Save", "Saving...").Click()),

        ["AssessorProfileEdit Save"] = new(
            (test, hold) => test.Page<AssessorProfileEdit>(
                new Sender(hold, request => request is CreateOrUpdateAssessorProfileCommand, request => request switch
                {
                    GetAssessorProfileByIdQuery or CreateOrUpdateAssessorProfileCommand => AssessorProfile(),
                    _ => null
                }),
                query: ("id", 6)),
            SubmitButton,
            Submit),

        // Change password (/account/change-password), Save profile on My account (/account/profile) and Verify on the export
        // check (/portfolio/verify) are not here: since T265 (My account since the review of the t335 branch) each is a form
        // the browser sends, and its result arrives with the page it loads (ActionResult.FocusOnLoad).
        // Account/ChangePasswordPageTests, Account/ProfilePageTests and Portfolio/VerifyExportPageTests hold them to the rule.

        ["AdoptionsList Adopt"] = new(
            (test, hold) =>
            {
                // An InstitutionalAdmin's own institution is chosen for them.
                test.SignIn(WombatRoles.InstitutionalAdmin, new Claim(WombatClaimTypes.InstitutionId, "4"));
                return test.Page<AdoptionsList>(
                    new Sender(hold, request => request is AdoptCurriculumCommand, request => request switch
                    {
                        GetAdoptableCurriculaQuery => new[]
                        {
                            new AdoptableCurriculumDto(3, 9, "CMSA", "Paediatrics", "Neonatology", "Neonatal curriculum", "v11.1")
                        },
                        _ => null
                    }));
            },
            cut => Named(cut, "Adopt", "Adopting..."),
            cut =>
            {
                cut.Find("#adoption-curriculum").Change("3");
                Named(cut, "Adopt", "Adopting...").Click();
            }),

        ["InvitationsList Issue invitation"] = new(
            (test, hold) =>
            {
                test.Services.AddSingleton(Options.Create(new WombatOptions { BaseUrl = "https://wombat.test" }));
                return test.Page<InvitationsList>(
                    new Sender(hold, request => request is IssueInvitationCommand, request => request switch
                    {
                        IssueInvitationCommand => new IssuedInvitationResult(1, "token-1"),
                        _ => null
                    }));
            },
            SubmitButton,
            cut =>
            {
                cut.Find("#invitation-email").Change("registrar@hospital.test");
                Submit(cut);
            }),

        ["GroupMappings Add mapping"] = new(
            (test, hold) =>
            {
                test.Services.AddSingleton(Options.Create(new SsoOptions
                {
                    Providers = [new SsoProviderOptions { Key = "hospital", DisplayName = "Hospital sign-in", InstitutionId = 4 }]
                }));
                return test.Page<GroupMappings>(
                    new Sender(hold, request => request is CreateSsoGroupMappingCommand, request => request switch
                    {
                        GetInstitutionsListQuery => new[] { Institution() },
                        _ => null
                    }));
            },
            SubmitButton,
            cut =>
            {
                cut.Find("#mapping-group-id").Change("group-1");
                cut.Find("#mapping-group-name").Change("Registrars");
                cut.Find("#mapping-institution").Change("4");
                Submit(cut);
            }),

        ["UserDetail Lock out user"] = new(
            (test, hold) => test.UserDetailPage(hold, request => request is SetUserLockoutCommand),
            cut => Named(cut, "Lock out user"),
            cut => Named(cut, "Lock out user").Click()),

        ["UserDetail Add role"] = new(
            (test, hold) => test.UserDetailPage(hold, request => request is AddRoleToUserCommand),
            cut => Named(cut, "Add role"),
            cut =>
            {
                if (cut.Find("#add-role-select").GetAttribute("value") != WombatRoles.Assessor)
                {
                    cut.Find("#add-role-select").Change(WombatRoles.Assessor);
                }

                Named(cut, "Add role").Click();
            }),

        ["UserDetail Reset password"] = new(
            (test, hold) => test.UserDetailPage(hold, request => request is ResetUserPasswordCommand),
            cut => Named(cut, "Reset password"),
            cut =>
            {
                cut.Find("#reset-password-input").Input("A-long-password-1");
                Named(cut, "Reset password").Click();
            },
            // The password is cleared once the reset has answered, which disables Reset password.
            RefusalTakesFocus: true),

        ["DataRights Save preferences"] = new(
            (test, hold) => test.Page<DataRights>(
                new Sender(hold, request => request is UpdateObjectionFlagsCommand, request => request switch
                {
                    GetObjectionFlagsQuery => new ObjectionFlagsDto(false, false),
                    _ => null
                })),
            cut => Named(cut, "Save preferences", "Saving..."),
            cut => Named(cut, "Save preferences", "Saving...").Click()),

        ["DataRights Submit request"] = new(
            (test, hold) => test.Page<DataRights>(
                new Sender(hold, request => request is SubmitDataRightsRequestCommand, request => request switch
                {
                    GetObjectionFlagsQuery => new ObjectionFlagsDto(false, false),
                    SubmitDataRightsRequestCommand => DataRightsRequest(DataRightsRequestStatus.Submitted),
                    _ => null
                })),
            cut => Named(cut, "Submit request", "Submitting..."),
            cut =>
            {
                cut.Find("#dr-reason").Change("I would like a copy of my data.");
                Named(cut, "Submit request", "Submitting...").Click();
            }),

        ["RequestDetail Approve"] = new(
            (test, hold) => test.Page<RequestDetail>(
                new Sender(hold, request => request is ApproveDataRightsRequestCommand, request => request switch
                {
                    GetDataRightsRequestByIdQuery => DataRightsRequest(DataRightsRequestStatus.Submitted),
                    ApproveDataRightsRequestCommand => DataRightsRequest(DataRightsRequestStatus.Approved),
                    _ => null
                }),
                parameters => parameters.Add(page => page.Id, RequestId)),
            cut => Named(cut, "Approve", "Processing..."),
            cut =>
            {
                cut.Find("#decision-note").Change("Identity confirmed.");
                Named(cut, "Approve", "Processing...").Click();
            }),

        ["ScheduledJobsList Disable"] = new(
            (test, hold) => test.Page<ScheduledJobsList>(
                new Sender(hold, request => request is DisableScheduledJobCommand, request => request switch
                {
                    GetScheduledJobStatusQuery => new[]
                    {
                        new ScheduledJobDto(1, "digest", "0 6 * * *", true, "Sends the daily digest.", null, null, null)
                    },
                    _ => null
                })),
            cut => Named(cut, "Disable"),
            cut => Named(cut, "Disable").Click()),

        ["ExportPortfolio Export PDF"] = new(
            (test, hold) => test.Page<ExportPortfolio>(
                new Sender(hold, request => request is ExportPortfolioCommand, request => request switch
                {
                    ExportPortfolioCommand => new PortfolioExportResult([1, 2, 3], "portfolio.pdf", "hash-1"),
                    _ => null
                }),
                parameters => parameters.Add(page => page.TraineeUserId, "trainee-1")),
            cut => Named(cut, "Export PDF", "Generating..."),
            cut => Named(cut, "Export PDF", "Generating...").Click()),

        ["ActivityTypeEdit Save draft"] = new(
            (test, hold) => test.ActivityTypePage(hold, request => request is SaveActivityTypeDraftCommand, hasDraft: false),
            cut => Named(cut, "Save draft"),
            cut => Named(cut, "Save draft").Click()),

        ["ActivityTypeEdit Discard draft"] = new(
            (test, hold) => test.ActivityTypePage(hold, request => request is DiscardActivityTypeDraftCommand, hasDraft: true),
            cut => Named(cut, "Discard draft"),
            cut => Named(cut, "Discard draft").Click()),

        ["ActivityTypeEdit Publish"] = new(
            (test, hold) => test.ActivityTypePage(hold, request => request is PublishActivityTypeDraftCommand, hasDraft: true),
            cut => Named(cut, "Publish"),
            cut => Named(cut, "Publish").Click()),

        ["CurriculumProgressRebuild Rebuild progress"] = new(
            (test, hold) => test.Page<CurriculumProgressRebuild>(
                new Sender(hold, request => request is RebuildCommand, request => request switch
                {
                    RebuildCommand => new RebuildResult(6, 7, 4, 2, 5),
                    _ => null
                })),
            cut => cut.FindAll(".form-container button").Single(button => button.Closest("dialog") is null),
            cut =>
            {
                cut.FindAll(".form-container button").Single(button => button.Closest("dialog") is null).Click();
                cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == "Rebuild progress").Click();
            }),

        // ---- T234 review: the actions reported fixed that no scenario held to the rule ----

        ["SpecialityEdit Deactivate"] = new(
            (test, hold) => test.Page<SpecialityEdit>(
                new Sender(hold, request => request is DeactivateSpecialityCommand, request => request switch
                {
                    GetCollegeByIdQuery => College(),
                    GetSpecialityByIdQuery => Speciality(),
                    _ => null
                }),
                parameters => parameters.Add(page => page.CollegeId, 1).Add(page => page.Id, 5)),
            cut => Named(cut, "Deactivate"),
            cut => Named(cut, "Deactivate").Click()),

        ["SubSpecialityEdit Deactivate"] = new(
            (test, hold) => test.Page<SubSpecialityEdit>(
                new Sender(hold, request => request is DeactivateSubSpecialityCommand, request => request switch
                {
                    GetSpecialityByIdQuery => Speciality(),
                    GetSubSpecialitiesForSpecialityQuery => new[] { SubSpeciality() },
                    _ => null
                }),
                parameters => parameters.Add(page => page.SpecialityId, 5).Add(page => page.Id, 9)),
            cut => Named(cut, "Deactivate"),
            cut => Named(cut, "Deactivate").Click()),

        ["CurriculumEdit Clone"] = new(
            (test, hold) => test.Page<CurriculumEdit>(
                new Sender(hold, request => request is CloneCurriculumAsNewVersionCommand, request => request switch
                {
                    GetCurriculumByIdQuery => Curriculum(),
                    CloneCurriculumAsNewVersionCommand => Curriculum() with { Id = 7 },
                    _ => null
                }),
                parameters => parameters.Add(page => page.Id, 2)),
            cut => cut.Find("#clone-version").Closest("form")!.QuerySelector("button[type=submit]")!,
            cut =>
            {
                cut.Find("#clone-version").Change("v12");
                cut.Find("#clone-version").Closest("form")!.Submit();
            },
            LeavesFor: "/admin/curricula/7"),

        ["InvitationsList Revoke"] = new(
            (test, hold) =>
            {
                test.Services.AddSingleton(Options.Create(new WombatOptions { BaseUrl = "https://wombat.test" }));
                return test.Page<InvitationsList>(
                    new Sender(hold, request => request is RevokeInvitationCommand, request => request switch
                    {
                        ListActiveInvitationsQuery => new[]
                        {
                            new ActiveInvitationDto(11, "registrar@hospital.test", WombatRoles.Trainee, 4, "Groote Schuur Hospital",
                                null, null, 5, "Paediatrics", 9, "Neonatology", Created, new DateOnly(2026, 12, 31),
                                InvitationDelivery.Sent, 0, false)
                        },
                        _ => null
                    }));
            },
            cut => Named(cut, "Revoke"),
            cut => Named(cut, "Revoke").Click()),

        // T283: a resend's result (the new link) takes the focus; a refusal that leaves the row its Resend keeps it there.
        ["InvitationsList Resend"] = new(
            (test, hold) =>
            {
                test.Services.AddSingleton(Options.Create(new WombatOptions { BaseUrl = "https://wombat.test" }));
                return test.Page<InvitationsList>(
                    new Sender(hold, request => request is ResendInvitationCommand, request => request switch
                    {
                        ListActiveInvitationsQuery => new[]
                        {
                            new ActiveInvitationDto(11, "registrar@hospital.test", WombatRoles.Trainee, 4, "Groote Schuur Hospital",
                                null, null, 5, "Paediatrics", 9, "Neonatology", Created, new DateOnly(2026, 12, 31),
                                InvitationDelivery.NotDelivered, 1, false)
                        },
                        ResendInvitationCommand => new IssuedInvitationResult(11, "token-2"),
                        _ => null
                    }));
            },
            cut => Named(cut, "Resend"),
            cut => Named(cut, "Resend").Click()),

        ["GroupMappings Delete"] = new(
            (test, hold) =>
            {
                test.Services.AddSingleton(Options.Create(new SsoOptions
                {
                    Providers = [new SsoProviderOptions { Key = "hospital", DisplayName = "Hospital sign-in", InstitutionId = 4 }]
                }));
                return test.Page<GroupMappings>(
                    new Sender(hold, request => request is DeleteSsoGroupMappingCommand, request => request switch
                    {
                        GetInstitutionsListQuery => new[] { Institution() },
                        ListSsoGroupMappingsQuery => new[]
                        {
                            new SsoGroupMappingDto(12, "hospital", "group-1", "Registrars", WombatRoles.Trainee, 4,
                                "Groote Schuur Hospital", null, null, null, null)
                        },
                        _ => null
                    }));
            },
            cut => Named(cut, "Delete"),
            cut => Named(cut, "Delete").Click()),

        ["DataRights Withdraw"] = new(
            (test, hold) => test.Page<DataRights>(
                new Sender(hold, request => request is WithdrawDataRightsRequestCommand, request => request switch
                {
                    GetObjectionFlagsQuery => new ObjectionFlagsDto(false, false),
                    GetMyDataRightsRequestsQuery => new[]
                    {
                        new DataRightsRequestSummaryDto(
                            RequestId, "Tia Trainee", Created, DataRightsRequestType.Access, DataRightsRequestStatus.Submitted)
                    },
                    _ => null
                })),
            cut => Named(cut, "Withdraw"),
            cut => Named(cut, "Withdraw").Click()),

        ["RequestDetail Reject"] = new(
            (test, hold) => test.Page<RequestDetail>(
                new Sender(hold, request => request is RejectDataRightsRequestCommand, request => request switch
                {
                    GetDataRightsRequestByIdQuery => DataRightsRequest(DataRightsRequestStatus.Submitted),
                    RejectDataRightsRequestCommand => DataRightsRequest(DataRightsRequestStatus.Rejected),
                    _ => null
                }),
                parameters => parameters.Add(page => page.Id, RequestId)),
            cut => Named(cut, "Reject", "Processing..."),
            cut =>
            {
                cut.Find("#decision-note").Change("Identity not confirmed.");
                Named(cut, "Reject", "Processing...").Click();
            }),

        ["UserDetail Remove"] = new(
            (test, hold) => test.UserDetailPage(hold, request => request is RemoveRoleFromUserCommand),
            cut => Named(cut, "Remove"),
            cut => Named(cut, "Remove").Click()),

        ["UserDetail Reactivate user"] = new(
            (test, hold) => test.UserDetailPage(hold, request => request is SetUserLockoutCommand, User() with { IsLockedOut = true }),
            cut => Named(cut, "Reactivate user"),
            cut => Named(cut, "Reactivate user").Click()),

        ["UserDetail Revoke all pending invitations"] = new(
            (test, hold) => test.UserDetailPage(
                hold,
                request => request is RevokePendingInvitationsForEmailCommand,
                User() with
                {
                    PendingInvitations =
                    [
                        new UserPendingInvitationDto(11, WombatRoles.Trainee, 4, "Groote Schuur Hospital", 5, "Paediatrics",
                            Created, new DateOnly(2026, 12, 31))
                    ]
                }),
            cut => Named(cut, "Revoke all pending invitations"),
            cut => Named(cut, "Revoke all pending invitations").Click()),

        ["ScheduledJobsList Run now"] = new(
            (test, hold) => test.Page<ScheduledJobsList>(
                new Sender(hold, request => request is RunScheduledJobNowCommand, request => request switch
                {
                    GetScheduledJobStatusQuery => new[]
                    {
                        new ScheduledJobDto(1, "digest", "0 6 * * *", true, "Sends the daily digest.", null, null, null)
                    },
                    _ => null
                })),
            cut => Named(cut, "Run now"),
            cut => Named(cut, "Run now").Click()),

        // ---- T234 review: the committee review page, which the sweep missed ----

        ["ReviewDetail Start review"] = new(
            (test, hold) => test.ReviewPage(hold, request => request is StartCommitteeReviewCommand,
                Review(CommitteeReviewState.Scheduled), Review(CommitteeReviewState.InProgress)),
            cut => Named(cut, "Start review"),
            cut => Named(cut, "Start review").Click()),

        ["ReviewDetail Close review"] = new(
            (test, hold) => test.ReviewPage(hold, request => request is CloseFormativeReviewCommand,
                Review(CommitteeReviewState.InProgress, formative: true), Review(CommitteeReviewState.Final, formative: true)),
            cut => Named(cut, "Close review"),
            cut => Named(cut, "Close review").Click()),

        ["ReviewDetail Record decision"] = new(
            (test, hold) => test.ReviewPage(hold, request => request is RecordCommitteeDecisionCommand,
                Review(CommitteeReviewState.InProgress), Review(CommitteeReviewState.Decided)),
            cut => Named(cut, "Record decision"),
            cut =>
            {
                cut.Find("#decision-rationale").Change("On track.");
                cut.Find("#decision-rationale").Closest("form")!.Submit();
            }),

        ["ReviewDetail Ratify"] = new(
            (test, hold) => test.ReviewPage(hold, request => request is RatifyCommitteeDecisionCommand,
                Review(CommitteeReviewState.Decided), Review(CommitteeReviewState.Ratified)),
            cut => Named(cut, "Ratify"),
            cut => Named(cut, "Ratify").Click()),

        ["ReviewDetail Resolve appeal"] = new(
            (test, hold) => test.ReviewPage(hold, request => request is ResolveAppealCommand,
                Review(CommitteeReviewState.UnderAppeal), Review(CommitteeReviewState.Final)),
            cut => Named(cut, "Resolve appeal"),
            cut =>
            {
                // The Outcome opens on none (T307), so one is chosen, as a resolver must.
                cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Dismissed.ToString());
                cut.Find("#appeal-outcome").Closest("form")!.Submit();
            }),

        ["ReviewDetail Stage pending decision"] = new(
            (test, hold) => test.ReviewPage(hold, request => request is StagePendingEntrustmentDecisionCommand,
                Review(CommitteeReviewState.InProgress), Review(CommitteeReviewState.InProgress)),
            cut => Named(cut, "Stage pending decision"),
            cut =>
            {
                // A staged decision clears the form, which ticks no evidence and so disables this button.
                cut.Find("#pending-epa").Change("1");
                cut.Find("#pending-level").Change("1");
                cut.Find("#pending-rationale").Change("Consistent across the period.");
                cut.Find("#pending-evidence-501").Change(true);
                cut.Find("#pending-epa").Closest("form")!.Submit();
            }),

        ["ReviewDetail Remove"] = new(
            (test, hold) => test.ReviewPage(hold, request => request is RemovePendingEntrustmentDecisionCommand,
                Review(CommitteeReviewState.InProgress), Review(CommitteeReviewState.InProgress), [PendingStar()]),
            cut => Named(cut, "Remove"),
            cut => Named(cut, "Remove").Click()),

        ["ReviewDetail Defer"] = new(
            (test, hold) =>
            {
                // The agenda's Defer opens the deferral's form. Its submit is the button pressed; a done deferral closes it.
                var cut = test.ReviewPage(hold, request => request is DeferAgendaLineCommand,
                    Review(CommitteeReviewState.InProgress), Review(CommitteeReviewState.InProgress));
                Named(cut, "Defer").Click();

                // Opening the form puts the focus in its reason box (T212); what the deferral does comes after.
                cut.WaitForAssertion(() => test.FocusCalls().Should().ContainSingle());
                test.IgnoreFocusSoFar();
                return cut;
            },
            cut => Named(cut, "Defer PAED-001"),
            cut =>
            {
                cut.Find("#deferral-reason").Change("Not observed this semester.");
                cut.Find("#deferral-reason").Closest("form")!.Submit();
            }),

        ["ReviewDetail Reinstate"] = new(
            (test, hold) => test.ReviewPage(hold, request => request is ReinstateAgendaLineCommand,
                Review(CommitteeReviewState.InProgress), Review(CommitteeReviewState.InProgress)),
            cut => Named(cut, "Reinstate"),
            cut => Named(cut, "Reinstate").Click()),
    };

    // ---- rendering ----

    private IRenderedFragment Page<TPage>(
        IScopedSender sender,
        Action<ComponentParameterCollectionBuilder<TPage>>? parameters = null,
        (string Name, object Value)? query = null)
        where TPage : IComponent
    {
        Services.AddSingleton(sender);

        if (query is { } supplied)
        {
            // Read through SupplyParameterFromQuery, which bUnit fills from the navigation manager's URI.
            var navigation = Services.GetRequiredService<NavigationManager>();
            navigation.NavigateTo(navigation.GetUriWithQueryParameter(supplied.Name, supplied.Value.ToString()));
        }

        var cut = RenderComponent<TPage>(parameters ?? (_ => { }));
        cut.WaitForState(() => cut.FindAll(".form-actions button, .form-container button, td button").Count > 0);
        return cut;
    }

    private IRenderedFragment UserDetailPage(Hold hold, Func<object, bool> held, UserDetailDto? user = null)
        => Page<UserDetail>(
            new Sender(hold, held, request => request switch
            {
                GetUserByIdQuery => user ?? User(),
                _ => null
            }),
            parameters => parameters.Add(page => page.UserId, "user-9"));

    /// <remarks>
    /// A committee member, so the one role has a Remove: until T303 the user was a trainee, whose role the page no longer
    /// offers to remove.
    /// </remarks>
    private static UserDetailDto User() => new(
        "user-9", "registrar@hospital.test", "Rene", "Registrar", 4, "Groote Schuur Hospital", [], [],
        [WombatRoles.CommitteeMember], false, []);

    /// <summary>
    /// The committee review page, read by the panel's chair, who is also its appeal body. Each command answers with the
    /// review as it stands afterwards (<paramref name="after" />); the reads after it answer as before.
    /// </summary>
    private IRenderedFragment ReviewPage(
        Hold hold,
        Func<object, bool> held,
        CommitteeReviewDetailDto review,
        CommitteeReviewDetailDto after,
        PendingEntrustmentDecisionDto[]? pending = null)
        => Page<ReviewDetail>(
            new Sender(hold, held, request => request switch
            {
                GetCommitteeReviewByIdQuery => review,
                GetCommitteeAgendaQuery => review.Agenda,
                ListPendingEntrustmentDecisionsForReviewQuery => pending ?? Array.Empty<PendingEntrustmentDecisionDto>(),
                CountMsfCampaignsOutsideSnapshotQuery => MsfCampaignsOutsideSnapshotDto.None,
                ListStarEpaOptionsForReviewQuery => new[] { new StarEpaOptionDto(1, "PAED-001", "EPA 1", null, null) },
                GetEntrustmentScalesListQuery => new[] { Scale() },
                StartCommitteeReviewCommand or CloseFormativeReviewCommand or RecordCommitteeDecisionCommand
                    or RatifyCommitteeDecisionCommand or ResolveAppealCommand => after,
                _ => null
            }),
            parameters => parameters.Add(page => page.ReviewId, 30));

    /// <summary>
    /// The activity type builder, on a type with a draft or without one. Saving leaves a draft; discarding and publishing
    /// leave none, which takes Discard draft away and disables Publish.
    /// </summary>
    private IRenderedFragment ActivityTypePage(Hold hold, Func<object, bool> held, bool hasDraft)
    {
        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
        return Page<ActivityTypeEdit>(
            new Sender(hold, held, request => request switch
            {
                GetActivityTypeEditorQuery => Editor(hasDraft),
                SaveActivityTypeDraftCommand => Editor(hasDraft: true),
                DiscardActivityTypeDraftCommand => Editor(hasDraft: false),
                PublishActivityTypeDraftCommand => Editor(hasDraft: false) with { PublishedVersion = 2 },
                _ => null
            }),
            parameters => parameters.Add(page => page.ActivityTypeId, 4));
    }

    private void SignIn(string role, params Claim[] claims)
    {
        _auth.SetAuthorized("caller@test");
        _auth.SetRoles(role);
        _auth.SetClaims([new Claim(ClaimTypes.NameIdentifier, "admin-1"), .. claims]);
    }

    /// <summary>The focus calls made since the scenario was ready to act.</summary>
    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).Skip(_focusCallsBefore).ToList();

    /// <summary>A scenario whose setup moved the focus (opening a form) counts only what its action does after.</summary>
    private void IgnoreFocusSoFar() => _focusCallsBefore += FocusCalls().Count;

    private static IElement SubmitButton(IRenderedFragment cut) => cut.Find("form button[type=submit]");

    private static void Submit(IRenderedFragment cut) => cut.Find("form button[type=submit]").Closest("form")!.Submit();

    private static IElement Named(IRenderedFragment cut, params string[] labels)
        => cut.FindAll("button").Single(button => button.Closest("dialog") is null
                                                  && labels.Contains(button.TextContent.Trim()));

    // ---- data ----

    private static readonly Guid RequestId = new("6a1f0000-0000-0000-0000-000000000001");

    private static InstitutionDto Institution() => new(4, "Groote Schuur Hospital", "GSH", null, true, Created);

    private static CollegeDto College() => new(1, "College of Paediatricians", "CPSA", null, true, Created);

    private static SpecialityDto Speciality() => new(5, 1, "Paediatrics", null, true);

    private static SubSpecialityDto SubSpeciality() => new(9, 5, "Neonatology", null, true);

    private static CurriculumDto Curriculum() => new(
        2, 5, 9, "Paediatrics", "Neonatology", "CMSA", "Neonatal curriculum", "v11.1", new DateOnly(2026, 1, 1), null,
        true, true, [], null)
    {
        CanEditCurriculum = true
    };

    private static EntrustmentScaleDto Scale() => new(3, "CPSA ladder", null,
    [
        new EntrustmentLevelDto(1, 1, "Observe", null),
        new EntrustmentLevelDto(2, 2, "Direct supervision", null)
    ]);

    private static AssessorProfileDto AssessorProfile() => new(
        6, "assessor-1", "assessor@hospital.test", "Asa", "Assessor", "FCPaed", 4, "Groote Schuur Hospital", null, null,
        null, null, AssessorTrainingStatus.Trained, null);

    private const string TypeSchemaJson = """
        { "version": 1, "sections": [ { "key": "s", "title": "Details", "fields": [ { "key": "notes", "type": "text", "label": "Notes" } ] } ] }
        """;

    private const string TypeWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [ { "key": "draft", "label": "Draft" }, { "key": "done", "label": "Done", "terminal": true } ],
          "transitions": [ { "key": "submit", "from": "draft", "to": "done", "actor": "subject" } ]
        }
        """;

    private static ActivityTypeEditorDto Editor(bool hasDraft) => new(
        4, "reflection", "Reflection", null, ActivityScope.Global, null, true, null, 1, hasDraft,
        TypeSchemaJson, TypeWorkflowJson, "{}", "[]", TypeSchemaJson, TypeWorkflowJson, "{}", "[]", "admin-1", null, null, [], true, [new ActivityTypeScopeChoiceDto(ActivityScope.Global, [])], null);

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state, bool formative = false)
        => new CommitteeReviewDetailDto(
            30,
            "trainee-1",
            20,
            "Paediatrics CCC",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2),
            state,
            Created,
            "chair-1",
            null,
            null,
            null,
            state is CommitteeReviewState.Scheduled or CommitteeReviewState.InProgress || formative ? [] : [CommitteeDecision()],
            state == CommitteeReviewState.UnderAppeal
                ? [new CommitteeAppealDto(1, Created, "trainee-1", "Unfair.", null, null, null)]
                : [],
            [SnapshotLine(501, 1)])
        {
            AcademicYear = 2026,
            Semester = 1,
            IsFormative = formative,
            CallerChairs = true,
            CallerMayStart = true,
            CallerResolvesAppeals = true,
            TraineeName = "Lerato Molefe",
            Agenda = new CommitteeAgendaDto(
                30, 2026, 1, "2026 S1", formative,
                formative
                    ? []
                    :
                    [
                        AgendaLine(1, "PAED-001", CommitteeAgendaLineStatus.Due, CommitteeAgendaLineState.Due),
                        AgendaLine(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred)
                    ],
                []),
            PanelMembers =
            [
                new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu", MaySit = true },
                new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member) { Name = "Priya Naidoo", MaySit = true }
            ]
        };

    private static CommitteeDecisionDto CommitteeDecision()
        => new(41, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, Created, "chair-1", null);

    private static CommitteeAgendaLineDto AgendaLine(
        int epaId, string code, CommitteeAgendaLineStatus status, CommitteeAgendaLineState state)
        => new(
            100 + epaId, epaId, code, $"EPA {epaId}", CommitteeAgendaLineOrigin.Cadence, 2026, 1, "2026 S1",
            status == CommitteeAgendaLineStatus.Due, false, state, status, false, false,
            state == CommitteeAgendaLineState.Deferred ? "Later." : null, null, 1);

    private static CommitteeEvidenceDto SnapshotLine(int id, int epaId)
        => new(
            id, CommitteeEvidenceSourceType.Activity, id, null, null, $"Mini-CEX #{id}", "State: completed.", Created,
            EpaId: epaId, EpaCode: $"PAED-00{epaId}", EpaTitle: $"EPA {epaId}", InstrumentName: "Mini-CEX",
            ObservedOn: new DateOnly(2026, 2, 10), ObservedOnDeclared: true, SourceState: "completed", SourceFinished: true);

    private static PendingEntrustmentDecisionDto PendingStar()
        => new(9, 30, 1, "PAED-001", "EPA 1", 1, "Observe", new DateOnly(2026, 7, 2), null, "Ready.", [501], Created, "chair-1");

    private static DataRightsRequestDto DataRightsRequest(DataRightsRequestStatus status) => new(
        RequestId, "trainee-1", "Tia Trainee", Created, DataRightsRequestType.Access, status, "A copy, please.",
        status == DataRightsRequestStatus.Submitted ? null : "Identity confirmed.", null, null, null);

    // ---- the server ----

    /// <summary>
    /// Holds each request <c>held</c> picks until the test releases or refuses it, and counts how many it was sent: the
    /// window a second press falls into. Every request is answered from <c>answer</c>; a list it does not name is empty.
    /// </summary>
    private sealed class Sender(Hold hold, Func<object, bool> held, Func<object, object?> answer) : IScopedSender
    {
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (held(request))
            {
                await hold.Next();
            }

            return As<TResponse>(answer(request));
        }

        public async Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (held(request))
            {
                await hold.Next();
            }
        }

        private static T As<T>(object? value)
        {
            if (value is T typed)
            {
                return typed;
            }

            if (value is not null)
            {
                throw new InvalidCastException($"{value.GetType().Name} answers a request for {typeof(T).Name}.");
            }

            var type = typeof(T);
            if (type.IsInterface && type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                return (T)(object)Array.CreateInstance(type.GetGenericArguments()[0], 0);
            }

            return default!;
        }
    }

    /// <summary>The request the test holds open, released or refused when the test says.</summary>
    private sealed class Hold
    {
        private TaskCompletionSource? _pending;

        /// <summary>How many held requests the page has sent.</summary>
        public int Count { get; private set; }

        public Task Next()
        {
            Count++;
            _pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _pending.Task;
        }

        public void Release() => Pending.SetResult();

        public void Refuse() => Pending.SetException(new InvalidOperationException(Refusal));

        private TaskCompletionSource Pending => _pending ?? throw new InvalidOperationException("Nothing was sent.");
    }
}
