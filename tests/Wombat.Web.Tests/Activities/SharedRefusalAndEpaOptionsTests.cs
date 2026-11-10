using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T398 (flow 17, S1, S8): the two shared components' optional parameters. Unset, each behaves as flow 03's pages have it
/// (<c>RefusedFieldMarkingTests</c>, <c>LogAnActivityTests</c>, <c>NewActivitySubmitFlowTests</c>).
/// </summary>
public sealed class SharedRefusalAndEpaOptionsTests : TestContext
{
    private const string EpaSchema = """
        { "version": 1, "sections": [ { "key": "s", "title": "S", "fields": [ { "key": "epa_id", "type": "epa", "label": "EPA" } ] } ] }
        """;

    public SharedRefusalAndEpaOptionsTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ALineAction_RunsInPlaceOfTheFieldsFocus_ItsLinkNamingTheTarget()
    {
        RefusalLine? pressed = null;
        var line = new RefusalLine(null, "Form: Field key 'topic' is used by 2 fields.", null) { TargetId = "field-key" };
        var cut = RenderComponent<RefusalSummary>(parameters => parameters
            .Add(summary => summary.Title, "Nothing was saved.")
            .Add(summary => summary.Lines, [line, new RefusalLine(null, "A line with no place.", null)])
            .Add(summary => summary.LineAction, (RefusalLine pressedLine) => { pressed = pressedLine; }));

        var link = cut.Find("li a");
        link.GetAttribute("href").Should().Be("#field-key");
        link.Click();

        pressed.Should().BeSameAs(line);
        cut.FindAll("li").ElementAt(1).QuerySelector("a").Should().BeNull("a line with no target is text");
        JSInterop.Invocations.Should().NotContain(call => call.Identifier == "wombat.focusById", "the page's action places the focus");
    }

    [Fact]
    public void WithNoLineAction_AFieldsLine_LinksToItsInput_AsBefore()
    {
        var cut = RenderComponent<RefusalSummary>(parameters => parameters
            .Add(summary => summary.Title, "Nothing was saved.")
            .Add(summary => summary.Lines, [new RefusalLine("observed_on", "Date observed: The date cannot be after today.", null) { TargetId = "ignored" }]));

        cut.Find("li a").GetAttribute("href").Should().Be($"#{ActivityFieldIds.Input("observed_on")}");
    }

    [Fact]
    public void EpaOptions_AreOfferedInPlaceOfTheFormsOwnRead()
    {
        var reference = new CountingReferenceData();
        Services.AddSingleton<IActivityReferenceDataService>(reference);

        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, EpaSchema)
            .Add(form => form.EpaOptions, [new ActivityCatalogueOption("1", "PAED-001 — Providing paediatric emergency care to children")]));

        cut.WaitForAssertion(() => cut.FindAll("select option").Select(option => option.TextContent.Trim())
            .Should().Contain("PAED-001 — Providing paediatric emergency care to children"));
        reference.EpaReads.Should().Be(0, "the builder's preview has no registrar to read EPAs for (S8)");
    }

    [Fact]
    public void WithNoEpaOptions_TheFormReadsItsOwn()
    {
        var reference = new CountingReferenceData();
        Services.AddSingleton<IActivityReferenceDataService>(reference);

        RenderComponent<ActivityForm>(parameters => parameters.Add(form => form.SchemaJson, EpaSchema));

        reference.EpaReads.Should().Be(1);
    }

    private sealed class CountingReferenceData : StubActivityReferenceDataService
    {
        public int EpaReads { get; private set; }

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
            ClaimsPrincipal principal, EpaOptionScope? scope = null, CancellationToken cancellationToken = default)
        {
            EpaReads++;
            return base.GetEpaOptionsAsync(principal, scope, cancellationToken);
        }
    }
}
