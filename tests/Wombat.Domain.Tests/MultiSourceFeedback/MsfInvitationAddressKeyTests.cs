using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.MultiSourceFeedback;

/// <summary>
/// Two invitations to one campaign name the same respondent when their addresses agree after trimming, in any capitals.
/// (T228)
/// </summary>
/// <remarks>
/// That the add command refuses the second is <c>MsfInvitationAddressOnceTests</c>; that the database refuses it too,
/// racing adds included, is <c>MsfInvitationAddressOncePostgresTests</c>.
/// </remarks>
public sealed class MsfInvitationAddressKeyTests
{
    [Theory]
    [InlineData("peer-9@example.test")]
    [InlineData("PEER-9@EXAMPLE.TEST")]
    [InlineData("Peer-9@Example.Test")]
    [InlineData("  peer-9@example.test  ")]
    [InlineData("\tPeer-9@example.test\n")]
    public void AnAddressTypedAnyWay_HasOneKey(string typed)
        => Assert.Equal("peer-9@example.test", MsfInvitation.AddressKey(typed));

    [Fact]
    public void DifferentAddresses_HaveDifferentKeys()
        => Assert.NotEqual(MsfInvitation.AddressKey("peer-9@example.test"), MsfInvitation.AddressKey("peer-8@example.test"));
}
