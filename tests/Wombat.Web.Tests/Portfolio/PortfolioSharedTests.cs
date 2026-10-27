using System.Reflection;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Pages.Programme;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;

namespace Wombat.Web.Tests.Portfolio;

/// <summary>
/// T379 (flow 13, build-contracts § 0): the addresses, words and header action flow 13's lanes share. Each address is a route
/// its page takes and agrees with the menu; the staff export's action is drawn only where the page's DTO says
/// <c>MayExport</c> (F1, M1), with the words every page that offers it prints.
/// </summary>
public sealed class PortfolioSharedTests : TestContext
{
    [Fact]
    public void TheAddresses()
    {
        PortfolioLinks.ExportFor("u-1/x").Should().Be("/portfolio/export/u-1%2Fx");
        PortfolioLinks.TraineeProfile(2).Should().Be("/admin/trainees/edit?id=2");
        PortfolioLinks.ProgrammeTrainee(2).Should().Be("/programme/trainees/2");
        PortfolioLinks.Verify.Should().Be("/portfolio/verify");
    }

    [Fact]
    public void TheLinks_AgreeWithTheMenu()
    {
        NavItems.MyProgress.Href.Should().Be(PortfolioLinks.MyProgress);
        NavItems.MyActivities.Href.Should().Be(PortfolioLinks.MyActivities);
        NavItems.ExportPortfolio.Href.Should().Be(PortfolioLinks.Export);
        NavItems.Trainees.Href.Should().Be(PortfolioLinks.Trainees);
        NavItems.ProgrammeTrainees.Href.Should().Be(PortfolioLinks.ProgrammeTrainees);
    }

    public static TheoryData<string, Type> EveryLinkToAPageThatExists() => new()
    {
        { PortfolioLinks.MyProgress, typeof(MyProgress) },
        { PortfolioLinks.MyAuthorisations, typeof(MyAuthorisations) },
        { PortfolioLinks.MyActivities, typeof(MyActivities) },
        { PortfolioLinks.Export, typeof(ExportPortfolio) },
        // Lane E's (S2): the staff route is its own component, so the menu lights its owner by route.
        { "/portfolio/export/{TraineeUserId}", typeof(ExportTraineePortfolio) },
        { PortfolioLinks.Verify, typeof(VerifyExport) },
        { "/admin/trainees/edit", typeof(TraineeProfileEdit) },
        { "/programme/trainees/{ProfileId:int}", typeof(ProgrammeTraineeDetail) },
    };

    [Theory]
    [MemberData(nameof(EveryLinkToAPageThatExists))]
    public void EachLink_IsARouteItsPageTakes(string route, Type page)
        => page.GetCustomAttributes<RouteAttribute>().Select(attribute => attribute.Template).Should().Contain(route);

    /// <summary>The staff route is lane E's to move into a component of its own (S2); whichever page takes it, one does.</summary>
    [Fact]
    public void TheStaffExport_IsARouteSomePageTakes()
        => typeof(ExportPortfolio).Assembly.GetTypes()
            .SelectMany(type => type.GetCustomAttributes<RouteAttribute>())
            .Select(attribute => attribute.Template)
            .Should().Contain("/portfolio/export/{TraineeUserId}");

    [Fact]
    public void TheWords()
    {
        PortfolioWords.ExportAction("Lerato Molefe").Should().Be("Export Lerato Molefe's portfolio");
        PortfolioWords.ProgrammeOf("Pieter du Plessis").Should().Be("Pieter du Plessis's programme");
    }

    [Fact]
    public void TheAction_IsAnOutlineLinkToTheStaffExport_WhereTheExportWouldNotBeRefused()
    {
        var cut = RenderComponent<ExportPortfolioAction>(parameters => parameters
            .Add(action => action.TraineeUserId, "u-2")
            .Add(action => action.TraineeName, "Lerato Molefe")
            .Add(action => action.MayExport, true));

        var link = cut.Find("a");
        link.ClassList.Should().BeEquivalentTo(["btn", "btn-outline"]);
        link.GetAttribute("href").Should().Be("/portfolio/export/u-2");
        link.TextContent.Trim().Should().Be("Export Lerato Molefe's portfolio");
        link.QuerySelector("svg.icon use")!.GetAttribute("href").Should().Be("/icons/download.svg#i");
    }

    [Fact]
    public void TheAction_DrawsNothing_WhereTheExportWouldBeRefused()
    {
        var cut = RenderComponent<ExportPortfolioAction>(parameters => parameters
            .Add(action => action.TraineeUserId, "u-2")
            .Add(action => action.TraineeName, "Lerato Molefe")
            .Add(action => action.MayExport, false));

        cut.Markup.Trim().Should().BeEmpty("M1: no page offers an export that would be refused");
    }
}
