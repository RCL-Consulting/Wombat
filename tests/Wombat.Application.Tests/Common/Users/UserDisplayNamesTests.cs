using FluentAssertions;
using Wombat.Application.Common.Users;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Common.Users;

/// <summary>
/// The one lookup and the one fallback rule behind every page that names people. (T142)
/// </summary>
public sealed class UserDisplayNamesTests
{
    [Fact]
    public async Task EveryDistinctIdIsLookedUpOnce_InOneCall()
    {
        var users = new FakeUserDirectory(("amara", "Amara Okafor"), ("bongani", "Bongani Dlamini"));

        await UserDisplayNames.ResolveAsync(users, ["amara", "bongani", "amara", null, "", "amara"], CancellationToken.None);

        users.Lookups.Should().ContainSingle()
            .Which.Should().Equal(["amara", "bongani"], "a page of fifty rows about three people is one call about three ids");
    }

    [Fact]
    public async Task APageThatNamesNobody_MakesNoCall()
    {
        var users = new FakeUserDirectory(("amara", "Amara Okafor"));

        var names = await UserDisplayNames.ResolveAsync(users, [null, ""], CancellationToken.None);

        users.Lookups.Should().BeEmpty();
        names.NameOf("amara").Should().Be("amara", "nothing was looked up, so there is no name to give");
    }

    [Fact]
    public async Task AUserIsShownByName_AndTheIdOnlyWhereThereIsNoName()
    {
        var users = new FakeUserDirectory(("amara", "Amara Okafor"), ("nameless", " "));

        var names = await UserDisplayNames.ResolveAsync(users, ["amara", "departed", "nameless"], CancellationToken.None);

        names.NameOf("amara").Should().Be("Amara Okafor");
        names.NameOf("departed").Should().Be("departed", "no user by that id exists any more");
        names.NameOf("nameless").Should().Be("nameless", "a blank cell would read as nobody");
    }
}
