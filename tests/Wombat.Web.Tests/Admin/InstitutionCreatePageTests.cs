using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.CreateInstitution;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T400 (flow 18, lane A; boards R3-I07–I10; T378): Create institution. The subtitle, the mark line, the fields'
/// help; Name and Short code aria-required, never required, with no maxlength (S6, K14); each refusal under its field,
/// first in its aria-describedby, the first refused field focused, nothing sent; the duplicate under Short code (R3-I09);
/// no Back; Save lands on the record by its token (K13); any other failure the fixed sentence, logged.
/// </summary>
public sealed class InstitutionCreatePageTests : TestContext
{
    public InstitutionCreatePageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("devadmin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetPolicies("Administrator");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "devadmin"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void I07_TheForm_AsTheBoardDrawsIt()
    {
        var cut = Render(new Sender());

        Text(cut.Find("h1")).Should().Be("Create institution");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "A new institution is active from the moment it is saved, so its people can be invited.");
        cut.FindAll(".header-container .actions-cell").Should().BeEmpty("no Back: the trail and Cancel are the ways back");
        var form = cut.Find(".record-page form.form-container");
        form.HasAttribute("novalidate").Should().BeTrue();
        Text(form.QuerySelector("p.field-help")!).Should().Be("* marks a field you must fill in.");

        cut.FindAll("label").Select(Text).Should().Equal("Name * required", "Short code * required", "Contact email");
        foreach (var id in new[] { "institution-name", "institution-short-code" })
        {
            var input = cut.Find($"#{id}");
            input.GetAttribute("aria-required").Should().Be("true");
            input.HasAttribute("required").Should().BeFalse("S6: the page refuses in its own words, never the browser");
            input.HasAttribute("maxlength").Should().BeFalse("K14: a long value reaches the page's own refusal");
        }

        cut.Find("#institution-email").HasAttribute("aria-required").Should().BeFalse();
        cut.Find("#institution-email").GetAttribute("type").Should().Be("email");
        Help("institution-name").Should().Be("Up to 200 characters.");
        Help("institution-short-code").Should().Be("Up to 32 characters.");
        Help("institution-email").Should().Be("Optional.");
        cut.FindAll(".form-actions a, .form-actions button").Select(Text).Should().Equal("Cancel", "Save");
        cut.Find(".form-actions a").GetAttribute("href").Should().Be("/admin/institutions");

        string Help(string id)
        {
            var input = cut.Find($"#{id}");
            input.GetAttribute("aria-describedby").Should().Be($"{id}-help");
            return Text(cut.Find($"#{id}-help"));
        }
    }

    [Fact]
    public void I08_SavedEmpty_EachRefusalUnderItsField_FirstInItsDescription_TheFirstFocused_AndNothingSent()
    {
        var sender = new Sender();
        var cut = Render(sender);

        cut.Find("form").Submit();

        sender.Commands.Should().BeEmpty();
        AssertRefused(cut, "institution-name", "Type the institution's name. The record was not saved.");
        AssertRefused(cut, "institution-short-code", "Type a short code. The record was not saved.");
        cut.Find("#institution-email").HasAttribute("aria-invalid").Should().BeFalse();
        cut.FindAll(".alert").Should().BeEmpty("the refusals are beside their fields, with no summary");
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call =>
            call.Identifier == PageFocus.FocusByIdIdentifier && Equals(call.Arguments[0], "institution-name")));
    }

    [Theory]
    [InlineData(200, null)]
    [InlineData(201, "Keep the name to 200 characters. The record was not saved.")]
    public void K14_ALongName_IsRefusedByThePage(int length, string? refusal)
    {
        var sender = new Sender();
        var cut = Render(sender);

        cut.Find("#institution-name").Change(new string('n', length));
        cut.Find("#institution-short-code").Change("NEW");
        cut.Find("form").Submit();

        if (refusal is null)
        {
            sender.Commands.Should().ContainSingle();
        }
        else
        {
            sender.Commands.Should().BeEmpty();
            AssertRefused(cut, "institution-name", refusal);
        }
    }

    [Fact]
    public void I10_AMalformedAddress_IsRefusedUnderContactEmail_AndWhatWasTypedIsKept()
    {
        var sender = new Sender();
        var cut = Render(sender);

        cut.Find("#institution-name").Change("A new institution");
        cut.Find("#institution-short-code").Change("NEW");
        cut.Find("#institution-email").Change("no-domain");
        cut.Find("form").Submit();

        sender.Commands.Should().BeEmpty();
        AssertRefused(cut, "institution-email", "Enter a valid email address, for example name@example.org.");
        cut.Find("#institution-email").GetAttribute("value").Should().Be("no-domain");
        cut.Find("#institution-name").GetAttribute("value").Should().Be("A new institution");
    }

    [Theory]
    [InlineData(InstitutionRecordField.ShortCode, "institution-short-code")]
    [InlineData(InstitutionRecordField.Name, "institution-name")]
    public void I09_ADuplicate_IsTheCommandsFieldTiedRefusal_UnderItsField(InstitutionRecordField field, string fieldId)
    {
        var cut = Render(new Sender
        {
            Failure = new InstitutionUpdateRefusedException(
                InstitutionUpdateRefusal.Duplicate, field, "An institution with the same name or short code already exists.")
        });

        cut.Find("#institution-name").Change("Kgosi Kgari Teaching Hospital");
        cut.Find("#institution-short-code").Change("KGK");
        cut.Find("form").Submit();

        AssertRefused(cut, fieldId, "An institution with the same name or short code already exists.");
        cut.FindAll(".alert").Should().BeEmpty();
        cut.Find("#institution-short-code").GetAttribute("value").Should().Be("KGK");
    }

    [Fact]
    public void K13_Save_LandsOnTheRecord_ByItsToken()
    {
        var sender = new Sender();
        var cut = Render(sender);
        var navigation = Services.GetRequiredService<FakeNavigationManager>();

        cut.Find("#institution-name").Change(" Kgosi Kgari Teaching Hospital ");
        cut.Find("#institution-short-code").Change("KGK");
        cut.Find("#institution-email").Change(" ");
        cut.Find("form").Submit();

        var command = sender.Commands.Should().ContainSingle().Which;
        command.ContactEmail.Should().BeNull();
        navigation.Uri.Should().EndWith("/admin/institutions/7?created");
    }

    [Fact]
    public void AnUnexpectedFailure_IsTheFixedSentence_NeverTheExceptions()
    {
        var cut = Render(new Sender { Failure = new InvalidOperationException("Npgsql: deadlock detected") });

        cut.Find("#institution-name").Change("A new institution");
        cut.Find("#institution-short-code").Change("NEW");
        cut.Find("form").Submit();

        var failure = cut.Find(".action-result .alert.alert-danger");
        Text(failure).Should().Be("Could not save the institution. Nothing has changed. Try again, or come back in a few minutes.");
        Text(failure.QuerySelector("strong")!).Should().Be("Could not save the institution.");
        cut.Markup.Should().NotContain("Npgsql");
    }

    private void AssertRefused(IRenderedFragment cut, string fieldId, string words)
    {
        var input = cut.Find($"#{fieldId}");
        input.GetAttribute("aria-invalid").Should().Be("true");
        input.GetAttribute("aria-describedby").Should().Be($"{fieldId}-msg {fieldId}-help", "the refusal first (T378)");
        var message = cut.Find($"#{fieldId}-msg");
        Text(message).Should().Be(words);
        message.ClassList.Should().Contain("validation-message");
        message.ParentElement.Should().BeSameAs(input.ParentElement, "the refusal is under its own field");
    }

    private IRenderedComponent<InstitutionCreate> Render(Sender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<InstitutionCreate>();
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        public List<CreateInstitutionCommand> Commands { get; } = [];

        public Exception? Failure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var command = request.Should().BeOfType<CreateInstitutionCommand>().Subject;
            Commands.Add(command);
            return Failure is not null
                ? Task.FromException<TResponse>(Failure)
                : Task.FromResult((TResponse)(object)new InstitutionDto(
                    7, command.Name.Trim(), command.ShortCode.Trim(), command.ContactEmail, true, DateTime.UtcNow));
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
