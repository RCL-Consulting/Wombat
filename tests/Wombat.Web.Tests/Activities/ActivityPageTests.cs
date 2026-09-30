using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.SaveActivityDraft;
using Wombat.Application.Features.Activities.Commands.TransitionActivity;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.ListWaitingForYou;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T342, flow 03 (R3-C-Activity, R3-Spec § 1 and § 2): the activity page's status card for each holder, File it again,
/// the Cancel dialogs' words, Save draft, the refusal summary, the history's times and notes, and one page for an activity
/// that does not exist and one the reader may not open. On the seeded Mini-CEX's own form and workflow.
/// </summary>
public sealed class ActivityPageTests : TestContext
{
    private const string Subject = "trainee-1";
    private const string Assessor = "assessor-1";

    // 2026-09-29 07:12 UTC is 09:12 in South Africa.
    private static readonly DateTime Created = new(2026, 9, 29, 7, 10, 0, DateTimeKind.Utc);
    private static readonly DateTime Submitted = new(2026, 9, 29, 7, 12, 0, DateTimeKind.Utc);
    private static readonly DateTime Moved = new(2026, 9, 29, 9, 5, 0, DateTimeKind.Utc);

    private readonly TestAuthorizationContext _auth;
    private readonly RungStub _reference = new();

    public ActivityPageTests()
    {
        _auth = this.AddTestAuthorization();
        SignIn(Subject);
        Services.AddSingleton<IActivityReferenceDataService>(_reference);
        Services.AddScoped<ActivityNotices>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ---- the status card, one holder at a time ----

    [Fact]
    public void TheAuthorsDraft_IsWithThem_AndItsCardSaysItIsInNobodysInbox()
    {
        var cut = Render(new PageSender(Draft()));

        var card = cut.Find(".activity-status");
        card.ClassList.Should().Contain("detail-card--emphasis", "the viewer's move takes the emphasis stripe (T6)");
        card.QuerySelector(".activity-status-label")!.TextContent.Should().Be("Who has it now");
        card.QuerySelector(".badge")!.TextContent.Trim().Should().Be("Draft");
        Text(card, ".activity-status-headline").Should().Be("With you. Not submitted yet.");
        Text(card, ".activity-status-body").Should().Be("Finish the request and submit it. It is in nobody's inbox until you submit it.");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be("Sipho Ndlovu's request to Fatima Khumalo");
    }

    [Fact]
    public void ARequestHeldByItsAssessor_SaysSinceWhenInSouthAfricanTime_AndOffersTheQuietCancel()
    {
        var cut = Render(new PageSender(Requested()));

        var card = cut.Find(".activity-status");
        card.ClassList.Should().Contain("activity-status--others");
        Text(card, ".activity-status-headline").Should().Be("With Fatima Khumalo since 2026-09-29 09:12 SAST.");
        Text(card, ".activity-status-body").Should().Be("Nothing for you to do. You can cancel the request until Fatima Khumalo acts on it.");
        var cancel = card.QuerySelector(".activity-status-action button")!;
        cancel.TextContent.Trim().Should().Be("Cancel request…");
        cancel.ClassList.Should().Contain("btn-quiet");
        cut.FindAll(".form-actions--moves button").Should().BeEmpty("the one action is on the card while someone else holds it");
    }

    [Fact]
    public void ARequestToTheViewer_IsTheirMove_NamingWhoAskedAndWhen()
    {
        SignIn(Assessor);
        var cut = Render(new PageSender(Requested(
            holder: new ActivityHolderDto(ActivityHolderKind.Person, Assessor, "Fatima Khumalo", true, Submitted),
            actions: [new ActivityActionDto("complete", false), new ActivityActionDto("decline", true)])));

        var card = cut.Find(".activity-status");
        card.ClassList.Should().Contain("detail-card--emphasis");
        Text(card, ".activity-status-headline").Should().Be("Your move. Sipho Ndlovu asked you on 2026-09-29 09:12 SAST.");
        Text(card, ".activity-status-body").Should().Be("Complete it, or decline it with a note Sipho Ndlovu will read.");
    }

    [Fact]
    public void AMoveHeldByNoOnePerson_IsWaitingForItsState()
    {
        var cut = Render(new PageSender(Requested(holder: new ActivityHolderDto(ActivityHolderKind.Waiting, null, null, false, Submitted))));

        Text(cut.Find(".activity-status"), ".activity-status-headline").Should().Be("Waiting for Requested.");
    }

    [Fact]
    public void ADeclinedRequest_QuotesTheReason_AndOffersItsRegistrarFileItAgain()
    {
        var cut = Render(new PageSender(Declined()));

        var card = cut.Find(".activity-status");
        card.ClassList.Should().Contain("activity-status--declined");
        Text(card, ".activity-status-headline").Should().Be("Closed. Fatima Khumalo declined it on 2026-09-29 11:05 SAST.");
        Text(card, ".activity-status-quote p").Should().Be("Not my patient.");
        Text(card, ".activity-status-quote cite").Should().Be("Fatima Khumalo");
        Text(card, ".activity-status-body").Should().Contain("file it again and name someone else");
        var again = card.QuerySelector(".activity-status-action a")!;
        again.TextContent.Trim().Should().Be("File it again, to someone else");
        again.GetAttribute("href").Should().Be("/activities/new?from=7");
    }

    [Fact]
    public void ADeclinedRequest_OffersFileItAgain_OnlyToItsRegistrar()
    {
        // E5: the assessor who declined it reads the same record, and has nothing to file.
        SignIn(Assessor);
        var cut = Render(new PageSender(Declined()));

        cut.FindAll(".activity-status-action").Should().BeEmpty();
        cut.Markup.Should().NotContain("/activities/new?from=");
        Text(cut.Find(".activity-status"), ".activity-status-body").Should().NotContain("file it again");
    }

    [Fact]
    public void ACompletedRequest_SaysWhoCompletedIt_TheRungItRated_AndWhatItCredited()
    {
        var cut = Render(new PageSender(Completed()));

        var card = cut.Find(".activity-status");
        card.ClassList.Should().Contain("activity-status--done");
        Text(card, ".activity-status-headline").Should().Be("Done. David Naidoo completed it on 2026-09-29 11:05 SAST.");
        // The stored order is 5; its rung reads "4" (B12). "1 of 3 this semester" needs a progress read the page does not make.
        Text(card, ".activity-status-body").Should().Be("Rated 4. Credited 1 item to PAED-001.");
        var progress = card.QuerySelector(".activity-status-action a")!;
        progress.TextContent.Trim().Should().Be("Open My progress");
        progress.GetAttribute("href").Should().Be("/portfolio/progress");
    }

    [Fact]
    public void ReturnedWork_IsWithItsAuthor_QuotingTheNote()
    {
        var cut = Render(new PageSender(Returned()));

        var card = cut.Find(".activity-status");
        Text(card, ".activity-status-headline").Should().Be("With you. Sarah Botha returned it on 2026-09-29 11:05 SAST.");
        Text(card, ".activity-status-quote p").Should().Be("Say more about the handover.");
        Text(card, ".activity-status-body").Should().Be("Change your request and submit it again.");
    }

    // ---- About ----

    [Fact]
    public void About_NamesTheNomineeByItsFieldsLabel_TheEpaInFull_AndALateFiling()
    {
        var cut = Render(new PageSender(Requested(daysAfterEncounter: 20)));

        var about = About(cut);
        about["Registrar"].Should().Be("Sipho Ndlovu");
        about["Assessor"].Should().Be("Fatima Khumalo");
        about["EPA"].Should().Be("PAED-002 — Managing common paediatric presentations");
        about["Filed"].Should().Be("2026-09-29, 20 days after the encounter (late)");
        about["Credit"].Should().Be("None until it is completed");
        Text(cut.Find(".activity-status"), ".activity-status-meta").Should().Be("Filed 20 days after the encounter: recorded as late.");
    }

    // ---- Cancel ----

    [Fact]
    public void CancelOnADraft_IsLastInTheBar_AndItsDialogSpeaksOfTheDraft()
    {
        var cut = Render(new PageSender(Draft()));

        cut.FindAll(".activity-moves > .form-actions--moves > button").Last().TextContent.Trim().Should().Be("Cancel this draft…");
        var dialog = cut.Find("dialog");
        dialog.QuerySelector("h2")!.TextContent.Should().Be("Cancel this draft?");
        dialog.QuerySelector("p")!.TextContent.Should().Be("A cancelled draft cannot be reopened, and it credits nothing.");
        dialog.QuerySelectorAll("button").Select(button => button.TextContent.Trim()).Should().Equal("Keep the draft", "Cancel draft");
    }

    [Fact]
    public void CancelOnARequest_AsksFirst_InTheRequestsWords_ThenMovesAndSaysCancelled()
    {
        var sender = new PageSender(Requested(), Cancelled());
        var cut = Render(sender);

        var dialog = cut.Find("dialog");
        dialog.QuerySelector("h2")!.TextContent.Should().Be("Cancel this request?");
        dialog.QuerySelector("p")!.TextContent.Should().Be(
            "It leaves Fatima Khumalo's Activity inbox. A cancelled request cannot be reopened, and it credits nothing.");
        dialog.QuerySelectorAll("button").Select(button => button.TextContent.Trim()).Should().Equal("Keep the request", "Cancel request");

        cut.Find(".activity-status-action button").Click();
        sender.Transitions.Should().BeEmpty("the dialog asks first");
        cut.Find("dialog .btn-danger").Click();

        sender.Transitions.Should().ContainSingle().Which.TransitionKey.Should().Be("cancel");
        cut.WaitForAssertion(() => cut.Find("#activity-result").TextContent.Should().Contain("Cancelled."));
    }

    // ---- moves, Save draft and refusals ----

    [Fact]
    public void ASubmit_SaysWhereItIsNow_AndWhoseInboxItIsIn()
    {
        var sender = new PageSender(Draft(), Requested());
        var cut = Render(sender);

        Button(cut, "Submit to Fatima Khumalo").Click();

        cut.WaitForAssertion(() => cut.Find("#activity-result").TextContent.Should()
            .Contain("Submitted. It is now Requested.").And.Contain("It is in Fatima Khumalo's Activity inbox."));
        cut.Find("#activity-result .alert-success strong").TextContent.Should().Be("Submitted. It is now Requested.");

        // T350: a move made as its author has no way on, and the page reads nothing more for one.
        sender.WaitingReads.Should().Be(0);
        cut.FindAll(".way-on, .way-on-none").Should().BeEmpty();
    }

    [Fact]
    public void TheCheckLine_NamesTheNomineeAndTheStateItWillWaitIn()
    {
        // Encountered today, so the check line has no late clause to add (D6, below).
        var cut = Render(new PageSender(DraftEncountered(0)));

        cut.Find(".submit-check").TextContent.Trim().Should().Be(
            "When you submit: it goes to Fatima Khumalo's Activity inbox and stays Requested until Fatima Khumalo acts on it.");
    }

    [Fact]
    public void SaveDraft_SavesTheEditsWithoutAMove_AndSaysItIsNotSubmitted()
    {
        var sender = new PageSender(Draft(), Draft());
        var cut = Render(sender);

        cut.Find("#presenting_problem-in").Input("Nine-month-old with gastroenteritis");
        Button(cut, "Save draft").Click();

        var save = sender.Saves.Should().ContainSingle().Subject;
        save.ActivityId.Should().Be(7);
        using (var patch = JsonDocument.Parse(save.DataPatchJson))
        {
            patch.RootElement.GetProperty("presenting_problem").GetString().Should().Be("Nine-month-old with gastroenteritis");
        }

        sender.Transitions.Should().BeEmpty("a save is not a move (E3)");
        cut.WaitForAssertion(() => cut.Find("#activity-result").TextContent.Should()
            .Contain("Draft saved. It has not been submitted.").And.Contain("It is in nobody's inbox until you submit it."));
    }

    [Fact]
    public void ASaveRefusedForAField_IsSummarised_WithALinkToItsInput()
    {
        var sender = new PageSender(Draft())
        {
            SaveFailure = new ActivityFieldsRefusedException("Date observed: The date cannot be after today (2026-09-29).", ["observed_on"])
        };
        var cut = Render(sender);

        cut.Find("#presenting_problem-in").Input("Something");
        Button(cut, "Save draft").Click();

        cut.WaitForAssertion(() => cut.Find($"#{RefusalSummary.DefaultId}").Should().NotBeNull());
        var summary = cut.Find($"#{RefusalSummary.DefaultId}");
        summary.TextContent.Should().Contain("Not saved. It is still a draft.").And.Contain("Fix the field below and save again.");
        var link = summary.QuerySelector("a")!;
        link.GetAttribute("href").Should().Be("#observed_on-in");
        link.TextContent.Should().Be("Date observed: The date cannot be after today (2026-09-29).");
    }

    [Fact]
    public void ASubmitRefusedForTwoFields_SaysItIsStillADraft_AndLinksEachField()
    {
        var sender = new PageSender(Draft())
        {
            TransitionFailure = new ActivityFieldsRefusedException(
                "Presenting problem: A value is required. Case complexity: A value is required.",
                ["presenting_problem", "complexity"])
        };
        var cut = Render(sender);

        Button(cut, "Submit to Fatima Khumalo").Click();

        cut.WaitForAssertion(() => cut.Find($"#{RefusalSummary.DefaultId}").Should().NotBeNull());
        var summary = cut.Find($"#{RefusalSummary.DefaultId}");
        summary.GetAttribute("tabindex").Should().Be("-1", "it takes the focus (Spec § 1, Refusal)");
        summary.QuerySelector("strong")!.TextContent.Should().Be("Not submitted. It is still a draft.");
        summary.TextContent.Should().Contain("Fix the 2 fields below and submit again.");
        summary.QuerySelectorAll("a").Select(link => (link.GetAttribute("href"), link.TextContent)).Should().Equal(
            ("#presenting_problem-in", "Presenting problem: A value is required."),
            ("#complexity-in", "Case complexity: A value is required."));
    }

    [Fact]
    public void ARefusedSubmit_ShowsEachFieldItsOwnPartOfTheRefusal_UnderItsControl()
    {
        // The page hands the form the refusal's words (RefusalMessage), as Log an activity does, so the field says what is
        // wrong with it and not only the summary (T342, C8; the integration of lanes B and C).
        var sender = new PageSender(Draft())
        {
            TransitionFailure = new ActivityFieldsRefusedException(
                "Presenting problem: A value is required. Case complexity: A value is required.",
                ["presenting_problem", "complexity"])
        };
        var cut = Render(sender);

        Button(cut, "Submit to Fatima Khumalo").Click();

        cut.WaitForAssertion(() => cut.Find("#presenting_problem-msg").TextContent.Trim().Should().Be("A value is required."));
        cut.Find("#presenting_problem-in").GetAttribute("aria-describedby").Should().Contain(RefusalSummary.DefaultId);
    }

    [Fact]
    public void AFilledSection_SaysWhoFilledItIn_AndOnWhatDay()
    {
        // The mover who left the state the section could be filled in: the registrar's submit for the request, the
        // assessor's complete for the entrustment; the day on the South African calendar (Spec § 2). The feedback holds
        // nothing here, so it is a locked section, which names who was to fill it in instead.
        var cut = Render(new PageSender(Completed()));

        string Owner(string sectionKey)
            => cut.Find($"#{ActivityFieldIds.Section(sectionKey)}").ParentElement!.QuerySelector(".form-section-owner")!.TextContent.Trim();

        Owner("request").Should().Be("Filled in by Sipho Ndlovu, 2026-09-29");
        Owner("assessment").Should().Be("Filled in by David Naidoo, 2026-09-29");
        Owner("feedback").Should().Be("David Naidoo was to fill this in.");
    }

    [Fact]
    public void ACancelledRequest_NamesWhoFilledInTheRequest_AndNobodyForTheAssessorsPart()
    {
        var model = ActivityPageModel.From(Cancelled(), Subject);

        model.SectionAttributions().Should().Equal(new Dictionary<string, string>
        {
            ["request"] = "Filled in by Sipho Ndlovu, 2026-09-29"
        });
    }

    // ---- the history ----

    [Fact]
    public void TheHistory_IsOldestFirst_InSouthAfricanTime_WithTheCreateRowFromNothing_AndANoteOnItsOwnRow()
    {
        var cut = Render(new PageSender(Declined()));

        var rows = cut.FindAll("table.history-table tbody tr").ToList();
        rows.Should().HaveCount(4, "three moves and the decline's note");
        rows[0].QuerySelectorAll("td").Select(cell => cell.TextContent.Trim()).Take(4).Should().Equal(
            "Create", "— → Draft", "Sipho Ndlovu", "2026-09-29 09:10 SAST");
        rows[1].QuerySelectorAll("td")[3].TextContent.Trim().Should().Be("2026-09-29 09:12 SAST");
        var note = rows[3].QuerySelector("td")!;
        note.GetAttribute("colspan").Should().Be("5");
        note.TextContent.Trim().Should().Be("Note: Not my patient.");

        cut.Find(".history-details summary").TextContent.Trim().Should().Be("All 3 moves");
        cut.FindAll(".history-details summary svg.history-details-marker").Should().ContainSingle(
            "the flex summary draws its own disclosure marker (A7)");
        cut.Find(".history-zone").TextContent.Should().Be("Times are South African time.");
    }

    // ---- the page's other states ----

    [Fact]
    public void AnActivityThatDoesNotExist_OrCannotBeOpened_IsOnePage_ActivityUnavailable()
    {
        // C7, T101: the query answers null for both, and the page never tells them apart.
        var cut = Render(new PageSender((ActivityDetailDto?)null), waitFor: "Activity unavailable", acting: Acting(WombatRoles.Trainee));

        cut.Find("h1").TextContent.Trim().Should().Be("Activity unavailable");
        TabTitle.Of(this, cut).Should().Be("Activity unavailable · Wombat");
        cut.Find(".detail-card--empty").TextContent.Should().Contain("This activity does not exist, or you cannot open it.");
        cut.Find(".detail-card--empty a").GetAttribute("href").Should().Be("/activities/mine");
        cut.FindAll(".activity-status").Should().BeEmpty();
    }

    [Fact]
    public void ALoadThatFails_SaysNothingChanged_OffersTryAgain_AndNeverShowsTheExceptionsText()
    {
        var sender = new PageSender(Draft()) { LoadFailure = new InvalidOperationException("Npgsql: connection refused on 10.0.0.4") };
        var cut = Render(sender, waitFor: "Could not load this activity.");

        var alert = cut.Find(".alert-danger");
        alert.GetAttribute("role").Should().Be("alert");
        alert.TextContent.Should().Contain("Could not load this activity. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");
        cut.Find("h1").TextContent.Trim().Should().Be("Activity");

        sender.LoadFailure = null;
        Button(cut, "Try again").Click();
        cut.WaitForAssertion(() => cut.FindAll(".activity-status").Should().ContainSingle());
    }

    [Fact]
    public void TheHeadingIsTheActivitysName_AndSoIsItsTab()
    {
        var cut = Render(new PageSender(Requested()));

        cut.Find("h1").TextContent.Trim().Should().Be("Mini-CEX (Paediatrics) · PAED-002 · 2026-09-09");
        TabTitle.Of(this, cut).Should().Be("Mini-CEX (Paediatrics) · PAED-002 · 2026-09-09 · Wombat");
    }

    // ---- the fix pass (T342 step 6, the build review) ----

    [Fact]
    public void TheCheckLine_OnALateDraft_SaysItWillBeRecordedAsLate()
    {
        // D6: the draft's check line counts as Log an activity's does, from the encounter date and today in South Africa.
        var cut = Render(new PageSender(DraftEncountered(20)));

        cut.Find(".submit-check").TextContent.Trim().Should().Be(
            "When you submit: it goes to Fatima Khumalo's Activity inbox and stays Requested until Fatima Khumalo acts on it. " +
            "Filed today, 20 days after the encounter: it will be recorded as late.");
    }

    [Fact]
    public void AStoredNomineeNoLongerOffered_IsNamedNowhere_NotOnTheButtonTheCheckLineOrTheLockedSections()
    {
        // D1, Step A.6.6: the assessor named on the draft is no longer on the directory's list (locked out, or gone). The
        // server still sends their name as HandsToName and NomineeName; the page names them nowhere the draft is written,
        // as Log an activity names nobody for a value it no longer offers.
        _reference.Nominees = [new ActivityCatalogueOption(Assessor, $"Fatima Khumalo {NomineeNames.NotOnTheListSuffix}")];
        var cut = Render(new PageSender(DraftEncountered(0)));

        cut.FindAll(".activity-moves > .form-actions--moves > button").First().TextContent.Trim().Should().Be("Submit");
        cut.Find(".submit-check").TextContent.Trim().Should().Be(
            "When you submit: it goes to the Activity inbox of the assessor you name, and stays Requested until that assessor acts on it.");
        cut.FindAll(".form-section--locked .form-section-owner").Select(owner => owner.TextContent.Trim())
            .Should().NotBeEmpty().And.OnlyContain(line => line == "The assessor you name fills this in");
        cut.Find(".form-actions--moves").TextContent.Should().NotContain("Fatima Khumalo");
    }

    [Fact]
    public void ARefusalSummarysLink_FocusesItsFieldInPlace_AndNavigatesNowhere()
    {
        // A1: <base href="/"> would make "#observed_on-in" a navigation to Home, losing what was typed. The link keeps its
        // href for a page with no circuit, and pressed, focuses the field by script.
        var sender = new PageSender(Draft())
        {
            TransitionFailure = new ActivityFieldsRefusedException("Date observed: The date cannot be after today (2026-09-29).", ["observed_on"])
        };
        var cut = Render(sender);
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        var before = navigation.Uri;

        Button(cut, "Submit to Fatima Khumalo").Click();
        cut.WaitForAssertion(() => cut.Find($"#{RefusalSummary.DefaultId} a").Should().NotBeNull());
        var link = cut.Find($"#{RefusalSummary.DefaultId} a");
        link.GetAttribute("href").Should().Be("#observed_on-in");
        link.Attributes.Select(attribute => attribute.Name).Should().Contain(
            "blazor:onclick:preventdefault", "the browser must not follow the fragment against <base href=\"/\">");

        link.Click();

        JSInterop.Invocations.Where(call => call.Identifier == PageFocus.FocusByIdIdentifier)
            .Should().ContainSingle().Which.Arguments.Should().Equal("observed_on-in");
        navigation.Uri.Should().Be(before, "the link moves the focus, never the page");
        navigation.History.Should().BeEmpty();
    }

    [Fact]
    public void TryAgain_ThatReads_FocusesTheHeading_AndOneThatFailsAgain_FocusesTheAlert()
    {
        // A3: one StatePanel across the retry, so its answer takes the focus: the h1 (by script, FocusOnNavigate does not
        // run again on the same page), or the alert drawn again.
        var sender = new PageSender(Draft()) { LoadFailure = new InvalidOperationException("down") };
        var cut = Render(sender, waitFor: "Could not load this activity.");

        Button(cut, "Try again").Click();
        cut.WaitForAssertion(() => JSInterop.Invocations.Count(call => call.Identifier == "Blazor._internal.domWrapper.focus").Should().Be(1));
        var alert = cut.FindComponent<ActionResult>().Instance.Element;
        JSInterop.Invocations.Single(call => call.Identifier == "Blazor._internal.domWrapper.focus")
            .Arguments[0].Should().BeOfType<Microsoft.AspNetCore.Components.ElementReference>().Which.Id.Should().Be(alert.Id);
        JSInterop.Invocations.Should().NotContain(call => call.Identifier == PageFocus.FocusHeadingIdentifier);

        sender.LoadFailure = null;
        Button(cut, "Try again").Click();
        cut.WaitForAssertion(() => cut.FindAll(".activity-status").Should().ContainSingle());
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call => call.Identifier == PageFocus.FocusHeadingIdentifier));
        cut.Find("h1").TextContent.Trim().Should().Be("Mini-CEX (Paediatrics) · PAED-002 · 2026-09-09");
    }

    [Fact]
    public void OneHeader_HeadsTheLoadAndTheLoadedPage_ItsTitleChanging_AndTheLoadingLineIsAlwaysThere()
    {
        // A5: the h1 FocusOnNavigate focused on arrival is the one the loaded page shows, not a second header drawn in its
        // place. A6: the loading line's live region is on the page before it has words, and stays after.
        var sender = new PageSender(Draft()) { Gate = new TaskCompletionSource() };
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 7));

        var header = cut.FindComponent<PageHeader>();
        cut.FindComponents<PageHeader>().Should().ContainSingle();
        cut.Find("h1").TextContent.Trim().Should().Be("Loading the activity");
        StatusLine(cut).Should().Be("Loading the activity.");

        sender.Gate.SetResult();
        cut.WaitForState(() => cut.Markup.Contains("Who has it now"));

        cut.FindComponents<PageHeader>().Should().ContainSingle().Which.Should().BeSameAs(header);
        cut.Find("h1").TextContent.Trim().Should().Be("Mini-CEX (Paediatrics) · PAED-002 · 2026-09-09");
        StatusLine(cut).Should().BeEmpty("the region stays, emptied");
    }

    [Fact]
    public void OnArrival_OnlyTheResultIsBold_WhoseInboxItIsIn_IsPlain()
    {
        // D7: Log an activity hands over one message; the page shows it as it shows a move made here.
        Services.AddSingleton<IScopedSender>(new PageSender(Requested()));
        Services.GetRequiredService<ActivityNotices>().Post(
            7, "success", "Submitted. It is now Requested. It is in Fatima Khumalo's Activity inbox.");
        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 7));
        cut.WaitForState(() => cut.Markup.Contains("Who has it now"));

        var result = cut.Find("#activity-result .alert-success");
        result.QuerySelector("strong")!.TextContent.Should().Be("Submitted. It is now Requested.");
        result.TextContent.Trim().Should().EndWith("It is in Fatima Khumalo's Activity inbox.");
    }

    [Fact]
    public void SplitResult_PartsTheResultFromWhoseInboxItIsIn()
    {
        ActivityPageModel.SplitResult("Draft saved. It has not been submitted. It is in nobody's inbox until you submit it.")
            .Should().Be(("Draft saved. It has not been submitted.", "It is in nobody's inbox until you submit it."));
        ActivityPageModel.SplitResult("Logged.").Should().Be(("Logged.", (string?)null));
    }


    // ---- T350, flow 04: the assessor's side (R3-C-Activity; R3-Spec § 1, § 3, § 4; DESIGN.md R3) ----

    [Fact]
    public void AfterACompletion_TheResultSaysWhatIsLeft_AndTheWayOnFollowsItOutsideTheLiveRegion()
    {
        // Q6, C1, C2 (R3-P-completed-next): "Completed. 1 more waits for you.", then the next row, Open the next named for
        // it, and Back to the acting role's list. The activity just moved is left out of the read.
        SignIn(Assessor);
        var sender = new PageSender(ToRate(), Completed()) { Waiting = Waiting(Moved7(), Review9()) };
        var cut = Render(sender, acting: Acting(WombatRoles.Assessor));

        Button(cut, "Complete").Click();

        cut.WaitForAssertion(() => cut.FindAll(".way-on").Should().ContainSingle());
        sender.WaitingReads.Should().Be(1, "one more read, after the move commits");
        var result = cut.Find("#activity-result");
        Words(result).Should().Be("Completed. 1 more waits for you.", "the live region holds the sentence and the count only");
        result.QuerySelector(".alert")!.GetAttribute("role").Should().Be("status");
        result.QuerySelector("strong")!.TextContent.Should().Be("Completed.");
        result.QuerySelectorAll(".way-on, a, button").Should().BeEmpty();

        var wayOn = cut.Find("section.way-on");
        wayOn.GetAttribute("aria-label").Should().Be("The next activity waiting for you");
        result.NextElementSibling!.ClassList.Should().Contain("way-on", "straight after the result, outside it");
        var row = wayOn.QuerySelector("li.needs-you-row")!;
        row.ClassList.Should().Contain("needs-you-row--overdue");
        row.QuerySelector(".activity-link")!.TextContent.Should().Contain("Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-09-29")
            .And.Contain("from Pieter du Plessis");
        row.QuerySelector(".needs-you-badges")!.TextContent.Should().Contain("Awaiting review").And.Contain("Overdue");
        row.QuerySelector(".needs-you-why")!.TextContent.Trim().Should().Be("Waiting 8 days, since 2026-09-22 08:06 SAST.");

        var buttons = wayOn.QuerySelectorAll(".form-actions a").ToList();
        buttons.Select(button => button.TextContent.Trim()).Should().Equal("Open the next", "Back to Activity inbox");
        buttons[0].ClassList.Should().Contain("btn-primary");
        buttons[0].GetAttribute("href").Should().Be("/activities/9");
        buttons[0].GetAttribute("aria-label").Should().Be(
            "Open the next: Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-09-29, from Pieter du Plessis");
        buttons[1].GetAttribute("href").Should().Be("/activities/inbox");

        // C10 d: the result takes the focus; the next Tab is Open the next, the row's own link being out of the tab order.
        wayOn.QuerySelectorAll("a[href]:not([tabindex='-1']), button").First().TextContent.Trim().Should().Be("Open the next");
        row.QuerySelector(".activity-link")!.GetAttribute("tabindex").Should().Be("-1");
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call =>
            call.Identifier == "Blazor._internal.domWrapper.focus" &&
            ((Microsoft.AspNetCore.Components.ElementReference)call.Arguments[0]!).Id == cut.FindComponent<ActionResult>().Instance.Element.Id));
    }

    [Theory]
    [InlineData("Completed.", "Completed. Nothing else waits for you.")]
    [InlineData("Discussed.", "Discussed. Nothing else waits for you.")]
    public void WithNothingLeft_TheResultSaysSo_AndTheWayOnIsGoToHome(string sentence, string expected)
    {
        // C1 (R3-P-completed-last, R3-P-discussed): a move into a final state reads as that state (a Return: the next
        // test). Nothing left: no row, and Go to Home.
        SignIn(Assessor);
        var sender = new PageSender(ToRate(sentence), Completed()) { Waiting = Waiting(Moved7()) };
        var cut = Render(sender, acting: Acting(WombatRoles.Assessor));

        Button(cut, "Complete").Click();

        cut.WaitForAssertion(() => cut.FindAll(".way-on-none").Should().ContainSingle());
        Words(cut.Find("#activity-result")).Should().Be(expected);
        cut.FindAll("section.way-on").Should().BeEmpty();
        var home = cut.Find("p.way-on-none a");
        home.TextContent.Trim().Should().Be("Go to Home");
        home.GetAttribute("href").Should().Be("/");
        cut.Find("#activity-result").NextElementSibling!.ClassList.Should().Contain("way-on-none");
    }

    [Fact]
    public void AReturn_SaysItWentBackToTheRegistrar_ByName_NotTheStateItLeftBehind()
    {
        // T350 build review, D5: "It is now Draft." was the only result that named no act, and "Draft" read as the
        // assessor's own. A Return names the act and the person it went back to; the status card below still says Draft.
        SignIn(Assessor);
        var toReturn = ToRate() with
        {
            AvailableActions = [new ActivityActionDto("return", false) { TargetStateLabel = "Draft", ResultSentence = "It is now Draft." }]
        };
        var sender = new PageSender(toReturn, Returned()) { Waiting = Waiting(Moved7()) };
        var cut = Render(sender, acting: Acting(WombatRoles.Assessor));

        Button(cut, "Return").Click();

        cut.WaitForAssertion(() => cut.FindAll(".way-on-none").Should().ContainSingle());
        Words(cut.Find("#activity-result")).Should().Be("Returned to Sipho Ndlovu. Nothing else waits for you.");
    }

    [Fact]
    public void FromTheOtherRoleLine_ACommitteeMembersWayOn_GoesBackToHome_AndHerTrailIsHomeThenTheActivity()
    {
        // § 8 (R3-P-from-the-line): Dr Zulu, acting as Committee member, opens the Mini-CEX from her Home's line. No list
        // owns the page for her acting role: nothing is lit, the trail is Home › the activity, and Back to Home.
        SignIn(Assessor);
        var zulu = ActingRoleResolver.Resolve(WombatRoles.CommitteeMember, [WombatRoles.CommitteeMember, WombatRoles.Assessor]);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember), new Claim(ClaimTypes.Role, WombatRoles.Assessor)], "Test"));
        NavOwners.TrailTo(typeof(ActivityView), zulu.Role, NavItems.For(zulu, user)).Should().Equal(new Crumb("Home", "/"));
        NavOwners.Lit(typeof(ActivityView), zulu.Role, NavItems.For(zulu, user)).Current.Should().Be(NavCurrent.None);

        var sender = new PageSender(ToRate(), Completed()) { Waiting = Waiting(Moved7(), Review9()) };
        var cut = Render(sender, acting: zulu);
        Button(cut, "Complete").Click();

        cut.WaitForAssertion(() => cut.FindAll(".way-on").Should().ContainSingle());
        var back = cut.FindAll(".way-on .form-actions a").Last();
        back.TextContent.Trim().Should().Be("Back to Home");
        back.GetAttribute("href").Should().Be("/");
    }

    [Fact]
    public void AWayOnReadThatFails_CostsOnlyTheTailAndTheWayOn_NeverTheResult()
    {
        SignIn(Assessor);
        var sender = new PageSender(ToRate(), Completed()) { WaitingFailure = new InvalidOperationException("Npgsql: gone") };
        var cut = Render(sender, acting: Acting(WombatRoles.Assessor));

        Button(cut, "Complete").Click();

        cut.WaitForAssertion(() => Words(cut.Find("#activity-result")).Should().Be("Completed."));
        cut.FindAll(".way-on, .way-on-none").Should().BeEmpty();
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Theory]
    [InlineData(WombatRoles.Assessor, "Go to Activity inbox", "/activities/inbox")]
    [InlineData(WombatRoles.Trainee, "Go to My activities", "/activities/mine")]
    [InlineData(WombatRoles.CommitteeMember, "Go to Home", "/")]
    public void ActivityUnavailable_GoesToTheActingRolesList_ElseHome(string role, string label, string href)
    {
        // Round 1, E4; R3 (R3-P-unavailable).
        var cut = Render(new PageSender((ActivityDetailDto?)null), waitFor: "Activity unavailable", acting: Acting(role));

        var link = cut.Find(".detail-card--empty a");
        link.TextContent.Trim().Should().Be(label);
        link.GetAttribute("href").Should().Be(href);
    }

    [Fact]
    public void AReaderWithASectionToFill_HasAboutTwice_TheSecondUnderTheBarForAPhone_AndTheFormFolds()
    {
        // Round 1, E3; round 2, E2: About for 641px and up where it is, and under the bar below it, its ids suffixed.
        SignIn(Assessor);
        var cut = Render(new PageSender(ToRate()), acting: Acting(WombatRoles.Assessor));

        var abouts = cut.FindAll(".activity-about").ToList();
        abouts.Should().HaveCount(2);
        abouts[0].ClassList.Should().Contain("only-wide").And.NotContain("only-narrow");
        abouts[0].GetAttribute("aria-labelledby").Should().Be("activity-about-title");
        abouts[1].ClassList.Should().Contain("only-narrow").And.NotContain("only-wide");
        abouts[1].GetAttribute("aria-labelledby").Should().Be("activity-about-title-narrow");
        abouts[1].QuerySelector("#activity-about-title-narrow").Should().NotBeNull();
        abouts[1].QuerySelector("#activity-encounter-date-narrow").Should().NotBeNull();
        abouts[1].PreviousElementSibling!.ClassList.Should().Contain("activity-sections", "under the bar");
        cut.FindAll("[id]").GroupBy(element => element.Id).Where(group => group.Count() > 1).Should().BeEmpty("no id twice");
        cut.FindComponent<ActivityForm>().Instance.FoldFilledSections.Should().BeTrue();
    }

    [Fact]
    public void TheAuthorWritingTheirOwnDraft_KeepsFlow03sOrder_AboutOnce_AndNoFold()
    {
        var cut = Render(new PageSender(Draft()), acting: Acting(WombatRoles.Trainee));

        cut.FindAll(".activity-about").Should().ContainSingle().Which.ClassList.Should().NotContain("only-wide");
        cut.FindComponent<ActivityForm>().Instance.FoldFilledSections.Should().BeFalse();
    }

    [Fact]
    public void AnAssessorReadingWhatIsDone_KeepsThePagesOrder_AboutOnce_AndNoFold()
    {
        SignIn(Assessor);
        var cut = Render(new PageSender(Completed()), acting: Acting(WombatRoles.Assessor));

        cut.FindAll(".activity-about").Should().ContainSingle().Which.ClassList.Should().NotContain("only-wide");
        cut.FindComponent<ActivityDetail>().Instance.FoldFilledSections.Should().BeFalse();
    }

    [Fact]
    public void TheStatusCardsRegion_IsNamedByItsLabel_WhoHasItNow()
    {
        // Nit A10: a short name for the region, not the whole headline.
        var cut = Render(new PageSender(Requested()));

        var card = cut.Find(".activity-status");
        var labelledBy = card.GetAttribute("aria-labelledby");
        labelledBy.Should().Be("activity-status-label");
        cut.Find($"#{labelledBy}").TextContent.Trim().Should().Be("Who has it now");
    }

    [Fact]
    public void TheNotePanel_OnThePage_SpeaksOfTheRequest_AndARefusedNoteSaysItIsStillRequested()
    {
        // Note 4 and note 3's wiring (R3-P-decline-note, R3-P-decline-refused): the part is the form's first section's.
        SignIn(Assessor);
        var sender = new PageSender(ToRate()) { TransitionFailure = new InvalidOperationException("Decline requires a note.") };
        var cut = Render(sender, acting: Acting(WombatRoles.Assessor));

        Button(cut, "Decline").Click();
        cut.Find("#note-panel-title").TextContent.Should().Be("Decline this request");
        cut.Find("label[for='note-in']").TextContent.Trim().Should().Be("Note for Sipho Ndlovu *");
        Button(cut, "Keep the request").Should().NotBeNull();

        Button(cut, "Decline with this note").Click();

        cut.WaitForAssertion(() => cut.Find("#note-summary strong").TextContent.Should().Be("Not declined. It is still Requested."));
        cut.Find("#note-summary a").TextContent.Should().Be("Note for Sipho Ndlovu: Decline requires a note.");
    }

    // ---- helpers ----

    private static string StatusLine(IRenderedComponent<ActivityView> cut)
        => cut.FindAll("p[role='status']").Single(line => line.ParentElement!.ClassList.Contains("activity-page")).TextContent.Trim();

    private void SignIn(string userId)
    {
        _auth.SetAuthorized(userId);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private IRenderedComponent<ActivityView> Render(PageSender sender, string waitFor = "Who has it now", ActingRole? acting = null)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ActivityView>(parameters =>
        {
            parameters.Add(page => page.ActivityId, 7);
            if (acting is not null)
            {
                parameters.AddCascadingValue(acting);
            }
        });
        cut.WaitForState(() => cut.Markup.Contains(waitFor));
        return cut;
    }

    private static ActingRole Acting(string role) => ActingRoleResolver.Resolve(role, [role]);

    // An element's words, its whitespace collapsed.
    private static string Words(AngleSharp.Dom.IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private static string Text(AngleSharp.Dom.IElement scope, string selector)
        => scope.QuerySelector(selector)!.TextContent.Trim();

    private static AngleSharp.Dom.IElement Button(IRenderedComponent<ActivityView> cut, string label)
        => cut.FindAll("button").First(button => button.TextContent.Trim() == label);

    private static Dictionary<string, string> About(IRenderedComponent<ActivityView> cut)
        => cut.FindAll(".activity-about .details-list > div").ToDictionary(
            row => row.QuerySelector("dt")!.TextContent.Trim(),
            row => row.QuerySelector("dd")!.TextContent.Trim());

    private static ActivityTransitionDto Row(int id, string from, string to, string key, string fromLabel, string toLabel,
        string actor, string actorName, DateTime at, string? note = null, int? credited = null, int? daysAfter = null)
        => new(id, from, to, key, fromLabel, toLabel, key == "create" ? "Create" : char.ToUpperInvariant(key[0]) + key[1..],
            actor, at, note, "{}", credited, null, daysAfter) { ActorName = actorName };

    private static ActivityTransitionDto CreateRow() => Row(1, "draft", "draft", "create", "Draft", "Draft", Subject, "Sipho Ndlovu", Created);

    private static ActivityTransitionDto SubmitRow(int? daysAfter = null)
        => Row(2, "draft", "requested", "submit", "Draft", "Requested", Subject, "Sipho Ndlovu", Submitted, daysAfter: daysAfter);

    private const string StoredData = """{"epa_id":"12","assessor_user_id":"assessor-1","observed_on":"2026-09-09"}""";

    private static ActivityDetailDto Detail(
        string state,
        string stateLabel,
        ActivityHolderDto holder,
        IReadOnlyList<ActivityTransitionDto> transitions,
        IReadOnlyList<ActivityActionDto> actions,
        IReadOnlyList<string>? editable = null,
        string dataJson = StoredData,
        ActivityReturnDto? returned = null)
    {
        var activity = new ActivityDto(
            7, 2, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", "mini_cex",
            1,
            SeedSchemas.Schema("mini_cex_cpsa"),
            SeedSchemas.Workflow("mini_cex_cpsa"),
            "[]",
            SeedSchemas.CreditRules("mini_cex_cpsa")!,
            Subject, 1, Subject, state, stateLabel, dataJson, 12, null,
            new DateOnly(2026, 9, 9), true, Created, Submitted, transitions);

        return new ActivityDetailDto(activity, editable ?? [], actions)
        {
            Holder = holder,
            Returned = returned,
            NomineeName = "Fatima Khumalo",
            DisplayName = "Mini-CEX (Paediatrics) · PAED-002 · 2026-09-09",
            SubjectName = "Sipho Ndlovu",
            EpaCode = "PAED-002",
            EpaTitle = "Managing common paediatric presentations",
            EpaInForce = true
        };
    }

    private static ActivityActionDto SubmitAction() => new("submit", false)
    {
        HandOffFieldKey = "assessor_user_id",
        HandsToName = "Fatima Khumalo",
        TargetStateLabel = "Requested",
        ResultSentence = "Submitted. It is now Requested."
    };

    private static ActivityActionDto CancelAction() => new("cancel", false)
    {
        TargetStateLabel = "Cancelled",
        ResultSentence = "Cancelled.",
        TargetIsFinal = true
    };

    private static ActivityDetailDto Draft(string dataJson = StoredData)
        => Detail("draft", "Draft", new ActivityHolderDto(ActivityHolderKind.Author, Subject, "Sipho Ndlovu", true, Created),
            [CreateRow()], [SubmitAction(), CancelAction()],
            editable: ["epa_id", "assessor_user_id", "observed_on", "setting", "presenting_problem", "complexity"],
            dataJson: dataJson);

    /// <summary>A draft whose encounter was <paramref name="daysAgo" /> days before today in South Africa.</summary>
    private static ActivityDetailDto DraftEncountered(int daysAgo)
        => Draft($$"""{"epa_id":"12","assessor_user_id":"assessor-1","observed_on":"{{FilingLateness.Today().AddDays(-daysAgo):yyyy-MM-dd}}"}""");

    private static ActivityDetailDto Returned()
        => Detail("draft", "Draft", new ActivityHolderDto(ActivityHolderKind.Author, Subject, "Sipho Ndlovu", true, Moved),
            [CreateRow(), SubmitRow(), Row(3, "requested", "draft", "return", "Requested", "Draft", "sup-1", "Sarah Botha", Moved, "Say more about the handover.")],
            [SubmitAction(), CancelAction()],
            editable: ["presenting_problem"],
            returned: new ActivityReturnDto("sup-1", "Sarah Botha", Moved, "Say more about the handover."));

    private static ActivityDetailDto Requested(
        ActivityHolderDto? holder = null, IReadOnlyList<ActivityActionDto>? actions = null, int? daysAfterEncounter = null)
        => Detail("requested", "Requested",
            holder ?? new ActivityHolderDto(ActivityHolderKind.Person, Assessor, "Fatima Khumalo", false, Submitted),
            [CreateRow(), SubmitRow(daysAfterEncounter)],
            actions ?? [CancelAction()]);

    /// <summary>
    /// The request to its assessor, the viewer (T350): theirs to complete or decline, the Entrustment and Feedback theirs
    /// to write. <paramref name="completeSentence" /> is the Complete move's result sentence.
    /// </summary>
    private static ActivityDetailDto ToRate(string completeSentence = "Completed.")
        => Detail("requested", "Requested",
            new ActivityHolderDto(ActivityHolderKind.Person, Assessor, "Fatima Khumalo", true, Submitted),
            [CreateRow(), SubmitRow()],
            [
                new ActivityActionDto("complete", false) { TargetStateLabel = "Completed", ResultSentence = completeSentence, TargetIsFinal = true },
                new ActivityActionDto("decline", true) { TargetStateLabel = "Declined", ResultSentence = "Declined.", TargetIsFinal = true }
            ],
            editable: ["overall_level", "strengths", "improvements", "plan"]);

    // What waits for the assessor after the move: this activity (7), which the page leaves out, and the others.
    private static WaitingForYouDto Waiting(params ActivitySummaryDto[] items)
        => new(items, items.Count(item => item.IsOverdue), 7);

    private static ActivitySummaryDto Moved7() => ActivityRows.Waiting(7, subjectName: "Sipho Ndlovu");

    // Dr Patel's portfolio review, aged by Step 3.30: waiting 8 days since 2026-09-22 08:06 SAST.
    private static ActivitySummaryDto Review9()
        => ActivityRows.Waiting(
            9, typeName: "Portfolio and Logbook Review (Paediatrics)", subjectName: "Pieter du Plessis", state: "awaiting_review",
            stateLabel: "Awaiting review", waitedDays: 8, overdue: true, since: new DateTime(2026, 9, 22, 6, 6, 0, DateTimeKind.Utc),
            epaCode: "PAED-015", observedOn: new DateOnly(2026, 9, 29));

    private static ActivityDetailDto Declined()
        => Detail("declined", "Declined", new ActivityHolderDto(ActivityHolderKind.Closed, null, null, false, Moved),
            [CreateRow(), SubmitRow(), Row(3, "requested", "declined", "decline", "Requested", "Declined", Assessor, "Fatima Khumalo", Moved, "Not my patient.")],
            []);

    private static ActivityDetailDto Cancelled()
        => Detail("cancelled", "Cancelled", new ActivityHolderDto(ActivityHolderKind.Closed, null, null, false, Moved),
            [CreateRow(), SubmitRow(), Row(3, "requested", "cancelled", "cancel", "Requested", "Cancelled", Subject, "Sipho Ndlovu", Moved)],
            []);

    private static ActivityDetailDto Completed()
        => Detail("completed", "Completed", new ActivityHolderDto(ActivityHolderKind.Done, null, null, false, Moved),
            [CreateRow(), SubmitRow(), Row(3, "requested", "completed", "complete", "Requested", "Completed", "assessor-2", "David Naidoo", Moved, credited: 1)],
            [],
            dataJson: """{"epa_id":"12","assessor_user_id":"assessor-2","observed_on":"2026-09-09","overall_level":"5"}""")
            with { NomineeName = "David Naidoo", EpaCode = "PAED-001" };

    /// <summary>
    /// The CPSA ladder: order 5 reads "4". And the nominee directory, as the server answers it: Fatima Khumalo is offered.
    /// Since D1 an editable form names its nominee from this list, as the picker does, so a stored nominee the directory no
    /// longer offers is not named (<see cref="Nominees" />, "(not on the current list)", C4).
    /// </summary>
    private sealed class RungStub : StubActivityReferenceDataService
    {
        public IReadOnlyList<ActivityCatalogueOption> Nominees { get; set; } =
            [new ActivityCatalogueOption(Assessor, "Fatima Khumalo (fatima@kgk)")];

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetNomineeOptionsAsync(
            NomineeOptionScope scope, CancellationToken cancellationToken = default)
            => Task.FromResult(Nominees);

        public override Task<IReadOnlyList<EntrustmentRung>> GetEntrustmentScaleRungsAsync(
            string? scaleKey, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<EntrustmentRung>>(
            [
                new EntrustmentRung(1, "1", null), new EntrustmentRung(2, "2", null), new EntrustmentRung(3, "3a", null),
                new EntrustmentRung(4, "3b", null), new EntrustmentRung(5, "4", "Rung 4"), new EntrustmentRung(6, "5", null)
            ]);
    }

    private sealed class PageSender : IScopedSender
    {
        private readonly ActivityDetailDto?[] _details;
        private int _loads;

        public PageSender(params ActivityDetailDto?[] details) => _details = details;

        public List<TransitionActivityCommand> Transitions { get; } = [];

        public List<SaveActivityDraftCommand> Saves { get; } = [];

        public Exception? TransitionFailure { get; set; }

        public Exception? SaveFailure { get; set; }

        public Exception? LoadFailure { get; set; }

        /// <summary>What <see cref="ListWaitingForYouQuery" /> answers after a move (T350, the way on).</summary>
        public WaitingForYouDto? Waiting { get; set; }

        public Exception? WaitingFailure { get; set; }

        public int WaitingReads { get; private set; }

        /// <summary>Held, the first load waits for it: the page is seen loading.</summary>
        public TaskCompletionSource? Gate { get; set; }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetActivityByIdQuery && Gate is { } gate)
            {
                await gate.Task;
            }

            return await SendNow(request);
        }

        private Task<TResponse> SendNow<TResponse>(IRequest<TResponse> request)
        {
            switch (request)
            {
                case GetActivityByIdQuery:
                    if (LoadFailure is not null)
                    {
                        throw LoadFailure;
                    }

                    var detail = _details[Math.Min(_loads, _details.Length - 1)];
                    _loads++;
                    return Task.FromResult((TResponse)(object)detail!);
                case TransitionActivityCommand transition:
                    Transitions.Add(transition);
                    if (TransitionFailure is not null)
                    {
                        throw TransitionFailure;
                    }

                    return Task.FromResult((TResponse)(object)_details[^1]!.Activity);
                case ListWaitingForYouQuery:
                    WaitingReads++;
                    if (WaitingFailure is not null)
                    {
                        throw WaitingFailure;
                    }

                    return Task.FromResult((TResponse)(object)Waiting!);
                case SaveActivityDraftCommand save:
                    Saves.Add(save);
                    if (SaveFailure is not null)
                    {
                        throw SaveFailure;
                    }

                    return Task.FromResult((TResponse)(object)_details[^1]!.Activity);
                default:
                    // The programme start the form's hint reads (T192): none.
                    return Task.FromResult(default(TResponse)!);
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
