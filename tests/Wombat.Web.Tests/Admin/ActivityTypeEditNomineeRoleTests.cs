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
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T102 fix 3, on the page: the builder's "Names a" select for a User field, and the default keys "Add section" and
/// "Add field" propose.
/// </summary>
/// <remarks>
/// A User field's people come from the directory, so it has no Options and no Catalogue key; showing those boxes would
/// invite an operator to author something the publish check then refuses. The role is the one setting a User field
/// has, and the select must show what the schema holds.
/// </remarks>
public sealed class ActivityTypeEditNomineeRoleTests : TestContext
{
    // The first field is selected when the page loads, so the User field's editor is what renders first.
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "s",
              "title": "Details",
              "fields": [
                { "key": "coordinator_user_id", "type": "user", "label": "Coordinator", "role": "Coordinator" },
                { "key": "field_2", "type": "text", "label": "Notes" }
              ]
            }
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

    public ActivityTypeEditNomineeRoleTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    [Fact]
    public void AUserField_ShowsTheNamesASelect_AndNoOptionsOrCatalogue()
    {
        var cut = RenderPage(new FakeSender(SchemaJson));

        cut.FindAll("#field-role").Should().ContainSingle();
        cut.Find("#field-role").GetAttribute("value").Should().Be(WombatRoles.Coordinator, "the select shows the stored role");
        cut.FindAll("#field-options").Should().BeEmpty("a User field's people come from the directory, not an inline list");
        cut.FindAll("#field-catalogue").Should().BeEmpty();
    }

    [Fact]
    public void ANonUserField_ShowsOptionsAndCatalogue_AndNoRoleSelect()
    {
        var cut = RenderPage(new FakeSender(SchemaJson));

        EditField(cut, "field_2");

        cut.FindAll("#field-role").Should().BeEmpty("only a User field names a person, and the parser refuses a role anywhere else");
        cut.FindAll("#field-options").Should().ContainSingle();
        cut.FindAll("#field-catalogue").Should().ContainSingle();
    }

    [Fact]
    public void TheRoleSelect_OffersTheDefaultFirst_ThenEveryNominableRole_AndNoNationalPendingOrSpecialityScopedRole()
    {
        var cut = RenderPage(new FakeSender(SchemaJson));

        var options = cut.FindAll("#field-role option")
            .Select(option => (Value: option.GetAttribute("value") ?? string.Empty, Label: option.TextContent.Trim()))
            .ToList();

        options.First().Should().Be((string.Empty, "Assessor (default)"),
            "choosing the default saves no role, which is what every seed and every pinned version already means");
        options.Skip(1).Select(option => option.Value).Should()
            .BeEquivalentTo(WombatRoles.Nominable.Where(role => role != WombatRoles.Assessor),
                "Assessor is the default entry, not a second option that would save an explicit copy of it");
        options.Select(option => option.Value).Should()
            .NotContain([WombatRoles.Administrator, WombatRoles.CollegeAdmin, WombatRoles.PendingTrainee]);
        options.Select(option => option.Value).Should()
            .NotContain([WombatRoles.SpecialityAdmin, WombatRoles.SubSpecialityAdmin],
                "their scope is narrower than the institution, which is all the nominee directory checks");
    }

    [Fact]
    public void AStoredExplicitAssessorRole_IsShownAsStored()
    {
        // A schema may spell the default out. The select must show that value, not read as the "" default entry and
        // then quietly save the field without it.
        var cut = RenderPage(new FakeSender(SchemaJson.Replace("\"role\": \"Coordinator\"", "\"role\": \"Assessor\"")));

        cut.Find("#field-role").GetAttribute("value").Should().Be(WombatRoles.Assessor);
        cut.FindAll("#field-role option").Select(option => option.GetAttribute("value"))
            .Should().Contain(WombatRoles.Assessor);
    }

    [Fact]
    public void SwitchingATextFieldToUser_SwapsOptionsForTheRoleSelect()
    {
        var cut = RenderPage(new FakeSender(SchemaJson));
        EditField(cut, "field_2");

        cut.Find("#field-type").Change(nameof(FieldType.User));

        cut.FindAll("#field-role").Should().ContainSingle();
        cut.FindAll("#field-options").Should().BeEmpty();
        cut.FindAll("#field-catalogue").Should().BeEmpty();
    }

    [Fact]
    public void ChoosingARole_IsSavedOnTheField()
    {
        var sender = new FakeSender(SchemaJson);
        var cut = RenderPage(sender);

        cut.Find("#field-role").Change(WombatRoles.CommitteeMember);
        SaveDraft(cut);

        var saved = FormSchemaParser.Parse(sender.Saves.Should().ContainSingle().Subject.DraftSchemaJson);
        Field(saved, "coordinator_user_id").NomineeRole.Should().Be(WombatRoles.CommitteeMember);
    }

    [Fact]
    public void ChoosingTheDefault_SavesNoRole()
    {
        var sender = new FakeSender(SchemaJson);
        var cut = RenderPage(sender);

        cut.Find("#field-role").Change(string.Empty);
        SaveDraft(cut);

        var json = sender.Saves.Should().ContainSingle().Subject.DraftSchemaJson;
        json.Should().NotContain("\"role\"");
        ActorFieldRules.RequiredRolesForUserField(FormSchemaParser.Parse(json), "coordinator_user_id")
            .Should().Equal([WombatRoles.Assessor]);
    }

    [Fact]
    public void ASaveThatNeverTouchesTheRole_KeepsIt()
    {
        // The everyday edit: an operator changes a label and saves. The role must ride along untouched.
        var sender = new FakeSender(SchemaJson);
        var cut = RenderPage(sender);

        cut.Find("#field-label").Change("Programme coordinator");
        SaveDraft(cut);

        var saved = FormSchemaParser.Parse(sender.Saves.Should().ContainSingle().Subject.DraftSchemaJson);
        Field(saved, "coordinator_user_id").NomineeRole.Should().Be(WombatRoles.Coordinator);
        Field(saved, "coordinator_user_id").Label.Should().Be("Programme coordinator");
    }

    [Fact]
    public void ChangingTheRoleOfAPublishedField_ShowsAPublishWarning()
    {
        var cut = RenderPage(new FakeSender(SchemaJson));

        cut.Find("#field-role").Change(WombatRoles.Trainee);
        // The warnings are recomputed against the published version after a save (and on structural edits).
        SaveDraft(cut);

        cut.Markup.Should().Contain("Publish warnings").And.Contain("will name a Trainee");
    }

    [Fact]
    public void TheLivePreview_AsksWithNoSubject_AndFollowsTheRoleSelect()
    {
        // The preview renders the draft being edited, about nobody: with no subject the service lists nobody, so the
        // preview never shows an operator a real institution's staff. It must still ask with the draft's role, and ask
        // again when the operator changes it, or the preview would describe a different field from the one saved.
        var recorder = new RecordingReferenceDataService();
        Services.AddSingleton<IActivityReferenceDataService>(recorder);
        var cut = RenderPage(new FakeSender(SchemaJson));

        var first = recorder.NomineeScopes.Should().ContainSingle().Subject;
        first.SubjectUserId.Should().BeNull();
        first.ForExistingActivity.Should().BeFalse();
        first.RequiredRoles.Should().Equal([WombatRoles.Coordinator]);

        cut.Find("#field-role").Change(WombatRoles.Trainee);

        recorder.NomineeScopes.Should().HaveCount(2);
        recorder.NomineeScopes[1].RequiredRoles.Should().Equal([WombatRoles.Trainee]);
    }

    [Fact]
    public void AddSection_ProposesKeysNoOtherSectionOrFieldUses_AndTheSavedDraftIsPublishable()
    {
        // The old AddSection keyed the new section's first field field_{sections+1}: with one section it proposed
        // field_2, which this schema already uses, and publish now refuses a key declared twice.
        var sender = new FakeSender(SchemaJson);
        var cut = RenderPage(sender);

        ClickButton(cut, "Add section");
        ClickButton(cut, "Add section");
        AddFieldTo(cut, sectionIndex: 0);

        SaveDraft(cut);

        var saved = FormSchemaParser.Parse(sender.Saves.Should().ContainSingle().Subject.DraftSchemaJson);
        saved.Sections.Select(section => section.Key).Should().OnlyHaveUniqueItems();
        saved.Sections.SelectMany(section => section.Fields).Select(field => field.Key).Should().OnlyHaveUniqueItems()
            .And.HaveCount(5, "the two original fields, one per added section, and one added to the first section");

        var publish = () => ActorFieldRules.EnsurePublishable(saved, WorkflowParser.Parse(WorkflowJson));
        publish.Should().NotThrow();
    }

    // ---- helpers ----

    private IRenderedComponent<ActivityTypeEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ActivityTypeEdit>(parameters => parameters.Add(page => page.ActivityTypeId, 4));
        cut.WaitForState(() => cut.FindAll("#field-key").Count == 1);

        return cut;
    }

    private static void EditField(IRenderedComponent<ActivityTypeEdit> cut, string fieldKey)
    {
        // Each field card shows its key in a <code> and has an "Edit" button; pick the card by the key.
        var card = cut.FindAll(".detail-card--interactive")
            .Single(element => element.QuerySelector("code")?.TextContent == fieldKey);
        card.QuerySelectorAll("button").First(button => button.TextContent.Trim() == "Edit")
            .Click();
        cut.WaitForState(() => cut.Find("#field-key").GetAttribute("value") == fieldKey);
    }

    private static void AddFieldTo(IRenderedComponent<ActivityTypeEdit> cut, int sectionIndex)
        => cut.FindAll("button").Where(button => button.TextContent.Trim() == "Add field").ElementAt(sectionIndex).Click();

    private static void ClickButton(IRenderedComponent<ActivityTypeEdit> cut, string label)
        => cut.FindAll("button").First(button => button.TextContent.Trim() == label).Click();

    private static void SaveDraft(IRenderedComponent<ActivityTypeEdit> cut)
        => ClickButton(cut, "Save draft");

    private static Wombat.Domain.Activities.Schema.FormField Field(FormSchema schema, string key)
        => schema.Sections.SelectMany(section => section.Fields).Single(field => field.Key == key);

    private sealed class FakeSender : IScopedSender
    {
        private readonly ActivityTypeEditorDto _editor;

        public FakeSender(string schemaJson)
        {
            _editor = new ActivityTypeEditorDto(
                4,
                "custom_form",
                "Custom form",
                null,
                ActivityScope.Global,
                null,
                true,
                null,
                1,
                false,
                schemaJson,
                WorkflowJson,
                "{}",
                "[]",
                schemaJson,
                WorkflowJson,
                "{}",
                "[]",
                "admin-1",
                null,
                null,
                [],
                true,
                [new ActivityTypeScopeChoiceDto(ActivityScope.Global, [])],
                null);
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
