using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// ConfirmDialog's form mode (T339, flow 02, C7): the dialog holds a real form the browser posts, so an endpoint can issue
/// the sign-in cookie again, which nothing in a circuit can. The action mode, every earlier caller's, is unchanged.
/// </summary>
public sealed class ConfirmDialogFormModeTests : TestContext
{
    public ConfirmDialogFormModeTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TheFormMode_IsAPostedForm_WithItsToken_ItsHiddenFields_TheCallersContent_AndASubmit()
    {
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.Title, "Remove your sign-in?")
            .Add(dialog => dialog.Body, "What remains.")
            .Add(dialog => dialog.ConfirmLabel, "Remove sign-in")
            .Add(dialog => dialog.DangerAction, true)
            .Add(dialog => dialog.FormAction, "/account/external-logins/remove")
            .Add(dialog => dialog.HiddenFields, new Dictionary<string, string> { ["Provider"] = "kgk" })
            .Add(dialog => dialog.FormContent, (RenderFragment)(builder => builder.AddMarkupContent(0, "<input name=\"Password\" type=\"password\">"))));

        var form = cut.Find("dialog > form.dialog-form");
        form.GetAttribute("method").Should().Be("post");
        form.GetAttribute("action").Should().Be("/account/external-logins/remove");
        form.HasAttribute("data-submit-once").Should().BeTrue();
        cut.FindComponents<AntiforgeryToken>().Should().ContainSingle("the post carries its token");
        form.QuerySelector("input[type=hidden][name=Provider]")!.GetAttribute("value").Should().Be("kgk");
        form.QuerySelector("input[name=Password]").Should().NotBeNull();

        var submit = form.QuerySelector("button[type=submit]")!;
        submit.TextContent.Trim().Should().Be("Remove sign-in");
        submit.ClassList.Should().Contain("btn-danger");
        submit.HasAttribute("blazor:onclick").Should().BeFalse("the browser posts it; the circuit does not take it");

        var cancel = form.QuerySelector(".dialog-actions button[type=button]")!;
        cancel.TextContent.Trim().Should().Be("Cancel");
        cancel.HasAttribute("autofocus").Should().BeTrue("with no field of the caller's autofocused, Cancel takes the focus on open");
        form.QuerySelectorAll(".dialog-actions button").Select(button => button.TextContent.Trim())
            .Should().Equal("Cancel", "Remove sign-in");

        var dialog = cut.Find("dialog");
        cut.Find($"#{dialog.GetAttribute("aria-labelledby")}").TextContent.Trim().Should().Be("Remove your sign-in?");
        cut.Find($"#{dialog.GetAttribute("aria-describedby")}").TextContent.Trim().Should().Be("What remains.");
        dialog.ClassList.Should().Contain("dialog-card--form");
    }

    [Fact]
    public async Task Cancel_ClosesTheDialog_AndTellsTheCaller_WithoutPosting()
    {
        var cancelled = 0;
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.FormAction, "/somewhere")
            .Add(dialog => dialog.OnCancel, () => cancelled++));
        await cut.InvokeAsync(() => cut.Instance.ShowAsync().AsTask());

        cut.Find(".dialog-actions button[type=button]").Click();

        cancelled.Should().Be(1);
        JSInterop.Invocations.Select(invocation => invocation.Identifier).Should().EndWith("wombatDialog.close");
    }

    [Fact]
    public void Closing_TellsTheCaller_HoweverItClosed()
    {
        var closed = 0;
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.FormAction, "/somewhere")
            .Add(dialog => dialog.OnClosed, () => closed++));

        cut.Find("dialog").TriggerEvent("onclose", EventArgs.Empty);

        closed.Should().Be(1, "Escape closes a modal dialog natively, and the caller puts the focus back");
    }

    [Fact]
    public void TheActionMode_IsUnchanged_ItsConfirmRunsInTheCircuit()
    {
        var confirmed = 0;
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.Title, "Withdraw?")
            .Add(dialog => dialog.ConfirmLabel, "Withdraw")
            .Add(dialog => dialog.OnConfirm, () => confirmed++));

        cut.FindAll("form").Should().BeEmpty();
        cut.Find(".form-container h2").TextContent.Should().Be("Withdraw?");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Withdraw").Click();

        confirmed.Should().Be(1);
    }
}
