using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Components.Pages.Admin.Epas;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The catalogue pages' addresses and the one-shot tokens a result crosses a page change by (T392, flow 15, build-contracts
/// § 0, F1, M5, M6). The item pages' own routes are lane B's; it adds them to <see cref="EveryLinkToAPageThatExists" />.
/// </summary>
public sealed class CatalogueLinksTests
{
    [Fact]
    public void EachAddress_IsWrittenOnce_Here()
    {
        CatalogueLinks.Curricula.Should().Be("/admin/curricula");
        CatalogueLinks.Epas.Should().Be("/admin/epas");
        CatalogueLinks.NewEpa.Should().Be("/admin/epas/new");
        CatalogueLinks.Epa(17).Should().Be("/admin/epas/17");
        CatalogueLinks.EpaCreated(17).Should().Be("/admin/epas/17?created");
        CatalogueLinks.Items(4).Should().Be("/admin/curricula/4/items");
        CatalogueLinks.NewItem(4).Should().Be("/admin/curricula/4/items/new");
        CatalogueLinks.Item(4, 31).Should().Be("/admin/curricula/4/items/31");
        CatalogueLinks.ItemAdded(4, 31).Should().Be("/admin/curricula/4/items?added=31");
        CatalogueLinks.ItemSaved(4, 31).Should().Be("/admin/curricula/4/items?saved=31");
        CatalogueLinks.BackFromItem(4, 31).Should().Be("/admin/curricula/4/items?back=31");
    }

    [Fact]
    public void AnIdIsWrittenInPlainDigits_WhateverTheCulture()
    {
        var before = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            CatalogueLinks.ItemAdded(12345, 67890).Should().Be("/admin/curricula/12345/items?added=67890");
            CatalogueLinks.EpaCreated(12345).Should().Be("/admin/epas/12345?created");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = before;
        }
    }

    [Fact]
    public void TheMenusItems_AgreeWithTheLinks()
    {
        NavItems.Curricula.Href.Should().Be(CatalogueLinks.Curricula);
        NavItems.Epas.Href.Should().Be(CatalogueLinks.Epas);
        OnboardingLinks.Curricula.Should().Be(CatalogueLinks.Curricula);
    }

    [Fact]
    public void A_TokenWithNoValue_IsRead_AndAnAbsentOneIsNot()
    {
        CatalogueLinks.HasToken("http://localhost/admin/epas/17?created", CatalogueLinks.CreatedToken).Should().BeTrue();
        CatalogueLinks.HasToken("/admin/epas/17?created=", CatalogueLinks.CreatedToken).Should().BeTrue();
        CatalogueLinks.HasToken("/admin/epas/17", CatalogueLinks.CreatedToken).Should().BeFalse();
        CatalogueLinks.HasToken("/admin/epas/17?createdx", CatalogueLinks.CreatedToken).Should().BeFalse();
    }

    [Theory]
    [InlineData("http://localhost/admin/curricula/4/items?added=31", "added", 31)]
    [InlineData("/admin/curricula/4/items?saved=31", "saved", 31)]
    [InlineData("/admin/curricula/4/items?back=31#top", "back", 31)]
    [InlineData("/admin/curricula/4/items?added=x", "added", null)]
    [InlineData("/admin/curricula/4/items?added=-3", "added", null)]
    [InlineData("/admin/curricula/4/items", "added", null)]
    public void A_TokensId_IsReadAsANumber_OrNotAtAll(string uri, string token, int? expected)
        => CatalogueLinks.TokenId(uri, token).Should().Be(expected);

    [Theory]
    [InlineData("http://localhost/admin/epas/17?created", "http://localhost/admin/epas/17")]
    [InlineData("/admin/curricula/4/items?added=31", "/admin/curricula/4/items")]
    [InlineData("/admin/curricula/4/items?back=31&page=2", "/admin/curricula/4/items?page=2")]
    [InlineData("/admin/curricula/4/items?page=2&saved=31#row", "/admin/curricula/4/items?page=2#row")]
    [InlineData("/admin/curricula/4/items", "/admin/curricula/4/items")]
    public void WithoutTokens_DropsTheFourTokens_AndKeepsTheRest(string uri, string expected)
        => CatalogueLinks.WithoutTokens(uri).Should().Be(expected);

    public static TheoryData<string, Type> EveryLinkToAPageThatExists() => new()
    {
        { CatalogueLinks.Curricula, typeof(CurriculaList) },
        { CatalogueLinks.Epas, typeof(EpasList) },
        { CatalogueLinks.NewEpa, typeof(EpaEdit) },
        { CatalogueLinks.EpaCreated(17), typeof(EpaEdit) },
        { CatalogueLinks.ItemAdded(4, 31), typeof(CurriculumItemsEdit) },
        { CatalogueLinks.BackFromItem(4, 31), typeof(CurriculumItemsEdit) },
        // Lane B (T392, Q14): an item's page of its own, the add and the edit.
        { CatalogueLinks.NewItem(4), typeof(CurriculumItemPage) },
        { CatalogueLinks.Item(4, 31), typeof(CurriculumItemPage) },
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

    // A route template as a pattern: "{Id:int}" takes one segment of digits, anything else is literal.
    private static bool Matches(string template, string path)
    {
        var pattern = Regex.Replace(Regex.Escape(template), @"\\{[A-Za-z]+:int}", "[0-9]+");
        pattern = Regex.Replace(pattern, @"\\{[A-Za-z]+(:[a-z]+)?}", "[^/]+");
        return Regex.IsMatch(path, $"^{pattern}$");
    }
}
