using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Behaviours;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// The audit row for inviting a respondent holds no address, whether the invitation is added or refused. (T184)
/// </summary>
/// <remarks>
/// Closing a campaign anonymises its invitations, but not the audit trail, which is the log kept longest. Until T184
/// every respondent's address was written into <c>SummaryJson</c> there, beside the campaign it was invited to. These run
/// the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> and <see cref="AuditWriter" /> and read the stored
/// row, so they cover what is written, not only what the serializer would produce.
/// </remarks>
public sealed class MsfInvitationAuditRowTests
{
    private const string Respondent = "peer-9@example.test";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task AnAddedInvitation_IsAuditedWithoutTheRespondentsAddress()
    {
        var campaignId = await SeedCampaignAsync(MsfCampaignState.Draft);

        await using (var db = CreateDb())
        {
            await AddThroughTheAuditPipelineAsync(db, campaignId);
        }

        await using var read = CreateDb();
        var row = await read.AuditEntries.AsNoTracking().SingleAsync();
        row.Action.Should().Be(nameof(AddMsfInvitationCommand));
        row.Success.Should().BeTrue();
        ShouldHoldNoAddress(row.SummaryJson, row.ErrorMessage, campaignId);

        // Only the log is redacted: the invitation itself still holds the address it will be mailed to.
        (await read.MsfInvitations.AsNoTracking().SingleAsync()).RespondentEmail.Should().Be(Respondent);
    }

    [Fact]
    public async Task ARefusedInvitation_IsAuditedWithoutTheRespondentsAddress()
    {
        var campaignId = await SeedCampaignAsync(MsfCampaignState.Open);

        await using (var db = CreateDb())
        {
            var add = () => AddThroughTheAuditPipelineAsync(db, campaignId);
            await add.Should().ThrowAsync<InvalidOperationException>();
        }

        await using var read = CreateDb();
        var row = await read.AuditEntries.AsNoTracking().SingleAsync();
        row.Success.Should().BeFalse();
        ShouldHoldNoAddress(row.SummaryJson, row.ErrorMessage, campaignId);
        (await read.MsfInvitations.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task AnInvitationRefusedByItsValidator_IsAuditedWithoutTheAddress()
    {
        // Longer than the 320 the validator allows. The refusal's message is what the audit row records as its error,
        // so it must name the field without repeating the value.
        var tooLong = new string('a', 320) + "@example.test";
        var campaignId = await SeedCampaignAsync(MsfCampaignState.Draft);

        await using (var db = CreateDb())
        {
            var add = () => AddThroughTheAuditPipelineAsync(db, campaignId, tooLong, validate: true);
            await add.Should().ThrowAsync<ValidationException>();
        }

        await using var read = CreateDb();
        var row = await read.AuditEntries.AsNoTracking().SingleAsync();
        row.Success.Should().BeFalse();
        row.ErrorMessage.Should().Contain(nameof(AddMsfInvitationCommand.RespondentEmail));
        ShouldHoldNoAddress(row.SummaryJson, row.ErrorMessage, campaignId, tooLong);
    }

    private static void ShouldHoldNoAddress(
        string summaryJson, string? errorMessage, int campaignId, string address = Respondent)
    {
        summaryJson.Should().NotContain(address);
        (errorMessage ?? string.Empty).Should().NotContain(address);

        var summary = JsonDocument.Parse(summaryJson).RootElement;
        summary.GetProperty("respondentEmail").GetString().Should().Be("[REDACTED]");
        summary.GetProperty("campaignId").GetInt32().Should().Be(campaignId, "the row still says which campaign");
    }

    /// <summary>The audit behaviour outermost, then (optionally) validation, then the handler: the app's order.</summary>
    private static Task<int> AddThroughTheAuditPipelineAsync(
        ApplicationDbContext db, int campaignId, string address = Respondent, bool validate = false)
    {
        var command = new AddMsfInvitationCommand(
            campaignId, address, MsfRespondentCategory.PeerDoctor, TestPrincipals.Administrator());
        var handler = new AddMsfInvitationCommandHandler(db, new InvitationTokenService(), FakeUserDirectory.Trainees("trainee-1"));

        RequestHandlerDelegate<int> handle = () => handler.Handle(command, CancellationToken.None);
        RequestHandlerDelegate<int> inner = validate
            ? () => new ValidationBehavior<AddMsfInvitationCommand, int>([new AddMsfInvitationCommandValidator()])
                .Handle(command, handle, CancellationToken.None)
            : handle;

        return new AuditPipelineBehavior<AddMsfInvitationCommand, int>(new AuditWriter(db), new FixedAuditContext())
            .Handle(command, inner, CancellationToken.None);
    }

    private async Task<int> SeedCampaignAsync(MsfCampaignState state)
    {
        await using var db = CreateDb();
        if (!await db.Set<Wombat.Domain.Identity.TraineeProfile>().AnyAsync(profile => profile.UserId == "trainee-1"))
        {
            // A current trainee (T284: opening a draft, or inviting to one, asks for one, as create does).
            db.Set<Wombat.Domain.Identity.TraineeProfile>().Add(new Wombat.Domain.Identity.TraineeProfile
            {
                UserId = "trainee-1", InstitutionId = 1, CurriculumId = 1,
                ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                IsActive = true
            });
        }

        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            State = state,
            Template = new MsfTemplate
            {
                Name = "Annual MSF",
                Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
            }
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private sealed class FixedAuditContext : IAuditContextProvider
    {
        public string? UserId => "admin-user";
        public string? UserDisplay => "Admin";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => null;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }
}
