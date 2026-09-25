using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// A campaign invites an address once, in any capitals and with any spaces around it; a second add is refused before
/// anything is written, with a refusal that names no address. (T228)
/// </summary>
/// <remarks>
/// <para>
/// Until T228 the add command had no such check: an address added twice was a second invitation, and opening the
/// campaign mailed each copy its own working link, so one respondent could answer twice and fill a group's minimum
/// alone (the F2 browser check: campaign 11 on dev, two addresses invited twice each, two mails each in the sink).
/// </para>
/// <para>
/// Each add runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real
/// <see cref="AuditWriter" /> on the handler's own context, as a request does, because that is where the audit trap
/// springs: the failure row's save flushes whatever the handler left tracked. The in-memory provider has no unique
/// index and no <c>xmin</c>; the database's half, racing adds included, is <c>MsfInvitationAddressOncePostgresTests</c>.
/// </para>
/// </remarks>
public sealed class MsfInvitationAddressOnceTests
{
    private const string Invited = "peer-9@example.test";
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Theory]
    [InlineData("peer-9@example.test")]
    [InlineData("PEER-9@EXAMPLE.TEST")]
    [InlineData("Peer-9@Example.Test")]
    [InlineData("  peer-9@example.test  ")]
    public async Task AnAddressTheCampaignAlreadyInvites_TypedAnyWay_IsRefused_AndNothingIsWritten(string typed)
    {
        var campaignId = await SeedDraftCampaignAsync();
        await using (var db = CreateDb())
        {
            await AddThroughTheAuditPipelineAsync(db, campaignId, Invited, MsfRespondentCategory.PeerDoctor);
        }

        await using (var db = CreateDb())
        {
            // Another group, even: one person is one respondent, whichever group they were invited in.
            var again = () => AddThroughTheAuditPipelineAsync(db, campaignId, typed, MsfRespondentCategory.Consultant);
            (await again.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(AddMsfInvitationCommandHandler.AlreadyInvited)
                .Which.InnerException.Should().BeNull("refused by the check, before anything reached the database");

            // The audit trap: the failure row was saved through this context, and a further save commits whatever is
            // still tracked. Nothing may be.
            db.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged).Should().BeEmpty();
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            var stored = await db.MsfInvitations.AsNoTracking().Where(invitation => invitation.CampaignId == campaignId).ToListAsync();
            stored.Should().ContainSingle("the second add stored nothing");
            stored[0].RespondentEmail.Should().Be(Invited);
            stored[0].RespondentCategory.Should().Be(MsfRespondentCategory.PeerDoctor, "the first invitation is left as it was");
        }

        await using var read = CreateDb();
        // In the order they happened, which is OccurredAt, as the audit log orders them. Not Id: a version 7 id is ordered
        // only to the millisecond, and two adds in the same millisecond sorted either way (T244).
        var rows = await read.AuditEntries.AsNoTracking().OrderBy(entry => entry.OccurredAt).ToListAsync();
        rows.Select(row => row.Success).Should().Equal(true, false);
        rows[1].ErrorMessage.Should().Be(AddMsfInvitationCommandHandler.AlreadyInvited);
    }

    [Fact]
    public void TheRefusal_NamesNoAddress()
    {
        // The audit row keeps a refusal's message (T184), and the page shows it beside no list of invitees (T217).
        AddMsfInvitationCommandHandler.AlreadyInvited.Should().NotContain("@");
        AddMsfInvitationCommandHandler.AlreadyInvited.Should().NotContain(Invited);
    }

    [Fact]
    public async Task TheSameAddress_IsInvitedToAnotherCampaign()
    {
        var first = await SeedDraftCampaignAsync();
        var second = await SeedDraftCampaignAsync();

        await using (var db = CreateDb())
        {
            await AddThroughTheAuditPipelineAsync(db, first, Invited, MsfRespondentCategory.PeerDoctor);
        }

        await using (var db = CreateDb())
        {
            await AddThroughTheAuditPipelineAsync(db, second, Invited.ToUpperInvariant(), MsfRespondentCategory.PeerDoctor);
        }

        await using var read = CreateDb();
        (await read.MsfInvitations.AsNoTracking().Select(invitation => invitation.CampaignId).ToListAsync())
            .Should().BeEquivalentTo([first, second]);
    }

    [Fact]
    public async Task AnErasedAddress_BlocksNobody()
    {
        // An invitation anonymised holds no address (T207), so it is no one's and cannot clash.
        var campaignId = await SeedDraftCampaignAsync(erasedInvitations: 2);

        await using (var db = CreateDb())
        {
            await AddThroughTheAuditPipelineAsync(db, campaignId, Invited, MsfRespondentCategory.PeerDoctor);
        }

        await using var read = CreateDb();
        (await read.MsfInvitations.AsNoTracking().CountAsync(invitation => invitation.RespondentEmail == Invited)).Should().Be(1);
    }

    [Fact]
    public async Task OnACampaignNoLongerADraft_TheStateIsTheRefusal_SoItsInviteesCannotBeProbed()
    {
        // An open campaign has responses, so whether an address is invited to it is asked of nobody: the add is refused
        // for the campaign's state before the address is looked at.
        var campaignId = await SeedDraftCampaignAsync();
        await using (var db = CreateDb())
        {
            await AddThroughTheAuditPipelineAsync(db, campaignId, Invited, MsfRespondentCategory.PeerDoctor);
            var campaign = await db.MsfCampaigns.SingleAsync(candidate => candidate.Id == campaignId);
            campaign.State = MsfCampaignState.Open;
            await db.SaveChangesAsync();
        }

        await using (var db = CreateDb())
        {
            var again = () => AddThroughTheAuditPipelineAsync(db, campaignId, Invited, MsfRespondentCategory.PeerDoctor);
            (await again.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage("Invitations can only be added while a campaign is in draft.");
        }
    }

    [Fact]
    public async Task SomeoneWhoDoesNotRunTheCampaign_IsRefusedForTheCampaign_NeverTheAddress_SoItsInviteesCannotBeProbed()
    {
        // The scope check comes before the address is looked at (T113, T228 review). A coordinator elsewhere who asked
        // about an address the draft invites would otherwise learn two things the page never tells them: that the id
        // names a campaign ("already invites" is not the scope refusal an id that names nothing gets), and whom it
        // invites. So they get exactly the refusal an uninvited address gets, and a campaign id that names nothing.
        var campaignId = await SeedDraftCampaignAsync(subjectInstitution: HostInstitution);
        await using (var db = CreateDb())
        {
            await AddThroughTheAuditPipelineAsync(db, campaignId, Invited, MsfRespondentCategory.PeerDoctor);
        }

        var outsider = TestPrincipals.Coordinator(OtherInstitution, "coordinator-elsewhere");
        var invited = await RefusalAsync(campaignId, Invited.ToUpperInvariant(), outsider);
        var notInvited = await RefusalAsync(campaignId, "peer-10@example.test", outsider);
        var noSuchCampaign = await RefusalAsync(campaignId + 1000, Invited, outsider);

        invited.Should().BeOfType<UnauthorizedAccessException>();
        invited.Message.Should().NotBe(AddMsfInvitationCommandHandler.AlreadyInvited);
        notInvited.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(invited.Message);
        noSuchCampaign.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(invited.Message);

        // Guard: the campaign's own coordinator, whom the check does answer, is told the address is already invited.
        var own = await RefusalAsync(campaignId, Invited.ToUpperInvariant(), TestPrincipals.Coordinator(HostInstitution));
        own.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be(AddMsfInvitationCommandHandler.AlreadyInvited);

        await using var read = CreateDb();
        (await read.MsfInvitations.AsNoTracking().CountAsync(invitation => invitation.CampaignId == campaignId))
            .Should().Be(1, "no refusal stored anything");
    }

    /// <summary>The audit behaviour outermost, then the handler: the app's order.</summary>
    private static Task<int> AddThroughTheAuditPipelineAsync(
        ApplicationDbContext db, int campaignId, string address, MsfRespondentCategory category, ClaimsPrincipal? principal = null)
    {
        var command = new AddMsfInvitationCommand(campaignId, address, category, principal ?? TestPrincipals.Administrator());
        var handler = new AddMsfInvitationCommandHandler(db, new InvitationTokenService(), FakeUserDirectory.Trainees("trainee-1"));

        return new AuditPipelineBehavior<AddMsfInvitationCommand, int>(new AuditWriter(db), new FixedAuditContext())
            .Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None);
    }

    /// <summary>What an add refuses with, run in a context of its own as a request would be.</summary>
    private async Task<Exception> RefusalAsync(int campaignId, string address, ClaimsPrincipal principal)
    {
        await using var db = CreateDb();
        var thrown = await Record.ExceptionAsync(
            () => AddThroughTheAuditPipelineAsync(db, campaignId, address, MsfRespondentCategory.Consultant, principal));
        thrown.Should().NotBeNull("the add must be refused");
        return thrown!;
    }

    /// <summary>
    /// A draft about trainee-1, a current trainee admitted at <paramref name="subjectInstitution" />, or at an institution
    /// no test's coordinator runs when none is given (only an Administrator runs the campaign then). A current trainee,
    /// because inviting to a draft asks for one (T284).
    /// </summary>
    private async Task<int> SeedDraftCampaignAsync(int erasedInvitations = 0, int? subjectInstitution = null)
    {
        await using var db = CreateDb();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var institutionId = subjectInstitution ?? 999;
        if (!await db.Set<TraineeProfile>().AnyAsync(profile => profile.UserId == "trainee-1"))
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                UserId = "trainee-1", InstitutionId = institutionId, CurriculumId = 1,
                ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                IsActive = true
            });
        }

        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = today,
            ClosesOn = today.AddDays(14),
            State = MsfCampaignState.Draft,
            Template = new MsfTemplate
            {
                Name = "Annual MSF",
                Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
            },
            Invitations = Enumerable.Range(0, erasedInvitations)
                .Select(_ => new MsfInvitation
                {
                    RespondentEmail = null,
                    RespondentCategory = MsfRespondentCategory.Nurse,
                    TokenHash = "erased",
                    IssuedOn = DateTime.UtcNow,
                    ExpiresOn = today.AddDays(21),
                    AnonymizedOn = DateTime.UtcNow
                })
                .ToList()
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
