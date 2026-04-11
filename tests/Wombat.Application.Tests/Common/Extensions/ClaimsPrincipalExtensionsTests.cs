using System.Security.Claims;
using FluentAssertions;
using Wombat.Application.Common.Extensions;
using Wombat.Infrastructure.Identity;

namespace Wombat.Application.Tests.Common.Extensions;

public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetInstitutionId_ReturnsExpectedValue()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(WombatClaims.InstitutionId, "42")
            ],
            "test"));

        principal.GetInstitutionId().Should().Be(42);
    }
}
