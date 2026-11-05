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
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.CreateSubSpeciality;
using Wombat.Application.Features.Institutions.Commands.DeactivateSubSpeciality;
using Wombat.Application.Features.Institutions.Commands.UpdateSubSpeciality;
using Wombat.Application.Features.Institutions.Queries.GetCollegeTree;
using Wombat.Application.Features.Institutions.Queries.GetSpecialityById;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Navigation;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T394 (flow 16, lane A): a sub-speciality's record and Create sub-speciality as R3-A05, A06, A08, A13a, A14a, A15, A16a
/// and R2-A18c, A19b, A20b draw them: titled by its name, "Sub-speciality of ‹speciality›"; the default ladder chosen at
/// create (Q1); D56's dialog and the inactive line; refusals under Name (T324); back to the tree by token (S13);
/// Sub-speciality and Speciality unavailable (Q7, N2).
/// </summary>
public sealed class SubSpecialityPageTests : TestContext
{
    private const string Cpsa = "College of Paediatricians of South Africa";

    public SubSpecialityPageTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void TheRecord_IsTitledByItsName_ItsSpecialityInTheSubtitle_WithTheLadderChosen_AndItsHelp()
    {
        var cut = Render(new Sender(), 4);

        Text(cut.Find("h1")).Should().Be("Neonatology");
        Text(cut.Find(".page-subtitle")).Should().Be("Sub-speciality of Paediatrics");
        cut.Find(".user-page").ClassList.Should().Contain("sub-speciality-page");
        Text(cut.Find("p.field-help")).Should().Be("* marks a field you must fill in.");
        cut.FindAll("#sub-speciality-scale option").Select(Text).Should().Equal(
            "No default — offer every scale", "CNSA Neurology Entrustment Scale", "CPSA Paediatric Entrustment Scale v11.1", "O-R Scale");
        cut.Find("#sub-speciality-scale").GetAttribute("value").Should().Be("2", "CPSA's ladder is its default");
        cut.Find("#sub-speciality-scale").GetAttribute("aria-describedby").Should().Be("sub-speciality-scale-help");
        Text(cut.Find("#sub-speciality-scale-help")).Should().Be("Committee STARs for this programme's trainees are limited to this scale's levels.");
        Text(cut.Find("#sub-speciality-active-help")).Should().StartWith("An inactive sub-speciality cannot be named by a new invitation");
        cut.FindAll(".form-actions a").Single().GetAttribute("href").Should().Be("/admin/colleges/2/specialities#speciality-2");
    }

    [Fact]
    public void TheDialog_NamesTheKindAndTheRecord()
    {
        var cut = Render(new Sender(), 4);

        Text(cut.Find("dialog h2")).Should().Be("Deactivate the sub-speciality Neonatology?");
        cut.FindAll("dialog .dialog-body p").Select(Text).Should().Equal(
            "A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on.",
            "You can reactivate it on this page.");
    }

    [Fact]
    public void Deactivating_StaysOnTheRecord_WithItsResult_AndTheInactiveLine()
    {
        var sender = new Sender();
        var cut = Render(sender, 4);

        cut.Find("#sub-speciality-deactivate").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Deactivate").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be(
            "Sub-speciality Neonatology deactivated. A new invitation can no longer name it."));
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<DeactivateSubSpecialityCommand>();
        Text(cut.Find("#sub-speciality-active-help")).Should().Be(
            "Inactive. A new invitation cannot name it, and no institution can give anyone a role for it. Tick Active and save to reactivate it.");
        cut.FindAll("#sub-speciality-deactivate").Should().BeEmpty();
    }

    [Fact]
    public void SaveWithActiveUnticked_AsksFirst()
    {
        var sender = new Sender();
        var cut = Render(sender, 4);

        cut.Find("#sub-speciality-active").Change(false);
        cut.Find("button[type=submit]").GetAttribute("aria-haspopup").Should().Be("dialog");
        cut.Find("form").Submit();
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void Save_ReturnsToTheTree_ByItsToken_WithTheLadderChosen()
    {
        var sender = new Sender();
        var cut = Render(sender, 4);

        cut.Find("#sub-speciality-scale").Change("1");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/colleges/2/specialities?saved=sub:4"));
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateSubSpecialityCommand>()
            .Which.DefaultEntrustmentScaleId.Should().Be(1);
    }

    [Fact]
    public void Create_OffersTheDefaultLadder_AndLeavesForTheTree_ByItsToken()
    {
        var sender = new Sender();
        var cut = Render(sender, null);

        Text(cut.Find("h1")).Should().Be("Create sub-speciality");
        Text(cut.Find(".page-subtitle")).Should().Be("A sub-speciality of Paediatrics, in College of Paediatricians of South Africa.");
        cut.FindAll("#sub-speciality-scale").Should().ContainSingle("the default ladder is chosen at create (Q1)");
        cut.Find("#sub-speciality-scale").GetAttribute("value").Should().BeNullOrEmpty("No default — offer every scale is kept");
        cut.FindAll("fieldset").Should().BeEmpty();
        cut.FindAll("button").Where(button => button.Closest("dialog") is null).Select(Text).Should().Equal("Create sub-speciality");

        cut.Find("#sub-speciality-name").Change("Neonatal medicine");
        cut.Find("#sub-speciality-description").Change("Certificate training in Neonatology.");
        cut.Find("#sub-speciality-scale").Change("2");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/colleges/2/specialities?created=sub:11"));
        var command = sender.Commands.Should().ContainSingle().Which.Should().BeOfType<CreateSubSpecialityCommand>().Subject;
        (command.SpecialityId, command.Name, command.DefaultEntrustmentScaleId).Should().Be((2, "Neonatal medicine", (int?)2));
    }

    [Fact]
    public void AnEmptyName_IsRefusedUnderName_NoSummary()
    {
        var sender = new Sender();
        var cut = Render(sender, null);

        cut.Find("form").Submit();

        sender.Commands.Should().BeEmpty();
        cut.FindAll(".validation-message").Select(Text).Should().Equal("Enter a name.");
        cut.FindAll(".validation-summary-errors").Should().BeEmpty();
    }

    [Fact]
    public void ATakenName_IsTheCommandsRefusal_UnderName()
    {
        var taken = Render(new Sender
        {
            CreateFailure = new FieldRefusalException("Name", "A sub-speciality with the same name already exists for this speciality.")
        }, null);
        taken.Find("#sub-speciality-name").Change("Paediatrics");
        taken.Find("form").Submit();
        taken.WaitForAssertion(() => Text(taken.Find("#sub-speciality-name-refusal")).Should().Be(
            "A sub-speciality with the same name already exists for this speciality."));
    }

    [Fact]
    public void ACreateThatFails_IsTheFixedSentence_NeverTheExceptionsText()
    {
        var cut = Render(new Sender { CreateFailure = new InvalidOperationException("Entrustment scale 99 was not found.") }, null);
        cut.Find("#sub-speciality-name").Change("Neonatal medicine");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "The sub-speciality could not be created. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("was not found");
    }

    [Theory]
    [InlineData(4, false, "Sub-speciality unavailable", "This sub-speciality does not exist, or you cannot open it.")]
    [InlineData(77, true, "Sub-speciality unavailable", "This sub-speciality does not exist, or you cannot open it.")]
    [InlineData(null, false, "Speciality unavailable", "This speciality does not exist, or you cannot open it.")]
    public void WhatTheCallerCannotOpen_IsUnavailable(int? id, bool specialityRead, string heading, string body)
    {
        var cut = Render(new Sender { SpecialityRead = specialityRead }, id);

        Text(cut.Find("h1")).Should().Be(heading);
        Text(cut.Find(".detail-card--empty div.state-panel-copy")).Should().Be(body);
        // T394 fix pass (pages S4): every Unavailable is flow 15's markup, its link in .form-actions with no icon.
        cut.Find(".detail-card--empty .form-actions a").QuerySelector("svg").Should().BeNull();
        cut.FindAll("form").Where(form => form.Closest("dialog") is null).Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsTitledByTheKind_WithTheFixedSentence()
    {
        var cut = Render(new Sender { TreeFailure = new InvalidOperationException("Npgsql") }, 4);

        Text(cut.Find("h1")).Should().Be("Sub-speciality");
        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load this sub-speciality. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");
    }

    private IRenderedComponent<SubSpecialityEdit> Render(Sender sender, int? id, string role = WombatRoles.CollegeAdmin)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{role}@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, $"{role}-1"));
        Services.AddSingleton<IScopedSender>(sender);

        var acting = ActingRoleResolver.Resolve(role, [role]);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "Test"));
        var cut = RenderComponent<SubSpecialityEdit>(parameters => parameters
            .Add(page => page.SpecialityId, 2)
            .Add(page => page.Id, id)
            .AddCascadingValue(acting)
            .AddCascadingValue(NavItems.For(acting, user))
            .AddCascadingValue(new RouteData(typeof(SubSpecialityEdit), new Dictionary<string, object?> { ["SpecialityId"] = 2, ["Id"] = id })));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        public bool SpecialityRead { get; init; } = true;
        public Exception? TreeFailure { get; init; }
        public Exception? CreateFailure { get; init; }
        public List<object> Commands { get; } = [];

        private static CollegeTreeDto Tree() => new(2, Cpsa, "CPSA",
        [
            new CollegeTreeSpecialityDto(2, "Paediatrics", null, true,
                [new CollegeTreeSubSpecialityDto(4, 2, "Neonatology", "Certificate training in Neonatology.", true, 2, "CPSA Paediatric Entrustment Scale v11.1")])
        ]);

        private static IReadOnlyList<EntrustmentScaleDto> Scales() =>
        [
            new(3, "CNSA Neurology Entrustment Scale", null, []),
            new(2, "CPSA Paediatric Entrustment Scale v11.1", null, []),
            new(1, "O-R Scale", null, [])
        ];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? answer = request switch
            {
                GetSpecialityByIdQuery => SpecialityRead ? new SpecialityDto(2, 2, "Paediatrics", null, true) : null,
                GetCollegeTreeQuery when TreeFailure is not null => throw TreeFailure,
                GetCollegeTreeQuery => Tree(),
                GetEntrustmentScalesListQuery => Scales(),
                UpdateSubSpecialityCommand command => Record(command, () => new SubSpecialityDto(command.Id, command.SpecialityId, command.Name, command.Description, command.IsActive, command.DefaultEntrustmentScaleId)),
                CreateSubSpecialityCommand command => CreateFailure is not null
                    ? Fail(command, CreateFailure)
                    : Record(command, () => new SubSpecialityDto(11, command.SpecialityId, command.Name, command.Description, true, command.DefaultEntrustmentScaleId)),
                _ => throw new NotSupportedException(request.GetType().Name),
            };
            return Task.FromResult((TResponse)answer!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Commands.Add(request);
            return Task.CompletedTask;
        }

        private object Record(object command, Func<object> answer)
        {
            Commands.Add(command);
            return answer();
        }

        private object Fail(object command, Exception failure)
        {
            Commands.Add(command);
            throw failure;
        }
    }
}
