using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Pages.Admin.Assessors;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Components.Pages.Admin.Sso;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T376 (flow 12, build-contracts § 0): the people pages' addresses are built in one place, <see cref="PeopleLinks" />; each
/// is a route its page takes, and the user page's way back to Users is allow-listed (S6), so a crafted <c>back</c> can send
/// nobody to another page or host.
/// </summary>
public sealed class PeopleLinksTests
{
    [Fact]
    public void TheAddresses()
    {
        PeopleLinks.Users.Should().Be("/admin/users");
        PeopleLinks.Assessors.Should().Be("/admin/assessors");
        PeopleLinks.LegacyAssessorEdit.Should().Be("/admin/assessors/edit");
        PeopleLinks.SsoMappings.Should().Be("/admin/sso/group-mappings");
        PeopleLinks.User("u-1/x").Should().Be("/admin/users/u-1%2Fx");
        PeopleLinks.UserTraining("u 1").Should().Be("/admin/users/u%201?section=training");
        PeopleLinks.Institution(1).Should().Be("/admin/institutions/1");
        PeopleLinks.UsersFiltered(role: "Assessor").Should().Be("/admin/users?role=Assessor");
        PeopleLinks.UsersFiltered(q: "thandi zulu", page: 2, size: 20).Should().Be("/admin/users?q=thandi%20zulu&page=2&size=20");
        PeopleLinks.UsersFiltered().Should().Be("/admin/users");
        PeopleLinks.User("u-1", back: "/admin/users?role=Assessor").Should().Be("/admin/users/u-1?back=%2Fadmin%2Fusers%3Frole%3DAssessor");
    }

    [Fact]
    public void ThePlainLinks_AgreeWithOnboardingsUserLink_AndTheMenu()
    {
        PeopleLinks.User("u-1/x").Should().Be(OnboardingLinks.User("u-1/x"));
        NavItems.Users.Href.Should().Be(PeopleLinks.Users);
        NavItems.Assessors.Href.Should().Be(PeopleLinks.Assessors);
        NavItems.SsoMappings.Href.Should().Be(PeopleLinks.SsoMappings);
    }

    [Theory]
    [InlineData(null, "/admin/users")]
    [InlineData("", "/admin/users")]
    [InlineData("/admin/users", "/admin/users")]
    [InlineData("/admin/users?role=Assessor&page=2", "/admin/users?role=Assessor&page=2")]
    [InlineData("/admin/users?q=thandi%20zulu&status=active&size=50", "/admin/users?q=thandi%20zulu&status=active&size=50")]
    [InlineData("/admin/users?role=none", "/admin/users?role=none")]
    [InlineData("/admin/users?page=abc", "/admin/users")]
    [InlineData("/admin/users?returnUrl=https://evil.example", "/admin/users")]
    [InlineData("https://evil.example/admin/users", "/admin/users")]
    [InlineData("//evil.example/admin/users", "/admin/users")]
    [InlineData("/admin/users/other-person", "/admin/users")]
    [InlineData("/admin/invitations", "/admin/users")]
    [InlineData("/Admin/Users?role=Assessor", "/admin/users")]
    public void TheWayBackToUsers_IsAllowListed(string? back, string expected)
        => PeopleLinks.SafeUsersReturn(back).Should().Be(expected);

    public static TheoryData<string, Type> EveryLinkToAPageThatExists() => new()
    {
        { PeopleLinks.Users, typeof(UsersList) },
        { "/admin/users/{UserId}", typeof(UserDetail) },
        { PeopleLinks.Assessors, typeof(AssessorsList) },
        { PeopleLinks.LegacyAssessorEdit, typeof(AssessorProfileEdit) },
        { PeopleLinks.SsoMappings, typeof(GroupMappings) },
        { "/admin/institutions/{Id:int}", typeof(InstitutionEdit) }
    };

    [Theory]
    [MemberData(nameof(EveryLinkToAPageThatExists))]
    public void EachLink_IsARouteItsPageTakes(string route, Type page)
        => page.GetCustomAttributes<RouteAttribute>().Select(attribute => attribute.Template).Should().Contain(route);
}
