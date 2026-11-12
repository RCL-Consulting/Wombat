using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Audit;
using Wombat.Application.Features.Audit.Queries.GetAuditEntryById;
using Wombat.Domain.Audit;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Audit;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// An audit entry (T400, flow 18, lane C; boards R3-E02–E09, R2-E01–E11): the four h2 sections, no Payload for an
/// institution's administrator, the laid-out payload in its own region named once, the address sentence by its prefix,
/// the trail's way back to the rows, Unavailable, loading and a failed read in fixed words.
/// </summary>
public sealed class AuditEntryPageTests : WombatTestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";
    private const string Secret = "Npgsql: the connection string was rejected.";
    private static readonly Guid EntryId = Guid.Parse("019a6f2e-0000-7000-8000-000000000013");

    // 2026-11-10 19:10:20.376 SAST.
    private static readonly DateTime At = new(2026, 11, 10, 17, 10, 20, 376, DateTimeKind.Utc);

    private const string StoredPayload =
        """{"note":"[REDACTED]","principal":"[PRINCIPAL]","activityId":13,"actorUserId":"535c41ed","dataPatchJson":"[REDACTED]","transitionKey":"complete"}""";

    private Func<object, object?> _answer = _ => null;

    [Fact]
    public void E03_TheAdministrator_ReadsFourSections_AndThePayloadLaidOutInItsRegion()
    {
        _answer = _ => Naidoo();

        var cut = Render(WombatRoles.Administrator);

        Text(cut.Find("h1")).Should().Be("Audit entry");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "TransitionActivityCommand by David Naidoo, 2026-11-10 19:10:20 SAST · Success");
        cut.Find(".user-page").Should().NotBeNull();
        cut.FindAll(".user-page section h2").Select(Text).Should().Equal("What happened", "Who", "What it was done to", "Payload");

        var happened = Section(cut, "What happened");
        Terms(happened).Should().Equal("When", "Category", "Action", "Institution", "Result");
        Text(Value(happened, "When")).Should().Be("2026-11-10 19:10:20.376 SASTStored as 2026-11-10 17:10:20.376 UTC");
        Text(Value(happened, "Category")).Should().Be("Command");
        Text(Value(happened, "Institution")).Should().Be(Kgk);
        Text(Value(happened, "Result")).Should().Be("Success");

        var who = Section(cut, "Who");
        Text(Value(who, "Name")).Should().Be("David Naidoo");
        Text(Value(who, "User id")).Should().Be("535c41ed");
        Text(Value(who, "Address")).Should().Be("::/48Cut to its first 48 bits when stored.");

        Text(Section(cut, "What it was done to").QuerySelector("p")!).Should().Be("No subject");

        var payload = Section(cut, "Payload");
        payload.HasAttribute("aria-labelledby").Should().BeFalse("the section carries no name; its region does (S4)");
        payload.HasAttribute("aria-label").Should().BeFalse();
        Text(payload.QuerySelector("p")!).Should().Be(
            "As stored, laid out one property a line: secrets read \"[REDACTED]\" and the signed-in person \"[PRINCIPAL]\".");
        var region = payload.QuerySelector("pre")!;
        region.ClassList.Should().Contain(["code-block", "code-block--region"]);
        region.GetAttribute("tabindex").Should().Be("0");
        region.GetAttribute("role").Should().Be("region");
        region.GetAttribute("aria-labelledby").Should().Be(payload.QuerySelector("h2")!.Id);
        region.TextContent.Replace("\r\n", "\n").Should().Be(
            """
            {
              "note": "[REDACTED]",
              "principal": "[PRINCIPAL]",
              "activityId": 13,
              "actorUserId": "535c41ed",
              "dataPatchJson": "[REDACTED]",
              "transitionKey": "complete"
            }
            """.Replace("\r\n", "\n"));

        cut.Markup.Should().NotContain("Back to log").And.NotContain("Display name").And.NotContain("(UTC)");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void R2E01_AnInstitutionsAdministrator_ReadsNoPayload_AndAFailurePutsItsWordsUnderTheBadge()
    {
        _answer = _ => Naidoo() with
        {
            Action = "CreateActivityCommand",
            PersonName = "Sipho Ndlovu",
            Success = false,
            ErrorMessage = "Date observed: The date cannot be after today (2026-11-10).",
            SummaryJson = null,
        };

        var cut = Render(WombatRoles.InstitutionalAdmin);

        cut.FindAll(".user-page section h2").Select(Text).Should().Equal("What happened", "Who", "What it was done to");
        cut.FindAll("pre").Should().BeEmpty();
        Text(cut.Find(".page-subtitle")).Should().EndWith("· Failed");
        Text(Value(Section(cut, "What happened"), "Result")).Should().Be(
            "FailedDate observed: The date cannot be after today (2026-11-10).");
    }

    [Fact]
    public void E02_TheRespondentsSubmission_SaysThereIsNoPerson()
    {
        _answer = _ => Naidoo() with
        {
            Action = "SubmitMsfResponseCommand",
            ActorKind = AuditActorKind.MsfRespondent,
            PersonName = null,
            ActorUserId = null,
            InstitutionId = null,
            InstitutionName = null,
        };

        var cut = Render(WombatRoles.Administrator);

        Text(cut.Find(".page-subtitle")).Should().StartWith("SubmitMsfResponseCommand by MSF respondent (no account), ");
        var who = Section(cut, "Who");
        Text(Value(who, "Name")).Should().Be("No person: an MSF respondent answers by link, with no account.");
        Text(Value(who, "User id")).Should().Be("None");
        Text(Value(Section(cut, "What happened"), "Institution")).Should().Be("No institution");
    }

    [Fact]
    public void ARowWithNobodySignedIn_SaysSo()
    {
        _answer = _ => Naidoo() with { Action = "LoginFailed", ActorKind = AuditActorKind.None, PersonName = null, ActorUserId = null, ActorIpAddress = null };

        var cut = Render(WombatRoles.Administrator);

        Text(Value(Section(cut, "Who"), "Name")).Should().Be("No signed-in person");
        Text(Value(Section(cut, "Who"), "Address")).Should().Be("None");
    }

    [Theory]
    [InlineData("10.0.0.0/24", "10.0.0.0/24Cut to its first 24 bits when stored.")]
    [InlineData("2001:db8:85a3::/48", "2001:db8:85a3::/48Cut to its first 48 bits when stored.")]
    [InlineData("127.0.0.1", "127.0.0.1")]
    public void K9_TheAddress_SaysHowMuchOfItWasKept_ByItsPrefix(string address, string expected)
    {
        _answer = _ => Naidoo() with { ActorIpAddress = address };

        var cut = Render(WombatRoles.Administrator);

        Text(Value(Section(cut, "Who"), "Address")).Should().Be(expected);
    }

    [Fact]
    public void E07_ASubject_IsReadWithItsInstitutionAndSpecialityByName()
    {
        var subject = Guid.NewGuid();
        _answer = _ => Naidoo() with { SubjectType = "CommitteeReview", SubjectId = subject, SpecialityId = 4, SpecialityName = "Paediatrics" };

        var cut = Render(WombatRoles.Administrator);

        var done = Section(cut, "What it was done to");
        Terms(done).Should().Equal("Type", "Id", "Institution", "Speciality");
        Text(Value(done, "Type")).Should().Be("CommitteeReview");
        Text(Value(done, "Id")).Should().Be(subject.ToString());
        Text(Value(done, "Institution")).Should().Be(Kgk);
        Text(Value(done, "Speciality")).Should().Be("Paediatrics");
    }

    [Fact]
    public void R2E08_AnUnknownOrForeignEntry_IsUnavailable_WithTheWayBackToTheRows()
    {
        _answer = _ => null;
        const string back = "/admin/audit?result=failure&shown=2026-11-10T18%3A00%3A00.0000000Z";

        var cut = Render(WombatRoles.InstitutionalAdmin, back: back);

        Text(cut.Find("h1")).Should().Be("Audit entry unavailable");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be("This audit entry does not exist, or you cannot open it.");
        var go = cut.Find(".detail-card--empty a.btn-primary");
        Text(go).Should().Be("Go to Audit log");
        go.GetAttribute("href").Should().Be(back);
        Crumbs(cut).Should().Equal("Home", "Audit log", "Audit entry unavailable");
        cut.Find("nav[aria-label=Breadcrumb] li.breadcrumb-parent a").GetAttribute("href").Should().Be(back);
        cut.Markup.Should().NotContain(EntryId.ToString(), "never which id, nor why");
    }

    [Theory]
    [InlineData(
        "/admin/audit?result=failure&shown=2026-11-10T18%3A00%3A00.0000000Z&page=2",
        "/admin/audit?result=failure&shown=2026-11-10T18%3A00%3A00.0000000Z&page=2")]
    [InlineData("https://evil.example/admin/audit", "/admin/audit")]
    [InlineData("/admin/audit?next=https%3A%2F%2Fevil.example", "/admin/audit")]
    [InlineData(null, "/admin/audit")]
    public void T277_TheTrailsAuditLog_ReturnsToTheRowsTheEntryWasOpenedFrom_AndNoOtherPlace(string? back, string expected)
    {
        _answer = _ => Naidoo();

        var cut = Render(WombatRoles.InstitutionalAdmin, back: back);

        Crumbs(cut).Should().Equal("Home", "Audit log", "Audit entry");
        cut.Find("nav[aria-label=Breadcrumb] li.breadcrumb-parent a").GetAttribute("href").Should().Be(
            $"{expected}{(expected.Contains('?') ? '&' : '?')}entry={EntryId}",
            "the entry is named, so its Detail takes the focus back on the list (R3-L13)");
    }

    [Fact]
    public void R2E10_WhileLoading_TheLineSaysWhat()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _answer = _ => Naidoo();

        var cut = Render(WombatRoles.Administrator, gate: gate.Task, wait: false);

        Text(cut.Find("h1")).Should().Be("Audit entry");
        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading this audit entry.");
        gate.SetResult();
        cut.WaitForState(() => cut.FindAll(".user-page section").Count > 0, AsyncWorkTimeout);
    }

    [Fact]
    public void R2E11_AFailedRead_IsTheHouseSentence_WithTryAgain_NeverTheExceptionsText()
    {
        _answer = _ => new InvalidOperationException(Secret);

        var cut = Render(WombatRoles.Administrator, waitFor: "Could not load");

        Text(cut.Find("h1")).Should().Be("Audit entry");
        Text(cut.Find(".alert-row-text")).Should().Be(
            "Could not load this audit entry. Nothing has changed. Try again, or come back in a few minutes.");
        cut.FindAll(".alert-row button").Select(Text).Should().Equal("Try again");
        cut.Markup.Should().NotContain(Secret);
    }

    [Fact]
    public void APayloadThatDoesNotParse_IsShownAsStored()
        => AuditWords.LaidOut("not json {").Should().Be("not json {");

    [Fact]
    public void ThePayload_KeepsItsWordsAsStored_NotEscaped()
        => AuditWords.LaidOut("""{"name":"O'Brien é"}""").Replace("\r\n", "\n").Should().Be("{\n  \"name\": \"O'Brien é\"\n}");

    // ─── helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<AuditDetail> Render(
        string role, string? back = null, Task? gate = null, bool wait = true, string? waitFor = null)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{role}@test");
        auth.SetRoles(role);
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, $"{role}-1") };
        if (role != WombatRoles.Administrator)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, "1"));
        }

        auth.SetClaims(claims.ToArray());
        Services.AddSingleton<IScopedSender>(new Sender(() => _answer, gate));
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(AuditLinks.Entry(EntryId, back));

        var acting = ActingRoleResolver.Resolve(role, [role]);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "Test"));
        var cut = RenderComponent<AuditDetail>(parameters => parameters
            .Add(page => page.Id, EntryId)
            .AddCascadingValue(acting)
            .AddCascadingValue(NavItems.For(acting, user))
            .AddCascadingValue(new RouteData(typeof(AuditDetail), new Dictionary<string, object?> { ["Id"] = EntryId })));
        if (wait)
        {
            cut.WaitForState(
                () => waitFor is null ? cut.FindAll(".loading-line").Count == 0 : cut.Markup.Contains(waitFor),
                AsyncWorkTimeout);
        }

        return cut;
    }

    private static AuditEntryDetailDto Naidoo()
        => new(
            EntryId, At, AuditCategory.Command, "TransitionActivityCommand", true, null, AuditActorKind.Person,
            "David Naidoo", "535c41ed", "::/48", 1, Kgk, null, null, null, null, StoredPayload);

    private static IElement Section(IRenderedFragment cut, string heading)
        => cut.FindAll(".user-page section").Single(section => Text(section.QuerySelector("h2")!) == heading);

    private static IReadOnlyList<string> Terms(IElement section) => section.QuerySelectorAll("dt").Select(Text).ToList();

    private static IElement Value(IElement section, string term)
        => section.QuerySelectorAll("dt").Single(dt => Text(dt) == term).NextElementSibling!;

    private static IReadOnlyList<string> Crumbs(IRenderedFragment cut)
        => cut.FindAll("nav[aria-label=Breadcrumb] li").Select(Text).ToList();

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender(Func<Func<object, object?>> answer, Task? gate) : IScopedSender
    {
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (gate is not null)
            {
                await gate;
            }

            var reply = answer()(request);
            if (reply is Exception exception)
            {
                throw exception;
            }

            return reply is null ? default! : (TResponse)reply;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
