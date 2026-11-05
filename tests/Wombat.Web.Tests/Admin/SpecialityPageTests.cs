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
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.CreateSpeciality;
using Wombat.Application.Features.Institutions.Commands.DeactivateSpeciality;
using Wombat.Application.Features.Institutions.Commands.UpdateSpeciality;
using Wombat.Application.Features.Institutions.Queries.GetCollegeTree;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Navigation;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T394 (flow 16, lane A): a speciality's record and Create speciality as R3-A03, A04, A09, A11, A17 and R2-A18b, A19b,
/// A20b draw them. Titled by its name (S11), the kind in the subtitle (M8); the required line (M4); D56's Deactivate… and
/// Save with Active unticked asking the same (N10, N20); the inactive line; refusals under Name (T324); the fixed failure
/// (T272, T393); Save and a create back to the tree by token (S13); Speciality and College unavailable (Q7).
/// </summary>
public sealed class SpecialityPageTests : TestContext
{
    private const string Cpsa = "College of Paediatricians of South Africa";

    public SpecialityPageTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void TheRecord_IsTitledByItsName_TheKindInItsSubtitle_UnderTheCollegesTree_WithNoBackLink()
    {
        var cut = Render(new Sender(), 2);

        Text(cut.Find("h1")).Should().Be("Paediatrics");
        Text(cut.Find(".page-subtitle")).Should().Be("Speciality of College of Paediatricians of South Africa");
        cut.Find(".user-page").ClassList.Should().Contain("speciality-page");
        Text(cut.Find("p.field-help")).Should().Be("* marks a field you must fill in.");
        cut.Find("#speciality-name").GetAttribute("aria-required").Should().Be("true");
        Text(cut.Find("label[for=speciality-name]")).Should().Be("Name* required");
        cut.FindAll("a").Should().NotContain(link => Text(link).StartsWith("Back", StringComparison.Ordinal));
        cut.FindAll(".validation-summary-errors").Should().BeEmpty();

        Text(cut.Find("fieldset legend")).Should().Be("Status");
        cut.Find("#speciality-active").GetAttribute("aria-describedby").Should().Be("speciality-active-help");
        Text(cut.Find("#speciality-active-help")).Should().Be(
            "An inactive speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on.");

        var cancel = cut.FindAll(".form-actions a").Single();
        Text(cancel).Should().Be("Cancel");
        cancel.GetAttribute("href").Should().Be("/admin/colleges/2/specialities#speciality-2");
        cut.FindAll(".form-actions button").Where(b => b.Closest("dialog") is null).Select(Text).Should().Equal("Deactivate…", "Save");
    }

    [Theory]
    [InlineData(WombatRoles.CollegeAdmin)]
    [InlineData(WombatRoles.Administrator)]
    public void TheTrail_RunsThroughTheCollegesTree(string role)
    {
        var cut = Render(new Sender(), 2, role);

        var crumbs = cut.FindAll("nav[aria-label='Breadcrumb'] li")
            .Select(li => (Text(li), li.QuerySelector("a")?.GetAttribute("href"))).ToList();
        crumbs.Should().Equal(role == WombatRoles.Administrator
            ? [("Home", "/"), ("Colleges", "/admin/colleges"), (Cpsa, "/admin/colleges/2"), ("Specialities", "/admin/colleges/2/specialities"), ("Paediatrics", null)]
            : [("Home", "/"), ("Specialities", "/admin/specialities"), (Cpsa, "/admin/colleges/2/specialities"), ("Paediatrics", null)]);
    }

    [Fact]
    public void Deactivate_AsksFirst_NamingTheKindAndTheRecord_AndSaysOnlyWhatItDoes()
    {
        var cut = Render(new Sender(), 2);

        var trigger = cut.Find("#speciality-deactivate");
        trigger.ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger");
        trigger.GetAttribute("aria-haspopup").Should().Be("dialog");
        Text(cut.Find("dialog h2")).Should().Be("Deactivate the speciality Paediatrics?");
        cut.FindAll("dialog .dialog-body p").Select(Text).Should().Equal(
            "A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it or any of its sub-specialities. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on.",
            "You can reactivate it on this page.");
        cut.FindAll("dialog button").Select(Text).Should().Equal("Cancel", "Deactivate");
        cut.Find("dialog button.btn-danger").TextContent.Trim().Should().Be("Deactivate");
    }

    [Fact]
    public void Deactivating_StaysOnTheRecord_TheResultFocused_ActiveUnticked_WithTheInactiveLine_AndNoDeactivate()
    {
        var sender = new Sender();
        var cut = Render(sender, 2);

        cut.Find("#speciality-deactivate").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Deactivate").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be(
            "Speciality Paediatrics deactivated. A new invitation can no longer name it."));
        Text(cut.Find(".action-result .alert strong")).Should().Be("Speciality Paediatrics deactivated.");
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<DeactivateSpecialityCommand>();
        cut.Find("#speciality-active").HasAttribute("checked").Should().BeFalse();
        Text(cut.Find("#speciality-active-help")).Should().Be(
            "Inactive. A new invitation cannot name it, and no institution can give anyone a role for it or any of its sub-specialities. Tick Active and save to reactivate it.");
        cut.FindAll("#speciality-deactivate").Should().BeEmpty();
        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/colleges/2/specialities/2");
    }

    [Fact]
    public void SaveWithActiveUnticked_SaysItOpensADialog_AndAsksTheSameQuestion_BeforeSaving()
    {
        var sender = new Sender();
        var cut = Render(sender, 2);
        cut.Find("button[type=submit]").HasAttribute("aria-haspopup").Should().BeFalse();

        cut.Find("#speciality-active").Change(false);
        cut.Find("button[type=submit]").GetAttribute("aria-haspopup").Should().Be("dialog");
        cut.Find("form").Submit();

        sender.Commands.Should().BeEmpty("the dialog asks first");
        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "wombatDialog.showModal");
        cut.FindAll("dialog button").Single(button => Text(button) == "Deactivate").Click();

        cut.WaitForAssertion(() => sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateSpecialityCommand>()
            .Which.IsActive.Should().BeFalse());
        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().StartWith("Speciality Paediatrics deactivated."));
    }

    [Fact]
    public void AnInactiveRecord_OffersNoDeactivate_ShowsTheInactiveLine_AndTickingActiveAndSaving_AsksNothing()
    {
        var sender = new Sender { Active = false };
        var cut = Render(sender, 2);

        cut.FindAll("#speciality-deactivate").Should().BeEmpty();
        Text(cut.Find("#speciality-active-help")).Should().StartWith("Inactive. A new invitation cannot name it");
        cut.Find("#speciality-active").Change(true);
        cut.Find("button[type=submit]").HasAttribute("aria-haspopup").Should().BeFalse();
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateSpecialityCommand>()
            .Which.IsActive.Should().BeTrue());
    }

    [Fact]
    public void Save_ReturnsToTheTree_ByItsToken()
    {
        var sender = new Sender();
        var cut = Render(sender, 2);

        cut.Find("#speciality-description").Change("Specialist training in Paediatrics, and certificate training in its sub-specialities.");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/colleges/2/specialities?saved=speciality:2"));
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateSpecialityCommand>()
            .Which.Description.Should().Be("Specialist training in Paediatrics, and certificate training in its sub-specialities.");
    }

    [Fact]
    public void AnEmptyName_IsRefusedUnderName_Once_InTheLabelsWords_Focused_NothingSent()
    {
        var sender = new Sender();
        var cut = Render(sender, 2);

        cut.Find("#speciality-name").Change("  ");
        cut.Find("form").Submit();

        sender.Commands.Should().BeEmpty();
        cut.FindAll(".validation-message").Select(Text).Should().Equal("Enter a name.");
        Text(cut.Find("#speciality-name-refusal")).Should().Be("Enter a name.");
        cut.Find("#speciality-name").GetAttribute("aria-describedby").Should().Be("speciality-name-refusal");
        cut.Find("#speciality-name").GetAttribute("aria-invalid").Should().Be("true");
        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "wombat.focusById"
            && Equals(invocation.Arguments[0], "speciality-name"));
    }

    [Fact]
    public void ADescriptionPastItsCap_IsRefusedUnderDescription()
    {
        var sender = new Sender();
        var cut = Render(sender, 2);

        cut.Find("#speciality-description").Change(new string('D', 2001));
        cut.Find("form").Submit();

        sender.Commands.Should().BeEmpty();
        Text(cut.Find("#speciality-description-refusal")).Should().Be("A description is at most 2,000 characters.");
    }

    [Fact]
    public void ATakenName_IsTheCommandsRefusal_UnderName_WhatWasTypedKept()
    {
        var sender = new Sender { UpdateFailure = new FieldRefusalException("Name", "A speciality with the same name already exists for this college.") };
        var cut = Render(sender, 2);

        cut.Find("#speciality-name").Change("Surgery");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#speciality-name-refusal")).Should().Be(
            "A speciality with the same name already exists for this college."));
        cut.Find("#speciality-name").GetAttribute("value").Should().Be("Surgery");
        cut.FindAll(".action-result .alert").Should().BeEmpty();
        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/colleges/2/specialities/2");
    }

    [Fact]
    public void AnyOtherFailure_IsTheFixedSentence_ItsFirstSentenceBold_NeverTheExceptionsText()
    {
        var sender = new Sender { UpdateFailure = new InvalidOperationException("23505 duplicate key value violates unique constraint") };
        var cut = Render(sender, 2);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "The speciality could not be saved. Nothing has changed. Try again, or come back in a few minutes."));
        Text(cut.Find(".action-result .alert-danger strong")).Should().Be("The speciality could not be saved.");
        cut.Markup.Should().NotContain("23505");
    }

    [Fact]
    public void Create_IsTitledCreateSpeciality_UnderItsCollege_AndLeavesForTheTree_ByItsToken()
    {
        var sender = new Sender();
        var cut = Render(sender, null, WombatRoles.Administrator);

        Text(cut.Find("h1")).Should().Be("Create speciality");
        Text(cut.Find(".page-subtitle")).Should().Be("A speciality of College of Paediatricians of South Africa.");
        cut.FindAll("fieldset").Should().BeEmpty("a new speciality is active");
        cut.FindAll("button").Where(button => button.Closest("dialog") is null).Select(Text).Should().Equal("Create speciality");
        Text(cut.FindAll("nav[aria-label='Breadcrumb'] li").Last()).Should().Be("Create speciality");

        cut.Find("#speciality-name").Change("Neurology");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/colleges/2/specialities?created=speciality:7"));
    }

    [Fact]
    public void ACreateThatFails_IsTheCreatesFixedSentence()
    {
        var cut = Render(new Sender { CreateFailure = new InvalidOperationException("boom") }, null);

        cut.Find("#speciality-name").Change("Neurology");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().StartWith("The speciality could not be created."));
        cut.Markup.Should().NotContain("boom");
    }

    [Theory]
    [InlineData(2, false, "Speciality unavailable", "This speciality does not exist, or you cannot open it.")]
    [InlineData(99, true, "Speciality unavailable", "This speciality does not exist, or you cannot open it.")]
    [InlineData(null, false, "College unavailable", "This College does not exist, or you cannot open it.")]
    public void WhatTheCallerCannotOpen_IsUnavailable_NoIdShown(int? id, bool treeRead, string heading, string body)
    {
        var cut = Render(new Sender { TreeRead = treeRead }, id);

        Text(cut.Find("h1")).Should().Be(heading);
        Text(cut.Find(".detail-card--empty div.state-panel-copy")).Should().Be(body);
        // T394 fix pass (pages S4): every Unavailable is flow 15's markup, its link in .form-actions with no icon.
        Text(cut.Find(".detail-card--empty .form-actions a")).Should().Be("Go to Specialities");
        cut.Find(".detail-card--empty .form-actions a").QuerySelector("svg").Should().BeNull();
        cut.FindAll("form").Where(form => form.Closest("dialog") is null).Should().BeEmpty();
        // Guids can hold "99" by chance: ConfirmDialog's ids ("confirm-<Guid>") and bUnit's blazor:elementReference
        // attributes. The id under test is the route's, so every guid is taken out first.
        System.Text.RegularExpressions.Regex.Replace(
                cut.Markup, "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}|[0-9a-f]{32}", "guid")
            .Should().NotContain("99");
    }

    [Fact]
    public void AFailedRead_IsTitledByTheKind_WithTheFixedSentence_AndTryAgain()
    {
        var cut = Render(new Sender { TreeFailure = new InvalidOperationException("Npgsql") }, 2);

        Text(cut.Find("h1")).Should().Be("Speciality");
        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load this speciality. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");
    }

    private IRenderedComponent<SpecialityEdit> Render(Sender sender, int? id, string role = WombatRoles.CollegeAdmin)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{role}@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, $"{role}-1"));
        Services.AddSingleton<IScopedSender>(sender);
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(id is { } value ? $"/admin/colleges/2/specialities/{value}" : "/admin/colleges/2/specialities/new");

        var acting = ActingRoleResolver.Resolve(role, [role]);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "Test"));
        var cut = RenderComponent<SpecialityEdit>(parameters => parameters
            .Add(page => page.CollegeId, 2)
            .Add(page => page.Id, id)
            .AddCascadingValue(acting)
            .AddCascadingValue(NavItems.For(acting, user))
            .AddCascadingValue(new RouteData(typeof(SpecialityEdit), new Dictionary<string, object?> { ["CollegeId"] = 2, ["Id"] = id })));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        /// <summary>The tree is read: false answers null, as for another College's.</summary>
        public bool TreeRead { get; init; } = true;
        public bool Active { get; init; } = true;
        public Exception? TreeFailure { get; init; }
        public Exception? UpdateFailure { get; init; }
        public Exception? CreateFailure { get; init; }
        public List<object> Commands { get; } = [];

        private CollegeTreeDto Tree() => new(2, Cpsa, "CPSA",
            [new CollegeTreeSpecialityDto(2, "Paediatrics", "Specialist training in Paediatrics.", Active, [])]);

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetCollegeTreeQuery when TreeFailure is not null:
                    return Task.FromException<TResponse>(TreeFailure);
                case GetCollegeTreeQuery:
                    return Task.FromResult((TResponse)(object?)(TreeRead ? Tree() : null)!);
                case UpdateSpecialityCommand command:
                    Commands.Add(command);
                    return UpdateFailure is not null
                        ? Task.FromException<TResponse>(UpdateFailure)
                        : Task.FromResult((TResponse)(object)new SpecialityDto(command.Id, command.CollegeId, command.Name.Trim(), command.Description, command.IsActive));
                case CreateSpecialityCommand command:
                    Commands.Add(command);
                    return CreateFailure is not null
                        ? Task.FromException<TResponse>(CreateFailure)
                        : Task.FromResult((TResponse)(object)new SpecialityDto(7, command.CollegeId, command.Name, command.Description, true));
                default:
                    throw new NotSupportedException(request.GetType().Name);
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Commands.Add(request);
            return Task.CompletedTask;
        }
    }
}
