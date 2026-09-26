using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Activities;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T122: the two pages that render an EPA picker hand it the instrument the activity type is.
/// </summary>
/// <remarks>
/// <para>
/// <c>ActivityForm</c> forwarding the key is pinned in <see cref="EpaPickerScopeTests" />. These pin the hop
/// before it, which no compiler checks: <c>WbaToolKey</c> is an optional parameter, so a page that forgets to
/// bind it compiles, renders, and hands the service null — which D21 reads as "unrestricted". The picker then
/// offers every EPA on the curriculum while the write path refuses the ones the tool lists exclude, and the
/// trainee finds out at Submit, after filling in the form.
/// </para>
/// <para>
/// The key threaded is the type ROW's current key, carried on the DTO, because that is the key the write path
/// checks. It is not versioned with the schema.
/// </para>
/// </remarks>
public sealed class WbaToolKeyThreadingTests : TestContext
{
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA assessed" },
                { "key": "notes", "type": "text", "label": "Notes" }
              ]
            }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "subject" }
          ]
        }
        """;

    private const string CreditsEpaId = """
        { "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }
        """;

    private readonly RecordingReferenceDataService _recorder = new();

    public WbaToolKeyThreadingTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IActivityReferenceDataService>(_recorder);
        Services.AddSingleton<IWorkflowEvaluator, WorkflowEvaluator>();
        Services.AddSingleton<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        Services.AddScoped<ActivityNotices>();
    }

    // ---- /activities/new ----

    [Fact]
    public void NewActivity_HandsTheSelectedTypesToolKeyToThePicker()
    {
        Services.AddSingleton<IScopedSender>(new NewActivitySender(
            Type(2, "mini_cex_cpsa", "Mini-CEX (CPSA)", "mini_cex")));

        var cut = RenderComponent<NewActivity>();
        Select(cut, 2);

        var credited = CreditedScopes().Should().ContainSingle().Subject;
        credited.WbaToolKey.Should().Be("mini_cex");

        // The creator IS the subject on this page (T108), so the tool narrows the trainee's own curriculum.
        credited.SubjectUserId.Should().Be("trainee-1");
    }

    [Fact]
    public void NewActivity_ATypeThatIsNoInstrument_HandsTheServiceNull()
    {
        // The generic seeds other than mini_cex/dops/cbd, and every non-WBA type, carry a null key. Null
        // must arrive as null, not as an empty string the service would look up in the vocabulary.
        Services.AddSingleton<IScopedSender>(new NewActivitySender(
            Type(5, "acat", "ACAT", null)));

        var cut = RenderComponent<NewActivity>();
        Select(cut, 5);

        CreditedScopes().Should().ContainSingle()
            .Which.WbaToolKey.Should().BeNull();
    }

    [Fact]
    public void NewActivity_SwitchingBetweenTypesThatDifferOnlyInTheirTool_ReQueriesWithTheNewKey()
    {
        // The case the page actually produces: the same ActivityForm instance stays mounted while the type
        // picker changes, and these two types share their schema, their credit rules and their subject. The
        // tool is the ONLY thing that differs, so a stale key here would serve the Mini-CEX allow-list to a
        // DOPS the trainee is about to file.
        Services.AddSingleton<IScopedSender>(new NewActivitySender(
            Type(2, "mini_cex_cpsa", "Mini-CEX (CPSA)", "mini_cex"),
            Type(3, "dops_cpsa", "DOPS (CPSA)", "dops")));

        var cut = RenderComponent<NewActivity>();
        Select(cut, 2);
        Select(cut, 3);

        CreditedScopes().Select(scope => scope.WbaToolKey).Should().Equal("mini_cex", "dops");
    }

    /// <summary>
    /// T300 review: the page reads the selected type's definition, and not what the builder would let the caller change
    /// (<c>CanWrite</c>, <c>WritableScopes</c>, <c>ScopeTargetName</c>): for an Administrator the writable scopes read every
    /// institution, speciality and sub-speciality, on every selection.
    /// </summary>
    [Fact]
    public void NewActivity_ReadsTheSelectedTypesDefinition_NotWhatTheBuilderWouldOfferTheCaller()
    {
        var sender = new NewActivitySender(Type(2, "mini_cex_cpsa", "Mini-CEX (CPSA)", "mini_cex"));
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<NewActivity>();
        Select(cut, 2);

        sender.EditorQueries.Should().ContainSingle().Which.ForBuilder.Should().BeFalse();
    }

    // ---- /activities/{id} ----

    [Fact]
    public void ActivityView_TheReadOnlyRender_HandsTheActivitysToolKeyToThePicker()
    {
        // A completed activity renders ActivityDetail. The stored EPA is always offered back whatever the
        // narrowing, so the key cannot hide recorded evidence — but a detail page that narrowed by a
        // different key from the editable render would be the first place the two drifted apart.
        var cut = RenderView(Detail("completed", toolKey: "mini_cex", editable: [], actions: []));

        cut.FindAll("#discard-changes").Should().BeEmpty("this is the read-only branch");
        cut.Find("#epa_id").HasAttribute("disabled").Should().BeTrue();

        var credited = CreditedScopes().Should().ContainSingle().Subject;
        credited.WbaToolKey.Should().Be("mini_cex");
        credited.SubjectUserId.Should().Be("trainee-1");
        credited.CurrentValue.Should().Be("17");
    }

    [Fact]
    public void ActivityView_TheEditableRender_HandsTheActivitysToolKeyToThePicker()
    {
        // The branch where the EPA can still be changed and a transition will re-check it at submit. The
        // viewer here is the subject; the key comes from the activity DTO, not from anything about the viewer.
        var cut = RenderView(Detail(
            "draft",
            toolKey: "dops",
            editable: ["epa_id", "notes"],
            actions: [new ActivityActionDto("submit", false)]));

        cut.FindAll("#discard-changes").Should().ContainSingle("this is the editable branch");
        cut.Find("#epa_id").HasAttribute("disabled").Should().BeFalse();

        var credited = CreditedScopes().Should().ContainSingle().Subject;
        credited.WbaToolKey.Should().Be("dops");
        credited.SubjectUserId.Should().Be("trainee-1");
    }

    [Fact]
    public void ActivityView_AnActivityOfATypeThatIsNoInstrument_HandsTheServiceNull()
    {
        var cut = RenderView(Detail(
            "draft",
            toolKey: null,
            editable: ["epa_id"],
            actions: [new ActivityActionDto("submit", false)]));

        cut.FindAll("#discard-changes").Should().ContainSingle();
        CreditedScopes().Should().ContainSingle()
            .Which.WbaToolKey.Should().BeNull();
    }

    private IEnumerable<EpaOptionScope> CreditedScopes()
        => _recorder.Scopes.OfType<EpaOptionScope>().Where(scope => scope.NarrowToCreditable);

    private static void Select(IRenderedComponent<NewActivity> cut, int activityTypeId)
    {
        cut.WaitForState(() => cut.FindAll("#activity-type option").Count > 1);
        cut.Find("#activity-type").Change(activityTypeId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        cut.WaitForState(() => cut.FindAll("#epa_id").Count == 1);
    }

    private IRenderedComponent<ActivityView> RenderView(ActivityDetailDto detail)
    {
        Services.AddSingleton<IScopedSender>(new ActivityViewSender(detail));

        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 21));
        cut.WaitForState(() => cut.FindAll("#epa_id").Count == 1);

        return cut;
    }

    private static ActivityTypeEditorDto Type(int id, string key, string name, string? toolKey)
        => new(
            id,
            key,
            name,
            null,
            ActivityScope.Speciality,
            3,
            true,
            toolKey,
            1,
            false,
            SchemaJson,
            WorkflowJson,
            CreditsEpaId,
            "[]",
            SchemaJson,
            WorkflowJson,
            CreditsEpaId,
            "[]",
            "admin-1",
            null,
            null,
            [],
            false,
            [],
            null);

    private static ActivityDetailDto Detail(
        string state,
        string? toolKey,
        IReadOnlyList<string> editable,
        IReadOnlyList<ActivityActionDto> actions)
    {
        var activity = new ActivityDto(
            21,
            2,
            "mini_cex_cpsa",
            "Mini-CEX (CPSA)",
            toolKey,
            1,
            SchemaJson,
            WorkflowJson,
            "[]",
            CreditsEpaId,
            "trainee-1",
            1,
            "trainee-1",
            state,
            PinnedWorkflows.StateLabel(PinnedWorkflows.TryParse(WorkflowJson), state),
            """{"epa_id":"17"}""",
            null,
            null,
            new DateOnly(2026, 9, 23),
            true,
            new DateTime(2026, 9, 23, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 23, 8, 0, 0, DateTimeKind.Utc),
            []);

        return new ActivityDetailDto(activity, editable, actions);
    }

    private sealed class RecordingReferenceDataService : StubActivityReferenceDataService
    {
        private readonly List<EpaOptionScope?> _scopes = [];

        public IReadOnlyList<EpaOptionScope?> Scopes => _scopes;

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
            ClaimsPrincipal principal,
            EpaOptionScope? scope = null,
            CancellationToken cancellationToken = default)
        {
            _scopes.Add(scope);
            return Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([new ActivityCatalogueOption("17", "PAED-001")]);
        }
    }

    private sealed class NewActivitySender : IScopedSender
    {
        private readonly IReadOnlyList<ActivityTypeEditorDto> _types;

        public NewActivitySender(params ActivityTypeEditorDto[] types) => _types = types;

        public List<GetActivityTypeEditorQuery> EditorQueries { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListActivityTypesQuery:
                    IReadOnlyList<ActivityTypeListItemDto> items = _types
                        .Select(type => new ActivityTypeListItemDto(type.Id, type.Key, type.Name, type.Scope, type.ScopeId, type.PublishedVersion, type.IsActive))
                        .ToList();
                    return Task.FromResult((TResponse)(object)items);

                case GetActivityTypeEditorQuery query:
                    EditorQueries.Add(query);
                    return Task.FromResult((TResponse)(object)_types.Single(type => type.Id == query.ActivityTypeId));

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class ActivityViewSender : IScopedSender
    {
        private readonly ActivityDetailDto _detail;

        public ActivityViewSender(ActivityDetailDto detail) => _detail = detail;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetActivityByIdQuery
                ? Task.FromResult((TResponse)(object)_detail)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
