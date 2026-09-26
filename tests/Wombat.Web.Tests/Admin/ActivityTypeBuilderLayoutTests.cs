using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T266: the builder's two columns collapse by the width of the card they sit in, not the viewport's. app.css makes the
/// card a size container through <c>.form-container:has(&gt; .builder-two-col)</c> and collapses the columns under 46.5rem
/// of it (<c>Design/NarrowLayoutTests</c>), so the columns must be the card's own child: anywhere else they would never
/// collapse. Until T266 a (max-width: 900px) media query collapsed them, and from 901px to 1115px the columns were wider
/// than the card (the page scrolled 158px at 901px).
/// </summary>
public sealed class ActivityTypeBuilderLayoutTests : TestContext
{
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            { "key": "s", "title": "Details", "fields": [ { "key": "notes", "type": "longtext", "label": "Notes" } ] }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [ { "key": "draft", "label": "Draft" }, { "key": "done", "label": "Done", "terminal": true } ],
          "transitions": [ { "key": "submit", "from": "draft", "to": "done", "actor": "subject" } ]
        }
        """;

    public ActivityTypeBuilderLayoutTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
        Services.AddSingleton<IScopedSender>(new FakeSender());
    }

    [Fact]
    public void TheBuildersColumns_AreItsCardsOwnChild_TheEditorRailThenThePreview()
    {
        var cut = RenderComponent<ActivityTypeEdit>(parameters => parameters.Add(page => page.ActivityTypeId, 4));
        cut.WaitForState(() => cut.FindAll(".builder-two-col").Count == 1);

        var columns = cut.Find(".builder-two-col");
        columns.ParentElement!.ClassList.Should().Contain("form-container",
            "the card is the size container the columns answer to (.form-container:has(> .builder-two-col))");
        columns.Children.Should().HaveCount(2);
        columns.Children[0].QuerySelector("h3")!.TextContent.Should().Be("Sections");
        columns.Children[1].QuerySelector("h3")!.TextContent.Should().Be("Live preview");
    }

    private sealed class FakeSender : IScopedSender
    {
        private readonly ActivityTypeEditorDto _editor = new(
            4, "probe", "Probe", null, ActivityScope.Global, null, true, null, 1, false,
            SchemaJson, WorkflowJson, """{ "counts_for": [] }""", "[]",
            SchemaJson, WorkflowJson, """{ "counts_for": [] }""", "[]",
            "admin-1", null, null, [], true, [new ActivityTypeScopeChoiceDto(ActivityScope.Global, [])], null);

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                GetActivityTypeEditorQuery => _editor,
                GetInstitutionsListQuery => (IReadOnlyList<InstitutionDto>)[],
                GetSpecialitiesListQuery => (IReadOnlyList<SpecialityDto>)[],
                GetSubSpecialitiesListQuery => (IReadOnlyList<SubSpecialityDto>)[],
                GetWbaToolsQuery => (IReadOnlyList<WbaToolDto>)[],
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
