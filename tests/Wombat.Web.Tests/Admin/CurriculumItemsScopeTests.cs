using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T211: the curriculum item editor offers Edit and Remove only on the items the caller may change, as the query marks
/// them (<see cref="CurriculumItemDto.CanEdit" />, <c>CurriculumAdminScope.ForCaller</c>), and an institution reading a
/// curriculum it adopted is told what it may do there.
/// </summary>
/// <remarks>
/// Before T211 every row had Edit and Remove. A CollegeAdmin was offered them on an institution's own items, and the
/// commands refused the save; an InstitutionalAdmin could not open the page at all. The page does not work the rule out
/// from the caller's roles: it reads the query's answer, so these tests give it the answer and check what it offers.
/// </remarks>
public sealed class CurriculumItemsScopeTests : TestContext
{
    private const int InstitutionA = 40;

    private static readonly EpaDto OwnLocalEpa = FakeSender.Epa(41, "LOC-A01");

    [Fact]
    public void AnInstitutionReadingAnAdoptedCurriculum_IsOfferedEditAndRemove_OnlyOnItsOwnItems()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderPage(InstitutionsView());

        foreach (var code in new[] { "PAED-001", "PAED-002" })
        {
            var row = Row(cut, code);
            row.QuerySelectorAll("button").Should().BeEmpty($"{code} is the College's, and the Update and Remove commands refuse it");
            row.QuerySelectorAll("td").Last().TextContent.Trim().Should().Be("Set by the College");
            row.TextContent.Should().NotContain("own item");
        }

        var own = Row(cut, "PAED-003");
        Buttons(own).Should().Equal("Edit", "Remove");
        OwnerLine(own).Should().Be("Your institution's own item",
            "the query names no owner to an institution, every local item it reads being its own (T222)");
    }

    [Fact]
    public void AnAdministrator_IsToldWhoseOwnItemEachLocalItemIs()
    {
        // T222. An Administrator reads every institution's items; the query names each owner, and the row says whose.
        SignIn(WombatRoles.Administrator);
        var cut = RenderPage(new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, owningInstitutionId: InstitutionA, owningInstitutionName: "Groote Schuur Hospital"),
            FakeSender.Item(13, 3, 1, QuotaPeriod.AcademicYear, null, owningInstitutionId: 41, owningInstitutionName: "Red Cross War Memorial Children's Hospital")
        ]));

        OwnerLine(Row(cut, "PAED-001")).Should().BeNull("a national item is the College's, and says nothing");
        OwnerLine(Row(cut, "PAED-002")).Should().Be("Groote Schuur Hospital's own item");
        OwnerLine(Row(cut, "PAED-003")).Should().Be("Red Cross War Memorial Children's Hospital's own item");
    }

    /// <summary>The line under an item's EPA saying whose own item it is, or null for a national item.</summary>
    private static string? OwnerLine(IElement row)
        => row.QuerySelector("td")!.QuerySelector(".muted.text-sm")?.TextContent.Trim();

    [Fact]
    public void AnInstitution_IsToldTheNationalItemsAreReadOnly_AndIsNotSentToTheCurriculumsOwnPage()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderPage(InstitutionsView());

        cut.FindAll(".alert.alert-warning").Should().ContainSingle()
            .Which.TextContent.Should().Contain("The College sets this curriculum's national items, so they are read only here.");
        cut.FindAll("a").Select(link => link.TextContent.Trim()).Should().NotContain("Back to curriculum",
            "the curriculum's own page edits its details and clones it, which only the College can do");
        cut.FindAll("a").Select(link => link.TextContent.Trim()).Should().Contain("Back to curricula");
        cut.FindAll("h3").Select(heading => heading.TextContent.Trim()).Should().Contain("Add your institution's own item");
    }

    [Fact]
    public void AnInstitutionEditingItsOwnItem_IsOfferedTheNationalEpasAndItsOwn_AndSavesThatItem()
    {
        // The T195 picker for a local item, answered for this item: the sub-speciality's national EPAs and the institution's
        // own local EPAs. The page shows that answer and saves the item it was opened on.
        SignIn(WombatRoles.InstitutionalAdmin);
        var sender = InstitutionsView();
        var cut = RenderPage(sender);

        ClickIn(Row(cut, "PAED-003"), "Edit");

        cut.FindAll("#edit-epa option").Select(option => option.TextContent.Trim().Split(' ')[0])
            .Should().Equal("PAED-001", "PAED-002", "PAED-003", "LOC-A01");
        sender.EpaOptionQueries.Last().ItemId.Should().Be(13);

        cut.Find("#edit-epa").Change(OwnLocalEpa.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        ClickIn(cut.Find("tr.is-editing td[colspan]"), "Save");

        var update = sender.Updates.Should().ContainSingle().Subject;
        update.ItemId.Should().Be(13);
        update.EpaId.Should().Be(OwnLocalEpa.Id);
    }

    [Fact]
    public void AnInstitution_CanRemoveItsOwnItem()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var sender = InstitutionsView();
        var cut = RenderPage(sender);

        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();

        ClickIn(Row(cut, "PAED-003"), "Remove");
        cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == "Remove item").Click();

        sender.Removes.Should().ContainSingle().Which.ItemId.Should().Be(13);
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(2));
    }

    [Fact]
    public void ACollegeAdmin_IsOfferedNoEditOrRemove_OnAnotherInstitutionsItem()
    {
        // The query does not show a CollegeAdmin an institution's own item at all; were one ever in the curriculum it was
        // handed, marked as not theirs to change, the page must still offer nothing on it.
        SignIn(WombatRoles.CollegeAdmin);
        var sender = new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(13, 3, 1, QuotaPeriod.AcademicYear, null, owningInstitutionId: InstitutionA, canEdit: false)
        ]);
        var cut = RenderPage(sender);

        var local = Row(cut, "PAED-003");
        local.QuerySelectorAll("button").Should().BeEmpty();
        local.QuerySelectorAll("td").Last().TextContent.Trim().Should().Be("Another institution's item");

        Buttons(Row(cut, "PAED-001")).Should().Equal("Edit", "Remove");
        cut.FindAll("a").Select(link => link.TextContent.Trim()).Should().Contain("Back to curriculum");
        cut.FindAll("h3").Select(heading => heading.TextContent.Trim()).Should().Contain("Add item");
        cut.Find(".alert.alert-warning").TextContent.Should().Contain("in every institution that has adopted it");
    }

    /// <summary>
    /// What the query answers an InstitutionalAdmin of institution A on an adopted curriculum: two national items they may
    /// not change, and one of their own they may. The edit row's picker answers the national EPAs and A's own.
    /// </summary>
    private static FakeSender InstitutionsView()
        => new(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null, canEdit: false),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, canEdit: false),
            FakeSender.Item(13, 3, 1, QuotaPeriod.AcademicYear, null, owningInstitutionId: InstitutionA)
        ])
        {
            CanEditCurriculum = false,
            // The Add form's owner and item 13's are both institution A, so the two pickers get one answer.
            EpaOptionsFor = _ => [.. FakeSender.Epas, OwnLocalEpa]
        };

    private void SignIn(string role)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{role}@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, $"{role}-1"));
    }

    private IRenderedComponent<CurriculumItemsEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => parameters.Add(page => page.Id, FakeSender.CurriculumId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);

        return cut;
    }

    private static IElement Row(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => cut.FindAll("tbody tr")
            .Where(row => row.QuerySelector("td[colspan]") is null)
            .Single(row => row.QuerySelector("td")!.TextContent.Trim().StartsWith(epaCode, StringComparison.Ordinal));

    private static IReadOnlyList<string> Buttons(IElement row)
        => row.QuerySelectorAll("button").Select(button => button.TextContent.Trim()).ToList();

    private static void ClickIn(IElement container, string label)
        => container.QuerySelectorAll("button").First(button => button.TextContent.Trim() == label).Click();
}
