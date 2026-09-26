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

        cut.Find("#epa_id").HasAttribute("disabled").Should().BeFalse();
        cut.Find("#assessor_user_id").HasAttribute("disabled").Should().BeFalse();

        cut.Find("#overall_level").HasAttribute("disabled").Should()
            .BeTrue("the trainee must not be able to pre-fill the assessor's rating");
        cut.Find("#strengths").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void ATypedRatingInTheLockedSection_NeverReachesTheForm()
    {
        var cut = SelectTheActivityType();

        cut.Find("#overall_level").Input("5");
        cut.Find("#epa_id").Input("3");

        // ActivityForm's own C# guard rejects the locked field, so the data the page would post
        // carries the request key only.
        cut.Find("#overall_level").GetAttribute("value").Should().BeEmpty();
        cut.Find("#epa_id").GetAttribute("value").Should().Be("3");
    }


    // ---- T111: /activities/new?type=<key> preselects that type ----

    [Fact]
    public void AQueryStringTypeKey_PreselectsTheTypeAndRendersItsForm()
    {
        // The defect: the page declared no [SupplyParameterFromQuery], so ?type= was discarded and the
        // trainee landed on a form that looked like it should already know what they clicked.
        NavigateWithType("mini_cex_cpsa");

        var cut = RenderComponent<NewActivity>();

        cut.WaitForState(() => cut.FindAll("#epa_id").Count == 1);
        cut.Find("#activity-type").GetAttribute("value").Should().Be("2");
    }

    [Fact]
    public void TheKeyMatchIsCaseInsensitive()
    {
        NavigateWithType("  Mini_CEX_CPSA  ");

        var cut = RenderComponent<NewActivity>();

        cut.WaitForState(() => cut.FindAll("#epa_id").Count == 1);
    }

    [Fact]
    public void AKeyThatResolvesToNothing_LeavesThePickerUnsetAndDoesNotThrow()
    {
        // Not published, out of scope, on the wrong ladder (T123 d3), or simply renamed. The link is a
        // convenience; it must never become an authorization statement, and a dead one is not an error.
        NavigateWithType("a_type_this_caller_cannot_see");

        var cut = RenderComponent<NewActivity>();

        cut.Find("#activity-type").GetAttribute("value").Should().Be("0");
        cut.FindAll("#epa_id").Should().BeEmpty();
    }

    [Fact]
    public void NoTypeKey_BehavesExactlyAsBefore()
    {
        var cut = RenderComponent<NewActivity>();

        cut.Find("#activity-type").GetAttribute("value").Should().Be("0");
        cut.FindAll("#epa_id").Should().BeEmpty();
    }

    private void NavigateWithType(string typeKey)
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("type", typeKey));
    }

    private IRenderedComponent<NewActivity> SelectTheActivityType()
    {
        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll("#activity-type option").Count > 1);

        cut.Find("#activity-type").Change("2");
        cut.WaitForState(() => cut.FindAll("#epa_id").Count == 1);

        return cut;
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
