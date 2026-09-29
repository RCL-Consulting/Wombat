using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>T342, flow 03 (R3-Spec § 1): the words My activities and Needs you print, in one place (C11).</summary>
public sealed class ActivityListWordsTests
{
    [Fact]
    public void AReturn_IsDatedOnTheSouthAfricanCalendar()
    {
        // 22:30 UTC on the 28th is already the 29th in South Africa.
        var row = ActivityRows.Returned(1) with
        {
            Returned = new ActivityReturnDto("sarah", "Sarah Botha", new DateTime(2026, 9, 28, 22, 30, 0, DateTimeKind.Utc), null)
        };

        ActivityListWords.NeedsYouWhy(row).Should().Be("Returned to you by Sarah Botha on 2026-09-29. Change it and submit again.");
    }

    [Fact]
    public void ADraft_IsNotSaidToBePrivate()
        => ActivityListWords.NeedsYouWhy(ActivityRows.Row(1)).Should().Be("Not submitted yet. It is in nobody's inbox until you submit it.");

    [Theory]
    [InlineData(ActivityTypeShape.Rated, "to David Naidoo")]
    [InlineData(ActivityTypeShape.DiscussedOrReviewed, "with David Naidoo")]
    [InlineData(ActivityTypeShape.LoggedByYou, "to David Naidoo")]
    public void TheSecondLine_IsToOrWith_ByTheTypesShape(ActivityTypeShape shape, string expected)
        => ActivityListWords.NomineeLine(ActivityRows.Row(1, shape: shape)).Should().Be(expected);

    [Fact]
    public void TheSecondLine_IsLeftOut_WithoutANominee_OrWhenTheNameCarriesThem()
    {
        ActivityListWords.NomineeLine(ActivityRows.Row(1, nominee: null)).Should().BeNull();
        ActivityListWords.NomineeLine(ActivityRows.Row(1) with { DisplayNameHasNominee = true }).Should().BeNull();
    }

    [Fact]
    public void WhoHasIt_NamesAnotherAuthor_AndSaysDashWhenUnknown()
    {
        ActivityListWords.WhoHasIt(ActivityRows.Row(1, holder: new ActivityHolderDto(ActivityHolderKind.Author, "t", "Sipho Ndlovu", false, null)))
            .Should().Be("Sipho Ndlovu");
        ActivityListWords.WhoHasIt(ActivityRows.Row(1) with { Holder = null }).Should().Be("—");
    }

    [Fact]
    public void AnUnnamedRow_IsNamedByItsType()
        => ActivityListWords.NameOf(ActivityRows.Row(1) with { DisplayName = null }).Should().Be("Mini-CEX (Paediatrics)");

    [Theory]
    [InlineData(1, "1 item")]
    [InlineData(2, "2 items")]
    public void ACount_IsReadAsWords(int count, string expected)
        => ActivityListWords.ItemCount(count).Should().Be(expected);
}
