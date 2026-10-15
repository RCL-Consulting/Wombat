using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// Lane T of flow 10 (T369): her list of released reports carries no count (F3), newest first and released only; and a
/// respondent's link is judged on the South African calendar, the one its last day was given in (T325).
/// </summary>
public sealed class MsfTraineeReportsAndLinkDayTests
{
    private const string TraineeUserId = "trainee-1";

    private readonly InvitationTokenService _tokenService = new();

    // ─── Her list (F3) ───────────────────────────────────────────────────────

    /// <summary>
    /// The released reports alone, newest release first, each carrying its template, its kind and when it was released,
    /// and no count: the record has no member a count could travel in.
    /// </summary>
    [Fact]
    public async Task HerList_IsHerReleasedReports_NewestFirst_WithNoCount()
    {
        await using var db = CreateDbContext();
        var older = Campaign("Default MSF", MsfCampaignState.Released, new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc));
        var newer = Campaign("Teaching feedback", MsfCampaignState.Released, new DateTime(2026, 10, 14, 14, 34, 0, DateTimeKind.Utc),
            MsfTemplateKind.LearnerFeedback);
        var underReview = Campaign("Default MSF", MsfCampaignState.UnderReview, null);
        var someoneElses = Campaign("Default MSF", MsfCampaignState.Released, new DateTime(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc));
        someoneElses.SubjectUserId = "trainee-2";
        db.MsfCampaigns.AddRange(older, newer, underReview, someoneElses);
        await db.SaveChangesAsync();

        var reports = await new ListMsfCampaignsForTraineeQueryHandler(db).Handle(
            new ListMsfCampaignsForTraineeQuery(TraineeUserId, TestPrincipals.Trainee(TraineeUserId)), CancellationToken.None);

        reports.Should().Equal(
            new MsfReleasedReportDto(newer.Id, "Teaching feedback", MsfTemplateKind.LearnerFeedback,
                new DateTime(2026, 10, 14, 14, 34, 0, DateTimeKind.Utc)),
            new MsfReleasedReportDto(older.Id, "Default MSF", MsfTemplateKind.Msf,
                new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)));

        typeof(MsfReleasedReportDto).GetProperties().Select(property => property.Name)
            .Should().BeEquivalentTo(["CampaignId", "TemplateName", "Kind", "ReleasedOn"],
                "her list carries no count (F3): the total took in the suppressed groups");
    }

    // ─── The link's last day, on the South African calendar (T325) ──────────

    /// <summary>
    /// 22:30 UTC on the link's last day is 00:30 the next day in South Africa: the link has expired, though the UTC date is
    /// still its last day. Both the questionnaire and the submit refuse it.
    /// </summary>
    [Fact]
    public async Task TheLink_IsRefused_OnTheSouthAfricanDayAfterItsLastDay_At2230Utc()
    {
        var clock = new FixedClock(new DateTime(2026, 10, 28, 22, 30, 0, DateTimeKind.Utc));
        await using var db = CreateDbContext();
        var token = await SeedOpenLinkAsync(db, lastDay: new DateOnly(2026, 10, 28));

        var form = () => new GetMsfResponseFormQueryHandler(db, _tokenService, FakeUserDirectory.Empty, clock)
            .Handle(new GetMsfResponseFormQuery(token), CancellationToken.None);
        var submit = () => new SubmitMsfResponseCommandHandler(db, _tokenService, clock).Handle(
            new SubmitMsfResponseCommand(token, [new SubmitMsfResponseAnswerItem(1, 4, null)]), CancellationToken.None);

        (await form.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkExpired);
        (await submit.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkExpired);
    }

    /// <summary>21:30 UTC on its last day is 23:30 in South Africa, still the last day: the link opens and takes an answer.</summary>
    [Fact]
    public async Task TheLink_IsAccepted_OnItsLastDay_InSouthAfrica()
    {
        var clock = new FixedClock(new DateTime(2026, 10, 28, 21, 30, 0, DateTimeKind.Utc));
        await using var db = CreateDbContext();
        var token = await SeedOpenLinkAsync(db, lastDay: new DateOnly(2026, 10, 28));

        var form = await new GetMsfResponseFormQueryHandler(db, _tokenService, FakeUserDirectory.Empty, clock)
            .Handle(new GetMsfResponseFormQuery(token), CancellationToken.None);
        form.LastDayToRespond.Should().Be(new DateOnly(2026, 10, 28));

        await new SubmitMsfResponseCommandHandler(db, _tokenService, clock).Handle(
            new SubmitMsfResponseCommand(token, [new SubmitMsfResponseAnswerItem(1, 4, null)]), CancellationToken.None);

        (await db.MsfResponses.SingleAsync()).SubmittedOn.Should().Be(clock.GetUtcNow().UtcDateTime);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// T369, build review R8: anyone who may not read about the trainee gets an empty list, never a refusal and never her
    /// reports: a classmate, and a coordinator of another institution.
    /// </summary>
    [Fact]
    public async Task HerList_IsEmpty_ForAReaderOutOfScope()
    {
        await using var db = CreateDbContext();
        db.MsfCampaigns.Add(Campaign("Default MSF", MsfCampaignState.Released, new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();

        var handler = new ListMsfCampaignsForTraineeQueryHandler(db);

        (await handler.Handle(new ListMsfCampaignsForTraineeQuery(TraineeUserId, TestPrincipals.Trainee("trainee-2")), CancellationToken.None))
            .Should().BeEmpty();
        (await handler.Handle(new ListMsfCampaignsForTraineeQuery(TraineeUserId, TestPrincipals.Coordinator(2)), CancellationToken.None))
            .Should().BeEmpty();
        (await handler.Handle(new ListMsfCampaignsForTraineeQuery(TraineeUserId, TestPrincipals.Trainee(TraineeUserId)), CancellationToken.None))
            .Should().ContainSingle("guard: the same list is hers");
    }

    private static MsfCampaign Campaign(
        string templateName, MsfCampaignState state, DateTime? releasedOn, MsfTemplateKind kind = MsfTemplateKind.Msf)
        => new()
        {
            SubjectUserId = TraineeUserId,
            CreatedByUserId = "coord-1",
            CreatedOn = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc),
            OpensOn = new DateOnly(2026, 3, 2),
            ClosesOn = new DateOnly(2026, 3, 16),
            State = state,
            ReleasedOn = releasedOn,
            Template = new MsfTemplate { Name = templateName, Kind = kind }
        };

    /// <summary>An open campaign's link whose last day to respond is <paramref name="lastDay" />.</summary>
    private async Task<string> SeedOpenLinkAsync(ApplicationDbContext db, DateOnly lastDay)
    {
        var token = _tokenService.GenerateSelectorToken().Token;
        db.MsfInvitations.Add(new MsfInvitation
        {
            Campaign = new MsfCampaign
            {
                SubjectUserId = TraineeUserId,
                CreatedByUserId = "coord-1",
                CreatedOn = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Utc),
                OpensOn = new DateOnly(2026, 10, 14),
                ClosesOn = lastDay,
                State = MsfCampaignState.Open,
                Template = new MsfTemplate
                {
                    Name = "Default MSF",
                    Questions = [new MsfQuestion { Order = 1, Prompt = "Judgement", Type = MsfQuestionType.Scale, Required = true }]
                }
            },
            RespondentEmail = "respondent@example.test",
            RespondentCategory = MsfRespondentCategory.Nurse,
            TokenSelector = _tokenService.SelectorOf(token),
            TokenHash = _tokenService.HashToken(token),
            IssuedOn = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Utc),
            ExpiresOn = lastDay
        });

        await db.SaveChangesAsync();
        return token;
    }

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
