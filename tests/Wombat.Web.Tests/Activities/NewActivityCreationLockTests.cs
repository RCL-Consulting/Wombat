using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Activities;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T070 step 8: the creation form offers the creator only the fields they own.
/// </summary>
/// <remarks>
/// This one runs against the REAL <see cref="FieldPermissionEvaluator" /> rather than a canned
/// writable set, because the thing worth testing is that a section's <c>editable_by</c> — declared
/// in the seeded schema, parsed by the Domain DSL — actually reaches the rendered form. Before
/// T070 a trainee could type a rating into the assessor's section at creation; the server now
/// strips it, and this stops the UI inviting input it would silently discard.
/// </remarks>
public sealed class NewActivityCreationLockTests : TestContext
{
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "text", "label": "EPA" },
                { "key": "assessor_user_id", "type": "text", "label": "Assessor" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Overall level" },
                { "key": "strengths", "type": "longtext", "label": "Strengths" }
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
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" }
          ]
        }
        """;

    public NewActivityCreationLockTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
        Services.AddSingleton<IWorkflowEvaluator, WorkflowEvaluator>();
        Services.AddSingleton<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        Services.AddSingleton<IScopedSender>(new FakeSender(SchemaJson, WorkflowJson));
        Services.AddScoped<ActivityNotices>();
    }

    [Fact]
    public void TheCreatorGetsTheRequestSection_AndTheAssessorsSectionIsLocked()
    {
        var cut = SelectTheActivityType();

        cut.Find("#epa_id-in").HasAttribute("disabled").Should().BeFalse();
        cut.Find("#assessor_user_id-in").HasAttribute("disabled").Should().BeFalse();

        // T342 (flow 03, C14): the assessor's section is a locked section, with no inputs for the trainee to pre-fill
        // the assessor's rating in, and a head row saying who fills it in.
        var locked = cut.Find("section.form-section--locked");
        locked.QuerySelector("h2")!.TextContent.Trim().Should().Be("Entrustment");
        locked.QuerySelector(".form-section-owner")!.TextContent.Trim().Should().Be("The assessor you name fills this in");
        locked.QuerySelectorAll("input, select, textarea").Should().BeEmpty();
        cut.FindAll("#overall_level-in, #strengths-in").Should().BeEmpty();
    }

    [Fact]
    public void ATypedRequest_ReachesTheForm_AndTheLockedSectionOffersNothingToType()
    {
        var cut = SelectTheActivityType();

        cut.Find("#epa_id-in").Input("3");

        // ActivityForm's own C# guard still rejects a locked field; there is no control on the page to send one from.
        cut.Find("#epa_id-in").GetAttribute("value").Should().Be("3");
        cut.FindAll("section.form-section--locked input").Should().BeEmpty();
    }


    // ---- T111: /activities/new?type=<key> preselects that type ----

    [Fact]
    public void AQueryStringTypeKey_PreselectsTheTypeAndRendersItsForm()
    {
        // The defect: the page declared no [SupplyParameterFromQuery], so ?type= was discarded and the
        // trainee landed on a form that looked like it should already know what they clicked.
        NavigateWithType("mini_cex_cpsa");

        var cut = RenderComponent<NewActivity>();

        cut.WaitForState(() => cut.FindAll("#epa_id-in").Count == 1);

        // T342 (flow 03): the form is headed by its type, with a way back to the picker.
        var subtitle = cut.Find(".header-container .page-subtitle");
        subtitle.TextContent.Should().Contain("Mini-CEX (CPSA)");
        subtitle.QuerySelector("a")!.GetAttribute("href").Should().Be("/activities/new");
        subtitle.QuerySelector("a")!.TextContent.Trim().Should().Be("Choose another type");
    }

    [Fact]
    public void TheKeyMatchIsCaseInsensitive()
    {
        NavigateWithType("  Mini_CEX_CPSA  ");

        var cut = RenderComponent<NewActivity>();

        cut.WaitForState(() => cut.FindAll("#epa_id-in").Count == 1);
    }

    [Fact]
    public void AKeyThatResolvesToNothing_LeavesThePickerUnsetAndDoesNotThrow()
    {
        // Not published, out of scope, on the wrong ladder (T123 d3), or simply renamed. The link is a
        // convenience; it must never become an authorization statement, and a dead one is not an error.
        NavigateWithType("a_type_this_caller_cannot_see");

        var cut = RenderComponent<NewActivity>();

        // The picker, as with no key (T342, Q1).
        cut.WaitForState(() => cut.FindAll(".instrument-link").Count > 0);
        cut.FindAll("#epa_id-in").Should().BeEmpty();
    }

    [Fact]
    public void NoTypeKey_ShowsThePicker_WhoseLinksAreTheTypesForms()
    {
        var cut = RenderComponent<NewActivity>();

        cut.WaitForState(() => cut.FindAll(".instrument-link").Count > 0);
        cut.Find(".instrument-link").GetAttribute("href").Should().Be("/activities/new?type=mini_cex_cpsa");
        cut.FindAll("#epa_id-in").Should().BeEmpty();
    }

    private void NavigateWithType(string typeKey)
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("type", typeKey));
    }

    private IRenderedComponent<NewActivity> SelectTheActivityType()
    {
        return NewActivityPage.Open(this, "mini_cex_cpsa", "#epa_id-in");
    }

    private sealed class FakeSender : IScopedSender
    {
        private readonly string _schemaJson;
        private readonly string _workflowJson;

        public FakeSender(string schemaJson, string workflowJson)
        {
            _schemaJson = schemaJson;
            _workflowJson = workflowJson;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListActivityTypesQuery:
                    IReadOnlyList<ActivityTypeListItemDto> types =
                        [new ActivityTypeListItemDto(2, "mini_cex_cpsa", "Mini-CEX (CPSA)", ActivityScope.Speciality, 3, 1, true)];
                    return Task.FromResult((TResponse)(object)types);

                case GetActivityTypeEditorQuery:
                    var editor = new ActivityTypeEditorDto(
                        2,
                        "mini_cex_cpsa",
                        "Mini-CEX (CPSA)",
                        null,
                        ActivityScope.Speciality,
                        3,
                        true,
                        "mini_cex",
                        1,
                        false,
                        _schemaJson,
                        _workflowJson,
                        "{}",
                        "[]",
                        _schemaJson,
                        _workflowJson,
                        "{}",
                        "[]",
                        "admin-1",
                        null,
                        null,
                        [],
                        false,
                        [],
                        null);
                    return Task.FromResult((TResponse)(object)editor);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
