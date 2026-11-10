using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Components.Pages.Admin.Colleges;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Components.Pages.Admin.Epas;
using Wombat.Web.Components.Pages.Admin.Institutions;
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

    // T394 (flow 16, build-contracts § 0): the College's and the Administrator's pages, and their results' tokens (Q5, S13).
    [Fact]
    public void FlowSixteensAddresses_AreWrittenOnce_Here()
    {
        CatalogueLinks.Colleges.Should().Be("/admin/colleges");
        CatalogueLinks.NewCollege.Should().Be("/admin/colleges/new");
        CatalogueLinks.College(4).Should().Be("/admin/colleges/4");
        CatalogueLinks.CollegeCreated(4).Should().Be("/admin/colleges/4?created");
        CatalogueLinks.Specialities.Should().Be("/admin/specialities");
        CatalogueLinks.CollegeTree(2).Should().Be("/admin/colleges/2/specialities");
        CatalogueLinks.CollegeTreeAt(2, 3).Should().Be("/admin/colleges/2/specialities#speciality-3");
        CatalogueLinks.TreeSectionId(3).Should().Be("speciality-3");
        CatalogueLinks.TreeCreated(2, TreeRecord.OfSubSpeciality(4)).Should().Be("/admin/colleges/2/specialities?created=sub:4");
        CatalogueLinks.TreeSaved(2, TreeRecord.OfSpeciality(2)).Should().Be("/admin/colleges/2/specialities?saved=speciality:2");
        CatalogueLinks.NewSpeciality(4).Should().Be("/admin/colleges/4/specialities/new");
        CatalogueLinks.Speciality(2, 2).Should().Be("/admin/colleges/2/specialities/2");
        CatalogueLinks.SubSpecialities(2).Should().Be("/admin/specialities/2/sub-specialities");
        CatalogueLinks.NewSubSpeciality(2).Should().Be("/admin/specialities/2/sub-specialities/new");
        CatalogueLinks.SubSpeciality(2, 4).Should().Be("/admin/specialities/2/sub-specialities/4");
        CatalogueLinks.EntrustmentScales.Should().Be("/admin/entrustment-scales");
        CatalogueLinks.NewEntrustmentScale.Should().Be("/admin/entrustment-scales/new");
        CatalogueLinks.EntrustmentScale(3).Should().Be("/admin/entrustment-scales/3");
        CatalogueLinks.EntrustmentScaleCreated(3).Should().Be("/admin/entrustment-scales/3?created");
        CatalogueLinks.NewCurriculum.Should().Be("/admin/curricula/new");
        CatalogueLinks.Curriculum(3).Should().Be("/admin/curricula/3");
        CatalogueLinks.CurriculumCreated(3).Should().Be("/admin/curricula/3?created");
        CatalogueLinks.CurriculumCloned(4, 2).Should().Be("/admin/curricula/4?cloned=2");
    }

    // T398 (flow 17, build-contracts § 0): the activity types list and the builder; the first save's token is the existing
    // created token, so no token is added (round 3 check, correction 6).
    [Fact]
    public void FlowSeventeensAddresses_AreWrittenOnce_Here()
    {
        CatalogueLinks.ActivityTypes.Should().Be("/admin/activity-types");
        CatalogueLinks.NewActivityType.Should().Be("/admin/activity-types/new");
        CatalogueLinks.ActivityType(23).Should().Be("/admin/activity-types/23");
        CatalogueLinks.ActivityTypeCreated(23).Should().Be("/admin/activity-types/23?created");
        CatalogueLinks.HasToken(CatalogueLinks.ActivityTypeCreated(23), CatalogueLinks.CreatedToken).Should().BeTrue();
        CatalogueLinks.WithoutTokens(CatalogueLinks.ActivityTypeCreated(23)).Should().Be(CatalogueLinks.ActivityType(23));
    }

    [Theory]
    [InlineData("/admin/colleges/2/specialities?created=sub:4", "created", TreeRecordKind.SubSpeciality, 4)]
    [InlineData("http://localhost/admin/colleges/2/specialities?saved=speciality:2#x", "saved", TreeRecordKind.Speciality, 2)]
    public void A_TreeToken_NamesItsRecord(string uri, string token, TreeRecordKind kind, int id)
        => CatalogueLinks.TreeRecordOf(uri, token).Should().Be(new TreeRecord(kind, id));

    [Theory]
    [InlineData("/admin/colleges/2/specialities?created=sub:x")]
    [InlineData("/admin/colleges/2/specialities?created=4")]
    [InlineData("/admin/colleges/2/specialities?created=college:4")]
    [InlineData("/admin/colleges/2/specialities?created=sub:-4")]
    [InlineData("/admin/colleges/2/specialities?created")]
    [InlineData("/admin/colleges/2/specialities")]
    public void A_MalformedTreeToken_NamesNothing(string uri)
        => CatalogueLinks.TreeRecordOf(uri, CatalogueLinks.CreatedToken).Should().BeNull();

    [Fact]
    public void WithoutTokens_DropsTheCloneToken_AndTheTreeTokens()
    {
        CatalogueLinks.WithoutTokens("/admin/curricula/4?cloned=2").Should().Be("/admin/curricula/4");
        CatalogueLinks.WithoutTokens("/admin/colleges/2/specialities?saved=sub:3&x=1").Should().Be("/admin/colleges/2/specialities?x=1");
        CatalogueLinks.TokenId("/admin/curricula/4?cloned=2", CatalogueLinks.ClonedToken).Should().Be(2);
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
            CatalogueLinks.TreeSaved(12345, TreeRecord.OfSubSpeciality(67890)).Should().Be("/admin/colleges/12345/specialities?saved=sub:67890");
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
        NavItems.Colleges.Href.Should().Be(CatalogueLinks.Colleges);
        NavItems.Specialities.Href.Should().Be(CatalogueLinks.Specialities);
        NavItems.EntrustmentScales.Href.Should().Be(CatalogueLinks.EntrustmentScales);
        NavItems.ActivityTypes.Href.Should().Be(CatalogueLinks.ActivityTypes);
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
        // T394 (flow 16, § 0): the College's and the Administrator's pages, as their files declare them at § 0.
        { CatalogueLinks.Colleges, typeof(CollegesList) },
        { CatalogueLinks.NewCollege, typeof(CollegeEdit) },
        { CatalogueLinks.CollegeCreated(4), typeof(CollegeEdit) },
        { CatalogueLinks.Specialities, typeof(MySpecialitiesRedirect) },
        { CatalogueLinks.TreeSaved(2, TreeRecord.OfSpeciality(2)), typeof(SpecialitiesList) },
        { CatalogueLinks.NewSpeciality(4), typeof(SpecialityEdit) },
        { CatalogueLinks.Speciality(2, 2), typeof(SpecialityEdit) },
        { CatalogueLinks.SubSpecialities(2), typeof(SubSpecialitiesList) },
        { CatalogueLinks.NewSubSpeciality(2), typeof(SubSpecialityEdit) },
        { CatalogueLinks.SubSpeciality(2, 4), typeof(SubSpecialityEdit) },
        { CatalogueLinks.EntrustmentScales, typeof(EntrustmentScalesList) },
        { CatalogueLinks.NewEntrustmentScale, typeof(EntrustmentScaleEdit) },
        { CatalogueLinks.EntrustmentScaleCreated(3), typeof(EntrustmentScaleEdit) },
        { CatalogueLinks.NewCurriculum, typeof(CurriculumEdit) },
        { CatalogueLinks.CurriculumCloned(4, 2), typeof(CurriculumEdit) },
        // T398 (flow 17, § 0): the list and the builder, both of the builder's routes.
        { CatalogueLinks.ActivityTypes, typeof(ActivityTypesList) },
        { CatalogueLinks.NewActivityType, typeof(ActivityTypeEdit) },
        { CatalogueLinks.ActivityType(23), typeof(ActivityTypeEdit) },
        { CatalogueLinks.ActivityTypeCreated(23), typeof(ActivityTypeEdit) },
    };

    [Theory]
    [MemberData(nameof(EveryLinkToAPageThatExists))]
    public void EachLink_IsARouteItsPageDeclares(string link, Type page)
    {
        var path = link.Split('?', '#')[0];
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
