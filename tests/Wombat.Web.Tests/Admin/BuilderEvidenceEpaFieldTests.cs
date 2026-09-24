using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T137's pointer in the builder: <c>evidence_epa_field</c> survives the Form tab's round trip from the day it exists
/// (T119 and T126's pointers did not, which was T133), and the Form tab can set it.
/// </summary>
public sealed class BuilderEvidenceEpaFieldTests : TestContext
{
    private const string WithPointer = """
        {
          "version": 1,
          "evidence_epa_field": "epa_id",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "second_epa", "type": "epa", "label": "Another EPA", "required": false },
                { "key": "notes", "type": "longtext", "label": "Notes", "required": false }
              ]
            }
          ]
        }
        """;

    private static readonly string WithoutPointer =
        WithPointer.Replace("\"evidence_epa_field\": \"epa_id\",", string.Empty, StringComparison.Ordinal);

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [ { "key": "draft", "label": "Draft" }, { "key": "done", "label": "Done", "terminal": true } ],
          "transitions": [ { "key": "submit", "from": "draft", "to": "done", "actor": "subject" } ]
        }
        """;

    public BuilderEvidenceEpaFieldTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    // ---- the model ------------------------------------------------------------------------------------------------

    [Fact]
    public void ThePointerSurvivesTheBuilderRoundTrip()
    {
        var model = BuilderSchemaModel.Parse(WithPointer);

        model.EvidenceEpaField.Should().Be("epa_id");
        FormSchemaParser.Parse(model.ToJson()).EvidenceEpaField.Should().Be("epa_id");
    }

    [Fact]
    public void ASchemaWithNoPointerStaysWithoutOne()
    {
        BuilderSchemaModel.Parse(WithoutPointer).ToJson().Should().NotContain("evidence_epa_field");
    }

    /// <summary>
    /// The T133 dead end, for this pointer: emitting it after its field was deleted, or retyped, would produce a schema
    /// the parser refuses, and the Form tab is the only door.
    /// </summary>
    [Fact]
    public void APointerWhoseFieldWasDeletedOrRetyped_IsDropped()
    {
        var deleted = BuilderSchemaModel.Parse(WithPointer);
        deleted.Sections.Single().Fields.RemoveAll(field => field.Key == "epa_id");

        var retyped = BuilderSchemaModel.Parse(WithPointer);
        retyped.Sections.Single().Fields.Single(field => field.Key == "epa_id").Type = FieldType.Text;

        foreach (var json in new[] { deleted.ToJson(), retyped.ToJson() })
        {
            json.Should().NotContain("evidence_epa_field");
            var parse = () => FormSchemaParser.Parse(json);
            parse.Should().NotThrow();
        }
    }

    [Fact]
    public void DeclaringRemovingAndMovingThePointer_AreEachWarnedAboutAtPublish()
    {
        var declared = BuilderSchemaModel.Parse(WithoutPointer);
        declared.EvidenceEpaField = "epa_id";
        BuilderSchemaModel.GetPublishWarnings(WithoutPointer, declared.ToJson())
            .Should().Contain(warning => warning.Contains("'epa_id' will become the EPA for this form"));

        var removed = BuilderSchemaModel.Parse(WithPointer);
        removed.EvidenceEpaField = null;
        BuilderSchemaModel.GetPublishWarnings(WithPointer, removed.ToJson())
            .Should().Contain(warning => warning.Contains("stop recording which field carries the EPA"));

        var moved = BuilderSchemaModel.Parse(WithPointer);
        moved.EvidenceEpaField = "second_epa";
        BuilderSchemaModel.GetPublishWarnings(WithPointer, moved.ToJson())
            .Should().Contain(warning => warning.Contains("The EPA moves from field 'epa_id' to 'second_epa'"));
    }

    // ---- the page -------------------------------------------------------------------------------------------------

    [Fact]
    public void TheFormTab_OffersNoneThenOnlyTheEpaFields_WithTheStoredPointerSelected()
    {
        var cut = RenderPage(new FakeSender(WithPointer));

        var options = cut.FindAll("#evidence-epa-field option")
            .Select(option => option.GetAttribute("value") ?? string.Empty)
            .ToList();

        options.Should().Equal(string.Empty, "epa_id", "second_epa");
        cut.Find("#evidence-epa-field").GetAttribute("value").Should().Be("epa_id");
    }

    [Fact]
    public void ChoosingAnEpaField_SavesThePointerInTheDraftSchema()
    {
        var sender = new FakeSender(WithoutPointer);
        var cut = RenderPage(sender);

        cut.Find("#evidence-epa-field").Change("second_epa");
        cut.FindAll("button").First(button => button.TextContent.Trim() == "Save draft").Click();

        FormSchemaParser.Parse(sender.Saves.Should().ContainSingle().Subject.DraftSchemaJson)
            .EvidenceEpaField.Should().Be("second_epa");
    }

    [Fact]
    public void ChoosingNone_SavesNoPointer()
    {
        var sender = new FakeSender(WithPointer);
        var cut = RenderPage(sender);

        cut.Find("#evidence-epa-field").Change(string.Empty);
        cut.FindAll("button").First(button => button.TextContent.Trim() == "Save draft").Click();

        FormSchemaParser.Parse(sender.Saves.Should().ContainSingle().Subject.DraftSchemaJson)
            .EvidenceEpaField.Should().BeNull();
    }

    private IRenderedComponent<ActivityTypeEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ActivityTypeEdit>(parameters => parameters.Add(page => page.ActivityTypeId, 4));
        cut.WaitForState(() => cut.FindAll("#evidence-epa-field").Count == 1);

        return cut;
    }

    private sealed class FakeSender : IScopedSender
    {
        private readonly ActivityTypeEditorDto _editor;

        public FakeSender(string schemaJson)
        {
            _editor = new ActivityTypeEditorDto(
                4,
                "probe",
                "Probe",
                null,
                ActivityScope.Global,
                null,
                true,
                null,
                1,
                false,
                schemaJson,
                WorkflowJson,
                """{ "counts_for": [] }""",
                "[]",
                schemaJson,
                WorkflowJson,
                """{ "counts_for": [] }""",
                "[]",
                "admin-1",
                null,
                null,
                []);
        }

        public List<SaveActivityTypeDraftCommand> Saves { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                GetActivityTypeEditorQuery => _editor,
                GetInstitutionsListQuery => (IReadOnlyList<InstitutionDto>)[],
                GetSpecialitiesListQuery => (IReadOnlyList<SpecialityDto>)[],
                GetSubSpecialitiesListQuery => (IReadOnlyList<SubSpecialityDto>)[],
                GetWbaToolsQuery => (IReadOnlyList<WbaToolDto>)[],
                SaveActivityTypeDraftCommand save => Save(save),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private ActivityTypeEditorDto Save(SaveActivityTypeDraftCommand command)
        {
            Saves.Add(command);
            return _editor with { DraftSchemaJson = command.DraftSchemaJson, HasDraft = true };
        }
    }
}
