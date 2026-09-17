using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T108: what the renderer tells the reference-data service about the field it is drawing.
/// </summary>
/// <remarks>
/// The key is not the contract. <c>epa_field</c> is a free-form string in the credit rules and
/// nothing validates it against the schema, so "the EPA that will be credited" can only be answered
/// by intersecting the schema's <c>epa</c> fields with the pinned version's credit rules. Every
/// counter-shape below is live in the seeded catalogue: a reflective note with an epa field and an
/// empty <c>counts_for</c>, an MSF form that credits a fixed curriculum item, and one activity type
/// whose v1 and v2 differ on whether an epa field is credited at all.
/// </remarks>
public sealed class EpaPickerScopeTests : TestContext
{
    private const string TwoEpaFieldsSchema = """
        {
          "version": 1,
          "sections": [
            {
              "key": "s",
              "title": "S",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA assessed" },
                { "key": "context_epa_id", "type": "epa", "label": "Related EPA" }
              ]
            }
          ]
        }
        """;

    private const string CreditsEpaId = """
        { "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }
        """;

    public EpaPickerScopeTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("assessor@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "assessor-1"));
    }

    [Fact]
    public void OnlyTheFieldTheCreditRulesRead_IsNarrowed_AndItNarrowsToTheSubject()
    {
        var recorder = Recorder();

        RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, TwoEpaFieldsSchema)
            .Add(component => component.DataJson, "{}")
            .Add(component => component.SubjectUserId, "registrar-1")
            .Add(component => component.CreditRulesJson, CreditsEpaId));

        recorder.Scopes.Should().HaveCount(2);

        // The credited field: narrowed, and narrowed to the SUBJECT — the principal here is the
        // assessor, a different person who may be in a different sub-speciality entirely.
        recorder.Scopes[0]!.NarrowToCreditable.Should().BeTrue();
        recorder.Scopes[0]!.SubjectUserId.Should().Be("registrar-1");

        // The contextual field the credit engine never reads: not narrowed.
        recorder.Scopes[1]!.NarrowToCreditable.Should().BeFalse();
    }

    [Fact]
    public void AReflectiveNoteStyleType_IsNeverNarrowed()
    {
        // An epa field with an empty counts_for: the field is required, credits nothing by design,
        // and a strict "hide what cannot be credited" rule would leave it with no options at all.
        var recorder = Recorder();

        RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, TwoEpaFieldsSchema)
            .Add(component => component.DataJson, "{}")
            .Add(component => component.SubjectUserId, "registrar-1")
            .Add(component => component.CreditRulesJson, """{ "counts_for": [] }"""));

        recorder.Scopes.Should().OnlyContain(scope => scope!.NarrowToCreditable == false);
    }

    [Fact]
    public void ATypeThatCreditsAFixedCurriculumItem_IsNeverNarrowed()
    {
        // msf_paed v1 in the seeded catalogue: an epa field for context, credit pinned to
        // curriculum_item_id 14. CreditApplier returns before it ever reads the epa field.
        var recorder = Recorder();

        RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, TwoEpaFieldsSchema)
            .Add(component => component.DataJson, "{}")
            .Add(component => component.SubjectUserId, "registrar-1")
            .Add(component => component.CreditRulesJson, """
                { "counts_for": [ { "curriculum_item_match": { "curriculum_item_id": 14 }, "amount": 1 } ] }
                """));

        recorder.Scopes.Should().OnlyContain(scope => scope!.NarrowToCreditable == false);
    }

    [Fact]
    public void TheBuilderPreview_PassesNoSubject_SoNothingIsNarrowed()
    {
        // ActivityTypeEdit's live preview renders the form with no subject and no credit rules.
        var recorder = Recorder();

        RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, TwoEpaFieldsSchema)
            .Add(component => component.DataJson, "{}"));

        recorder.Scopes.Should().OnlyContain(scope => scope!.NarrowToCreditable == false && scope.SubjectUserId == null);
    }

    [Fact]
    public void TheStoredValue_IsHandedToTheService_SoItCanBeOfferedBack()
    {
        // The service unions the stored value back into the options. It can only do that if the
        // renderer tells it what is stored — otherwise a value outside the narrowed set renders as
        // "Select…" and the page silently loses recorded evidence.
        var recorder = Recorder();

        RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, TwoEpaFieldsSchema)
            .Add(component => component.DataJson, """{"epa_id":"17"}""")
            .Add(component => component.SubjectUserId, "registrar-1")
            .Add(component => component.CreditRulesJson, CreditsEpaId));

        recorder.Scopes[0]!.CurrentValue.Should().Be("17");
        recorder.Scopes[1]!.CurrentValue.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ChangingTheSubject_ReloadsTheOptions()
    {
        // The reload key used to be the schema alone. Leaving it that way would serve one subject's
        // creditable EPAs to the next, which is the same class of bug with a different victim.
        var recorder = Recorder();

        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, TwoEpaFieldsSchema)
            .Add(component => component.DataJson, "{}")
            .Add(component => component.SubjectUserId, "registrar-1")
            .Add(component => component.CreditRulesJson, CreditsEpaId));

        recorder.Scopes.Should().HaveCount(2);

        cut.SetParametersAndRender(parameters => parameters
            .Add(component => component.SubjectUserId, "registrar-2"));

        recorder.Scopes.Should().HaveCount(4);
        recorder.Scopes[2]!.SubjectUserId.Should().Be("registrar-2");
    }

    private RecordingReferenceDataService Recorder()
    {
        var recorder = new RecordingReferenceDataService();
        Services.AddSingleton<IActivityReferenceDataService>(recorder);
        return recorder;
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
            return Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);
        }
    }
}
