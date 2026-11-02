using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T195, on an item's page of its own since T392 (flow 15, Q14, M6 of round 2): each EPA picker offers exactly what
/// <c>ListCurriculumItemEpaOptionsQuery</c> answers for it (the add's for a new item, an edit's for that item), which is
/// the rule the command it feeds enforces, so the page filters nothing itself. A page that reused one answer for another
/// item, or asked for the wrong item, fails.
/// </summary>
public sealed class CurriculumItemsEpaPickerTests : CurriculumItemsTestBase
{
    private static readonly EpaDto LocalEpaOfItem11 = FakeSender.Epa(41, "LOC-A01");
    private static readonly EpaDto LocalEpaOfItem12 = FakeSender.Epa(42, "LOC-B01");

    private static readonly IReadOnlyList<EpaDto> NationalOptions = [FakeSender.Epas[0], FakeSender.Epas[1], FakeSender.Epas[2]];

    public CurriculumItemsEpaPickerTests()
    {
        SignIn(WombatRoles.Administrator);
    }

    [Fact]
    public void TheAddPagesPicker_OffersExactlyTheAnswerForANewItem()
    {
        var sender = Sender();
        var cut = RenderItem(sender);

        Options(cut).Should().Equal("PAED-001", "PAED-002", "PAED-003");
        var query = sender.EpaOptionQueries.Should().ContainSingle().Subject;
        query.CurriculumId.Should().Be(FakeSender.CurriculumId);
        query.ItemId.Should().BeNull("a new item's owner is decided by who adds it, not by any stored item");
    }

    [Theory]
    [InlineData(11, "LOC-A01")]
    [InlineData(12, "LOC-B01")]
    public void AnEditPagesPicker_OffersExactlyTheAnswerForThatItem(int itemId, string local)
    {
        var sender = Sender();
        var cut = RenderItem(sender, itemId);

        Options(cut).Should().Equal("PAED-001", "PAED-002", "PAED-003", local);
        sender.EpaOptionQueries.Should().ContainSingle().Which.ItemId.Should().Be(itemId);
    }

    [Fact]
    public void TheOptions_ReadAsTheEpaPickerDoes_WithTheEmDash_AnInactiveOneSaidSo()
    {
        // S10: EpaOptionLabel's em dash; T158: an inactive EPA can be chosen and says so.
        var cut = RenderItem(new FakeSender { EpaList = FakeSender.EpasWithInactive(2) });

        cut.FindAll("#item-epa option").Select(Text).Should().Equal(
            "PAED-001 — PAED-001 title", "PAED-002 — PAED-002 title (inactive)", "PAED-003 — PAED-003 title");
    }

    [Fact]
    public void TheEditPage_KeepsTheItemsStoredEpaSelected()
    {
        var cut = RenderItem(Sender(), 12);

        cut.Find("#item-epa").GetAttribute("value").Should().Be("2");
        cut.FindAll("#item-epa-not-offered").Should().BeEmpty("the stored EPA is one the item can name");
        cut.Find("#item-epa").GetAttribute("aria-describedby").Should().Be("item-epa-help", "its help, and no refusal");
    }

    [Fact]
    public void AnItemWhoseStoredEpaItCannotName_SaysSo_UntilAnotherIsChosen()
    {
        // Its stored EPA is not among the options, so the select shows none selected; said before Save is pressed.
        var sender = new FakeSender { EpaOptionsFor = itemId => itemId == 11 ? [FakeSender.Epas[1], FakeSender.Epas[2]] : null };
        var cut = RenderItem(sender, 11);

        Text(cut.Find("#item-epa-not-offered")).Should().Be(
            "PAED-001 is not one of the EPAs this item can name, so none is selected. Choose one before saving.");
        cut.Find("#item-epa").GetAttribute("aria-describedby").Should().Be("item-epa-help item-epa-not-offered");

        cut.Find("#item-epa").Change("2");

        cut.FindAll("#item-epa-not-offered").Should().BeEmpty();
        cut.Find("#item-epa").GetAttribute("aria-describedby").Should().Be("item-epa-help");
    }

    [Fact]
    public void ACallerTheEditPickerRefuses_IsShownItemUnavailable_NotAFormSheCouldNotSave()
    {
        var sender = new FakeSender
        {
            EditOptionsFailure = new UnauthorizedAccessException("You do not have permission to modify this curriculum.")
        };
        var cut = RenderItem(sender, 11);

        Text(cut.Find("h1")).Should().Be("Item unavailable");
        cut.FindAll("form").Should().BeEmpty();
        cut.Markup.Should().NotContain("You do not have permission");
    }

    [Fact]
    public void ThePicker_SaysWhichEpasItLists()
    {
        var cut = RenderItem(Sender(), 11);

        var help = cut.Find($"#{cut.Find("#item-epa").GetAttribute("aria-describedby")!.Split(' ')[0]}");
        help.TextContent.Should().StartWith("Lists only the EPAs this item can name");
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

    // The option labels' codes, in the order the select lists them.
    private static IReadOnlyList<string> Options(IRenderedFragment cut)
        => cut.FindAll("#item-epa option").Select(option => option.TextContent.Trim().Split(' ')[0]).ToList();
}
