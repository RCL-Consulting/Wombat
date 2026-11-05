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
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T394 (flow 16, lane C): a curriculum's page as R3-D01–D14, D18–D20 and R2-D15–D17 draw it. One column of h2 sections
/// (Q4), titled by the version (T2), the sub-speciality a read-out once fixed (Q19), "Open for adoption" with its help and the
/// reach (T301, Q2), Hold back's question (T6, N20), the one-shot results (Q5), T7's save results, the refusals under their
/// fields (T324), decision 5's refusal, Unavailable (Q7), and never an exception's text (T393).
/// </summary>
public sealed class CurriculumPageTests : TestContext
{
    private const string Cpsa = "College of Paediatricians of South Africa";
    private const string Kgk = "Kgosi Kgari Teaching Hospital";
    private const string ReachKgk = "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.";

    public CurriculumPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("kruger@test");
        auth.SetRoles(WombatRoles.CollegeAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "kruger"));
        JSInterop.Mode = JSRuntimeMode.Loose;
        // N6: 22:30 UTC on 1 November is already 2 November in South Africa.
        Services.AddSingleton<TimeProvider>(new FixedClock(new DateTimeOffset(2026, 11, 1, 22, 30, 0, TimeSpan.Zero)));
    }

    // ---- Create curriculum (R3-D01–D04) ----

    [Fact]
    public void Create_IsOneSection_WithTheRequiredLine_TheSouthAfricanDay_AndNoBackLink()
    {
        var cut = Render(new Sender(), null);

        Text(cut.Find("h1")).Should().Be("Create curriculum");
        Text(cut.Find(".header-container .page-subtitle")).Should().Be("A new version starts held back: no institution can adopt it until you open it.");
        cut.Find(".user-page.curriculum-page").Should().NotBeNull();
        cut.FindAll("main h2, .curriculum-page h2").Select(Text).Should().Equal("Details");
        Text(cut.Find(".curriculum-page p.field-help")).Should().Be("* marks a field you must fill in.");
        cut.FindAll("#curriculum-sub-speciality option").Select(Text).Should().Equal("Paediatrics / Neonatology", "Paediatrics / Paediatrics");
        cut.Find("#curriculum-name").GetAttribute("aria-required").Should().Be("true");
        cut.Find("#curriculum-version").GetAttribute("aria-required").Should().Be("true");
        cut.Find("#curriculum-effective-from").GetAttribute("value").Should().Be("2026-11-02");
        cut.FindAll("#curriculum-open").Should().BeEmpty("a new version starts held back; no box until it exists");
        Buttons(cut).Should().Equal("Create curriculum");
        cut.FindAll("a").Select(Text).Should().NotContain(["Back to curricula", "Manage items"]);
        cut.FindAll(".validation-summary-errors").Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNameAndVersionEmpty_RefusesEachUnderItsField_FocusesName_AndSendsNothing()
    {
        var sender = new Sender();
        var cut = Render(sender, null);

        cut.Find("form").Submit();

        Text(cut.Find("#curriculum-name-refusal")).Should().Be("Enter a name.");
        Text(cut.Find("#curriculum-version-refusal")).Should().Be("Enter a version.");
        cut.Find("#curriculum-name").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#curriculum-name").GetAttribute("aria-describedby").Should().Be("curriculum-name-refusal");
        sender.Commands.Should().BeEmpty();
        JSInterop.Invocations.Should().Contain(invocation => invocation.Arguments.Contains("curriculum-name"));
    }

    [Fact]
    public void Create_LeavesForItsPage_ByTheCreatedToken()
    {
        var cut = Render(new Sender(), null);

        Type(cut, "Neonatology EPA Curriculum", "1.0");
        cut.Find("form").Submit();

        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/curricula/3002?created");
    }

    [Fact]
    public void ARealDuplicate_IsRefusedUnderVersion_WhatWasTypedKept()
    {
        var sender = new Sender { CreateFailure = new FieldRefusalException("Version", CurriculumFieldMessages.Duplicate) };
        var cut = Render(sender, null);

        Type(cut, "Neonatology EPA Curriculum", "1.0");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#curriculum-version-refusal"))
            .Should().Be("A curriculum with the same name and version already exists for this sub-speciality."));
        cut.Find("#curriculum-version").GetAttribute("value").Should().Be("1.0");
        cut.FindAll(".action-result .alert").Should().BeEmpty();
    }

    [Fact]
    public void AFailedCreate_IsTheFixedSentence_NeverTheExceptionsText()
    {
        var cut = Render(new Sender { CreateFailure = new InvalidOperationException("Npgsql exploded") }, null);

        Type(cut, "Neonatology EPA Curriculum", "1.0");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-danger")).Should().Be(
            "The curriculum could not be created. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Fact]
    public void AnEndBeforeTheStart_IsRefusedUnderEffectiveTo()
    {
        var cut = Render(new Sender(), null);

        Type(cut, "Neonatology EPA Curriculum", "1.0");
        cut.Find("#curriculum-effective-to").Change("2026-10-01");
        cut.Find("form").Submit();

        Text(cut.Find("#curriculum-effective-to-refusal")).Should().Be("Effective to must be on or after Effective from.");
    }

    // ---- A version's page (R3-D05–D08, D18) ----

    [Fact]
    public void ArrivingByCreated_SaysItIsHeldBack_FocusesIt_DropsTheToken_AndTheSelectStays()
    {
        var sender = new Sender();
        sender.Pages[3002] = Neonatology();
        var cut = RenderAt(sender, 3002, "/admin/curricula/3002?created");

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-success")).Should().Be(
            "Neonatology EPA Curriculum (1.0) created. It is held back: no institution can adopt it until you open it."));
        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/curricula/3002"));
        Text(cut.Find("h1")).Should().Be("Neonatology EPA Curriculum (1.0)");
        Text(cut.Find(".header-container .page-subtitle")).Should().Be($"{Cpsa} · Paediatrics / Neonatology · effective 2026-11-02");
        cut.FindAll("#curriculum-sub-speciality").Should().ContainSingle("an empty, unadopted version's sub-speciality may still change");
        cut.Find("#curriculum-open").HasAttribute("checked").Should().BeFalse();
        Text(cut.Find("#curriculum-open-help").ParentElement!.QuerySelector("p.muted")!).Should().Be("No institution has adopted this version yet.");
        Text(cut.Find("#curriculum-new-version-h + p")).Should().Be(
            "This version has no national items yet, so a new version starts with none. It starts held back.");
        cut.Find("#clone-version").GetAttribute("value").Should().Be("1.0-copy");
        cut.Find("#clone-effective-from").GetAttribute("value").Should().Be("2026-11-03");
        Buttons(cut).Should().Equal("Save", "Clone", "Cancel", "Hold back");
        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public void AnOpenAdoptedVersion_ReadsOutItsSubSpeciality_TellsItsReach_AndOffersHoldBack()
    {
        var cut = Render(Sender.With(Paediatric111()), 3000);

        Text(cut.Find("h1")).Should().Be("Paediatric EPA Curriculum (11.1)");
        var items = cut.Find(".header-container .actions-cell a");
        Text(items).Should().Be("Items (15)");
        items.GetAttribute("aria-label").Should().Be("Items (15) of Paediatric EPA Curriculum (11.1)");
        items.GetAttribute("href").Should().Be("/admin/curricula/3000/items");
        cut.FindAll(".curriculum-page h2").Where(h => h.Closest("dialog") is null).Select(Text)
            .Should().Equal("Details", "Open for adoption", "A new version");
        cut.FindAll("#curriculum-sub-speciality").Should().BeEmpty("Q19: a version with items is fixed");
        Text(cut.Find("dl.detail-list dt")).Should().Be("Sub-speciality");
        Text(cut.Find("dl.detail-list dd")).Should().Be("Paediatrics / Paediatrics");
        Text(cut.Find("dl.detail-list + p")).Should().Be(
            "Fixed once a version holds an item or has been adopted. To use another sub-speciality, create a curriculum there.");
        cut.Find("#curriculum-open").HasAttribute("checked").Should().BeTrue();
        cut.Find("#curriculum-open").GetAttribute("aria-describedby").Should().Be("curriculum-open-help");
        Text(cut.Find("#curriculum-open-help")).Should().Be(
            "An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it.");
        Text(cut.Find("#curriculum-open-help + p.muted")).Should().Be(ReachKgk);
        cut.Find("#curriculum-open-help + p.muted").HasAttribute("role").Should().BeFalse("a standing notice is not a live region");
        cut.Find("#curriculum-hold-back").GetAttribute("aria-haspopup").Should().Be("dialog");
        Text(cut.Find("#curriculum-hold-back")).Should().Be("Hold back…");
        cut.Find("#curriculum-save").HasAttribute("aria-haspopup").Should().BeFalse("a Save that keeps it open asks nothing");
        Text(cut.Find("#curriculum-new-version-h + p")).Should().Be(
            "A new version copies this version's 15 national items, with their targets, minimum levels and tools. An institution's own items are never copied. It starts held back.");
        cut.Find("#clone-version").GetAttribute("value").Should().Be("11.1-copy");
        cut.Find("#clone-effective-from").GetAttribute("value").Should().Be("2026-01-02");
        cut.Find("#clone-version").Closest("form")!.QuerySelector("button")!.GetAttribute("aria-label")
            .Should().Be("Clone Paediatric EPA Curriculum (11.1) as a new version");
        cut.Markup.Should().NotContain("Lerato", "the reach counts registrars, never names one");
    }

    [Fact]
    public void HoldBack_AsksFirst_NamingWhoAdoptedIt_ThenSendsTheStoredVersionHeldBack()
    {
        var sender = Sender.With(Paediatric111());
        var cut = Render(sender, 3000);

        Text(cut.Find("dialog h2")).Should().Be("Hold back Paediatric EPA Curriculum (11.1)?");
        cut.FindAll("dialog .dialog-body p").Select(Text).Should().Equal(
            "No institution can adopt it while it is held back.",
            $"{ReachKgk} Holding it back changes nothing for them: Kgosi Kgari Teaching Hospital keeps it, and its registrars stay on it.",
            "You can open it again on this page.");
        cut.FindAll("dialog button").Select(Text).Should().Equal("Cancel", "Hold back");

        cut.Find("#curriculum-name").Change("Edited, not saved");
        cut.Find("#curriculum-hold-back").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Hold back").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-success")).Should().Be(
            "Curriculum saved. It is held back: no institution can adopt it until you open it again."));
        var sent = sender.Commands.OfType<UpdateCurriculumCommand>().Single();
        sent.IsActive.Should().BeFalse();
        sent.Name.Should().Be("Paediatric EPA Curriculum", "Hold back… changes the box alone");
        cut.FindAll("#curriculum-hold-back").Should().BeEmpty("a held-back version is opened by ticking the box");
        cut.Find("#curriculum-name").GetAttribute("value").Should().Be("Edited, not saved");
    }

    [Fact]
    public void UntickingTheBox_MakesSaveAsk_AndCancelSendsNothing()
    {
        var sender = Sender.With(Paediatric111());
        var cut = Render(sender, 3000);

        cut.Find("#curriculum-open").Change(false);
        cut.Find("#curriculum-save").GetAttribute("aria-haspopup").Should().Be("dialog");
        cut.Find("form").Submit();
        cut.FindAll("dialog button").Single(button => Text(button) == "Cancel").Click();

        sender.Commands.Should().BeEmpty();
        cut.Find("#curriculum-open").HasAttribute("checked").Should().BeFalse("the box stays unticked, unsaved");
    }

    [Fact]
    public void OpeningAHeldBackVersion_AsksNothing_SaysSo_AndOffersHoldBack()
    {
        var held = Paediatric112() with { IsActive = false };
        var sender = Sender.With(held);
        sender.AfterSave = Paediatric112();
        var cut = Render(sender, 3001);

        cut.FindAll("#curriculum-hold-back").Should().BeEmpty();
        cut.Find("#curriculum-open").Change(true);
        cut.Find("#curriculum-save").HasAttribute("aria-haspopup").Should().BeFalse("opening asks nothing (Q4)");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-success")).Should().Be(
            "Curriculum saved. It is open for adoption: an institution can adopt it now."));
        sender.Commands.OfType<UpdateCurriculumCommand>().Single().IsActive.Should().BeTrue();
        cut.FindAll("#curriculum-hold-back").Should().ContainSingle();
    }

    [Fact]
    public void ArrivingByCloned_NamesTheSource_AndTheNationalItems_FocusesIt_AndDropsTheToken()
    {
        var sender = Sender.With(Paediatric111());
        sender.Pages[3001] = Paediatric112() with { IsActive = false, NationalItemCount = 15, Reach = CurriculumReachDto.None };
        var cut = RenderAt(sender, 3001, "/admin/curricula/3001?cloned=3000");

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-success")).Should().Be(
            "Paediatric EPA Curriculum (11.2) cloned from 11.1, with its 15 national items. It is held back."));
        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/curricula/3001"));
        cut.FindAll("#curriculum-sub-speciality").Should().BeEmpty("11.2 holds items");
    }

    [Theory]
    [InlineData(9999)]
    [InlineData(3002)]
    public void AStaleOrForeignClonedToken_WritesNothing(int fromId)
    {
        var sender = Sender.With(Paediatric112());
        sender.Pages[3002] = Neonatology();
        var cut = RenderAt(sender, 3001, $"/admin/curricula/3001?cloned={fromId}");

        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/curricula/3001"));
        cut.FindAll(".action-result .alert").Should().BeEmpty();
    }

    [Fact]
    public void Clone_LeavesForTheNewVersion_ByTheClonedToken()
    {
        var cut = Render(Sender.With(Paediatric111()), 3000);

        cut.Find("#clone-version").Change("11.2");
        cut.Find("#clone-version").Closest("form")!.Submit();

        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/curricula/3001?cloned=3000");
    }

    [Fact]
    public void Clone_WithNoNewVersion_IsRefusedUnderIt_AndSendsNothing()
    {
        var sender = Sender.With(Paediatric111());
        var cut = Render(sender, 3000);

        cut.Find("#clone-version").Change("");
        cut.Find("#clone-version").Closest("form")!.Submit();

        Text(cut.Find("#clone-version-refusal")).Should().Be("Enter the new version.");
        cut.Find("#clone-version").GetAttribute("aria-invalid").Should().Be("true");
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void Clone_IntoATakenVersion_IsRefusedUnderNewVersion()
    {
        var sender = Sender.With(Paediatric111());
        sender.CloneFailure = new FieldRefusalException("Version", CurriculumFieldMessages.Duplicate);
        var cut = Render(sender, 3000);

        cut.Find("#clone-version").Change("11.2");
        cut.Find("#clone-version").Closest("form")!.Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#clone-version-refusal")).Should().Be(CurriculumFieldMessages.Duplicate));
        cut.FindAll("#curriculum-version-refusal").Should().BeEmpty();
    }

    // ---- Q19's refusal (R3-D20, decision 5) ----

    [Fact]
    public void ASubSpecialityFoundFixedOnSave_IsRefusedByItsCause_AndThePageIsReadAgain()
    {
        var sender = Sender.With(Neonatology());
        sender.UpdateFailure = new CurriculumSubSpecialityFixedException(CurriculumSubSpecialityFixedCause.Adopted, [Kgk]);
        sender.AfterRefusal = Neonatology() with { SubSpecialityFixed = true };
        var cut = Render(sender, 3002);

        cut.Find("#curriculum-sub-speciality").Change("1000");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-danger")).Should().Be(
            "Not saved. Kgosi Kgari Teaching Hospital has adopted this version, so its sub-speciality can no longer change. The page has been read again."));
        cut.FindAll("#curriculum-sub-speciality").Should().BeEmpty("read again, the sub-speciality is its read-out");
        Text(cut.Find("dl.detail-list dd")).Should().Be("Paediatrics / Neonatology");
    }

    [Fact]
    public void TheRefusalsWords_NameTheCause()
    {
        CurriculumWords.SubSpecialityFixed(new(CurriculumSubSpecialityFixedCause.HoldsItems, [])).Should().Be(
            "Not saved. This version now holds items, so its sub-speciality can no longer change. The page has been read again.");
        CurriculumWords.SubSpecialityFixed(new(CurriculumSubSpecialityFixedCause.Adopted, [Kgk, "Other Hospital"])).Should().Be(
            "Not saved. Kgosi Kgari Teaching Hospital and Other Hospital have adopted this version, so its sub-speciality can no longer change. The page has been read again.");
        CurriculumWords.SubSpecialityFixed(new(CurriculumSubSpecialityFixedCause.Adopted, ["A", "B", "C"])).Should().StartWith(
            "Not saved. A, B and 1 other institution have adopted this version,");
        CurriculumWords.SubSpecialityFixed(new(CurriculumSubSpecialityFixedCause.Adopted, ["A", "B", "C", "D"])).Should().StartWith(
            "Not saved. A, B and 2 other institutions have adopted this version,");
    }

    [Fact]
    public void TheWordsHaveTheirSingularsAndNones()
    {
        CurriculumWords.CloneIntro(1).Should().Be(
            "A new version copies this version's 1 national item, with its target, minimum level and tools. An institution's own items are never copied. It starts held back.");
        CurriculumWords.Cloned(Paediatric112() with { NationalItemCount = 1 }, "11.1").Should().Be(
            "Paediatric EPA Curriculum (11.2) cloned from 11.1, with its 1 national item. It is held back.");
        CurriculumWords.Cloned(Neonatology() with { Version = "1.1" }, "1.0").Should().Be(
            "Neonatology EPA Curriculum (1.1) cloned from 1.0, with no national items. It is held back.");
        CurriculumWords.HoldBackReach(CurriculumReachDto.None).Should().Be("No institution has adopted this version yet.");
        CurriculumWords.HoldBackReach(new CurriculumReachDto(["A", "B"], 5, [], 0)).Should().Be(
            "Adopted by 2 institutions, A and B; 5 registrars in training on it. Holding it back changes nothing for them: A and B keep it, and their registrars stay on it.");
        // Pages N5: one registrar, one verb.
        CurriculumWords.HoldBackReach(new CurriculumReachDto(["Kgosi Kgari Teaching Hospital"], 1, [], 0)).Should().Be(
            "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 1 registrar in training on it. Holding it back changes nothing for them: Kgosi Kgari Teaching Hospital keeps it, and its registrar stays on it.");
        CurriculumWords.HoldBackReach(new CurriculumReachDto(["Kgosi Kgari Teaching Hospital"], 0, [], 0)).Should().Be(
            "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; no registrar in training on it. Holding it back changes nothing for them: Kgosi Kgari Teaching Hospital keeps it.");
    }

    // ---- Unavailable, loading, failure (R2-D15–D17) ----

    [Fact]
    public void AVersionSheCannotOpen_IsUnavailable_NoIdShown()
    {
        var cut = Render(new Sender(), 1);

        Text(cut.Find("h1")).Should().Be("Curriculum unavailable");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be("This curriculum does not exist, or you cannot open it.");
        var go = cut.Find(".detail-card--empty a");
        Text(go).Should().Be("Go to Curricula");
        go.GetAttribute("href").Should().Be("/admin/curricula");
        cut.FindAll("form").Where(form => form.Closest("dialog") is null).Should().BeEmpty();
    }

    [Fact]
    public void WhileItIsRead_ItIsTitledCurriculum_OfferingNothing()
    {
        var sender = new Sender { Held = new TaskCompletionSource<CurriculumDto?>() };
        var cut = RenderPending(sender, 3000);

        Text(cut.Find("h1")).Should().Be("Curriculum");
        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading this curriculum.");
        cut.FindAll(".header-container .actions-cell a").Should().BeEmpty();
        cut.FindAll("button").Where(button => button.Closest("dialog") is null).Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsTitledCurriculum_InFixedWords_WithTryAgain()
    {
        var cut = RenderPending(new Sender { ReadFailure = new InvalidOperationException("Npgsql exploded") }, 3000);

        cut.WaitForAssertion(() => Text(cut.Find(".alert-row-text")).Should().Be(
            "Could not load this curriculum. Nothing has changed. Try again, or come back in a few minutes."));
        Text(cut.Find("h1")).Should().Be("Curriculum");
        cut.FindAll("button").Select(Text).Should().Contain("Try again");
        cut.FindAll("form").Where(form => form.Closest("dialog") is null).Should().BeEmpty();
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Fact]
    public void AFailedSave_IsTheFixedSentence_NeverTheExceptionsText()
    {
        var sender = Sender.With(Paediatric111());
        sender.UpdateFailure = new InvalidOperationException("Npgsql exploded");
        var cut = Render(sender, 3000);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-danger")).Should().Be(
            "The curriculum could not be saved. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Npgsql");
    }

    // ---- Fixtures ----

    private void Type(IRenderedComponent<CurriculumEdit> cut, string name, string version)
    {
        cut.Find("#curriculum-name").Change(name);
        cut.Find("#curriculum-version").Change(version);
    }

    private IRenderedComponent<CurriculumEdit> Render(Sender sender, int? id)
    {
        var cut = RenderPending(sender, id);
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private IRenderedComponent<CurriculumEdit> RenderAt(Sender sender, int id, string uri)
    {
        Services.AddSingleton<IScopedSender>(sender);
        Services.GetRequiredService<NavigationManager>().NavigateTo(uri);
        var cut = RenderComponent<CurriculumEdit>(parameters => parameters.Add(page => page.Id, id));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private IRenderedComponent<CurriculumEdit> RenderPending(Sender sender, int? id)
    {
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<CurriculumEdit>(parameters => parameters.Add(page => page.Id, id));
    }

    private static List<string> Buttons(IRenderedComponent<CurriculumEdit> cut)
        => cut.FindAll("button").Select(Text).ToList();

    private static CurriculumDto Version(int id, int subSpecialityId, string subSpeciality, string name, string version, DateOnly from, bool open)
        => new(id, 100, subSpecialityId, "Paediatrics", subSpeciality, Cpsa, name, version, from, null, open, [], null)
        {
            CanEditCurriculum = true
        };

    private static CurriculumDto Paediatric111() => Version(3000, 1000, "Paediatrics", "Paediatric EPA Curriculum", "11.1", new DateOnly(2026, 1, 1), true) with
    {
        NationalItemCount = 15,
        HasNationalEpas = true,
        SubSpecialityFixed = true,
        Reach = new CurriculumReachDto([Kgk], 3, [], 0)
    };

    private static CurriculumDto Paediatric112() => Version(3001, 1000, "Paediatrics", "Paediatric EPA Curriculum", "11.2", new DateOnly(2026, 11, 2), true) with
    {
        NationalItemCount = 16,
        HasNationalEpas = true,
        SubSpecialityFixed = true,
        Reach = CurriculumReachDto.None
    };

    private static CurriculumDto Neonatology() => Version(3002, 1001, "Neonatology", "Neonatology EPA Curriculum", "1.0", new DateOnly(2026, 11, 2), false) with
    {
        Reach = CurriculumReachDto.None
    };

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class Sender : IScopedSender
    {
        public Dictionary<int, CurriculumDto> Pages { get; } = [];
        public TaskCompletionSource<CurriculumDto?>? Held { get; init; }
        public Exception? ReadFailure { get; init; }
        public Exception? CreateFailure { get; init; }
        public Exception? UpdateFailure { get; set; }
        public Exception? CloneFailure { get; set; }

        /// <summary>What the page reads again after a save, if not itself as sent.</summary>
        public CurriculumDto? AfterSave { get; set; }

        /// <summary>What the page reads again after a refused save.</summary>
        public CurriculumDto? AfterRefusal { get; set; }

        public List<object> Commands { get; } = [];

        public static Sender With(CurriculumDto curriculum)
        {
            var sender = new Sender();
            sender.Pages[curriculum.Id] = curriculum;
            return sender;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetSpecialitiesListQuery:
                    return Result<TResponse>(new List<SpecialityDto> { new(100, 5, "Paediatrics", null, true) });
                case GetSubSpecialitiesListQuery:
                    return Result<TResponse>(new List<SubSpecialityDto>
                    {
                        new(1001, 100, "Neonatology", null, true),
                        new(1000, 100, "Paediatrics", null, true)
                    });
                case GetCurriculumByIdQuery when ReadFailure is not null:
                    return Task.FromException<TResponse>(ReadFailure);
                case GetCurriculumByIdQuery when Held is not null:
                    return Held.Task.ContinueWith(task => (TResponse)(object)task.Result!, cancellationToken);
                case GetCurriculumByIdQuery query:
                    return Result<TResponse>(Pages.GetValueOrDefault(query.Id));
                case CreateCurriculumCommand command:
                    Commands.Add(command);
                    return CreateFailure is not null ? Task.FromException<TResponse>(CreateFailure) : Result<TResponse>(Neonatology());
                case UpdateCurriculumCommand command:
                    Commands.Add(command);
                    if (UpdateFailure is not null)
                    {
                        if (AfterRefusal is not null)
                        {
                            Pages[command.Id] = AfterRefusal;
                        }

                        return Task.FromException<TResponse>(UpdateFailure);
                    }

                    var saved = (AfterSave ?? Pages[command.Id]) with { Name = command.Name, Version = command.Version, IsActive = command.IsActive };
                    Pages[command.Id] = saved;
                    return Result<TResponse>(saved with { Reach = null, NationalItemCount = 0, SubSpecialityFixed = false });
                case CloneCurriculumAsNewVersionCommand command:
                    Commands.Add(command);
                    return CloneFailure is not null ? Task.FromException<TResponse>(CloneFailure) : Result<TResponse>(Paediatric112() with { IsActive = false });
                default:
                    throw new NotSupportedException(request.GetType().Name);
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private static Task<TResponse> Result<TResponse>(object? value) => Task.FromResult((TResponse)value!);
    }
}
