using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T070 step 5: per-field locking in <see cref="ActivityForm" />. Since T342 (flow 03) a locked field has no control at
/// all: among writable fields it is read out as text, a section with nothing writable is read out whole when it holds
/// anything, and a locked section when it holds nothing. The C# guards in <c>UpdateValue</c> and
/// <c>ToggleMultiChoice</c> stay, with no element left to forge an event at.
/// </summary>
public sealed class ActivityFormEditableFieldsTests : TestContext
{
    // A request field the trainee owns, two assessor fields, and a multi-choice (a separate guard).
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "text", "label": "EPA" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Overall level" },
                { "key": "strengths", "type": "longtext", "label": "Strengths" },
                { "key": "settings", "type": "multichoice", "label": "Settings", "options": ["ward", "clinic"] }
              ]
            }
          ]
        }
        """;

    private const string DataJson = """{ "epa_id": "3", "overall_level": "2" }""";

    public ActivityFormEditableFieldsTests()
    {
        this.AddTestAuthorization().SetAuthorized("assessor@test");
        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    [Fact]
    public void EditableFieldKeys_GivesControlsToExactlyThoseFields()
    {
        var cut = Render(Writable("overall_level", "strengths"));

        cut.Find("#overall_level-in").LocalName.Should().Be("input");
        cut.Find("#strengths-in").LocalName.Should().Be("textarea");

        // T342 (flow 03): a field the actor may not write has no control, not a disabled one. The trainee's request,
        // which the assessor may not change, is read out; so is the multi-choice beside the assessor's own fields.
        cut.Find("#epa_id-in").LocalName.Should().Be("div", "the Request section holds a value, so it is read out");
        cut.Find("#epa_id-in dd").TextContent.Trim().Should().Be("3");
        cut.Find("#settings-in").LocalName.Should().Be("dl", "a locked field among writable ones is text (T302)");
        cut.FindAll("#settings-ward").Should().BeEmpty();

        cut.FindAll("input, textarea, select").Should().HaveCount(2);
        cut.FindAll("[disabled]").Should().BeEmpty("nothing is shown disabled: what cannot be written is read out");
    }

    [Fact]
    public void ALockedField_OffersNothingToForge_AndAnEditToAWritableOneKeepsItsValue()
    {
        // The C# guard in UpdateValue stays (a forged event at a key the actor may not write changes nothing), but since
        // T342 there is no element to dispatch one at: a locked field renders no control.
        string? captured = null;
        var cut = Render(Writable("overall_level"), value => captured = value);

        cut.FindAll("#epa_id-in input, #epa_id-in select, #epa_id-in textarea").Should().BeEmpty();

        cut.Find("#overall_level-in").Input("5");
        captured.Should().NotBeNull();
        captured.Should().Contain("\"overall_level\":\"5\"").And.Contain("\"epa_id\":\"3\"");
    }

    [Fact]
    public void ALockedMultiChoice_RendersNoCheckboxes()
    {
        var cut = Render(Writable("overall_level"));

        cut.FindAll("input[type=checkbox]").Should().BeEmpty("ToggleMultiChoice's guard has nothing to guard on the page");
        cut.Find("#settings-in dd").TextContent.Trim().Should().Be("Not filled in.");
    }

    [Fact]
    public void EmptyEditableFieldKeys_ReadsEverythingOut()
    {
        // The non-bound user on the detail page: nothing writable, so no controls. Each section that holds a value is
        // read out, every field under its label.
        var cut = Render(Writable());

        cut.FindAll("input, textarea, select").Should().BeEmpty();
        cut.Find("#epa_id-in dd").TextContent.Trim().Should().Be("3");
        cut.Find("#overall_level-in dd").TextContent.Trim().Should().Be("2");
        cut.Find("#strengths-in dd").TextContent.Trim().Should().Be("Not filled in.");
        cut.FindAll(".form-section--locked").Should().BeEmpty("both sections hold something");
    }

    [Fact]
    public void ReadOnly_OverridesEditableFieldKeys()
    {
        var cut = Render(Writable("overall_level"), readOnly: true);

        cut.FindAll("input, textarea, select").Should().BeEmpty();
        cut.Find("#overall_level-in dd").TextContent.Trim().Should().Be("2");
    }

    [Fact]
    public void NullEditableFieldKeys_KeepsThePreT070Semantics()
    {
        // ActivityDetail, ActivityTypeEdit's live preview and NewActivity all pass no writable set.
        // Without one the form must behave exactly as it did before T070: ReadOnly alone decides.
        string? captured = null;
        var editable = Render(writableFieldKeys: null, onChanged: value => captured = value);

        editable.FindAll("input, textarea, select").Should().HaveCount(5, "every field has its control");
        editable.FindAll("[disabled]").Should().BeEmpty();

        editable.Find("#epa_id-in").Input("9");
        captured.Should().NotBeNull();

        var locked = Render(writableFieldKeys: null, readOnly: true);
        locked.FindAll("input, textarea, select").Should().BeEmpty();
    }

    private IRenderedComponent<ActivityForm> Render(
        IReadOnlySet<string>? writableFieldKeys,
        Action<string>? onChanged = null,
        bool readOnly = false)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, SchemaJson)
            .Add(component => component.DataJson, DataJson)
            .Add(component => component.ReadOnly, readOnly)
            .Add(component => component.EditableFieldKeys, writableFieldKeys)
            .Add(component => component.DataJsonChanged, EventCallback.Factory.Create<string>(
                this,
                value => onChanged?.Invoke(value))));

    private static IReadOnlySet<string> Writable(params string[] keys)
        => new HashSet<string>(keys, StringComparer.Ordinal);
}
