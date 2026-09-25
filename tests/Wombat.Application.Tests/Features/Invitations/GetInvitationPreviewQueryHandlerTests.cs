using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Invitations;

public sealed class GetInvitationPreviewQueryHandlerTests
{
    private readonly InvitationTokenService _tokenService = new();
    private readonly AddressStatusProvisioner _provisioner = new();

    [Fact]
    public async Task Handle_RejectsExpiredInvitation()
    {
        var (dbContext, token) = await CreateDbContextWithInvitationAsync(
            invitation => invitation.ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        var handler = new GetInvitationPreviewQueryHandler(dbContext, _tokenService, _provisioner);

        var act = () => handler.Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        // A refusal the register page says in its own words (T285); still an InvalidOperationException, so nothing that
        // catches one changes.
        (await act.Should().ThrowAsync<InvitationRefusedException>().WithMessage("*expired*"))
            .Which.Reason.Should().Be(InvitationRefusal.Expired);
    }

    [Fact]
    public async Task Handle_RejectsRevokedInvitation()
    {
        var (dbContext, token) = await CreateDbContextWithInvitationAsync(
            invitation => invitation.RevokedOn = DateTime.UtcNow);

        var handler = new GetInvitationPreviewQueryHandler(dbContext, _tokenService, _provisioner);

        var act = () => handler.Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        // A refusal the register page says in its own words (T285); still an InvalidOperationException, so nothing that
        // catches one changes.
        (await act.Should().ThrowAsync<InvitationRefusedException>().WithMessage("*revoked*"))
            .Which.Reason.Should().Be(InvitationRefusal.Revoked);
    }

    [Fact]
    public async Task Handle_RejectsUsedInvitation()
    {
        var (dbContext, token) = await CreateDbContextWithInvitationAsync(
            invitation => invitation.UsedOn = DateTime.UtcNow);

        var handler = new GetInvitationPreviewQueryHandler(dbContext, _tokenService, _provisioner);

        var act = () => handler.Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        // A refusal the register page says in its own words (T285); still an InvalidOperationException, so nothing that
        // catches one changes.
        (await act.Should().ThrowAsync<InvitationRefusedException>().WithMessage("*used*"))
            .Which.Reason.Should().Be(InvitationRefusal.Used);
    }

    [Fact]
    public async Task Handle_RefusesATokenNoInvitationHolds_AsInvalid()
    {
        var (dbContext, _) = await CreateDbContextWithInvitationAsync();

        var handler = new GetInvitationPreviewQueryHandler(dbContext, _tokenService, _provisioner);

        var act = () => handler.Handle(new GetInvitationPreviewQuery(_tokenService.GenerateToken()), CancellationToken.None);

        (await act.Should().ThrowAsync<InvitationRefusedException>().WithMessage("This invitation is invalid."))
            .Which.Reason.Should().Be(InvitationRefusal.Invalid);
    }

    [Fact]
    public async Task Handle_PreviewsAnInvitationWhoseAddressAnAccountCanBeCreatedFor()
    {
        var (dbContext, token) = await CreateDbContextWithInvitationAsync();

        var preview = await new GetInvitationPreviewQueryHandler(dbContext, _tokenService, _provisioner)
            .Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        preview.Should().Be(new InvitationPreviewDto("invitee@example.test", "Assessor"));
        _provisioner.AskedAbout.Should().Equal("invitee@example.test");
    }

    /// <summary>
    /// T285 review: the register page offers its form only for an invitation the preview answers, and it keeps the form
    /// under a refusal the person can put right. An invitation to an address an account holds was previewed, so every
    /// submit came back to the form, refused as AccountExists again. The preview now refuses it as the submit does, by the
    /// provisioner's own test.
    /// </summary>
    [Theory]
    [InlineData(InvitedAddressStatus.Taken, InvitationRefusal.AccountExists, "A user with this email address already exists.")]
    [InlineData(InvitedAddressStatus.NotAccepted, InvitationRefusal.AddressNotAccepted,
        "This email address cannot be used for an account. Ask your administrator for an invitation to another address.")]
    public async Task Handle_RefusesAnInvitationWhoseAddressNoAccountCanBeCreatedFor_AsTheSubmitWould(
        InvitedAddressStatus status,
        InvitationRefusal expected,
        string sentence)
    {
        var (dbContext, token) = await CreateDbContextWithInvitationAsync();
        _provisioner.Status = status;

        var act = () => new GetInvitationPreviewQueryHandler(dbContext, _tokenService, _provisioner)
            .Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        (await act.Should().ThrowAsync<InvitationRefusedException>().WithMessage(sentence))
            .Which.Reason.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_RefusesAnAcceptedInvitation_AsUsed_ThoughItsAccountNowHoldsTheAddress()
    {
        var (dbContext, token) = await CreateDbContextWithInvitationAsync(invitation => invitation.UsedOn = DateTime.UtcNow);
        _provisioner.Status = InvitedAddressStatus.Taken;

        var act = () => new GetInvitationPreviewQueryHandler(dbContext, _tokenService, _provisioner)
            .Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        (await act.Should().ThrowAsync<InvitationRefusedException>()).Which.Reason.Should().Be(InvitationRefusal.Used,
            "the invitation's own state is judged first: the account a registration created holds its address");
        _provisioner.AskedAbout.Should().BeEmpty();
    }

    [Fact]
    public void AnAddressStatus_MapsToItsRefusal_AndAnAvailableAddressToNone()
    {
        InvitationRefusals.For(InvitedAddressStatus.Available).Should().BeNull();
        InvitationRefusals.For(InvitedAddressStatus.Taken).Should().Be(InvitationRefusal.AccountExists);
        InvitationRefusals.For(InvitedAddressStatus.NotAccepted).Should().Be(InvitationRefusal.AddressNotAccepted);
        Enum.GetValues<InvitedAddressStatus>().Should().OnlyContain(status =>
            status == InvitedAddressStatus.Available || InvitationRefusals.For(status) != null);
    }

    [Fact]
    public void EveryRefusal_HasASentence_WhichIsTheExceptionsMessage()
    {
        foreach (var reason in Enum.GetValues<InvitationRefusal>())
        {
            var sentence = InvitationRefusals.Describe(reason);
            sentence.Should().NotBeNullOrWhiteSpace();
            new InvitationRefusedException(reason).Message.Should().Be(sentence);
            new InvitationRefusedException(reason).ErrorCodes.Should().BeEmpty();
        }

        Enum.GetValues<InvitationRefusal>().Select(InvitationRefusals.Describe).Should().OnlyHaveUniqueItems();
    }

    /// <summary>Answers the preview's question about the address with <see cref="Status" />, and records who asked.</summary>
    private sealed class AddressStatusProvisioner : IInvitedUserProvisioner
    {
        public InvitedAddressStatus Status { get; set; } = InvitedAddressStatus.Available;

        public List<string> AskedAbout { get; } = [];

        public Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default)
        {
            AskedAbout.Add(email);
            return Task.FromResult(Status);
        }

        public Task<ProvisionedInvitationUser> ProvisionAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            string targetRole,
            int? institutionId,
            int? collegeId,
            int? specialityId,
            int? subSpecialityId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException("A preview creates no account.");
    }

    private async Task<(ApplicationDbContext DbContext, string Token)> CreateDbContextWithInvitationAsync(Action<Invitation>? configure = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        var token = _tokenService.GenerateToken();
        var invitation = new Invitation
        {
            Email = "invitee@example.test",
            TokenHash = _tokenService.HashToken(token),
            TargetRole = "Assessor",
            InstitutionId = 1,
            SpecialityId = 2,
            SubSpecialityId = 3,
            IssuedByUserId = "admin-1",
            IssuedOn = DateTime.UtcNow.AddDays(-1),
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))
        };

        configure?.Invoke(invitation);

        dbContext.Invitations.Add(invitation);
        await dbContext.SaveChangesAsync();
        return (dbContext, token);
    }
}
