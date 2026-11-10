using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, lane B, Q2): the credit rules in words as R2-B05, R2-D03 and R3-D04 draw them, from read DTOs built by
/// hand: "Credits nothing." with its line, or one sentence a directive.
/// </summary>
public sealed class CreditReadoutTests : TestContext
{
    [Fact]
    public void RulesThatCreditNothing_SaySo_WithTheDateRuleInWords()
    {
        var cut = Render(new ActivityTypeCreditReadDto(true, []));

        cut.Find("div.credit-readout").Should().NotBeNull();
        var lines = cut.FindAll("div.credit-readout > p");
        lines.Select(Text).Should().Equal(
            "Credits nothing.",
            "An activity of this type counts towards no curriculum item, and its encounter date may not be after today.");
        lines.ElementAt(1).ClassList.Should().Contain("muted");
    }

    [Fact]
    public void EachDirective_IsOneSentence()
    {
        var cut = Render(new ActivityTypeCreditReadDto(false,
        [
            "Counts 1 towards the curriculum item for the EPA in EPA, at the level Supervision required for this encounter records.",
            "Counts 1 towards the item for the EPA in Second EPA."
        ]));

        cut.FindAll("div.credit-readout > p").Select(Text).Should().Equal(
            "Counts 1 towards the curriculum item for the EPA in EPA, at the level Supervision required for this encounter records.",
            "Counts 1 towards the item for the EPA in Second EPA.");
        cut.Markup.Should().NotContain("Credits nothing");
        cut.FindAll("h2, h3, h4").Should().BeEmpty("the builder's panel h2 heads it");
    }

    private IRenderedComponent<CreditReadout> Render(ActivityTypeCreditReadDto read)
        => RenderComponent<CreditReadout>(parameters => parameters.Add(component => component.Read, read));

    private static string Text(IElement element) => element.TextContent.Trim();
}
