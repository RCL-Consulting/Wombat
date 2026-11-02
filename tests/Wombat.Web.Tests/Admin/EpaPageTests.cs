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
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Epas;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T392 (flow 15, lane C): an EPA's page as R3-C08, C12, C18a, C18b and R2-C04–C07, C10, C11 draw it. Titled by the EPA
/// (T7), the form or the national record by "may change" (Q2), Unavailable for what the caller cannot open (Q19), the
/// own EPA's deactivate words (Q11), "EPA saved." by ?created (F1, M5), and refusals under their fields (F4).
/// </summary>
public sealed class EpaPageTests : TestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";
    private const string Paediatrics = "Paediatrics / Paediatrics";

    public EpaPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("nolwazi@test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "inst-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TheOwnEpasPage_IsTitledByTheEpa_WithItsCodeTheCrumb_AndNoBackLink()
    {
        var cut = Render(new Sender { Page = Page(Own()) }, 17);

        Text(cut.Find("h1")).Should().Be("KGK-001 — Running a paediatric outreach clinic at a district hospital");
        Text(cut.Find(".page-subtitle")).Should().Be("Define the EPA code, scope, and descriptive guidance.");
        cut.FindAll("a").Should().NotContain(link => Text(link) == "Back to EPAs");
        cut.FindAll(".validation-summary-errors").Should().BeEmpty("refusals sit under their fields, not at the top");
    }

    [Fact]
    public void TheOwnEpasActiveHelp_AndDialog_NameItsInstitutionsTrainees()
    {
        var cut = Render(new Sender { Page = Page(Own()) }, 17);

        Text(cut.Find("#epa-active-help")).Should().StartWith(
            "An inactive EPA cannot be chosen by Kgosi Kgari Teaching Hospital's trainees for a new activity and is not a target on their running programmes' progress pages or dashboards;");

        var trigger = cut.FindAll("button").Single(button => button.Closest("dialog") is null && Text(button) == "Deactivate…");
        trigger.GetAttribute("aria-haspopup").Should().Be("dialog");
        Text(cut.Find("dialog h2")).Should().Be("Deactivate KGK-001?");
        Text(cut.Find("dialog")).Should().Contain("progress pages and dashboards, for Kgosi Kgari Teaching Hospital's trainees.");
    }

    [Fact]
    public void ANationalEpasDialog_SaysEveryInstitutionThatHasAdoptedIt()
    {
        var cut = Render(new Sender { Page = Page(National(mayChange: true)) }, 13);

        Text(cut.Find("dialog h2")).Should().Be("Deactivate PAED-012?");
        Text(cut.Find("dialog")).Should().Contain("dashboards, for every institution that has adopted it.");
    }

    [Fact]
    public void ANationalEpaSheMayNotChange_IsItsRecord_WithNoFormNoSaveNoDeactivate()
    {
        var cut = Render(new Sender { Page = Page(National(mayChange: false)) }, 13);

        Text(cut.Find("h1")).Should().Be("PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "College of Paediatricians of South Africa · Paediatrics / Paediatrics · Set by the College, so read only here");
        cut.FindAll("form").Where(form => form.Closest("dialog") is null).Should().BeEmpty();
        cut.FindAll("button").Where(button => button.Closest("dialog") is null).Should().BeEmpty();
        cut.FindAll("h2").Where(heading => heading.Closest("dialog") is null).Select(Text)
            .Should().Equal("About this EPA", "Description", "Required knowledge and skills");
        cut.FindAll("dl.detail-list dt").Select(Text).Should().Equal("College", "Sub-speciality", "Category", "Status");
        cut.FindAll("dl.detail-list dd").Select(Text).Should().Equal(
            "College of Paediatricians of South Africa", Paediatrics, "Core", "Active");
    }

    [Fact]
    public void AnEpaSheCannotOpen_IsUnavailable_NoIdShown()
    {
        var cut = Render(new Sender { Page = new EpaPageDto(null, [], false, null, false) }, 1);

        Text(cut.Find("h1")).Should().Be("EPA unavailable");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be("This EPA does not exist, or you cannot open it.");
        var go = cut.Find(".detail-card--empty a");
        Text(go).Should().Be("Go to EPAs");
        go.GetAttribute("href").Should().Be("/admin/epas");
        cut.Markup.Should().NotContain("EPA-001");
    }

    [Fact]
    public void Create_ForHerInstitution_ArrivesWithTheAdoptedDisciplineChosen_AndItsHelp()
    {
        var cut = Render(new Sender { Page = new EpaPageDto(null, [new EpaDisciplineDto(2, Paediatrics)], true, Kgk, true) }, null);

        Text(cut.Find("h1")).Should().Be("Create EPA");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "An EPA of your institution's own. It applies only to Kgosi Kgari Teaching Hospital's trainees.");
        cut.Find("#epa-sub-speciality").GetAttribute("aria-describedby").Should().Be("epa-sub-speciality-help");
        Text(cut.Find("#epa-sub-speciality-help")).Should().Be("The disciplines your institution has adopted.");
        cut.FindAll("#epa-sub-speciality option").Select(Text).Should().Equal(Paediatrics);
        cut.FindAll("#epa-active").Should().BeEmpty();
        cut.FindAll("button").Where(button => button.Closest("dialog") is null).Select(Text).Should().Equal("Save");
    }

    [Fact]
    public void ACreate_LeavesForItsPage_ByTheCreatedToken()
    {
        var sender = new Sender { Page = new EpaPageDto(null, [new EpaDisciplineDto(2, Paediatrics)], true, Kgk, true) };
        var cut = Render(sender, null);

        cut.Find("#epa-code").Change("KGK-001");
        cut.Find("#epa-title").Change("Outreach");
        cut.Find("form").Submit();

        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/epas/17?created");
    }

    [Fact]
    public void ArrivingByCreated_WritesEpaSaved_FocusesIt_AndDropsTheToken()
    {
        Services.AddSingleton<IScopedSender>(new Sender { Page = Page(Own()) });
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin/epas/17?created");
        var cut = RenderComponent<EpaEdit>(parameters => parameters.Add(page => page.Id, 17));

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-success")).Should().Be("EPA saved."));
        cut.WaitForAssertion(() => navigation.Uri.Should().EndWith("/admin/epas/17"));
        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public void ARealDuplicate_IsRefusedUnderCode_Focused_WhatWasTypedKept()
    {
        var sender = new Sender
        {
            Page = Page(Own()),
            UpdateFailure = new FieldRefusalException("Code", "An EPA with the same code already exists for this sub-speciality.")
        };
        var cut = Render(sender, 17);

        cut.Find("#epa-code").Change("PAED-001");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#epa-code-refusal")).Should().Be("An EPA with the same code already exists for this sub-speciality."));
        cut.Find("#epa-code-refusal").GetAttribute("role").Should().Be("alert");
        cut.Find("#epa-code").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#epa-code").GetAttribute("aria-describedby").Should().Be("epa-code-refusal");
        cut.Find("#epa-code").GetAttribute("value").Should().Be("PAED-001");
        cut.FindAll(".action-result .alert").Should().BeEmpty();
        JSInterop.Invocations.Should().Contain(invocation => invocation.Arguments.Contains("epa-code"));
    }

    [Fact]
    public void AnUnadoptedDiscipline_IsRefusedUnderSubSpeciality_BeforeItsHelp()
    {
        var sender = new Sender
        {
            Page = new EpaPageDto(null, [new EpaDisciplineDto(2, Paediatrics)], true, Kgk, true),
            CreateFailure = new FieldRefusalException("SubSpecialityId", "Your institution has not adopted Paediatrics / Neonatology, so an EPA cannot be added to it.")
        };
        var cut = Render(sender, null);

        cut.Find("#epa-code").Change("KGK-001");
        cut.Find("#epa-title").Change("Outreach");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.Find("#epa-sub-speciality").GetAttribute("aria-describedby")
            .Should().Be("epa-sub-speciality-refusal epa-sub-speciality-help"));
        Text(cut.Find("#epa-sub-speciality-refusal")).Should().Be(
            "Your institution has not adopted Paediatrics / Neonatology, so an EPA cannot be added to it.");
    }

    [Fact]
    public void AnEmptyCode_IsRefusedUnderIt_AndNothingIsSent()
    {
        var sender = new Sender { Page = Page(Own()) };
        var cut = Render(sender, 17);

        cut.Find("#epa-code").Change("");
        cut.Find("form").Submit();

        Text(cut.Find("#epa-code-refusal")).Should().Be("Enter a code.");
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void AnyOtherFailure_IsTheFixedSentence_NeverTheExceptionsText()
    {
        var sender = new Sender { Page = Page(Own()), UpdateFailure = new InvalidOperationException("Npgsql exploded") };
        var cut = Render(sender, 17);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-danger"))
            .Should().Be("The EPA could not be saved. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Fact]
    public void WhileItIsRead_ItIsTitledEpa_AndSaysSo()
    {
        var cut = RenderPending(new Sender { HeldPage = new TaskCompletionSource<EpaPageDto>() }, 17);

        Text(cut.Find("h1")).Should().Be("EPA");
        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading this EPA.");
        cut.FindAll("form").Where(form => form.Closest("dialog") is null).Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsTitledEpa_InFixedWords_WithTryAgain()
    {
        var cut = RenderPending(new Sender { PageFailure = new InvalidOperationException("Npgsql exploded") }, 17);

        cut.WaitForAssertion(() => Text(cut.Find(".alert-row-text"))
            .Should().Be("Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes."));
        Text(cut.Find("h1")).Should().Be("EPA");
        cut.FindAll("button").Should().Contain(button => Text(button) == "Try again");
        cut.Markup.Should().NotContain("Npgsql");
    }

    private IRenderedComponent<EpaEdit> Render(Sender sender, int? id)
    {
        var cut = RenderPending(sender, id);
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private IRenderedComponent<EpaEdit> RenderPending(Sender sender, int? id)
    {
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<EpaEdit>(parameters => parameters.Add(page => page.Id, id));
    }

    private static EpaPageDto Page(EpaDto epa) => new(epa, [new EpaDisciplineDto(2, Paediatrics)], true, Kgk, true);

    private static EpaDto Own()
        => new EpaDto(17, 2, "Paediatrics", "College of Paediatricians of South Africa", "KGK-001",
            "Running a paediatric outreach clinic at a district hospital", null, null, EpaCategory.Elective, true, DateTime.UnixEpoch)
        {
            OwningInstitutionId = 3,
            OwningInstitutionName = Kgk,
            MayChange = true,
            DisciplineName = Paediatrics
        };

    private static EpaDto National(bool mayChange)
        => new EpaDto(13, 2, "Paediatrics", "College of Paediatricians of South Africa", "PAED-012",
            "Communicating with and counselling patients, caregivers and healthcare teams", "Sharing clear health information.",
            "12.1 Communicate\n12.2 Counsel", EpaCategory.Core, true, DateTime.UnixEpoch)
        {
            MayChange = mayChange,
            DisciplineName = Paediatrics
        };

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        public EpaPageDto? Page { get; init; }
        public TaskCompletionSource<EpaPageDto>? HeldPage { get; init; }
        public Exception? PageFailure { get; init; }
        public Exception? UpdateFailure { get; init; }
        public Exception? CreateFailure { get; init; }
        public List<object> Commands { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetEpaPageQuery when PageFailure is not null:
                    return Task.FromException<TResponse>(PageFailure);
                case GetEpaPageQuery when HeldPage is not null:
                    return HeldPage.Task.ContinueWith(task => (TResponse)(object)task.Result, cancellationToken);
                case GetEpaPageQuery:
                    return Task.FromResult((TResponse)(object)Page!);
                case UpdateEpaCommand command:
                    Commands.Add(command);
                    return UpdateFailure is not null
                        ? Task.FromException<TResponse>(UpdateFailure)
                        : Task.FromResult((TResponse)(object)new UpdateEpaResult(Page!.Epa!, 0));
                case CreateEpaCommand command:
                    Commands.Add(command);
                    return CreateFailure is not null
                        ? Task.FromException<TResponse>(CreateFailure)
                        : Task.FromResult((TResponse)(object)(Own() with { Id = 17 }));
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
