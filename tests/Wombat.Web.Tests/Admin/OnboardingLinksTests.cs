using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Pages.Admin.Adoptions;
using Wombat.Web.Components.Pages.Admin.Invitations;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T373 (flow 11, build-contracts § 0): the onboarding routes are built in one place, <see cref="OnboardingLinks" />, and
/// each one a page already declares is a route that page takes. The two new pages (Invite a person, Admit) are their lanes'
/// to add here once they exist.
/// </summary>
public sealed class OnboardingLinksTests
{
    [Fact]
    public void TheAddresses()
    {
        OnboardingLinks.Invitations.Should().Be("/admin/invitations");
        OnboardingLinks.InviteAPerson.Should().Be("/admin/invitations/new");
        OnboardingLinks.InviteFrom(42).Should().Be("/admin/invitations/new?from=42");
        OnboardingLinks.Trainees.Should().Be("/admin/trainees");
        OnboardingLinks.Admit("a b/c").Should().Be("/admin/trainees/admit?userId=a%20b%2Fc");
        OnboardingLinks.Admitted(7).Should().Be("/admin/trainees?admitted=7");
        OnboardingLinks.BackFromAdmit("a b/c").Should().Be("/admin/trainees?back=a%20b%2Fc");
        OnboardingLinks.User("u-1/x").Should().Be("/admin/users/u-1%2Fx");
        OnboardingLinks.CurriculumAdoptions.Should().Be("/admin/adoptions");
    }

    [Fact]
    public void AnIdIsWrittenInPlainDigits_WhateverTheCulture()
    {
        var before = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            OnboardingLinks.InviteFrom(12345).Should().Be("/admin/invitations/new?from=12345");
            OnboardingLinks.Admitted(12345).Should().Be("/admin/trainees?admitted=12345");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = before;
        }
    }

    /// <summary>The menu's items and the links name the same pages: one address each.</summary>
    [Fact]
    public void TheMenusItems_AgreeWithTheLinks()
    {
        NavItems.Invitations.Href.Should().Be(OnboardingLinks.Invitations);
        NavItems.Trainees.Href.Should().Be(OnboardingLinks.Trainees);
        NavItems.CurriculumAdoptions.Href.Should().Be(OnboardingLinks.CurriculumAdoptions);
    }

    public static TheoryData<string, Type> EveryLinkToAPageThatExists() => new()
    {
        { OnboardingLinks.Invitations, typeof(InvitationsList) },
        { OnboardingLinks.InviteAPerson, typeof(InviteAPerson) },
        { OnboardingLinks.InviteFrom(12), typeof(InviteAPerson) },
        { OnboardingLinks.Trainees, typeof(PendingTraineesList) },
        { OnboardingLinks.Admit("u-1"), typeof(AdmitRegistrar) },
        { OnboardingLinks.User("u-1"), typeof(UserDetail) },
        { OnboardingLinks.CurriculumAdoptions, typeof(AdoptionsList) },
    };

    [Theory]
    [MemberData(nameof(EveryLinkToAPageThatExists))]
    public void EachLink_IsARouteItsPageDeclares(string link, Type page)
    {
        var path = link.Split('?')[0];
        var templates = page.GetCustomAttributes<RouteAttribute>().Select(route => route.Template).ToList();

        templates.Should().Contain(template => Matches(template, path),
            $"{page.Name} declares {string.Join(", ", templates)}, and one of them should take {path}");
    }

    // A route template as a pattern: "{UserId}" takes one segment, anything else is literal.
    private static bool Matches(string template, string path)
    {
        var pattern = Regex.Replace(Regex.Escape(template), @"\\{[A-Za-z]+(:[a-z]+)?}", "[^/]+");
        return Regex.IsMatch(path, $"^{pattern}$");
    }
}
