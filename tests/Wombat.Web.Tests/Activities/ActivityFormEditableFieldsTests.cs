using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T070 step 5: per-field locking in <see cref="ActivityForm" />. The <c>disabled</c> attribute is
/// a browser hint; the assertions that matter here are the ones driving an event at a locked field
/// and watching <c>DataJsonChanged</c> stay silent, because that is the C# guard doing the work.
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
    public void EditableFieldKeys_EnablesExactlyThoseFields()
    {
        var cut = Render(Writable("overall_level", "strengths"));

        cut.Find("#overall_level").HasAttribute("disabled").Should().BeFalse();
        cut.Find("#strengths").HasAttribute("disabled").Should().BeFalse();
        cut.Find("#epa_id").HasAttribute("disabled").Should().BeTrue();
        cut.Find("#settings-ward").HasAttribute("disabled").Should().BeTrue();

        cut.FindAll("input:not([disabled]), textarea:not([disabled]), select:not([disabled])")
            .Should().HaveCount(2);
    }

    [Fact]
    public void LockedField_RejectsAForgedInput_WhileAnEditableFieldStillWorks()
    {
        string? captured = null;
        var cut = Render(Writable("overall_level"), value => captured = value);

        // The event is dispatched at a disabled element exactly as a tampered-with client would.
        cut.Find("#epa_id").Input("99");
        captured.Should().BeNull("a locked field must not reach DataJsonChanged");

        cut.Find("#overall_level").Input("5");
        captured.Should().NotBeNull();
        captured.Should().Contain("\"overall_level\":\"5\"").And.Contain("\"epa_id\":\"3\"");
    }

    [Fact]
    public void LockedMultiChoice_RejectsAForgedChange()
    {
        string? captured = null;
        var cut = Render(Writable("overall_level"), value => captured = value);

        cut.Find("#settings-ward").Change(true);

        captured.Should().BeNull("ToggleMultiChoice carries its own guard");
    }

    [Fact]
    public void EmptyEditableFieldKeys_LockEveryField()
    {
        // The non-bound user on the detail page: nothing writable, nothing editable.
        var cut = Render(Writable());

        cut.FindAll("input, textarea, select")
            .Should().OnlyContain(element => element.HasAttribute("disabled"));

        string? captured = null;
        var editable = Render(Writable(), value => captured = value);
        editable.Find("#overall_level").Input("5");
        captured.Should().BeNull();
    }

    [Fact]
    public void ReadOnly_OverridesEditableFieldKeys()
    {
        string? captured = null;
        var cut = Render(Writable("overall_level"), value => captured = value, readOnly: true);

        cut.Find("#overall_level").HasAttribute("disabled").Should().BeTrue();
        cut.Find("#overall_level").Input("5");
        captured.Should().BeNull();
    }

    [Fact]
    public void NullEditableFieldKeys_KeepsThePreT070Semantics()
    {
        // ActivityDetail, ActivityTypeEdit's live preview and NewActivity all pass no writable set.
        // Without one the form must behave exactly as it did before T070: ReadOnly alone decides.
        string? captured = null;
        var editable = Render(writableFieldKeys: null, onChanged: value => captured = value);

        editable.FindAll("input, textarea, select")
            .Should().OnlyContain(element => !element.HasAttribute("disabled"));

        editable.Find("#epa_id").Input("9");
        captured.Should().NotBeNull();

        var locked = Render(writableFieldKeys: null, readOnly: true);
        locked.FindAll("input, textarea, select")
            .Should().OnlyContain(element => element.HasAttribute("disabled"));
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
