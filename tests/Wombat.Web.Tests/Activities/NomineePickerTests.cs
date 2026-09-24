using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T102: what <see cref="ActivityForm" /> asks the reference-data service about a <c>user</c> field.
/// </summary>
/// <remarks>
/// <para>
/// The picker and the write path share one predicate (<c>NomineeDirectory</c>): users holding every required role, at
/// the activity's institution, not deactivated, not the subject. The service can only answer that if the form hands it
/// the subject, the field's roles, and which institution to use, exactly as the page passed them. A form that got any of
/// them wrong would offer people submitting then refuses, or hide the one a trainee is allowed to name.
/// </para>
/// <para>
/// A field the viewer cannot write asks only for the stored person's label: a reader is shown who was named, never the
/// list of everyone who could have been.
/// </para>
/// </remarks>
public sealed class NomineePickerTests : TestContext
{
    // Two user fields with different roles, plus an EPA field, so each assertion can say which call is which.
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA" },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor" },
                { "key": "coordinator_user_id", "type": "user", "label": "Coordinator", "role": "Coordinator" },
                { "key": "notes", "type": "text", "label": "Notes" }
              ]
            }
          ]
        }
        """;

    private readonly RecordingReferenceDataService _recorder = new();

    public NomineePickerTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IActivityReferenceDataService>(_recorder);
    }

    // ---- a writable user field asks the directory, with the page's scope exactly as passed ----

    [Fact]
    public void AWritableUserField_AsksForNominees_WithTheSubjectRolesAndStampedInstitution()
    {
        RenderForm(
            dataJson: """{"assessor_user_id":"assessor-9"}""",
            isExistingActivity: true,
            activityInstitutionId: 42);

        _recorder.NomineeScopes.Should().HaveCount(2, "one call per user field");

        var assessor = _recorder.NomineeScopes[0];
        assessor.SubjectUserId.Should().Be("trainee-1", "the subject, never the viewer: the subject is who may not be named");
        assessor.RequiredRoles.Should().Equal([WombatRoles.Assessor], "a user field with no role names an Assessor");
        assessor.ForExistingActivity.Should().BeTrue();
        assessor.ActivityInstitutionId.Should().Be(42, "an existing activity is judged against its stamped institution");
        assessor.StoredValue.Should().Be("assessor-9", "the service keeps a stored nominee on the list even if they fell off it");

        var coordinator = _recorder.NomineeScopes[1];
        coordinator.RequiredRoles.Should().Equal([WombatRoles.Coordinator]);
        coordinator.StoredValue.Should().BeNull("an empty field has no stored nominee to keep");

        _recorder.UserLabelRequests.Should().BeEmpty("a writer gets the list, not a single label");
    }

    [Fact]
    public void TheCreatePage_AsksWithNoStampedInstitution_SoTheServiceResolvesItFromTheSubject()
    {
        // NewActivity passes neither: the activity does not exist yet, and the service resolves the institution the
        // create will stamp (SubjectScopeResolver), the same way the write path will.
        RenderForm(dataJson: "{}");

        _recorder.NomineeScopes.Should().HaveCount(2);
        _recorder.NomineeScopes.Should().OnlyContain(scope =>
            scope.ForExistingActivity == false &&
            scope.ActivityInstitutionId == null &&
            scope.SubjectUserId == "trainee-1" &&
            scope.StoredValue == null);
    }

    [Fact]
    public void AnExistingActivityWithNoStamp_IsPassedOnAsExistingWithANullInstitution()
    {
        // Not "the create page": a null stamp on an existing activity lists nobody, exactly as the write path accepts
        // nobody. Collapsing it into the create-page shape would have the service resolve an institution from the
        // subject and offer people the gate then refuses.
        RenderForm(dataJson: "{}", isExistingActivity: true, activityInstitutionId: null);

        _recorder.NomineeScopes.Should().OnlyContain(scope =>
            scope.ForExistingActivity == true && scope.ActivityInstitutionId == null);
    }

    [Fact]
    public void AKeyDeclaredTwiceWithDifferentRoles_AsksForBothRoles()
    {
        // Publish refuses a duplicate key from T102 on, but a stored version may carry one. The gate requires every
        // role any declaration names; the picker must ask for the same conjunction, or it offers people the gate refuses.
        const string duplicated = """
            {
              "version": 1,
              "sections": [
                { "key": "a", "title": "A", "fields": [ { "key": "assessor_user_id", "type": "user", "label": "Assessor" } ] },
                { "key": "b", "title": "B", "fields": [ { "key": "assessor_user_id", "type": "user", "label": "Assessor again", "role": "Coordinator" } ] }
              ]
            }
            """;

        RenderForm(dataJson: "{}", schemaJson: duplicated);

        _recorder.NomineeScopes.Should().NotBeEmpty()
            .And.OnlyContain(scope => scope.RequiredRoles.SequenceEqual(new[] { WombatRoles.Assessor, WombatRoles.Coordinator }));
    }

    [Fact]
    public void TheDirectorysOptions_AreRendered_AndTheStoredNomineeIsSelected()
    {
        _recorder.Nominees =
        [
            new ActivityCatalogueOption("assessor-1", "Dr One"),
            new ActivityCatalogueOption("assessor-9", "Dr Nine (not on the current list)")
        ];

        var cut = RenderForm(dataJson: """{"assessor_user_id":"assessor-9"}""", isExistingActivity: true, activityInstitutionId: 42);

        var options = cut.FindAll("#assessor_user_id option");
        options.Select(option => option.GetAttribute("value")).Should().Equal("", "assessor-1", "assessor-9");
        options.Single(option => option.HasAttribute("selected")).GetAttribute("value").Should().Be("assessor-9");
        cut.Find("#assessor_user_id").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void ChoosingANominee_ReportsTheirIdAsTheFieldValue()
    {
        _recorder.Nominees = [new ActivityCatalogueOption("assessor-1", "Dr One")];
        string? captured = null;

        var cut = RenderForm(dataJson: "{}", onChanged: value => captured = value);
        cut.Find("#assessor_user_id").Change("assessor-1");

        captured.Should().Be("""{"assessor_user_id":"assessor-1"}""");
    }

    // ---- a locked or read-only user field asks for one label, never the list ----

    [Fact]
    public void ALockedUserField_AsksOnlyForTheStoredPersonsLabel()
    {
        // The assessor on the detail page: the trainee's request, including who they named, is locked for them. They
        // are shown the name, and are not sent the institution's whole list of assessors.
        _recorder.Labels["assessor-9"] = new ActivityCatalogueOption("assessor-9", "Dr Nine");

        var cut = RenderForm(
            dataJson: """{"assessor_user_id":"assessor-9"}""",
            isExistingActivity: true,
            activityInstitutionId: 42,
            editableFieldKeys: Writable("notes"));

        _recorder.NomineeScopes.Should().BeEmpty("a reader is never sent the list of everyone who could have been named");
        _recorder.UserLabelRequests.Should().Equal(["assessor-9"],
            "only the field with a stored value asks, and only for that value");

        var select = cut.Find("#assessor_user_id");
        select.HasAttribute("disabled").Should().BeTrue();
        var options = cut.FindAll("#assessor_user_id option");
        options.Select(option => option.TextContent.Trim()).Should().Equal("Select…", "Dr Nine");
        options.Single(option => option.HasAttribute("selected")).GetAttribute("value").Should().Be("assessor-9");
    }

    [Fact]
    public void AReadOnlyForm_AsksOnlyForLabels_EvenWithNoWritableSet()
    {
        // ActivityDetail: ReadOnly with a null writable set. ReadOnly alone must lock the field.
        _recorder.Labels["assessor-9"] = new ActivityCatalogueOption("assessor-9", "Dr Nine");

        RenderForm(
            dataJson: """{"assessor_user_id":"assessor-9","coordinator_user_id":"coord-3"}""",
            isExistingActivity: true,
            activityInstitutionId: 42,
            readOnly: true);

        _recorder.NomineeScopes.Should().BeEmpty();
        _recorder.UserLabelRequests.Should().Equal(["assessor-9", "coord-3"]);
    }

    [Fact]
    public void AWorkingValueTheActivityDoesNotStore_IsNeverLookedUp()
    {
        // The leak the T102 review found: a value typed into the page by hand (any user's id, from any institution) was
        // passed on as the value to keep, and the service answered with that person's name and email. Only a value
        // stored on the activity may be labelled.
        var cut = RenderForm(
            dataJson: """{"assessor_user_id":"someone-elsewhere"}""",
            isExistingActivity: true,
            activityInstitutionId: 42,
            storedDataJson: "{}");

        _recorder.NomineeScopes.Should().HaveCount(2);
        _recorder.NomineeScopes[0].StoredValue.Should().BeNull("the activity stores no nominee, whatever the page holds");
        _recorder.UserLabelRequests.Should().BeEmpty();
    }

    [Fact]
    public void OnTheCreatePage_NothingIsStored_SoAHandTypedValueIsNeverLookedUp()
    {
        RenderForm(dataJson: """{"assessor_user_id":"someone-elsewhere"}""");

        _recorder.NomineeScopes.Should().OnlyContain(scope => scope.StoredValue == null);
        _recorder.UserLabelRequests.Should().BeEmpty();
    }

    [Fact]
    public void ALockedFieldWhoseWorkingValueDiffersFromTheStoredOne_LabelsOnlyTheStoredOne()
    {
        _recorder.Labels["assessor-9"] = new ActivityCatalogueOption("assessor-9", "Dr Nine");

        RenderForm(
            dataJson: """{"assessor_user_id":"someone-elsewhere"}""",
            isExistingActivity: true,
            activityInstitutionId: 42,
            readOnly: true,
            storedDataJson: """{"assessor_user_id":"assessor-9"}""");

        _recorder.UserLabelRequests.Should().Equal(["assessor-9"]);
    }

    [Fact]
    public void ALockedEmptyUserField_AsksForNothing()
    {
        var cut = RenderForm(dataJson: "{}", readOnly: true);

        _recorder.NomineeScopes.Should().BeEmpty();
        _recorder.UserLabelRequests.Should().BeEmpty();
        cut.FindAll("#assessor_user_id option").Should().ContainSingle().Which.TextContent.Trim().Should().Be("Select…");
    }

    // ---- a working value the list does not hold ----

    [Fact]
    public void AWritableFieldsWorkingValueOffTheList_IsShownSelected_LabelledOnThePage_AndNeverLookedUp()
    {
        // After a refusal the remounted form's fresh list no longer holds the refused person, but the working copy
        // does, and the next action would send it. The select must show it as chosen (not fall back to "Select…", which
        // would make choosing "Select…" a no-op), under a label built here: the id may be anyone's.
        _recorder.Nominees = [new ActivityCatalogueOption("assessor-1", "Dr One")];

        var cut = RenderForm(
            dataJson: """{"assessor_user_id":"x"}""",
            isExistingActivity: true,
            activityInstitutionId: 42,
            storedDataJson: "{}");

        var options = cut.FindAll("#assessor_user_id option");
        options.Select(option => option.GetAttribute("value")).Should().Equal("", "assessor-1", "x");
        var selected = options.Single(option => option.HasAttribute("selected"));
        selected.GetAttribute("value").Should().Be("x");
        selected.TextContent.Trim().Should().Be("Not available (choose someone else)");
        _recorder.UserLabelRequests.Should().BeEmpty("the working value is labelled on the page, never looked up");

        cut.FindAll("#coordinator_user_id option").Select(option => option.GetAttribute("value"))
            .Should().Equal(["", "assessor-1"], "an empty field gets no extra option");
    }

    [Fact]
    public void ALockedFieldsWorkingValueOffTheList_GetsNoNeutralOption()
    {
        // A locked field cannot send its working value, so there is nothing to show: it shows the stored person only.
        _recorder.Labels["assessor-9"] = new ActivityCatalogueOption("assessor-9", "Dr Nine");

        var cut = RenderForm(
            dataJson: """{"assessor_user_id":"someone-elsewhere"}""",
            isExistingActivity: true,
            activityInstitutionId: 42,
            editableFieldKeys: Writable("notes"),
            storedDataJson: """{"assessor_user_id":"assessor-9"}""");

        var options = cut.FindAll("#assessor_user_id option");
        options.Select(option => option.GetAttribute("value")).Should().Equal("", "assessor-9");
        options.Select(option => option.TextContent.Trim()).Should().NotContain("Not available (choose someone else)");
        _recorder.UserLabelRequests.Should().Equal(["assessor-9"]);
    }

    [Fact]
    public void ALockedUserFieldNamingNobodyKnown_StillShowsTheStoredValue()
    {
        // An erased or mistyped id: the select must not fall back to "Select…", which would read as "nobody was
        // named" on a record that names someone.
        var cut = RenderForm(dataJson: """{"assessor_user_id":"ghost-1"}""", readOnly: true);

        var selected = cut.FindAll("#assessor_user_id option").Single(option => option.HasAttribute("selected"));
        selected.GetAttribute("value").Should().Be("ghost-1");
        selected.TextContent.Trim().Should().Be("Unknown person");
    }

    [Fact]
    public void AWritableAndALockedUserField_AreEachAskedTheirOwnWay()
    {
        _recorder.Labels["coord-3"] = new ActivityCatalogueOption("coord-3", "Ms Three");

        RenderForm(
            dataJson: """{"assessor_user_id":"assessor-9","coordinator_user_id":"coord-3"}""",
            isExistingActivity: true,
            activityInstitutionId: 42,
            editableFieldKeys: Writable("assessor_user_id"));

        _recorder.NomineeScopes.Should().ContainSingle()
            .Which.RequiredRoles.Should().Equal([WombatRoles.Assessor]);
        _recorder.UserLabelRequests.Should().Equal(["coord-3"]);
    }

    // ---- inline options never reach a user or EPA field ----

    [Fact]
    public void AUserFieldsInlineOptions_AreIgnored_InFavourOfTheDirectory()
    {
        // Publish refuses options on a user field, but a stored version may carry them. Rendering them would offer ids
        // the write path refuses, and would hide the service's stored-value fallback behind a stale inline list.
        const string withOptions = """
            {
              "version": 1,
              "sections": [
                { "key": "s", "title": "S", "fields": [
                  { "key": "assessor_user_id", "type": "user", "label": "Assessor", "options": ["inline-person"] }
                ] }
              ]
            }
            """;
        _recorder.Nominees = [new ActivityCatalogueOption("assessor-1", "Dr One")];

        var cut = RenderForm(dataJson: "{}", schemaJson: withOptions);

        cut.FindAll("#assessor_user_id option").Select(option => option.GetAttribute("value"))
            .Should().Equal("", "assessor-1");
        _recorder.NomineeScopes.Should().ContainSingle("the directory is still asked even though inline options exist");
    }

    [Fact]
    public void AnEpaFieldsInlineOptions_AreIgnored_InFavourOfTheServersList()
    {
        // The same rule for EPA fields (T122): the server's list is what the tool gate and credit check.
        const string withOptions = """
            {
              "version": 1,
              "sections": [
                { "key": "s", "title": "S", "fields": [
                  { "key": "epa_id", "type": "epa", "label": "EPA", "options": ["999"] }
                ] }
              ]
            }
            """;
        _recorder.Epas = [new ActivityCatalogueOption("17", "PAED-017")];

        var cut = RenderForm(dataJson: "{}", schemaJson: withOptions);

        cut.FindAll("#epa_id option").Select(option => option.GetAttribute("value"))
            .Should().Equal("", "17");
    }

    [Fact]
    public void AChoiceFieldsInlineOptions_AreStillRendered()
    {
        // The guard is scoped to user and EPA fields; every other select still renders its own options.
        const string choice = """
            {
              "version": 1,
              "sections": [
                { "key": "s", "title": "S", "fields": [
                  { "key": "setting", "type": "choice", "label": "Setting", "options": ["ward", "clinic"] }
                ] }
              ]
            }
            """;

        var cut = RenderForm(dataJson: "{}", schemaJson: choice);

        cut.FindAll("#setting option").Select(option => option.GetAttribute("value"))
            .Should().Equal("", "ward", "clinic");
    }

    // ---- when the options reload ----

    [Fact]
    public void BecomingWritable_ReloadsTheOptions_SoTheFieldGetsItsList()
    {
        // On the detail page the writable set changes with the state. A field that was locked loaded only its label;
        // if the reload key ignored the writable set, the now-writable field would offer only the stored person.
        var cut = RenderForm(
            dataJson: """{"assessor_user_id":"assessor-9"}""",
            isExistingActivity: true,
            activityInstitutionId: 42,
            editableFieldKeys: Writable("notes"));

        _recorder.NomineeScopes.Should().BeEmpty();

        cut.SetParametersAndRender(parameters => parameters
            .Add(component => component.EditableFieldKeys, Writable("notes", "assessor_user_id")));

        _recorder.NomineeScopes.Should().ContainSingle()
            .Which.StoredValue.Should().Be("assessor-9");
    }

    [Fact]
    public void BecomingLocked_ReloadsTheOptions_AndStopsOfferingTheList()
    {
        _recorder.Nominees = [new ActivityCatalogueOption("assessor-1", "Dr One"), new ActivityCatalogueOption("assessor-2", "Dr Two")];
        _recorder.Labels["assessor-1"] = new ActivityCatalogueOption("assessor-1", "Dr One");

        var cut = RenderForm(
            dataJson: """{"assessor_user_id":"assessor-1"}""",
            isExistingActivity: true,
            activityInstitutionId: 42,
            editableFieldKeys: Writable("assessor_user_id"));

        cut.FindAll("#assessor_user_id option").Should().HaveCount(3);

        cut.SetParametersAndRender(parameters => parameters
            .Add(component => component.EditableFieldKeys, Writable("notes")));

        _recorder.UserLabelRequests.Should().Contain("assessor-1");
        cut.FindAll("#assessor_user_id option").Select(option => option.GetAttribute("value"))
            .Should().Equal(["", "assessor-1"], "a reader sees who was named and no one else");
    }

    [Fact]
    public void TheSameWritableSet_InADifferentOrderOrInstance_DoesNotReload()
    {
        // The reload key is the other half of the contract: data changes on every keystroke, and ActivityView builds a
        // new HashSet on every load. Re-querying for an equal set is the concurrent-DbContext hazard the key avoids.
        var cut = RenderForm(
            dataJson: "{}",
            isExistingActivity: true,
            activityInstitutionId: 42,
            editableFieldKeys: Writable("assessor_user_id", "notes"));

        var calls = _recorder.NomineeScopes.Count;

        cut.SetParametersAndRender(parameters => parameters
            .Add(component => component.EditableFieldKeys, Writable("notes", "assessor_user_id"))
            .Add(component => component.DataJson, """{"notes":"typed"}"""));

        _recorder.NomineeScopes.Should().HaveCount(calls);
    }

    [Fact]
    public void ADifferentStampedInstitution_ReloadsTheOptions()
    {
        var cut = RenderForm(dataJson: "{}", isExistingActivity: true, activityInstitutionId: 42);

        cut.SetParametersAndRender(parameters => parameters
            .Add(component => component.ActivityInstitutionId, 43));

        _recorder.NomineeScopes.Should().HaveCount(4);
        _recorder.NomineeScopes.Skip(2).Should().OnlyContain(scope => scope.ActivityInstitutionId == 43);
    }

    [Fact]
    public void SwitchingFromCreateToExisting_ReloadsTheOptions_EvenWithTheSameNullInstitution()
    {
        // The flag alone changes the answer: a create resolves the institution from the subject, an existing activity
        // with no stamp lists nobody. So the flag must be in the reload key on its own, not only via the institution.
        var cut = RenderForm(dataJson: "{}");

        cut.SetParametersAndRender(parameters => parameters
            .Add(component => component.IsExistingActivity, true));

        _recorder.NomineeScopes.Should().HaveCount(4);
        _recorder.NomineeScopes.Skip(2).Should().OnlyContain(scope => scope.ForExistingActivity && scope.ActivityInstitutionId == null);
    }

    [Fact]
    public void BecomingReadOnly_ReloadsTheOptions_AndAsksOnlyForLabels()
    {
        // ReadOnly locks every field, so a form that turns read-only must drop the list it loaded as a writer.
        _recorder.Nominees = [new ActivityCatalogueOption("assessor-1", "Dr One"), new ActivityCatalogueOption("assessor-2", "Dr Two")];
        _recorder.Labels["assessor-1"] = new ActivityCatalogueOption("assessor-1", "Dr One");

        var cut = RenderForm(dataJson: """{"assessor_user_id":"assessor-1"}""", isExistingActivity: true, activityInstitutionId: 42);

        cut.SetParametersAndRender(parameters => parameters
            .Add(component => component.ReadOnly, true));

        _recorder.UserLabelRequests.Should().Equal(["assessor-1"]);
        cut.FindAll("#assessor_user_id option").Select(option => option.GetAttribute("value"))
            .Should().Equal(["", "assessor-1"]);
    }

    // ---- each load has a scope of its own, and only the newest load's lists are kept ----

    [Fact]
    public void EachOptionLoad_ResolvesTheServiceFromAScopeOfItsOwn()
    {
        // The circuit's DbContext is shared by everything on the page, and a keyed remount can start a load while
        // another instance's is still awaiting; two queries on one context throw. So each load resolves the service, and
        // with it a DbContext, from its own scope. A single injected instance would serve every load.
        var log = new ScopedServiceLog();
        Services.AddScoped<IActivityReferenceDataService>(_ => new ScopedRecorder(log));

        var cut = RenderForm(
            dataJson: "{}",
            isExistingActivity: true,
            activityInstitutionId: 42,
            editableFieldKeys: Writable("assessor_user_id"));

        var firstLoad = log.Calls.ToList();

        cut.SetParametersAndRender(parameters => parameters
            .Add(component => component.EditableFieldKeys, Writable("assessor_user_id", "notes")));

        var secondLoad = log.Calls.Skip(firstLoad.Count).ToList();

        firstLoad.Should().HaveCount(2, "the EPA field and the writable user field each ask once");
        secondLoad.Should().HaveCount(2, "the writable set changed, so the options reload");
        firstLoad.Select(call => call.Instance).Distinct().Should().ContainSingle("one load, one scope");
        secondLoad.Select(call => call.Instance).Distinct().Should().ContainSingle("one load, one scope");
        secondLoad[0].Instance.Should().NotBe(firstLoad[0].Instance, "each load gets a scope, and an instance, of its own");
    }

    [Fact]
    public void ALoadThatFinishesAfterANewerOne_IsDiscarded()
    {
        // The first load is still waiting on the directory when the stamp changes and a second load finishes. When the
        // first one comes back, its list is for parameters the form no longer has, and must not replace the second's.
        var gated = new GatedNomineeService();
        Services.AddSingleton<IActivityReferenceDataService>(gated);

        var cut = RenderForm(dataJson: "{}", schemaJson: OneUserFieldSchema, isExistingActivity: true, activityInstitutionId: 42);
        gated.Scopes.Should().ContainSingle().Which.ActivityInstitutionId.Should().Be(42);

        cut.SetParametersAndRender(parameters => parameters
            .Add(component => component.ActivityInstitutionId, 43));
        cut.WaitForAssertion(() => AssessorOptionValues(cut).Should().Equal("", "at-43"));

        var rendersBefore = cut.RenderCount;
        gated.FirstLoad.SetResult([new ActivityCatalogueOption("at-42", "Dr Forty-Two")]);

        // The first load's completion renders once more; after it, the form must still show the second load's list.
        cut.WaitForState(() => cut.RenderCount > rendersBefore);
        AssessorOptionValues(cut).Should().Equal(["", "at-43"], "the older load finished last, but its list is stale");
    }

    [Fact]
    public void WhileTheListIsStillLoading_AStoredNomineeIsNotCalledUnavailable()
    {
        // Before the directory answers, "not on the list" means "not known yet". Labelling the stored assessor
        // "Not available" during that window would tell the trainee something false (found by the round-2 test pass).
        var gated = new GatedNomineeService();
        Services.AddSingleton<IActivityReferenceDataService>(gated);

        var cut = RenderForm(
            dataJson: """{"assessor_user_id":"assessor-9"}""",
            schemaJson: OneUserFieldSchema,
            isExistingActivity: true,
            activityInstitutionId: 42);

        gated.Scopes.Should().ContainSingle("the load has started and is waiting on the directory");
        cut.Markup.Should().NotContain("Not available");

        var rendersBefore = cut.RenderCount;
        gated.FirstLoad.SetResult([new ActivityCatalogueOption("assessor-9", "Dr Nine")]);
        cut.WaitForState(() => cut.RenderCount > rendersBefore);

        cut.Markup.Should().NotContain("Not available");
        cut.FindAll("#assessor_user_id option").Single(option => option.HasAttribute("selected"))
            .TextContent.Trim().Should().Be("Dr Nine");
    }

    // ---- helpers ----

    private const string OneUserFieldSchema = """
        {
          "version": 1,
          "sections": [
            { "key": "s", "title": "S", "fields": [ { "key": "assessor_user_id", "type": "user", "label": "Assessor" } ] }
          ]
        }
        """;

    private static IReadOnlyList<string?> AssessorOptionValues(IRenderedComponent<ActivityForm> cut)
        => cut.FindAll("#assessor_user_id option").Select(option => option.GetAttribute("value")).ToList();

    /// <remarks>
    /// <paramref name="storedDataJson" /> defaults to what the pages pass: the activity's data on an existing activity
    /// or a read-only render (ActivityView, ActivityDetail), and nothing on the create page. <c>"{}"</c> says "the
    /// activity stores nothing", so the working copy differs from what is stored.
    /// </remarks>
    private IRenderedComponent<ActivityForm> RenderForm(
        string dataJson,
        string schemaJson = SchemaJson,
        bool isExistingActivity = false,
        int? activityInstitutionId = null,
        IReadOnlySet<string>? editableFieldKeys = null,
        bool readOnly = false,
        Action<string>? onChanged = null,
        string? storedDataJson = null)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, schemaJson)
            .Add(component => component.DataJson, dataJson)
            .Add(component => component.StoredDataJson, storedDataJson ?? (isExistingActivity || readOnly ? dataJson : null))
            .Add(component => component.SubjectUserId, "trainee-1")
            .Add(component => component.IsExistingActivity, isExistingActivity)
            .Add(component => component.ActivityInstitutionId, activityInstitutionId)
            .Add(component => component.EditableFieldKeys, editableFieldKeys)
            .Add(component => component.ReadOnly, readOnly)
            .Add(component => component.DataJsonChanged, EventCallback.Factory.Create<string>(
                this,
                value => onChanged?.Invoke(value))));

    private static IReadOnlySet<string> Writable(params string[] keys)
        => new HashSet<string>(keys, StringComparer.Ordinal);

    /// <summary>Which instance of the scoped service served each call, in order.</summary>
    private sealed class ScopedServiceLog
    {
        private int _instances;

        public List<(int Instance, string Call)> Calls { get; } = [];

        public int NextInstance() => ++_instances;
    }

    /// <summary>Registered as scoped: every scope gets a new instance, which stamps its own number on each call.</summary>
    private sealed class ScopedRecorder : StubActivityReferenceDataService
    {
        private readonly ScopedServiceLog _log;
        private readonly int _instance;

        public ScopedRecorder(ScopedServiceLog log)
        {
            _log = log;
            _instance = log.NextInstance();
        }

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetNomineeOptionsAsync(
            NomineeOptionScope scope, CancellationToken cancellationToken = default)
        {
            _log.Calls.Add((_instance, nameof(GetNomineeOptionsAsync)));
            return base.GetNomineeOptionsAsync(scope, cancellationToken);
        }

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
            ClaimsPrincipal principal, EpaOptionScope? scope = null, CancellationToken cancellationToken = default)
        {
            _log.Calls.Add((_instance, nameof(GetEpaOptionsAsync)));
            return base.GetEpaOptionsAsync(principal, scope, cancellationToken);
        }
    }

    /// <summary>
    /// Holds the first nominee request until the test releases it; every later one answers at once with a single person
    /// named after the institution asked about.
    /// </summary>
    private sealed class GatedNomineeService : StubActivityReferenceDataService
    {
        public TaskCompletionSource<IReadOnlyList<ActivityCatalogueOption>> FirstLoad { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<NomineeOptionScope> Scopes { get; } = [];

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetNomineeOptionsAsync(
            NomineeOptionScope scope, CancellationToken cancellationToken = default)
        {
            Scopes.Add(scope);
            return Scopes.Count == 1
                ? FirstLoad.Task
                : Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(
                    [new ActivityCatalogueOption($"at-{scope.ActivityInstitutionId}", $"At {scope.ActivityInstitutionId}")]);
        }
    }
}

/// <summary>
/// Records every nominee and label request, and answers with whatever the test sets. Shared by the page-level T102 tests.
/// </summary>
internal sealed class RecordingReferenceDataService : StubActivityReferenceDataService
{
    private readonly List<NomineeOptionScope> _nomineeScopes = [];
    private readonly List<string> _userLabelRequests = [];

    public IReadOnlyList<NomineeOptionScope> NomineeScopes => _nomineeScopes;

    public IReadOnlyList<string> UserLabelRequests => _userLabelRequests;

    public IReadOnlyList<ActivityCatalogueOption> Nominees { get; set; } = [];

    public IReadOnlyList<ActivityCatalogueOption> Epas { get; set; } = [];

    public Dictionary<string, ActivityCatalogueOption> Labels { get; } = new(StringComparer.Ordinal);

    public override Task<IReadOnlyList<ActivityCatalogueOption>> GetNomineeOptionsAsync(
        NomineeOptionScope scope, CancellationToken cancellationToken = default)
    {
        _nomineeScopes.Add(scope);
        return Task.FromResult(Nominees);
    }

    public override Task<ActivityCatalogueOption?> GetUserOptionAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        _userLabelRequests.Add(userId);
        return Task.FromResult(Labels.TryGetValue(userId, out var label) ? label : null);
    }

    public override Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
        ClaimsPrincipal principal, EpaOptionScope? scope = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Epas);
}
