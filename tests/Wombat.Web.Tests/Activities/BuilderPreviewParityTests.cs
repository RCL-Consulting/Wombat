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
    }
}
