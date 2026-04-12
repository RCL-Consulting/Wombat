using Bunit;
using FluentAssertions;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

public sealed class RuntimeRendererTests : TestContext
{
    [Fact]
    public void ActivityForm_HonorsVisibilityConditions()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    { "key": "kind", "type": "choice", "label": "Kind", "options": ["basic", "advanced"] },
                    { "key": "details", "type": "longtext", "label": "Advanced notes", "show_if": { "field": "kind", "operator": "equals", "value": "advanced" } }
                  ]
                }
              ]
            }
            """;

        var hidden = RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, """{ "kind": "basic" }""")
            .Add(component => component.ReadOnly, true));

        var visible = RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, """{ "kind": "advanced", "details": "More" }""")
            .Add(component => component.ReadOnly, true));

        hidden.Markup.Should().NotContain("Advanced notes");
        visible.Markup.Should().Contain("Advanced notes");
    }
}
