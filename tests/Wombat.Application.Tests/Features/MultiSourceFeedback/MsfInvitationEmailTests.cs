using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// An MSF invitation says whom the feedback is about, on which questionnaire, and over which window. (T202)
/// </summary>
/// <remarks>
/// Until T202 it named the template alone ("MSF request: Annual MSF"): two campaigns on one template sent invitations
/// that could not be told apart, and none said whom the respondent was asked to rate. MSF hides the respondent from the
/// trainee, not the trainee from the respondent (T021), so the trainee is named.
/// </remarks>
public sealed class MsfInvitationEmailTests
{
    private const string RespondUrl = "https://wombat.example/msf/respond";
    private const string TemplateName = "Annual MSF";

    private static readonly DateOnly OpensOn = new(2029, 3, 1);
    private static readonly DateOnly ClosesOn = new(2029, 3, 21);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task OpeningACampaign_MailsAnInvitationNamingTheTrainee_TheQuestionnaire_AndTheWindow()
    {
        var campaignId = await SeedDraftAsync("trainee-1", OpensOn, ClosesOn);
        var sender = new CapturingEmailSender();

        await OpenAsync(campaignId, sender, new FakeUserDirectory(("trainee-1", "Thandi Nkosi")));

        var message = sender.Messages.Should().ContainSingle().Subject;
        message.To.Should().Be("nurse-1@example.test");
        message.Subject.Should().Be("Feedback request: Thandi Nkosi (Annual MSF, 2029-03-01 to 2029-03-21)");

        message.TextBody.Should().Contain("give multi-source feedback on Thandi Nkosi, a trainee you have worked with.");
        message.TextBody.Should().Contain("Questionnaire: Annual MSF");
        message.TextBody.Should().Contain("Feedback window: 2029-03-01 to 2029-03-21");
        message.TextBody.Should().Contain("never shown to Thandi Nkosi");
        message.HtmlBody.Should().Contain("<strong>Thandi Nkosi</strong>");
        message.HtmlBody.Should().Contain("<strong>Annual MSF</strong>");
        message.HtmlBody.Should().Contain("2029-03-01 to 2029-03-21");

        // The link is still the respondent's own, on a line of its own, and it still works.
        var stored = await ReadInvitationAsync(campaignId);
        var link = message.TextBody.Split('\n', StringSplitOptions.TrimEntries)
            .Single(line => line.StartsWith(RespondUrl + "?token=", StringComparison.Ordinal));
        var token = Uri.UnescapeDataString(link[(RespondUrl + "?token=").Length..]);
        new InvitationTokenService().VerifyToken(token, stored.TokenHash).Should().BeTrue();
        message.Tags.Should().Contain(["msf-invite", $"campaign:{campaignId}"]);
    }

    [Fact]
    public async Task TwoCampaignsOnOneTemplate_SendInvitationsThatCanBeToldApart()
    {
        // Two trainees in one semester, and one trainee in two: all three on the same template.
        var sameWindowOtherTrainee = await SeedDraftAsync("trainee-2", OpensOn, ClosesOn);
        var firstSemester = await SeedDraftAsync("trainee-1", OpensOn, ClosesOn);
        var secondSemester = await SeedDraftAsync("trainee-1", new DateOnly(2029, 9, 1), new DateOnly(2029, 9, 21));
        var directory = new FakeUserDirectory(("trainee-1", "Thandi Nkosi"), ("trainee-2", "Pieter Botha"));
        var sender = new CapturingEmailSender();

        await OpenAsync(sameWindowOtherTrainee, sender, directory);
        await OpenAsync(firstSemester, sender, directory);
        await OpenAsync(secondSemester, sender, directory);

        sender.Messages.Should().HaveCount(3);
        sender.Messages.Select(message => message.Subject).Should().OnlyHaveUniqueItems();
        sender.Messages[0].Subject.Should().Contain("Pieter Botha");
        sender.Messages[2].Subject.Should().Contain("2029-09-01 to 2029-09-21");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task ATraineeWithNoName_IsRefused_BeforeAnyMail_AndNothingChanges(string? storedName)
    {
        var campaignId = await SeedDraftAsync("trainee-1", OpensOn, ClosesOn);
        var before = await ReadInvitationAsync(campaignId);
        var directory = storedName is null ? FakeUserDirectory.Empty : new FakeUserDirectory(("trainee-1", storedName));
        var sender = new CapturingEmailSender();

        await using (var db = CreateDb())
        {
            var open = () => Handler(db, sender, directory)
                .Handle(new OpenMsfCampaignCommand(campaignId, TestPrincipals.Administrator()), CancellationToken.None);

            (await open.Should().ThrowAsync<InvalidOperationException>())
                .Which.Message.Should().Be(OpenMsfCampaignCommandHandler.TraineeHasNoName);

            // The audit pipeline saves the request's context from its catch; nothing may be pending for it to commit.
            await db.SaveChangesAsync();
        }

        sender.Messages.Should().BeEmpty();
        var after = await ReadInvitationAsync(campaignId);
        after.Campaign.State.Should().Be(MsfCampaignState.Draft);
        after.TokenHash.Should().Be(before.TokenHash);
    }

    [Fact]
    public void TheInvitation_EncodesTheTraineesNameInItsHtml()
    {
        var message = MsfInvitationEmail.Build(new MsfInvitationEmailContent(
            7, "nurse-1@example.test", "Sam <b>Smith</b>", "MSF & more", OpensOn, ClosesOn, ClosesOn, RespondUrl + "?token=a&b"));

        message.HtmlBody.Should().Contain("Sam &lt;b&gt;Smith&lt;/b&gt;").And.NotContain("<b>Smith</b>");
        message.HtmlBody.Should().Contain("MSF &amp; more");
        message.HtmlBody.Should().Contain("href=\"https://wombat.example/msf/respond?token=a&amp;b\"");
        message.TextBody.Should().Contain("Sam <b>Smith</b>", "the plain-text body is not HTML");
    }

    /// <summary>
    /// The invitation's deadline is the last day its link takes a response, and it names no later date. (T202 review)
    /// </summary>
    /// <remarks>
    /// The invitation is added through the product's own command, so its expiry is whatever the product writes: a week
    /// after the window closes. The auto-close job closes the campaign the day after the window does, and a closed
    /// campaign refuses every link, so a respondent told the expiry date would click during that week and be refused.
    /// </remarks>
    [Fact]
    public async Task TheInvitation_NamesTheLastDayItsLinkWorks_AndNoLaterDate()
    {
        var campaignId = await SeedDraftAsync("trainee-1", OpensOn, ClosesOn, withInvitation: false);
        await using (var db = CreateDb())
        {
            await new AddMsfInvitationCommandHandler(db, new InvitationTokenService()).Handle(
                new AddMsfInvitationCommand(
                    campaignId, "nurse-1@example.test", MsfRespondentCategory.Nurse, TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        (await ReadInvitationAsync(campaignId)).ExpiresOn
            .Should().BeAfter(ClosesOn, "guard: the product writes an expiry later than the window's close");

        var sender = new CapturingEmailSender();
        await OpenAsync(campaignId, sender, new FakeUserDirectory(("trainee-1", "Thandi Nkosi")));

        var message = sender.Messages.Should().ContainSingle().Subject;
        message.TextBody.Should().Contain("The last day to respond is 2029-03-21.");
        message.HtmlBody.Should().Contain("The last day to respond is <strong>2029-03-21</strong>.");
        DatesIn(message.Subject, message.TextBody, message.HtmlBody)
            .Should().NotBeEmpty().And.OnlyContain(date => date <= ClosesOn);
    }

    /// <summary>
    /// T164: a learner is asked about the trainee's teaching, not to give multi-source feedback on a colleague, and is
    /// told their answers are pooled with the other learners' rather than grouped by role.
    /// </summary>
    [Fact]
    public void ALearnerFeedbackInvitation_AsksAboutTheTraineesTeaching()
    {
        var message = MsfInvitationEmail.Build(new MsfInvitationEmailContent(
            7, "learner-1@example.test", "Thandi Nkosi", "Learner feedback (interim questionnaire)", OpensOn, ClosesOn, ClosesOn,
            RespondUrl + "?token=t", MsfTemplateKind.LearnerFeedback));

        message.TextBody.Should().Contain("give feedback on the teaching of Thandi Nkosi, a trainee who has taught you.")
            .And.Contain("together with the other learners' feedback")
            .And.NotContain("multi-source");
        message.HtmlBody.Should().Contain("Learner feedback request")
            .And.Contain("the teaching of <strong>Thandi Nkosi</strong>")
            .And.NotContain("multi-source");
    }

    /// <summary>T164: a learner's reminder, like their invitation, asks about the trainee's teaching.</summary>
    [Fact]
    public void ALearnersExpiryReminder_AsksAboutTheTeaching_NotForMultiSourceFeedback()
    {
        var learner = MsfExpiryReminderEmail.Build("learner-1@example.test", RespondUrl + "?token=t", ClosesOn, MsfTemplateKind.LearnerFeedback);
        var colleague = MsfExpiryReminderEmail.Build("nurse-1@example.test", RespondUrl + "?token=t", ClosesOn);

        learner.Subject.Should().Be("Your learner feedback link expires soon", "a learner was never asked for MSF");
        learner.TextBody.Should().Contain("feedback on the teaching of a trainee who has taught you").And.NotContain("multi-source");
        learner.HtmlBody.Should().Contain("feedback on the teaching of a trainee who has taught you").And.NotContain("multi-source")
            .And.NotContain("MSF");
        colleague.Subject.Should().Be("Your MSF feedback link expires soon");
        colleague.TextBody.Should().Contain("multi-source feedback on a colleague");
    }

    /// <summary>An invitation whose own expiry comes first names that expiry as its deadline. (T202 review)</summary>
    [Fact]
    public void AnInvitationThatExpiresBeforeTheWindowCloses_NamesItsExpiryAsTheDeadline()
    {
        var message = MsfInvitationEmail.Build(new MsfInvitationEmailContent(
            7, "nurse-1@example.test", "Thandi Nkosi", TemplateName, OpensOn, ClosesOn, ClosesOn.AddDays(-3), RespondUrl + "?token=t"));

        message.TextBody.Should().Contain("The last day to respond is 2029-03-18.");
        message.HtmlBody.Should().Contain("The last day to respond is <strong>2029-03-18</strong>.");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task OpenAsync(int campaignId, IEmailSender sender, IUserAdministrationService directory)
    {
        await using var db = CreateDb();
        await Handler(db, sender, directory)
            .Handle(new OpenMsfCampaignCommand(campaignId, TestPrincipals.Administrator()), CancellationToken.None);
    }

    private static OpenMsfCampaignCommandHandler Handler(
        ApplicationDbContext db,
        IEmailSender sender,
        IUserAdministrationService directory)
        => new(
            db,
            sender,
            new InvitationTokenService(),
            directory,
            Options.Create(new WombatOptions { MsfRespondUrl = RespondUrl }));

    /// <summary>Every yyyy-MM-dd date written in the given texts, outside any token (whose characters border it).</summary>
    private static IReadOnlyList<DateOnly> DatesIn(params string[] texts)
        => texts
            .SelectMany(text => Regex.Matches(text, @"(?<![\w-])\d{4}-\d{2}-\d{2}(?![\w-])", RegexOptions.CultureInvariant))
            .Select(match => DateOnly.ParseExact(match.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture))
            .ToList();

    private async Task<int> SeedDraftAsync(string subjectUserId, DateOnly opensOn, DateOnly closesOn, bool withInvitation = true)
    {
        await using var db = CreateDb();
        var template = await db.MsfTemplates.SingleOrDefaultAsync(entity => entity.Name == TemplateName)
            ?? new MsfTemplate { Name = TemplateName, IsActive = true };

        var campaign = new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = opensOn,
            ClosesOn = closesOn,
            State = MsfCampaignState.Draft,
            Template = template,
            Invitations = withInvitation
                ?
                [
                    new MsfInvitation
                    {
                        RespondentEmail = "nurse-1@example.test",
                        RespondentCategory = MsfRespondentCategory.Nurse,
                        TokenHash = "unissued",
                        IssuedOn = DateTime.UtcNow,
                        ExpiresOn = closesOn.AddDays(7)
                    }
                ]
                : []
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    private async Task<MsfInvitation> ReadInvitationAsync(int campaignId)
    {
        await using var db = CreateDb();
        return await db.MsfInvitations.AsNoTracking()
            .Include(invitation => invitation.Campaign)
            .SingleAsync(invitation => invitation.CampaignId == campaignId);
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

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
