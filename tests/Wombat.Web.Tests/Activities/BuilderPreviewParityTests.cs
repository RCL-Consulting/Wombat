using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

public sealed class BuilderPreviewParityTests : TestContext
{
    public BuilderPreviewParityTests()
    {
        this.AddTestAuthorization().SetAuthorized("trainee@test");
        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    [Fact]
    public void ActivityForm_And_ActivityDetail_RenderTheSameSchemaShape()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "greeting",
                  "title": "Greeting",
                  "fields": [
                    { "key": "message", "type": "text", "label": "Your message", "required": true }
                  ]
                }
              ]
            }
            """;

        const string dataJson = """{ "message": "Hello" }""";

        var builderPreview = RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, dataJson)
            .Add(component => component.ReadOnly, true));

        var runtimeDetail = RenderComponent<ActivityDetail>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, dataJson));

        builderPreview.Markup.Should().Contain("Greeting");
        builderPreview.Markup.Should().Contain("Your message");
        runtimeDetail.Markup.Should().Contain("Greeting");
        runtimeDetail.Markup.Should().Contain("Your message");

        // T342 (flow 03): the builder's live preview is the same renderer, writable, so it draws the section as the author
        // will file it: a card named by its heading, the field's control with its required mark. A reader who may write
        // nothing (the detail page) gets the same section read out.
        var writablePreview = RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, dataJson));

        writablePreview.Find("section.form-section h2").TextContent.Trim().Should().Be("Greeting");
        writablePreview.Find("#message-in").GetAttribute("aria-required").Should().Be("true");
        writablePreview.Find("label[for=message-in]").TextContent.Should().Contain("Your message");
        runtimeDetail.Find("section.form-section h2").TextContent.Trim().Should().Be("Greeting");
        runtimeDetail.Find("#message-in dd").TextContent.Trim().Should().Be("Hello");
    }
}
