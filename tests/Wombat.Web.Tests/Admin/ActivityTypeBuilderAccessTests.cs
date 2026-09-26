using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
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
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T300: the builder follows the rule the commands refuse by. Where the editor says the caller may not write the type
/// (<c>ActivityTypeEditorDto.CanWrite</c>, from <c>ActivityTypeAdminScope</c>), it offers no Save draft, Discard draft or
/// Publish and no Add, Up, Down or Delete, shows what is stored as text, and says at the top whose type it is. A new
/// type's Scope picker lists exactly the scopes the caller may save in (<c>WritableScopes</c>) and starts on the first.
/// </summary>
/// <remarks>
/// Before T300 Prof Mbatha (InstitutionalAdmin) was offered every edit on Mini-CEX (Paediatrics), a College instrument
/// the guard refused her every save on (Step 1.25), and a new type started Global, which the guard refused on its first
/// save (Step 1.26). Which caller may write which type is the Application table's (<c>ActivityTypeScopeGuardTests</c>);
/// here the page is held to what the editor tells it.
/// </remarks>
public sealed class ActivityTypeBuilderAccessTests : TestContext
{
    private const int Kalafong = 7;
    private const int Paediatrics = 5;
    private const int PaediatricsSub = 9;

    private static readonly string[] EditLabels = ["Save draft", "Discard draft", "Publish", "Add section", "Add field", "Up", "Down", "Delete"];

    private const string SchemaJson = """
        {
          "version": 1,
          "rated_level_field": "overall_level",
          "sections": [
            {
              "key": "assessment",
              "title": "Assessment",
              "fields": [
                { "key": "overall_level", "type": "scale", "label": "Overall level", "scale_key": "cpsa-v11-1" },
                { "key": "notes", "type": "longtext", "label": "Notes" }
              ]
            },
            { "key": "sign_off", "title": "Sign-off", "fields": [ { "key": "comment", "type": "text", "label": "Comment" } ] }
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

    private const string CreditRulesJson = """{ "counts_for": [] }""";

    private static readonly IReadOnlyList<ActivityTypeScopeChoiceDto> InstitutionalAdminScopes =
    [
        new(ActivityScope.Institution, [new ActivityTypeScopeTargetDto(Kalafong, "Kalafong")])
    ];

    private static readonly IReadOnlyList<ActivityTypeScopeChoiceDto> CollegeAdminScopes =
    [
        new(ActivityScope.Speciality, [new ActivityTypeScopeTargetDto(Paediatrics, "Paediatrics")]),
        new(ActivityScope.SubSpeciality, [new ActivityTypeScopeTargetDto(PaediatricsSub, "Paediatrics / Paediatrics")])
    ];

    private static readonly IReadOnlyList<ActivityTypeScopeChoiceDto> AdministratorScopes =
    [
        new(ActivityScope.Global, []),
        new(ActivityScope.Institution, [new ActivityTypeScopeTargetDto(3, "Baragwanath"), new ActivityTypeScopeTargetDto(Kalafong, "Kalafong")]),
        new(ActivityScope.Speciality, [new ActivityTypeScopeTargetDto(Paediatrics, "Paediatrics")]),
        new(ActivityScope.SubSpeciality, [new ActivityTypeScopeTargetDto(PaediatricsSub, "Paediatrics / Paediatrics")])
    ];

    public ActivityTypeBuilderAccessTests()
    {
        Services.AddSingleton<IActivityReferenceDataService>(new ScaleOptions());
    }

    [Fact]
    public void ACollegeInstrument_ToAnInstitution_OffersNoEdit_AndSaysWhoseItIs()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = Render(new FakeSender(CollegeInstrument(canWrite: false)));

        EditButtons(cut).Should().BeEmpty("the guard refuses her every save, discard and publish on it");

        var alert = cut.Find("#activity-type-read-only");
        alert.ClassList.Should().Contain("alert-info");
        alert.HasAttribute("role").Should().BeFalse("it is standing content, there on every visit (Role=\"\")");
        alert.TextContent.Should().Contain("Set by the College").And.Contain("Paediatrics").And.Contain("not change it");

        cut.Find("h1").TextContent.Should().Be("Mini-CEX (Paediatrics)", "she is not editing it");
    }

    [Fact]
    public void ACollegeInstrument_ToItsCollege_OffersEveryEdit()
    {
        SignIn(WombatRoles.CollegeAdmin);
        var cut = Render(new FakeSender(CollegeInstrument(canWrite: true)));

        EditButtons(cut).Select(button => button.TextContent.Trim()).Distinct()
            .Should().BeEquivalentTo(EditLabels, "the College writes its disciplines' types (T091)");
        cut.FindAll("#activity-type-read-only").Should().BeEmpty();
        cut.Find("h1").TextContent.Should().Be("Edit Mini-CEX (Paediatrics)");
    }

    /// <summary>
    /// The field editor still opens to a reader, so a field can be read: Step 1.25 reads overall_level's scale there
    /// (T271 item 2). What it shows is text, not controls (DESIGN.md § Form system, T302).
    /// </summary>
    [Fact]
    public void AReader_StillOpensEachField_AndReadsItsSettingsAsText()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = Render(new FakeSender(CollegeInstrument(canWrite: false)));

        cut.FindAll("#field-key, #field-label, #field-type, #field-scale, #field-required, #section-key, #section-title")
            .Should().BeEmpty("a reader is given no control");
        FieldDetail(cut, "Entrustment scale").Should().Be("CPSA v11.1 ladder", "the first field opens with the page");

        cut.FindAll("button").Single(button => button.GetAttribute("aria-label") == "View field Notes").Click();

        FieldDetail(cut, "Label").Should().Be("Notes");
        FieldDetail(cut, "Type").Should().Be("Long text");
        cut.FindAll("button").Where(button => button.TextContent.Trim() == "Edit").Should().BeEmpty(
            "a reader's buttons say View, which is what they do");
    }

    [Fact]
    public void AReader_ReadsTheMetadataWorkflowAndCredit_AsText()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = Render(new FakeSender(CollegeInstrument(canWrite: false)));

        Tab(cut, "Metadata");
        cut.FindAll("input, select, textarea").Should().BeEmpty("the metadata is shown, not offered");
        Detail(cut, "Scope").Should().Be("Speciality · Paediatrics");
        Detail(cut, "Name").Should().Be("Mini-CEX (Paediatrics)");
        Detail(cut, "Status").Should().Be("Active");

        Tab(cut, "Workflow");
        cut.FindAll("textarea").Should().BeEmpty();
        cut.Find("pre.code-block").TextContent.Should().Contain("\"initial_state\": \"draft\"");

        Tab(cut, "Credit");
        cut.FindAll("textarea").Should().BeEmpty();
        cut.Find("pre.code-block").TextContent.Should().Contain("counts_for");
    }

    [Fact]
    public void ANewType_ToAnInstitutionalAdmin_OffersHerInstitutionOnly_AndSavesThereUntouched()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var sender = new FakeSender(NewType(InstitutionalAdminScopes));
        var cut = Render(sender, activityTypeId: null);

        Tab(cut, "Metadata");
        Options(cut, "#type-scope").Should().Equal(("Institution", "Institution"));
        Options(cut, "#type-scope-id").Should().Equal((Kalafong.ToString(), "Kalafong"));
        cut.Find("#type-scope-id").GetAttribute("value").Should().Be(Kalafong.ToString(), "her institution is chosen for her");

        cut.Find("#type-key").Change("kgk_teaching_log");
        cut.Find("#type-name").Change("KGK Teaching Session Log");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Save draft").Click();

        var save = sender.Saves.Should().ContainSingle().Subject;
        save.Scope.Should().Be(ActivityScope.Institution);
        save.ScopeId.Should().Be(Kalafong);
    }

    [Fact]
    public void ANewType_ToACollegeAdmin_OffersHisCollegesSpecialityAndSubSpeciality()
    {
        SignIn(WombatRoles.CollegeAdmin);
        var cut = Render(new FakeSender(NewType(CollegeAdminScopes)), activityTypeId: null);

        Tab(cut, "Metadata");
        Options(cut, "#type-scope").Should().Equal(("Speciality", "Speciality"), ("SubSpeciality", "Sub-speciality"));
        Options(cut, "#type-scope-id").Should().Equal((Paediatrics.ToString(), "Paediatrics"));

        cut.Find("#type-scope").Change("SubSpeciality");

        Options(cut, "#type-scope-id").Should().Equal((PaediatricsSub.ToString(), "Paediatrics / Paediatrics"));
        cut.Find("#type-scope-id").GetAttribute("value").Should().Be(PaediatricsSub.ToString(),
            "a scope's first target is chosen when the scope changes, so no save goes without one");
    }

    [Fact]
    public void ANewType_ToAnAdministrator_OffersAllFourScopes()
    {
        SignIn(WombatRoles.Administrator);
        var cut = Render(new FakeSender(NewType(AdministratorScopes)), activityTypeId: null);

        Tab(cut, "Metadata");
        Options(cut, "#type-scope").Select(option => option.Label)
            .Should().Equal("Global", "Institution", "Speciality", "Sub-speciality");
        cut.FindAll("#type-scope-id").Should().BeEmpty("a Global type has no target");
        cut.Find("h1").TextContent.Should().Be("New activity type");
    }

    [Fact]
    public void ANewType_ToACallerWithNoScope_OffersNothingToSave()
    {
        SignIn(WombatRoles.CollegeAdmin);
        var cut = Render(new FakeSender(NewType([])), activityTypeId: null);

        EditButtons(cut).Should().BeEmpty();
        cut.Find("#activity-type-no-scope").TextContent.Should().Contain("no scope");
        cut.FindAll(".tab-bar").Should().BeEmpty();
    }

    /// <summary>
    /// T295 sweep: while the editor loads, and after it fails to, the page is not a new type and offers no Save draft,
    /// whose create would be refused on a scope or an empty workflow.
    /// </summary>
    [Fact]
    public void WhileTheEditorLoads_ThePageIsNotANewType_AndOffersNoSaveDraft()
    {
        SignIn(WombatRoles.Administrator);
        var cut = Render(new FakeSender(new TaskCompletionSource<ActivityTypeEditorDto>().Task));

        cut.Markup.Should().NotContain("New activity type");
        EditButtons(cut).Should().BeEmpty();
        cut.Find("h1").TextContent.Should().Be("Activity type");
    }

    [Fact]
    public void AnUnknownType_IsNotANewType_AndOffersNoSaveDraft()
    {
        SignIn(WombatRoles.Administrator);
        var cut = Render(new FakeSender(Task.FromException<ActivityTypeEditorDto>(
            new InvalidOperationException("The activity type could not be found."))));

        cut.WaitForState(() => cut.Markup.Contains("could not be found"));
        cut.Markup.Should().NotContain("New activity type");
        EditButtons(cut).Should().BeEmpty();
    }

    /// <summary>
    /// A refusal is shown in its own words (DESIGN.md § Alerts, T213): a validation refusal by its failures' messages, not
    /// the log's "Validation failed: -- Key: …" (the T300 review; the page printed <c>exception.Message</c>).
    /// </summary>
    [Fact]
    public void ARefusedSave_IsShownInTheRefusalsOwnWords()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var sender = new FakeSender(NewType(InstitutionalAdminScopes))
        {
            SaveRefusal = new ValidationException([new ValidationFailure("Key", "Give the activity type a key.")])
        };
        var cut = Render(sender, activityTypeId: null);

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Save draft").Click();

        var refusal = cut.Find(".alert-danger");
        refusal.TextContent.Trim().Should().Be("Give the activity type a key.");
        refusal.TextContent.Should().NotContain("Validation failed");
    }

    // ---- helpers ----

    private void SignIn(string role)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("caller@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "caller-1"));
    }

    private IRenderedComponent<ActivityTypeEdit> Render(FakeSender sender, int? activityTypeId = 11)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ActivityTypeEdit>(parameters => parameters.Add(page => page.ActivityTypeId, activityTypeId));
        if (sender.Loads)
        {
            cut.WaitForState(() => cut.FindAll(".tab-bar, #activity-type-no-scope").Count > 0);
        }

        return cut;
    }

    private static List<IElement> EditButtons(IRenderedFragment cut)
        => cut.FindAll("button").Where(button => EditLabels.Contains(button.TextContent.Trim())).ToList();

    private static void Tab(IRenderedFragment cut, string label)
        => cut.FindAll(".tab-bar-tab").Single(tab => tab.TextContent.Trim() == label).Click();

    private static List<(string Value, string Label)> Options(IRenderedFragment cut, string select)
        => cut.FindAll($"{select} option")
            .Select(option => (option.GetAttribute("value") ?? string.Empty, option.TextContent.Trim()))
            .ToList();

    /// <summary>A metadata field shown as text: the <c>&lt;dd&gt;</c> under its <c>&lt;dt&gt;</c>.</summary>
    private static string Detail(IRenderedFragment cut, string term)
        => cut.FindAll("dl.form-group")
            .Single(list => list.QuerySelector("dt")!.TextContent.Trim() == term)
            .QuerySelector("dd")!.TextContent.Trim();

    /// <summary>A field editor setting shown as text.</summary>
    private static string FieldDetail(IRenderedFragment cut, string term)
        => cut.FindAll("#field-details dl.form-group")
            .Single(list => list.QuerySelector("dt")!.TextContent.Trim() == term)
            .QuerySelector("dd")!.TextContent.Trim();

    private static ActivityTypeEditorDto CollegeInstrument(bool canWrite) => new(
        11, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", "The College's Mini-CEX.", ActivityScope.Speciality, Paediatrics,
        true, "mini_cex", 1, true,
        SchemaJson, WorkflowJson, CreditRulesJson, "[]",
        SchemaJson, WorkflowJson, CreditRulesJson, "[]",
        "seeder", "seeder", null, [],
        canWrite,
        canWrite ? CollegeAdminScopes : [],
        "Paediatrics");

    private static ActivityTypeEditorDto NewType(IReadOnlyList<ActivityTypeScopeChoiceDto> writableScopes)
    {
        var first = writableScopes.FirstOrDefault();
        var target = first?.Targets.FirstOrDefault();
        return new ActivityTypeEditorDto(
            0, string.Empty, string.Empty, null, first?.Scope ?? ActivityScope.Global, target?.Id, true, null, 0, false,
            SchemaJson, WorkflowJson, CreditRulesJson, "[]",
            null, null, null, "[]",
            string.Empty, null, null, [],
            writableScopes.Count > 0,
            writableScopes,
            target?.Name);
    }

    private sealed class ScaleOptions : StubActivityReferenceDataService
    {
        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleOptionsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([new ActivityCatalogueOption("cpsa-v11-1", "CPSA v11.1 ladder")]);
    }

    private sealed class FakeSender : IScopedSender
    {
        private readonly Task<ActivityTypeEditorDto> _editor;

        public FakeSender(ActivityTypeEditorDto editor) : this(Task.FromResult(editor))
        {
        }

        public FakeSender(Task<ActivityTypeEditorDto> editor)
        {
            _editor = editor;
        }

        /// <summary>Whether the editor answers, so the test can wait for the page to have loaded.</summary>
        public bool Loads => _editor.IsCompletedSuccessfully;

        public List<SaveActivityTypeDraftCommand> Saves { get; } = [];

        /// <summary>When set, a save is refused with it.</summary>
        public Exception? SaveRefusal { get; init; }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                GetActivityTypeEditorQuery => await _editor,
                GetInstitutionsListQuery => (IReadOnlyList<InstitutionDto>)[],
                GetSpecialitiesListQuery => (IReadOnlyList<SpecialityDto>)[],
                GetSubSpecialitiesListQuery => (IReadOnlyList<SubSpecialityDto>)[],
                GetWbaToolsQuery => (IReadOnlyList<WbaToolDto>)[new WbaToolDto("mini_cex", "Mini-CEX", null)],
                SaveActivityTypeDraftCommand save => Save(save),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)response;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private ActivityTypeEditorDto Save(SaveActivityTypeDraftCommand command)
        {
            Saves.Add(command);
            if (SaveRefusal is not null)
            {
                throw SaveRefusal;
            }

            return _editor.Result with { Id = 42, HasDraft = true, Scope = command.Scope, ScopeId = command.ScopeId };
        }
    }
}
