using FluentAssertions;
using Wombat.Application.Common.Security;

namespace Wombat.Application.Tests.Features.Invitations;

public sealed class InvitationTokenServiceTests
{
    private readonly InvitationTokenService _service = new();

    [Fact]
    public void GenerateToken_ProducesUniqueValues()
    {
        var tokens = Enumerable.Range(0, 32)
            .Select(_ => _service.GenerateToken())
            .ToArray();

        tokens.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void HashAndVerify_RoundTrips()
    {
        var token = _service.GenerateToken();
        var hash = _service.HashToken(token);

        _service.VerifyToken(token, hash).Should().BeTrue();
        _service.VerifyToken("different-token", hash).Should().BeFalse();
    }
}
