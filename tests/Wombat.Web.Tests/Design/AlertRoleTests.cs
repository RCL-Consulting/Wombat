using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T193: a refusal is announced. <c>Alert</c> rendered every kind with no role, so a danger alert that appeared after
/// Submit was new text a screen reader might never read. A danger alert is now an <c>alert</c>, and a warning and a
/// success a <c>status</c>, unless the caller names a role: about fifty danger alerts and thirty success notices across
/// the pages rely on the default.
/// </summary>
public sealed class AlertRoleTests : TestContext
{
    [Theory]
    [InlineData("danger", "alert")]
    [InlineData("warning", "status")]
    [InlineData("success", "status")]
    public void AnAlertOfAnnouncedKind_HasItsRole_WhenTheCallerNamesNone(string kind, string role)
    {
        var cut = RenderAlert(kind, role: null);

        cut.Find($".alert.alert-{kind}").GetAttribute("role").Should().Be(role);
    }

    [Fact]
    public void AnInfoAlert_HasNoRole_WhenTheCallerNamesNone()
    {
        // Page content present from the first render: a role would have it announced for no reason.
        var cut = RenderAlert("info", role: null);

        cut.Find(".alert.alert-info").HasAttribute("role").Should().BeFalse();
    }

    [Theory]
    [InlineData("danger", "status")]
    [InlineData("warning", "alert")]
    [InlineData("success", "alert")]
    [InlineData("info", "status")]
    public void TheCallersRole_OverridesTheDefault(string kind, string role)
    {
        // A refused link on the MSF page, and a refused submit on the activity page, are warnings that must interrupt.
        var cut = RenderAlert(kind, role);

        cut.Find($".alert.alert-{kind}").GetAttribute("role").Should().Be(role);
    }

    [Theory]
    [InlineData("danger")]
    [InlineData("warning")]
    [InlineData("success")]
    public void AnEmptyRole_RendersNoRole_EvenOnAnAnnouncedKind(string kind)
    {
        // A warning that is standing page content, there on every visit, takes Role="" so it is not read out each time.
        var cut = RenderAlert(kind, string.Empty);

        cut.Find($".alert.alert-{kind}").HasAttribute("role").Should().BeFalse();
    }

    [Fact]
    public void AnAlertGivenAnId_RendersIt_SoAFieldCanNameIt()
    {
        var cut = RenderComponent<Alert>(parameters => parameters
            .Add(component => component.Kind, "danger")
            .Add(component => component.Id, "login-error")
            .Add(component => component.ChildContent, (RenderFragment)(builder => builder.AddContent(0, "Refused."))));

        cut.Find(".alert.alert-danger").Id.Should().Be("login-error");
    }

    [Fact]
    public void AnAlertWithNoId_RendersNone()
    {
        var cut = RenderAlert("danger", role: null);

        cut.Find(".alert.alert-danger").HasAttribute("id").Should().BeFalse();
    }

    [Fact]
    public void AStatePanelsLoadError_IsAnnounced()
    {
        // The shared error state names no role, so it takes the danger default.
        var cut = RenderComponent<StatePanel>(parameters => parameters
            .Add(component => component.LoadError, "The list could not be loaded.")
            .Add(component => component.ChildContent, (RenderFragment)(_ => { })));

        var alert = cut.Find(".alert.alert-danger");
        alert.GetAttribute("role").Should().Be("alert");
        alert.TextContent.Should().Contain("The list could not be loaded.");
    }

    private IRenderedComponent<Alert> RenderAlert(string kind, string? role)
        => RenderComponent<Alert>(parameters => parameters
            .Add(component => component.Kind, kind)
            .Add(component => component.Role, role)
            .Add(component => component.ChildContent, (RenderFragment)(builder => builder.AddContent(0, "Message"))));
}
