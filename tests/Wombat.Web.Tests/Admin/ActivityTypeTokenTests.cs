using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, T291 item 3; R3-C03): the first Save draft on /new moves to the type's address with the created token;
/// the arriving page writes "Draft saved.", focuses it, keeps Details and takes the token out. A stale or foreign id
/// writes nothing.
/// </summary>
public sealed class ActivityTypeTokenTests : TestContext
{
    public ActivityTypeTokenTests() => BuilderKit.SignIn(this, WombatRoles.InstitutionalAdmin);

    [Fact]
    public void TheFirstSave_MovesToTheTypesAddress_WithTheCreatedToken()
    {
        var sender = new BuilderSender(BuilderKit.New(BuilderKit.InstitutionScopes));
        var cut = BuilderKit.Render(this, sender, id: null);
        cut.Find("#type-key").Change("kgk_teaching_log");
        cut.Find("#type-name").Change("KGK Teaching Session Log");

        BuilderKit.Button(cut, "Save draft").Click();

        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith(CatalogueLinks.ActivityTypeCreated(42));
    }

    [Fact]
    public void ArrivingWithTheToken_SaysDraftSaved_FocusesIt_OnDetails_AndTakesTheTokenOut()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()), arriveAt: CatalogueLinks.ActivityTypeCreated(BuilderKit.KgkId));
        var navigation = Services.GetRequiredService<NavigationManager>();

        cut.Find(".action-result").TextContent.Trim().Should().Be("Draft saved.");
        BuilderKit.TabNamed(cut, "Details").GetAttribute("aria-selected").Should().Be("true", "a save keeps the tab (T2)");
        cut.WaitForAssertion(() => navigation.Uri.Should().EndWith(CatalogueLinks.ActivityType(BuilderKit.KgkId)));
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public void AStaleToken_WritesNothing()
    {
        var cut = BuilderKit.Render(this, new BuilderSender((ActivityTypeEditorDto?)null), id: 99, arriveAt: CatalogueLinks.ActivityTypeCreated(99));

        cut.Find("h1").TextContent.Should().Be("Activity type unavailable");
        cut.FindAll(".alert-success").Should().BeEmpty();
    }
}
