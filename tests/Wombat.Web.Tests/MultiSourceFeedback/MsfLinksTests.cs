using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// T369 (flow 10, build-contracts § 0): the MSF routes are built in one place, <see cref="MsfLinks" />, and each one is a
/// route the page it names declares, so a page's <c>@page</c> and the links to it cannot drift apart.
/// </summary>
public sealed class MsfLinksTests
{
    [Fact]
    public void TheAddresses()
    {
        MsfLinks.Campaigns.Should().Be("/msf/campaigns");
        MsfLinks.NewCampaign.Should().Be("/msf/campaigns/new");
        MsfLinks.Coverage.Should().Be("/msf/coverage");
        MsfLinks.MyReports.Should().Be("/msf/my-reports");
        MsfLinks.Campaign(12).Should().Be("/msf/campaigns/12");
        MsfLinks.Report(12).Should().Be("/msf/reports/12");
        MsfLinks.MyReport(12).Should().Be("/msf/my-reports/12");
    }

    [Fact]
    public void AnIdIsWrittenInPlainDigits_WhateverTheCulture()
    {
        var before = Thread.CurrentThread.CurrentCulture;
        try
        {
            // A culture that groups thousands would write 12 345 or 12.345 through an unqualified interpolation.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            MsfLinks.Campaign(12345).Should().Be("/msf/campaigns/12345");
            MsfLinks.Report(12345).Should().Be("/msf/reports/12345");
            MsfLinks.MyReport(12345).Should().Be("/msf/my-reports/12345");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = before;
        }
    }

    public static TheoryData<string, Type> EveryLinkAndItsPage() => new()
    {
        { MsfLinks.Campaigns, typeof(CampaignsList) },
        { MsfLinks.NewCampaign, typeof(NewCampaign) },
        { MsfLinks.Coverage, typeof(ProgrammeCoverage) },
        { MsfLinks.MyReports, typeof(MyMsfReports) },
        { MsfLinks.Campaign(7), typeof(CampaignEdit) },
        { MsfLinks.Report(7), typeof(CampaignReport) },
        { MsfLinks.MyReport(7), typeof(MyMsfReport) },
    };

    [Theory]
    [MemberData(nameof(EveryLinkAndItsPage))]
    public void EachLink_IsARouteItsPageDeclares(string link, Type page)
    {
        var templates = page.GetCustomAttributes<RouteAttribute>().Select(route => route.Template).ToList();

        templates.Should().Contain(template => Matches(template, link),
            $"{page.Name} declares {string.Join(", ", templates)}, and one of them should take {link}");
    }

    // A route template as a pattern: "{CampaignId:int}" takes plain digits, anything else is literal.
    private static bool Matches(string template, string link)
    {
        var pattern = Regex.Replace(Regex.Escape(template), @"\\{[A-Za-z]+:int}", "[0-9]+");
        return Regex.IsMatch(link, $"^{pattern}$");
    }
}
