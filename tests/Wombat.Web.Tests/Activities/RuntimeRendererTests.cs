using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

public sealed class RuntimeRendererTests : TestContext
{
    public RuntimeRendererTests()
    {
        this.AddTestAuthorization().SetAuthorized("trainee@test");
        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

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

    [Fact]
    public void ActivityForm_RendersEpaUserAndScaleAsSelects()
    {
        Services.AddSingleton<IActivityReferenceDataService>(new PopulatedReferenceDataService());

        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    { "key": "epa_id", "type": "epa", "label": "EPA" },
                    { "key": "assessor_user_id", "type": "user", "label": "Assessor" },
                    { "key": "overall_level", "type": "scale", "label": "Overall", "scale_key": "2" }
                  ]
                }
              ]
            }
            """;

        var rendered = RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, "{}"));

        // Three rich pickers render as <select>, plus their option labels.
        rendered.FindAll("select").Should().HaveCount(3);
        rendered.Markup.Should().Contain("PAED-001 — Acute admission");
        rendered.Markup.Should().Contain("Dr Naidoo (naidoo@kgk)");
        rendered.Markup.Should().Contain("4. Indirect supervision");
        rendered.Markup.Should().NotContain("type=\"number\"");
    }

    [Fact]
    public void ActivityForm_FallsBackToNumberWhenScaleUnbound()
    {
        // Stub returns no levels (default stub) -> an unbound Scale field degrades to a number input.
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                { "key": "r", "title": "R", "fields": [ { "key": "overall_level", "type": "scale", "label": "Overall" } ] }
              ]
            }
            """;

        var rendered = RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, "{}"));

        rendered.FindAll("select").Should().BeEmpty();
        rendered.Markup.Should().Contain("type=\"number\"");
    }

    private sealed class PopulatedReferenceDataService : StubActivityReferenceDataService
    {
        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
            System.Security.Claims.ClaimsPrincipal principal, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(
                [new ActivityCatalogueOption("1", "PAED-001 — Acute admission")]);

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetAssessorOptionsAsync(
            System.Security.Claims.ClaimsPrincipal principal, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(
                [new ActivityCatalogueOption("guid-naidoo", "Dr Naidoo (naidoo@kgk)")]);

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleLevelOptionsAsync(
            string? scaleKey, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(
                [new ActivityCatalogueOption("4", "4. Indirect supervision")]);
    }
}
