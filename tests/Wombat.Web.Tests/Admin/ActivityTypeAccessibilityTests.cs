using Bunit;
using FluentAssertions;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, R2-0-spec-3 § Accessibility): on every tab, a writer's and a reader's, with an editor open and a
/// refusal shown, every id a label or an aria attribute names exists once, and every control is named.
/// </summary>
public sealed class ActivityTypeAccessibilityTests : TestContext
{
    public ActivityTypeAccessibilityTests() => BuilderKit.SignIn(this, WombatRoles.InstitutionalAdmin);

    [Theory]
    [InlineData(true, "Details")]
    [InlineData(true, "Form")]
    [InlineData(true, "Workflow")]
    [InlineData(true, "Credit")]
    [InlineData(true, "Publish")]
    [InlineData(false, "Details")]
    [InlineData(false, "Form")]
    [InlineData(false, "Workflow")]
    public void EveryTab_HasEveryIdItNames_AndEveryControlNamed(bool writer, string tab)
    {
        var sender = new BuilderSender(BuilderKit.Kgk(publishedVersion: 1, canWrite: writer))
        {
            SaveRefusal = new ActivityTypeDraftRefusedException(
            [
                new RefusalPlace(RefusalTab.Form, RefusalKind.EmptySection, "Section 'session' must contain at least one field.") { SectionKey = "session" },
                new RefusalPlace(RefusalTab.Workflow, RefusalKind.MalformedJson, "bad") { JsonLine = 3 }
            ])
        };
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, tab);
        if (tab == "Form")
        {
            BuilderKit.Named(cut, writer ? "Edit field Audience" : "View field Audience").Click();
        }

        if (writer)
        {
            BuilderKit.Button(cut, "Save draft").Click();
        }

        IdReferences.Broken(cut).Should().BeEmpty();
        AccessibleNames.Unnamed(cut).Should().BeEmpty();
    }
}
