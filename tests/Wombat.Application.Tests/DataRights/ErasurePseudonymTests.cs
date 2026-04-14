using FluentAssertions;
using Wombat.Infrastructure.DataRights;

namespace Wombat.Application.Tests.DataRights;

public sealed class ErasurePseudonymTests
{
    [Fact]
    public void GeneratePseudonym_IsDeterministic()
    {
        var p1 = ErasureExecutor.GeneratePseudonym("user-123", "my-salt");
        var p2 = ErasureExecutor.GeneratePseudonym("user-123", "my-salt");

        p1.Should().Be(p2);
    }

    [Fact]
    public void GeneratePseudonym_StartsWithDeletedUserPrefix()
    {
        var pseudonym = ErasureExecutor.GeneratePseudonym("user-1", "salt");

        pseudonym.Should().StartWith("deleted_user_");
    }

    [Fact]
    public void GeneratePseudonym_DifferentSalts_ProduceDifferentPseudonyms()
    {
        var p1 = ErasureExecutor.GeneratePseudonym("user-1", "salt-a");
        var p2 = ErasureExecutor.GeneratePseudonym("user-1", "salt-b");

        p1.Should().NotBe(p2);
    }

    [Fact]
    public void GeneratePseudonym_DifferentUsers_ProduceDifferentPseudonyms()
    {
        var p1 = ErasureExecutor.GeneratePseudonym("user-1", "salt");
        var p2 = ErasureExecutor.GeneratePseudonym("user-2", "salt");

        p1.Should().NotBe(p2);
    }

    [Fact]
    public void GeneratePseudonym_HasExpectedLength()
    {
        var pseudonym = ErasureExecutor.GeneratePseudonym("user-1", "salt");

        // "deleted_user_" (13 chars) + 8 hex chars = 21
        pseudonym.Should().HaveLength(21);
    }
}
