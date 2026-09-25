using Wombat.Domain.Invitations;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.Invitations;

/// <summary>
/// What became of an account invitation's mail, as the invitation holds it: which links count as sent, being sent or not
/// delivered, when the list says to check the address, and the key a link's mail carries. (T283)
/// </summary>
public sealed class InvitationDeliveryTests
{
    private static readonly DateTime IssuedAt = new(2029, 3, 7, 9, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Soon = IssuedAt.AddMinutes(10);
    private static readonly DateTime Late = IssuedAt + Invitation.DeliveryReportDeadline + TimeSpan.FromMinutes(1);

    private static readonly string Hash = new string('A', 60) + "0f9E";

    // ─── What became of the mail ─────────────────────────────────────────────

    [Fact]
    public void AMailReportedSent_IsSent_HoweverLongAgo()
    {
        Assert.Equal(InvitationDelivery.Sent, Invitation.DeliveryOf(IssuedAt.AddSeconds(3), null, IssuedAt, Late.AddDays(5)));
    }

    [Fact]
    public void AMailReportedDropped_IsNotDelivered_AtOnce()
    {
        Assert.Equal(InvitationDelivery.NotDelivered, Invitation.DeliveryOf(null, IssuedAt.AddSeconds(8), IssuedAt, Soon));
    }

    [Fact]
    public void AMailNotReportedYet_IsBeingSent_ForAnHour_ThenNotDelivered()
    {
        Assert.Equal(InvitationDelivery.BeingSent, Invitation.DeliveryOf(null, null, IssuedAt, Soon));
        Assert.Equal(
            InvitationDelivery.BeingSent,
            Invitation.DeliveryOf(null, null, IssuedAt, IssuedAt + Invitation.DeliveryReportDeadline));
        Assert.Equal(InvitationDelivery.NotDelivered, Invitation.DeliveryOf(null, null, IssuedAt, Late));
    }

    [Fact]
    public void TheHourIsAnMsfLinksHour()
    {
        Assert.Equal(MsfInvitation.DeliveryReportDeadline, Invitation.DeliveryReportDeadline);
    }

    [Fact]
    public void AnInvitationAsksTheSameRule()
    {
        var invitation = new Invitation { IssuedOn = IssuedAt, DeliveryFailedOn = IssuedAt.AddSeconds(8) };

        Assert.Equal(InvitationDelivery.NotDelivered, invitation.DeliveryAt(Soon));
    }

    // ─── When to say to check the address ────────────────────────────────────

    [Theory]
    [InlineData(InvitationDelivery.NotDelivered, 0, false)]
    [InlineData(InvitationDelivery.NotDelivered, 1, false)]
    [InlineData(InvitationDelivery.NotDelivered, 2, true)]
    [InlineData(InvitationDelivery.NotDelivered, 5, true)]
    [InlineData(InvitationDelivery.Sent, 2, false)]
    [InlineData(InvitationDelivery.BeingSent, 3, false)]
    public void TheAddressIsQuestioned_OnlyFromTheSecondFailure_AndOnlyWhileNotDelivered(
        InvitationDelivery delivery, int failures, bool expected)
    {
        Assert.Equal(expected, Invitation.SuggestsCheckingAddress(delivery, failures));
    }

    // ─── The link's lifetime ─────────────────────────────────────────────────

    [Fact]
    public void ALinkWorksFourteenDaysFromItsIssue()
    {
        Assert.Equal(new DateOnly(2029, 3, 21), Invitation.LinkExpiresOn(IssuedAt));
    }

    // ─── The key a link's mail carries ───────────────────────────────────────

    [Fact]
    public void TheDeliveryKey_NamesTheInvitationAndTheLinksHash_AndReadsBack()
    {
        var key = Invitation.DeliveryKey(42, Hash);

        Assert.True(Invitation.TryReadDeliveryKey(key, out var invitationId, out var tokenHash));
        Assert.Equal(42, invitationId);
        Assert.Equal(Hash, tokenHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("msf-link:42:AbC-_12xyzAbC-_1")]
    [InlineData("job-mail:0123456789abcdef")]
    [InlineData("account-invitation:42")]
    [InlineData("account-invitation:0:" + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("account-invitation:-1:" + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("account-invitation:42:not-a-hash")]
    [InlineData("account-invitation:42:" + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAG")]
    [InlineData("account-invitation:42:" + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA:extra")]
    public void AnyOtherKey_ReadsAsNoInvitation(string? key)
    {
        Assert.False(Invitation.TryReadDeliveryKey(key, out var invitationId, out var tokenHash));
        Assert.Equal(0, invitationId);
        Assert.Equal(string.Empty, tokenHash);
    }

    [Fact]
    public void AKeyIsRefused_ForNoInvitation_OrSomethingThatIsNotAHash()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Invitation.DeliveryKey(0, Hash));
        Assert.Throws<ArgumentException>(() => Invitation.DeliveryKey(42, "token"));
        Assert.Throws<ArgumentException>(() => Invitation.DeliveryKey(42, Hash + ":1"));
    }
}
