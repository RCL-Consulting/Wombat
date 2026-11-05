using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common;
using Wombat.Application.Features.Colleges;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Colleges;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T394 (flow 16, lane B): Colleges and a College's record as R3-B01–B12, B17b and R2-B13–B15b draw them. The list's row
/// header, ISO South African day (N2) and row names; the record titled by name with "CNSA · Active" (S11, S15), no Back
/// (Q9), refusals under their fields naming the holder (S10, T324), the fixed failure (T272), "College created." by the
/// token (S13), Deactivate… and Save-unticked asking one question (D56, N20), the inactive line, and Unavailable (Q7).
/// </summary>
public sealed class CollegePageTests : TestContext
{
    // 2026-11-02 23:30 UTC is already 2026-11-03 in South Africa (N2).
    private static readonly DateTime LateOnTheSecond = new(2026, 11, 2, 23, 30, 0, DateTimeKind.Utc);

    private static readonly CollegeDto Cnsa = new(
        4, "College of Neurologists of South Africa", "CNSA", "Constituent College of the CMSA; owns the national Neurology catalogue.",
        true, LateOnTheSecond);

    private static readonly CollegeDto Cpsa = new(
        2, "College of Paediatricians of South Africa", "CPSA", "Owns the national Paediatric EPA catalogue.", true, LateOnTheSecond);

    public CollegePageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ---- Colleges ----

    [Fact]
    public void Colleges_NameEachRowByItsCollege_WithTheSouthAfricanDay_AndRowActionsNamedForIt()
    {
        Services.AddSingleton<IScopedSender>(new Sender { Colleges = [Cnsa, Cpsa] });
        var cut = RenderComponent<CollegesList>();

        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(2));
        Text(cut.Find("h1")).Should().Be("Colleges");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves.");
        cut.Find(".people-page.colleges-page").Should().NotBeNull("the catalogue's stack reads the page's class (§ 0)");
        var table = cut.Find("table");
        table.ClassList.Should().Contain("clinic-table--stack-wide");
        Text(table.QuerySelector("caption")!).Should().Be("Every College");
        cut.FindAll("tbody th[scope='row']").Select(Text).Should().Equal(Cnsa.Name, Cpsa.Name);
        cut.FindAll("tbody td[data-label='Created']").Select(Text).Should().Equal("2026-11-03", "2026-11-03");
        cut.FindAll("tbody tr").First().QuerySelectorAll("a").Select(link => (Text(link), link.GetAttribute("aria-label"), link.GetAttribute("href")))
            .Should().Equal(
                ("Edit", "Edit College of Neurologists of South Africa", "/admin/colleges/4"),
                ("Specialities", "Specialities of College of Neurologists of South Africa", "/admin/colleges/4/specialities"));
        Text(cut.Find(".header-container a.btn-primary")).Should().Be("Create college");
    }

    [Fact]
    public void Colleges_WhileLoading_SayIt_AndOfferNothing()
    {
        var hang = new Sender { HangList = true };
        Services.AddSingleton<IScopedSender>(hang);
        var cut = RenderComponent<CollegesList>();

        Text(cut.Find(".loading-line")).Should().Be("Loading the Colleges.");
        cut.FindAll(".header-container a").Should().BeEmpty("Create college waits for the read (R2-B14a)");
    }

    [Fact]
    public void Colleges_AFailedRead_IsFixedWords_WithTryAgain()
    {
        Services.AddSingleton<IScopedSender>(new Sender { ListFailure = new InvalidOperationException("Npgsql: boom") });
        var cut = RenderComponent<CollegesList>();

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-danger"));
        Text(cut.Find(".alert-row-text")).Should().Be(
            "Could not load the Colleges. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");
    }

    // ---- a College ----

    [Fact]
    public void TheRecord_IsTitledByItsName_WithItsCodeAndState_AndNoBackLink()
    {
        var cut = Render(new Sender { College = Cpsa }, 2);

        Text(cut.Find("h1")).Should().Be("College of Paediatricians of South Africa");
        Text(cut.Find(".page-subtitle")).Should().Be("CPSA · Active");
        cut.Find(".user-page.college-page").Should().NotBeNull();
        cut.FindAll("a").Should().NotContain(link => Text(link) == "Back to colleges");
        Text(cut.Find("p.field-help")).Should().Be("* marks a field you must fill in.");
        cut.Find("#college-name").GetAttribute("aria-required").Should().Be("true");
        Text(cut.Find("#college-active-help")).Should().Be(
            "An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are.");
        // T394 fix pass (pages S5): one label, so the box is named "Active"; the legend names the group.
        cut.FindAll("label[for=college-active]").Select(Text).Should().Equal("Active");
        Text(cut.Find("#college-active").Closest("fieldset.form-group")!.QuerySelector("legend")!).Should().Be("Status");
        cut.FindAll(".validation-summary-errors").Should().BeEmpty();
        var actions = cut.FindAll(".form-actions > *").Where(control => control.Closest("dialog") is null).Select(Text).ToList();
        actions.Should().Equal("Cancel", "Deactivate…", "Save");
    }

    [Fact]
    public void Create_RefusesEmptyFieldsUnderEachInItsWords_FocusingTheFirst_AndSendsNothing()
    {
        var sender = new Sender();
        var cut = Render(sender, null);

        Text(cut.Find("h1")).Should().Be("Create college");
        Text(cut.Find(".page-subtitle")).Should().Be("A national College keeps its own specialities, EPAs and curricula. It starts active.");
        cut.FindAll("#college-active").Should().BeEmpty("a new College starts active");
        cut.Find("form").Submit();

        Text(cut.Find("#college-name-refusal")).Should().Be("Enter a name.");
        Text(cut.Find("#college-short-code-refusal")).Should().Be("Enter a short code.");
        cut.Find("#college-name").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#college-name").GetAttribute("aria-describedby").Should().Be("college-name-refusal");
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombat.focusById" && call.Arguments.Contains("college-name"));
        sender.Sent.Should().BeEmpty();
    }

    [Fact]
    public void Create_ATakenShortCode_IsUnderShortCode_NamingItsHolder_WhatWasTypedKept()
    {
        var sender = new Sender
        {
            CreateFailure = new FieldRefusalException("ShortCode", "CPSA is already the short code of College of Paediatricians of South Africa. Choose another.")
        };
        var cut = Render(sender, null);

        cut.Find("#college-name").Change("College of Neurologists of South Africa");
        cut.Find("#college-short-code").Change("CPSA");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#college-short-code-refusal")).Should().Be(
            "CPSA is already the short code of College of Paediatricians of South Africa. Choose another."));
        cut.Find("#college-short-code").GetAttribute("value").Should().Be("CPSA");
        cut.FindAll(".action-result .alert").Should().BeEmpty();
    }

    [Fact]
    public void Create_LeavesForItsRecord_ByTheCreatedToken()
    {
        var sender = new Sender { Created = Cnsa };
        var cut = Render(sender, null);

        cut.Find("#college-name").Change(Cnsa.Name);
        cut.Find("#college-short-code").Change("CNSA");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/colleges/4?created"));
    }

    [Fact]
    public void ArrivingByCreated_WritesCollegeCreated_FocusesIt_AndDropsTheToken()
    {
        Services.AddSingleton<IScopedSender>(new Sender { College = Cnsa });
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin/colleges/4?created");
        var cut = RenderComponent<CollegeEdit>(parameters => parameters.Add(page => page.Id, 4));

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be("College created."));
        cut.WaitForAssertion(() => navigation.Uri.Should().EndWith("/admin/colleges/4"));
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public void ASave_StaysOnTheRecord_SayingCollegeSaved()
    {
        var sender = new Sender { College = Cpsa, Updated = Cpsa with { Description = "Edited." } };
        var cut = Render(sender, 2);

        cut.Find("#college-description").Change("Edited.");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be("College saved."));
    }

    [Fact]
    public void AFailedSave_IsTheFixedSentence_NeverTheExceptionsText()
    {
        var sender = new Sender { College = Cpsa, UpdateFailure = new InvalidOperationException("23503 foreign key violation") };
        var cut = Render(sender, 2);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "The College could not be saved. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("23503");
    }

    [Fact]
    public void DeactivateAsksFirst_NamingTheCollege_WithNoCount()
    {
        var cut = Render(new Sender { College = Cnsa }, 4);

        var trigger = cut.Find("#college-deactivate");
        Text(trigger).Should().Be("Deactivate…");
        trigger.GetAttribute("aria-haspopup").Should().Be("dialog");
        trigger.ClassList.Should().Contain("btn-outline");
        Text(cut.Find("dialog h2")).Should().Be("Deactivate College of Neurologists of South Africa?");
        cut.FindAll("dialog .dialog-body p").Select(Text).Should().Equal(
            "A new invitation will no longer be able to name it. That is all it changes: its specialities, EPAs and curricula stay as they are, every institution that has adopted one of its curricula carries on, and everyone already invited keeps their invitation.",
            "You can reactivate it on this page.");
        cut.FindAll("dialog .form-actions button").Select(Text).Should().Equal("Cancel", "Deactivate");
    }

    [Fact]
    public void SaveWithActiveUnticked_AsksTheSameQuestion_AndItsConfirmSavesTheDeactivation()
    {
        var sender = new Sender { College = Cnsa, Updated = Cnsa with { IsActive = false } };
        var cut = Render(sender, 4);

        cut.Find("#college-active").Change(false);
        cut.Find("form button[type=submit]").GetAttribute("aria-haspopup").Should().Be("dialog", "N20: Save would deactivate");
        cut.Find("form").Submit();

        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombatDialog.showModal");
        sender.Sent.Should().BeEmpty("nothing is saved until the dialog's Deactivate");

        cut.FindAll("dialog .form-actions button").Single(button => Text(button) == "Deactivate").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be(
            "College of Neurologists of South Africa deactivated. A new invitation can no longer name it."));
        sender.Sent.OfType<UpdateCollegeCommand>().Single().IsActive.Should().BeFalse();
        Text(cut.Find(".page-subtitle")).Should().Be("CNSA · Inactive");
        Text(cut.Find("#college-active-help")).Should().Be(
            "Inactive. A new invitation cannot name this College. Tick Active and save to reactivate it.");
        cut.FindAll("#college-deactivate").Should().BeEmpty("Deactivate… only on a College stored active");
    }

    [Fact]
    public void AnUnknownId_IsCollegeUnavailable_InThePagesOwnPlace()
    {
        var cut = Render(new Sender { College = null }, 99);

        Text(cut.Find("h1")).Should().Be("College unavailable");
        Text(cut.Find(".state-panel-copy")).Should().Be("This College does not exist, or you cannot open it.");
        var go = cut.Find(".detail-card--empty a");
        Text(go).Should().Be("Go to Colleges");
        go.GetAttribute("href").Should().Be("/admin/colleges");
        cut.FindAll("form").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsTitledCollege_WithFixedWordsAndTryAgain()
    {
        var cut = Render(new Sender { ReadFailure = new TimeoutException("10.0.0.4") }, 4);

        Text(cut.Find("h1")).Should().Be("College");
        Text(cut.Find(".alert-row-text")).Should().Be(
            "Could not load this College. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("10.0.0.4");
    }

    private IRenderedComponent<CollegeEdit> Render(Sender sender, int? id)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CollegeEdit>(parameters => parameters.Add(page => page.Id, id));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        public IReadOnlyList<CollegeDto> Colleges { get; init; } = [];
        public CollegeDto? College { get; init; }
        public CollegeDto? Created { get; init; }
        public CollegeDto? Updated { get; init; }
        public Exception? CreateFailure { get; init; }
        public Exception? UpdateFailure { get; init; }
        public Exception? ReadFailure { get; init; }
        public Exception? ListFailure { get; init; }
        public bool HangList { get; init; }
        public List<object> Sent { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? answer;
            switch (request)
            {
                case GetCollegesListQuery:
                    if (HangList)
                    {
                        return new TaskCompletionSource<TResponse>().Task;
                    }

                    if (ListFailure is not null)
                    {
                        return Task.FromException<TResponse>(ListFailure);
                    }

                    answer = Colleges;
                    break;
                case GetCollegeByIdQuery:
                    if (ReadFailure is not null)
                    {
                        return Task.FromException<TResponse>(ReadFailure);
                    }

                    answer = College;
                    break;
                case CreateCollegeCommand:
                    Sent.Add(request);
                    if (CreateFailure is not null)
                    {
                        return Task.FromException<TResponse>(CreateFailure);
                    }

                    answer = Created ?? throw new NotSupportedException("No create expected.");
                    break;
                case UpdateCollegeCommand:
                    Sent.Add(request);
                    if (UpdateFailure is not null)
                    {
                        return Task.FromException<TResponse>(UpdateFailure);
                    }

                    answer = Updated ?? College;
                    break;
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            return Task.FromResult((TResponse)answer!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            return Task.CompletedTask;
        }
    }
}
