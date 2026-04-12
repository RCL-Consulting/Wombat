using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Layout;

namespace Wombat.Web.Tests.Navigation;

public sealed class NavMenuAuthorizationTests : TestContext
{
    [Theory]
    [InlineData(WombatRoles.Trainee, new[] { "Activities", "My Activities", "My Curriculum", "My Account", "Logout" })]
    [InlineData(WombatRoles.Assessor, new[] { "Activity Inbox", "Recent Activities", "My Account", "Logout" })]
    [InlineData(WombatRoles.Coordinator, new[] { "Invitations", "Stalled Activities", "My Account", "Logout" })]
    [InlineData(WombatRoles.Administrator, new[] { "Institutions", "Invitations", "Users", "Activity Types", "System", "My Account", "Logout" })]
    public void NavMenu_ShowsExpectedLinksForRole(string role, string[] expectedLinks)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("user@example.com");
        auth.SetRoles(role);

        var cut = RenderComponent<NavMenu>();
        var text = cut.Markup;

        foreach (var link in expectedLinks)
        {
            text.Should().Contain(link);
        }
    }
}
