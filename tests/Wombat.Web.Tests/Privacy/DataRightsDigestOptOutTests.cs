using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Profile;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Privacy;

/// <summary>
/// T240, rewritten for T387 (flow 14; Q6, Q24, Q27, F7, N3): My data rights' Reminder emails says what "Stop the regular
/// reminder emails" stops, and what it does not.
/// </summary>
/// <remarks>
/// Since T151 and T240 the flag stops three periodic reminders, of which only the coordinator's is called a digest: the
/// draft reminder and the reminder of activities waiting on an assessor stop too. The box is described by help naming all
/// three, and what still arrives: since T387 (Q27, F7) only mail Wombat sends today, checked against <c>IEmailSender</c>'s
/// and <c>AccountNotices</c>' callers, with the entrustment expiry notice among the examples; "a request to assess" and "a
/// committee or entrustment decision", which no code mails, are gone (T320). The optional-processing box went with its
/// flag (T318, Q6): the reminder box is the one preference, and its save says what it did, or that it changed nothing.
/// </remarks>
public sealed class DataRightsDigestOptOutTests : TestContext
{
    private readonly RecordingSender _sender = new();

    public DataRightsDigestOptOutTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IScopedSender>(_sender);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TheBox_IsDescribedByWhatItStops_AndTheMailStillSent()
    {
        var cut = RenderComponent<DataRights>();

        var checkbox = cut.Find($"#{DataRights.RemindersId}");
        checkbox.GetAttribute("aria-describedby").Should().Be(DataRights.RemindersHelpId);

        Text(cut.Find($"#{DataRights.RemindersHelpId}")).Should().Be(
            "Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 " +
            "days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one " +
            "particular thing is still sent, such as an invitation to give feedback, a coordinator's reminder about one " +
            "activity, or a notice that an entrustment decision is about to expire or has expired.");
    }

    /// <summary>Q27: only mail Wombat sends today is named (T320's mails change it again when they land).</summary>
    [Fact]
    public void TheHelp_NamesNoMailWombatDoesNotSend()
    {
        var cut = RenderComponent<DataRights>();

        var help = Text(cut.Find($"#{DataRights.RemindersHelpId}"));
        help.Should().NotContain("a request to assess")
            .And.NotContain("a committee or entrustment decision")
            .And.NotContain("password reset");
    }

    /// <summary>
    /// T358, review 11: the nudge's days are one setting, <c>DashboardThresholds.AssessorNudgeDays</c>, which this help reads
    /// where it wrote 5 by hand; the words are otherwise unchanged (E1).
    /// </summary>
    [Fact]
    public void TheNudgesDays_AreReadFromTheSetting()
    {
        Services.Configure<Wombat.Application.Common.Options.DashboardThresholds>(thresholds => thresholds.AssessorNudgeDays = 3);

        var cut = RenderComponent<DataRights>();

        Text(cut.Find($"#{DataRights.RemindersHelpId}"))
            .Should().Contain("the reminder of activities that have waited more than 3 days for you to assess");
    }

    /// <summary>T318, Q6: the reminder box is the one checkbox on the page, labelled, in Reminder emails.</summary>
    [Fact]
    public void TheReminderBox_IsThePagesOneCheckbox_LabelledAndEveryReferenceResolves()
    {
        var cut = RenderComponent<DataRights>();

        cut.FindAll("input[type='checkbox']").Should().ContainSingle()
            .Which.Id.Should().Be(DataRights.RemindersId);
        var checkbox = cut.Find($"#{DataRights.RemindersId}");
        checkbox.ParentElement!.ClassList.Should().Contain("form-check");
        cut.FindAll($"label[for='{DataRights.RemindersId}']").Should().ContainSingle();
        AccessibleNames.NameOf(cut, checkbox).Should().Be("Stop the regular reminder emails");
        cut.Markup.Should().NotContain("optional processing").And.NotContain("Opt out of digest emails");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TickingTheBox_AndSaving_SendsIt_AndSaysTheRemindersStopped()
    {
        var cut = RenderComponent<DataRights>();

        cut.Find($"#{DataRights.RemindersId}").Change(true);
        Save(cut).Click();

        _sender.Received.OfType<UpdateObjectionFlagsCommand>().Should().ContainSingle()
            .Which.OptOutOfDigestEmails.Should().BeTrue();
        cut.WaitForAssertion(() => Text(ReminderSection(cut).QuerySelector(".action-result")!)
            .Should().Be("Reminder emails stopped."));
    }

    [Fact]
    public void UntickingTheBox_AndSaving_SaysTheyWillBeSentAgain()
    {
        _sender.OptedOut = true;
        var cut = RenderComponent<DataRights>();
        cut.Find($"#{DataRights.RemindersId}").HasAttribute("checked").Should().BeTrue("the stored state is shown");

        cut.Find($"#{DataRights.RemindersId}").Change(false);
        Save(cut).Click();

        cut.WaitForAssertion(() => Text(ReminderSection(cut).QuerySelector(".action-result")!)
            .Should().Be("Reminder emails will be sent again."));
    }

    /// <summary>N3: a save that changes nothing says so, from the state before (ReminderEmailsSaved.Changed).</summary>
    [Theory]
    [InlineData(true, "Nothing changed: reminder emails are still stopped.")]
    [InlineData(false, "Nothing changed: reminder emails are still sent.")]
    public void ASaveThatChangesNothing_SaysSo(bool optedOut, string expected)
    {
        _sender.OptedOut = optedOut;
        _sender.Changed = false;
        var cut = RenderComponent<DataRights>();

        Save(cut).Click();

        cut.WaitForAssertion(() => Text(ReminderSection(cut).QuerySelector(".action-result")!).Should().Be(expected));
        cut.Markup.Should().NotContain("Processing preferences saved.");
    }

    private static IElement Save(IRenderedFragment cut)
        => cut.FindAll("button").Single(button => Text(button) == "Save");

    private static IElement ReminderSection(IRenderedFragment cut)
        => cut.Find("section[aria-labelledby='dr-reminders-heading']");

    private static string Text(IElement element)
        => string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed class RecordingSender : IScopedSender
    {
        public List<object> Received { get; } = [];

        public bool OptedOut { get; set; }

        public bool Changed { get; set; } = true;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            object answer = request switch
            {
                GetObjectionFlagsQuery => new ObjectionFlagsDto(OptOutOfDigestEmails: OptedOut),
                GetMyDataRightsRequestsQuery => new MyDataRightsDto("Kgosi Kgari Teaching Hospital", []),
                UpdateObjectionFlagsCommand command => new ReminderEmailsSaved(command.OptOutOfDigestEmails, Changed),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };
            return Task.FromResult((TResponse)answer);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
