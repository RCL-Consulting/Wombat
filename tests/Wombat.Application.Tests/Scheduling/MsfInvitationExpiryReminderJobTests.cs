using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Scheduling;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// The reminder a respondent who has not answered is sent, once, from two days before the campaign's window closes, and
/// what it says. (T132, T206)
/// </summary>
/// <remarks>
/// <para>
/// Every campaign here is built as the product builds one: the respondents are added through
/// <see cref="AddMsfInvitationCommandHandler" />, which writes each invitation's expiry a week after the window closes,
/// and the campaign is opened through <see cref="OpenMsfCampaignCommandHandler" />, which mails each respondent a link.
/// Until T206 the job keyed on that expiry, while <see cref="MsfCampaignAutoCloseJob" /> closes the campaign the day
/// after the window does, so it never sent a reminder. Its test seeded an expiry equal to the window's close, which the
/// product never writes, and passed.
/// </para>
/// <para>
/// The open runs at the real time, so every date is relative to today, and each job run is given the day it runs as.
/// </para>
/// </remarks>
public sealed class MsfInvitationExpiryReminderJobTests
{
    private const string RespondUrl = "https://wombat.example/msf/respond";
    private const string TraineeId = "trainee-1";
    private const string TraineeName = "Thandi Nkosi";
    private const string TemplateName = "Annual MSF";
    private const string LearnerTemplateName = "Learner feedback (interim questionnaire)";

    private static readonly string[] Respondents =
    [
        "consultant-1@example.test",
        "nurse-1@example.test",
        "peer-1@example.test"
    ];

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly InvitationTokenService _tokens = new();

    [Fact]
    public async Task OverTheCampaignsLife_EachRespondentWhoHasNotAnswered_IsRemindedOnce_TwoDaysBeforeTheWindowCloses()
    {
        var closesOn = Today.AddDays(10);
        var reminders = new DatedEmailSender();
        using var provider = BuildProvider(reminders);
        var campaign = await OpenCampaignAsync(provider, closesOn);

        (await ReadInvitationsAsync(provider)).Should().OnlyContain(
            invitation => invitation.ExpiresOn == closesOn.AddDays(7),
            "guard: the product writes each invitation's expiry a week after the window closes");

        await RespondAsync(provider, campaign.Links["peer-1@example.test"]);

        // Every day of the campaign's life and a week past it, as the scheduler runs the two jobs.
        for (var day = Today; day <= closesOn.AddDays(8); day = day.AddDays(1))
        {
            reminders.Day = day;
            await RunAutoCloseAsync(provider, day);
            await RunReminderAsync(provider, day);
        }

        reminders.Sent.Select(sent => sent.Message.To)
            .Should().BeEquivalentTo(["consultant-1@example.test", "nurse-1@example.test"], "one reminder each, and none to the respondent who answered");
        reminders.Sent.Should().OnlyContain(sent => sent.Day == closesOn.AddDays(-2));
    }

    /// <summary>
    /// A learner is reminded as they were invited (T164): about the trainee's teaching, pooled with the other learners',
    /// never for multi-source feedback on a colleague.
    /// </summary>
    [Fact]
    public async Task ALearnersReminder_AsksAboutTheTraineesTeaching_AsTheirInvitationDid()
    {
        var closesOn = Today.AddDays(4);
        var reminders = new DatedEmailSender();
        using var provider = BuildProvider(reminders);
        await OpenCampaignAsync(provider, closesOn, kind: MsfTemplateKind.LearnerFeedback);

        await RunReminderAsync(provider, closesOn.AddDays(-2));

        reminders.Sent.Select(sent => sent.Message.To).Should().BeEquivalentTo(Respondents, "every learner who has not answered");
        var message = reminders.Sent.Select(sent => sent.Message).Single(sent => sent.To == Respondents[0]);
        var lastDay = Date(closesOn);

        message.Subject.Should().Be($"Reminder: feedback on {TraineeName} ({LearnerTemplateName}) is due by {lastDay}");
        message.TextBody.Should().Contain(
                $"You were asked to give feedback on the teaching of {TraineeName}, a trainee who has taught you, and your " +
                "response has not been received yet.")
            .And.Contain($"The last day to respond is {lastDay}.")
            .And.Contain("together with the other learners' feedback")
            .And.NotContain("multi-source")
            .And.NotContain("respondent role");
        message.HtmlBody.Should().Contain("Learner feedback reminder")
            .And.Contain($"feedback on the teaching of <strong>{TraineeName}</strong>")
            .And.NotContain("multi-source");
    }

    [Fact]
    public async Task TheReminder_NamesTheTrainee_TheQuestionnaire_AndTheLastDay_AndItsLinkReplacesTheInvitations_WhichStillWorks()
    {
        // Four days out, so the first reminder day is at least a day after the open, whatever the time of day the test
        // runs at (MsfInvitation.ReminderMinimumLinkAge).
        var closesOn = Today.AddDays(4);
        var reminders = new DatedEmailSender();
        using var provider = BuildProvider(reminders);
        var campaign = await OpenCampaignAsync(provider, closesOn);

        await RunReminderAsync(provider, closesOn.AddDays(-2));

        var message = reminders.Sent.Select(sent => sent.Message)
            .Single(sent => sent.To == "consultant-1@example.test");
        var lastDay = Date(closesOn);

        message.Subject.Should().Be($"Reminder: feedback on {TraineeName} ({TemplateName}) is due by {lastDay}");
        message.TextBody.Should().Contain($"give multi-source feedback on {TraineeName}, a trainee you have worked with")
            .And.Contain($"Questionnaire: {TemplateName}")
            .And.Contain($"Feedback window: {Date(Today)} to {lastDay}")
            .And.Contain($"The last day to respond is {lastDay}.")
            .And.Contain(
                "Please use this link. If you have already started with the link in your original invitation, that one " +
                $"also works until {lastDay}. Both links are yours alone, and between them take one response.")
            .And.NotContain("no longer works")
            .And.Contain($"never shown to {TraineeName}")
            .And.NotContain("colleague");
        message.HtmlBody.Should().Contain($"<strong>{TraineeName}</strong>")
            .And.Contain($"The last day to respond is <strong>{lastDay}</strong>.");
        message.Tags.Should().Contain($"campaign:{campaign.Id}");

        // One deadline, the one the invitation and the page give: never the expiry a week after the window closes.
        DatesIn(message.Subject, message.TextBody, message.HtmlBody)
            .Should().NotBeEmpty().And.OnlyContain(date => date <= closesOn);

        // The reminder's link is an absolute one on the configured page, and a respondent can answer through it.
        var reminderLink = LinkIn(message);
        reminderLink.Should().StartWith(RespondUrl + "?token=");
        await RespondAsync(provider, reminderLink);

        // The invitation's link was replaced, and is kept as the previous link: it still opens the questionnaire, so a
        // respondent part-way through it on that link does not lose their answers (T214). The reminder's link opens it
        // too: it carries the selector the reminder stored (T163).
        var reminderLinkToNurse = LinkIn(reminders.Sent.Single(sent => sent.Message.To == "nurse-1@example.test").Message);
        (await OpenFormAsync(provider, campaign.Links["nurse-1@example.test"])).TemplateName.Should().Be(TemplateName);
        (await OpenFormAsync(provider, reminderLinkToNurse)).TemplateName.Should().Be(TemplateName);

        var stored = (await ReadInvitationsAsync(provider)).Single(invitation => invitation.RespondentEmail == "nurse-1@example.test");
        _tokens.VerifyToken(TokenIn(campaign.Links["nurse-1@example.test"]), stored.TokenHash)
            .Should().BeFalse("the reminder's link is the current one (T132)");
        _tokens.VerifyToken(TokenIn(reminderLinkToNurse), stored.TokenHash).Should().BeTrue();
        stored.PreviousTokenSelector.Should().Be(_tokens.SelectorOf(TokenIn(campaign.Links["nurse-1@example.test"])));
        _tokens.VerifyToken(TokenIn(campaign.Links["nurse-1@example.test"]), stored.PreviousTokenHash!)
            .Should().BeTrue("the invitation's link is kept as the previous one (T214)");

        // The reminder asks what became of it, keyed by the invitation and the link it carries, the one now stored
        // (T251): a reminder the mail server drops is counted on the campaign page and can be sent again.
        var reminderToNurse = reminders.Sent.Single(sent => sent.Message.To == "nurse-1@example.test").Message;
        MsfInvitation.TryReadDeliveryKey(reminderToNurse.DeliveryKey, out var keyedInvitation, out var keyedSelector)
            .Should().BeTrue();
        keyedInvitation.Should().Be(stored.Id);
        keyedSelector.Should().Be(stored.TokenSelector);
        reminderToNurse.Tags.Should().NotContain(tag => tag.Contains(keyedSelector, StringComparison.Ordinal));

        // The respondent who answered through the reminder's link: nothing is left for the invitation's link to take, so
        // it is retired, and the page no longer recognises it.
        var answered = (await ReadInvitationsAsync(provider)).Single(invitation => invitation.RespondentEmail == "consultant-1@example.test");
        answered.RespondedOn.Should().NotBeNull();
        answered.PreviousTokenSelector.Should().BeNull();
        answered.PreviousTokenHash.Should().BeNull();
        var spent = () => OpenFormAsync(provider, campaign.Links["consultant-1@example.test"]);
        (await spent.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);
    }

    [Fact]
    public async Task ARunMissedOnTheFirstReminderDay_IsMadeUpTheNextDay_AndASecondRunThatDaySendsNothing()
    {
        var closesOn = Today.AddDays(5);
        var reminders = new DatedEmailSender();
        using var provider = BuildProvider(reminders);
        await OpenCampaignAsync(provider, closesOn);

        await RunReminderAsync(provider, closesOn.AddDays(-3));
        reminders.Sent.Should().BeEmpty("the reminder is not due until two days before the window closes");

        // No run on the first reminder day: the host was down.
        await RunReminderAsync(provider, closesOn.AddDays(-1));
        reminders.Sent.Should().HaveCount(Respondents.Length);
        var mailed = await ReadInvitationsAsync(provider);

        // The scheduler's catch-up, or an administrator's "Run now", the same day; then the last day.
        await RunReminderAsync(provider, closesOn.AddDays(-1), hour: 11);
        await RunReminderAsync(provider, closesOn);

        reminders.Sent.Should().HaveCount(Respondents.Length, "each link is replaced by a reminder once");
        (await ReadInvitationsAsync(provider)).Select(invitation => invitation.TokenHash)
            .Should().Equal(mailed.Select(invitation => invitation.TokenHash), "a second reminder would retire the first one's link");
    }

    [Fact]
    public async Task ACampaignOpenedInsideTheReminderWindow_SendsNoReminder()
    {
        // The respondents were added a week ago, and the campaign opened today, the day before its window closes: the
        // invitations just sent name the same last day, and a reminder would only retire their links.
        var closesOn = Today.AddDays(1);
        var reminders = new DatedEmailSender();
        using var provider = BuildProvider(reminders);
        await OpenCampaignAsync(provider, closesOn, addedDaysAgo: 7);

        await RunReminderAsync(provider, Today);
        await RunReminderAsync(provider, closesOn);

        reminders.Sent.Should().BeEmpty();
    }

    /// <summary>
    /// A draft's invitees were never sent a link, so there is nothing to remind them of, whatever its dates. (T206 review)
    /// </summary>
    [Fact]
    public async Task TheInviteesOfADraftCampaign_AreNotReminded()
    {
        var closesOn = Today.AddDays(4);
        var reminders = new DatedEmailSender();
        using var provider = BuildProvider(reminders);
        await DraftCampaignAsync(provider, closesOn);

        for (var day = Today; day <= closesOn; day = day.AddDays(1))
        {
            await RunReminderAsync(provider, day);
        }

        reminders.Sent.Should().BeEmpty();
        (await ReadInvitationsAsync(provider)).Should().HaveCount(Respondents.Length, "guard: the draft has invitees");
    }

    [Fact]
    public async Task ATraineeWithNoNameOnRecord_IsNotRemindedAbout_AndTheLinksMailedKeepWorking()
    {
        // Named when the campaign opened, and not by the time the reminder is due.
        var closesOn = Today.AddDays(4);
        var reminders = new DatedEmailSender();
        using var provider = BuildProvider(reminders, directory: FakeUserDirectory.Empty);
        var campaign = await OpenCampaignAsync(provider, closesOn);

        await RunReminderAsync(provider, closesOn.AddDays(-2));

        reminders.Sent.Should().BeEmpty("a reminder must say whom the feedback is about (T202)");
        foreach (var invitation in await ReadInvitationsAsync(provider))
        {
            _tokens.VerifyToken(TokenIn(campaign.Links[invitation.RespondentEmail!]), invitation.TokenHash).Should().BeTrue();
        }
    }

    [Fact]
    public async Task ASendThatFails_LeavesThatRespondentsLinkWorking_AndTheRemindersAlreadySentStored()
    {
        var closesOn = Today.AddDays(4);
        var reminders = new DatedEmailSender { FailOnSend = 2 };
        using var provider = BuildProvider(reminders);
        var campaign = await OpenCampaignAsync(provider, closesOn);

        var act = async () => await RunReminderAsync(provider, closesOn.AddDays(-2));
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("The mail queue is down.");

        var invitations = await ReadInvitationsAsync(provider);
        var reminded = reminders.Sent.Should().ContainSingle().Subject.Message;
        _tokens.VerifyToken(TokenIn(LinkIn(reminded)), invitations.Single(invitation => invitation.RespondentEmail == reminded.To).TokenHash)
            .Should().BeTrue("the reminder sent before the failure was stored, so its link works");

        foreach (var invitation in invitations.Where(invitation => invitation.RespondentEmail != reminded.To))
        {
            _tokens.VerifyToken(TokenIn(campaign.Links[invitation.RespondentEmail!]), invitation.TokenHash)
                .Should().BeTrue($"{invitation.RespondentEmail} was mailed no reminder, so the invitation's link must still work");
        }

        // The next run reminds the two it did not reach, and not the one it did.
        reminders.FailOnSend = null;
        await RunReminderAsync(provider, closesOn.AddDays(-1));
        reminders.Sent.Select(sent => sent.Message.To).Should().OnlyHaveUniqueItems().And.HaveCount(Respondents.Length);
    }

    [Fact]
    public async Task ExecuteAsync_RefusesToSendWhenTheRespondUrlIsNotConfigured()
    {
        // Fail before mailing anything, rather than re-issuing every token and then discovering
        // there is nowhere to send them — which would retire working links for nothing.
        var closesOn = Today.AddDays(4);
        var reminders = new DatedEmailSender();
        using var provider = BuildProvider(reminders, respondUrl: null);
        var campaign = await OpenCampaignAsync(provider, closesOn);

        var act = async () => await RunReminderAsync(provider, closesOn.AddDays(-2));

        await act.Should().ThrowAsync<InvalidOperationException>();
        reminders.Sent.Should().BeEmpty();
        foreach (var invitation in await ReadInvitationsAsync(provider))
        {
            _tokens.VerifyToken(TokenIn(campaign.Links[invitation.RespondentEmail!]), invitation.TokenHash)
                .Should().BeTrue("a misconfigured job must not retire a working token");
        }
    }

    // ─── The campaign, as the product builds it ──────────────────────────────

    private sealed record OpenedCampaign(int Id, IReadOnlyDictionary<string, string> Links);

    /// <summary>
    /// A draft whose window opens today, its respondents added through the product's command, then opened through the
    /// product's command. <paramref name="addedDaysAgo" /> dates the adds that many days before the open.
    /// </summary>
    private static async Task<OpenedCampaign> OpenCampaignAsync(
        ServiceProvider provider,
        DateOnly closesOn,
        int addedDaysAgo = 0,
        MsfTemplateKind kind = MsfTemplateKind.Msf)
    {
        var campaignId = await DraftCampaignAsync(provider, closesOn, addedDaysAgo, kind);

        var invitations = new CapturingEmailSender();
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await new OpenMsfCampaignCommandHandler(
                    db,
                    invitations,
                    new InvitationTokenService(),
                    new FakeUserDirectory((TraineeId, TraineeName)),
                    Options.Create(new WombatOptions { MsfRespondUrl = RespondUrl }))
                .Handle(new OpenMsfCampaignCommand(campaignId, TestPrincipals.Administrator()), CancellationToken.None);
        }

        return new OpenedCampaign(campaignId, invitations.Messages.ToDictionary(message => message.To, LinkIn));
    }

    /// <summary>
    /// A draft whose window opens today, its respondents added through the product's command, and not opened.
    /// <paramref name="addedDaysAgo" /> dates the adds that many days back. A learner-feedback campaign (T164) invites
    /// each respondent as a learner, taught on a ward round.
    /// </summary>
    private static async Task<int> DraftCampaignAsync(
        ServiceProvider provider,
        DateOnly closesOn,
        int addedDaysAgo = 0,
        MsfTemplateKind kind = MsfTemplateKind.Msf)
    {
        var learnerFeedback = kind == MsfTemplateKind.LearnerFeedback;
        int campaignId;
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var campaign = new MsfCampaign
            {
                SubjectUserId = TraineeId,
                CreatedByUserId = "coordinator-user",
                CreatedOn = DateTime.UtcNow,
                OpensOn = Today,
                ClosesOn = closesOn,
                State = MsfCampaignState.Draft,
                Template = new MsfTemplate
                {
                    Name = learnerFeedback ? LearnerTemplateName : TemplateName,
                    Kind = kind,
                    IsActive = true,
                    Questions =
                    [
                        new MsfQuestion { Order = 1, Prompt = "What should the trainee keep doing?", Type = MsfQuestionType.LongText, Required = true }
                    ]
                }
            };

            db.MsfCampaigns.Add(campaign);
            await db.SaveChangesAsync();
            campaignId = campaign.Id;

            var add = new AddMsfInvitationCommandHandler(db, new InvitationTokenService());
            foreach (var email in Respondents)
            {
                await add.Handle(
                    learnerFeedback
                        ? new AddMsfInvitationCommand(campaignId, email, MsfRespondentCategory.Learner, TestPrincipals.Administrator(), "Ward round")
                        : new AddMsfInvitationCommand(campaignId, email, MsfRespondentCategory.Nurse, TestPrincipals.Administrator()),
                    CancellationToken.None);
            }

            if (addedDaysAgo > 0)
            {
                foreach (var invitation in await db.MsfInvitations.ToListAsync())
                {
                    invitation.IssuedOn = invitation.IssuedOn.AddDays(-addedDaysAgo);
                }

                await db.SaveChangesAsync();
            }
        }

        return campaignId;
    }

    /// <summary>A response through a link, as the respondent page submits it.</summary>
    private static async Task RespondAsync(ServiceProvider provider, string link)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var questionId = await db.MsfQuestions.Select(question => question.Id).SingleAsync();

        await new SubmitMsfResponseCommandHandler(db, new InvitationTokenService()).Handle(
            new SubmitMsfResponseCommand(TokenIn(link), [new SubmitMsfResponseAnswerItem(questionId, null, "Calm with parents.")]),
            CancellationToken.None);
    }

    /// <summary>The questionnaire a link opens, as the respondent page asks for it.</summary>
    private static async Task<MsfResponseFormDto> OpenFormAsync(ServiceProvider provider, string link)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await new GetMsfResponseFormQueryHandler(db, new InvitationTokenService(), new FakeUserDirectory((TraineeId, TraineeName)))
            .Handle(new GetMsfResponseFormQuery(TokenIn(link)), CancellationToken.None);
    }

    private static async Task<List<MsfInvitation>> ReadInvitationsAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.MsfInvitations.AsNoTracking().OrderBy(invitation => invitation.Id).ToListAsync();
    }

    // ─── The jobs, as the scheduler runs them ────────────────────────────────

    /// <summary>At 08:00 UTC, the reminder's cron time, unless an hour is given.</summary>
    private static Task RunReminderAsync(ServiceProvider provider, DateOnly day, int hour = 8)
        => new MsfInvitationExpiryReminderJob(provider.GetRequiredService<IServiceScopeFactory>()).ExecuteAsync(
            new ScheduledJobContext(day.ToDateTime(new TimeOnly(hour, 0), DateTimeKind.Utc), NullLogger.Instance),
            CancellationToken.None);

    /// <summary>At 00:00 UTC, the first of the hourly auto-close runs on that day.</summary>
    private static Task RunAutoCloseAsync(ServiceProvider provider, DateOnly day)
        => new MsfCampaignAutoCloseJob(provider.GetRequiredService<IServiceScopeFactory>()).ExecuteAsync(
            new ScheduledJobContext(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), NullLogger.Instance),
            CancellationToken.None);

    private static ServiceProvider BuildProvider(
        DatedEmailSender emailSender,
        string? respondUrl = RespondUrl,
        FakeUserDirectory? directory = null)
    {
        // The name is computed once, not inside the lambda: the lambda runs per DbContext, so an
        // inline Guid gives every scope its own database and nothing seeded is ever read back.
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IEmailSender>(_ => emailSender);
        services.AddScoped<IInvitationTokenService, InvitationTokenService>();
        services.AddScoped<IUserAdministrationService>(_ => directory ?? new FakeUserDirectory((TraineeId, TraineeName)));
        services.AddScoped(_ => Options.Create(new WombatOptions { MsfRespondUrl = respondUrl }));
        return services.BuildServiceProvider();
    }

    // ─── Reading a mail ──────────────────────────────────────────────────────

    private static string LinkIn(EmailMessage message)
        => message.TextBody
            .Split('\n', StringSplitOptions.TrimEntries)
            .Single(line => line.StartsWith(RespondUrl + "?token=", StringComparison.Ordinal));

    private static string TokenIn(string link) => HttpUtility.ParseQueryString(new Uri(link).Query)["token"]!;

    private static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>Every yyyy-MM-dd date written in the given texts, outside any token (whose characters border it).</summary>
    private static IReadOnlyList<DateOnly> DatesIn(params string[] texts)
        => texts
            .SelectMany(text => Regex.Matches(text, @"(?<![\w-])\d{4}-\d{2}-\d{2}(?![\w-])", RegexOptions.CultureInvariant))
            .Select(match => DateOnly.ParseExact(match.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture))
            .ToList();

    /// <summary>
    /// Records each reminder with the day the test says the job ran on. <see cref="FailOnSend" /> makes that send (1 for
    /// the first) throw instead.
    /// </summary>
    private sealed class DatedEmailSender : IEmailSender
    {
        private int _attempts;

        public DateOnly Day { get; set; }

        public int? FailOnSend { get; set; }

        public List<(DateOnly Day, EmailMessage Message)> Sent { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (++_attempts == FailOnSend)
            {
                throw new InvalidOperationException("The mail queue is down.");
            }

            Sent.Add((Day, message));
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingEmailSender : IEmailSender
    {
        public List<EmailMessage> Messages { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
