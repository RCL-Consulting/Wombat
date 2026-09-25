using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T195: each EPA picker on the curriculum item editor offers exactly what the command it feeds accepts.
/// </summary>
/// <remarks>
/// <para>
/// Which EPAs an item may name depends on who owns the item: a national item names a national EPA of the curriculum's
/// sub-speciality, an institution's local item may also name that institution's local EPAs. The rule is
/// <c>CurriculumItemEpas.Nameable</c>, and <c>ListCurriculumItemEpaOptionsQuery</c> answers it for one picker. These tests
/// pin the page's half: the Add form asks for a new item, each edit row asks for its own item, and each shows what it
/// was answered and nothing else. Before T195 both pickers showed one list, the EPAs in the CALLER's scope filtered by
/// sub-speciality, so an Administrator was offered every institution's local EPAs for a national item.
/// </para>
/// <para>
/// The answers differ on purpose (the Add form's has no local EPA, the edit rows' have one each), so a picker that
/// showed the other's list, or asked for the wrong item, fails.
/// </para>
/// </remarks>
public sealed class CurriculumItemsEpaPickerTests : TestContext
{
    private static readonly EpaDto LocalEpaOfItem11 = FakeSender.Epa(41, "LOC-A01");
    private static readonly EpaDto LocalEpaOfItem12 = FakeSender.Epa(42, "LOC-B01");

    private static readonly IReadOnlyList<EpaDto> NationalOptions = [FakeSender.Epas[0], FakeSender.Epas[1], FakeSender.Epas[2]];

    public CurriculumItemsEpaPickerTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
    }

    [Fact]
    public void TheAddFormsPicker_OffersExactlyTheAnswerForANewItem()
    {
        var sender = Sender();
        var cut = RenderPage(sender);

        Options(cut, "curriculum-item-epa").Should().Equal("PAED-001", "PAED-002", "PAED-003");

        var query = sender.EpaOptionQueries.Should().ContainSingle("the page asks once on load, for the Add form").Subject;
        query.CurriculumId.Should().Be(FakeSender.CurriculumId);
        query.ItemId.Should().BeNull("a new item's owner is decided by who adds it, not by any stored item");
    }

    [Fact]
    public void EachEditRowsPicker_OffersExactlyTheAnswerForThatItem()
    {
        var sender = Sender();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");
        Options(cut, "edit-epa").Should().Equal("PAED-001", "PAED-002", "PAED-003", "LOC-A01");
        sender.EpaOptionQueries.Last().ItemId.Should().Be(11);

        ClickButton(cut, "Cancel");
        BeginEdit(cut, "PAED-002");
        Options(cut, "edit-epa").Should().Equal(
            new[] { "PAED-001", "PAED-002", "PAED-003", "LOC-B01" },
            "another item has another owner, so its picker is asked for again, not reused");
        sender.EpaOptionQueries.Last().ItemId.Should().Be(12);

        Options(cut, "curriculum-item-epa").Should().Equal(
            new[] { "PAED-001", "PAED-002", "PAED-003" },
            "the Add form keeps its own list while a row is open: it would add an item of its own owner");
    }

    [Fact]
    public void TheEditRow_KeepsTheItemsStoredEpaSelected()
    {
        // The item's own EPA is one of the options, so an unchanged save sends it back.
        var cut = RenderPage(Sender());

        BeginEdit(cut, "PAED-002");

        cut.Find("#edit-epa").GetAttribute("value").Should().Be("2");
        cut.FindAll("#edit-epa-refused").Should().BeEmpty("the stored EPA is one the item can name");
        cut.Find("#edit-epa").GetAttribute("aria-describedby").Should().Be("edit-epa-help", "its help, and no refusal");
    }

    [Fact]
    public void AnItemWhoseStoredEpaItCannotName_SaysSo_UntilAnotherIsChosen()
    {
        // Its stored EPA is not among the options (it was moved to another sub-speciality, say), so the select shows none
        // selected, and Save would be refused for an EPA the operator cannot see. The row says so before Save is pressed.
        var sender = new FakeSender { EpaOptionsFor = itemId => itemId == 11 ? [FakeSender.Epas[1], FakeSender.Epas[2]] : null };
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");

        cut.Find("#edit-epa-refused").TextContent.Should().Be(
            "PAED-001 is not one of the EPAs this item can name, so none is selected. Choose one before saving.");
        cut.Find("#edit-epa").GetAttribute("aria-describedby").Should().Be("edit-epa-help edit-epa-refused",
            "its help first, then the refusal (T193), in the order they are shown");
        cut.Markup.IndexOf("id=\"edit-epa-help\"", StringComparison.Ordinal).Should().BeLessThan(
            cut.Markup.IndexOf("id=\"edit-epa-refused\"", StringComparison.Ordinal), "the help is shown first too");

        cut.Find("#edit-epa").Change("2");

        cut.FindAll("#edit-epa-refused").Should().BeEmpty("the EPA now chosen is one the item can name");
        cut.Find("#edit-epa").GetAttribute("aria-describedby").Should().Be("edit-epa-help");
    }

    [Fact]
    public void ACallerWhoCannotSaveTheItem_IsToldSoAtTheTop_AndTheRowDoesNotOpen()
    {
        var sender = new FakeSender
        {
            EditOptionsFailure = new UnauthorizedAccessException("You do not have permission to modify this curriculum.")
        };
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");

        cut.FindAll(".alert.alert-danger").Should().ContainSingle()
            .Which.TextContent.Should().Contain("You do not have permission to modify this curriculum.");
        cut.FindAll("tbody td[colspan]").Should().BeEmpty("no form opens that its Save would refuse");
        cut.FindAll("#edit-epa").Should().BeEmpty();
    }

    [Fact]
    public void BothPickers_SayWhichEpasTheyList()
    {
        var cut = RenderPage(Sender());

        BeginEdit(cut, "PAED-001");

        foreach (var id in new[] { "curriculum-item-epa", "edit-epa" })
        {
            // The select names its help (T193), so the help is found the way a screen reader finds it.
            var help = cut.Find($"#{cut.Find($"#{id}").GetAttribute("aria-describedby")!.Split(' ')[0]}");
            help.TextContent.Should().StartWith("Lists only the EPAs this item can name", $"#{id} carries its help");
        }
    }

    private static FakeSender Sender()
        => new()
        {
            EpaOptionsFor = itemId => itemId switch
            {
                null => NationalOptions,
                11 => [.. NationalOptions, LocalEpaOfItem11],
                12 => [.. NationalOptions, LocalEpaOfItem12],
                _ => []
            }
        };

    private IRenderedComponent<CurriculumItemsEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => parameters.Add(page => page.Id, FakeSender.CurriculumId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);

        return cut;
    }

    // The option labels' codes, in the order the select lists them.
    private static IReadOnlyList<string> Options(IRenderedComponent<CurriculumItemsEdit> cut, string selectId)
        => cut.FindAll($"#{selectId} option")
            .Select(option => option.TextContent.Trim().Split(' ')[0])
            .ToList();

    private static void BeginEdit(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => cut.FindAll("tbody tr")
            .Where(row => row.QuerySelector("td[colspan]") is null)
            .Single(row => row.QuerySelector("td")!.TextContent.Trim().StartsWith(epaCode, StringComparison.Ordinal))
            .QuerySelectorAll("button")
            .First(button => button.TextContent.Trim() == "Edit")
            .Click();

    private static void ClickButton(IRenderedComponent<CurriculumItemsEdit> cut, string label)
        => cut.FindAll("button")
            .First(button => string.Equals(button.TextContent.Trim(), label, StringComparison.Ordinal))
            .Click();
}
