using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Services;
using DecisionsPage = Wombat.Web.Components.Pages.Admin.EntrustmentDecisions.Index;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T266: the entrustment decisions list's three states are StatePanel's (DESIGN.md § Alerts, validation, empty states),
/// and a refusal is shown in its own words (<c>RefusalText</c>). Until T266 the page said "Loading…" as text, a failed
/// read left it there under the error, and every failure printed <c>exception.Message</c>, which for a validator's
/// refusal is a log line ("Validation failed: -- Reason: …"). Since T366 (flow 09, T329) a failed read is fixed words,
/// the register's empty card is "No STARs issued yet", and a refused revocation shows in its dialog.
/// </summary>
public sealed class EntrustmentDecisionsListStatesTests : TestContext
{
    private readonly FakeSender _sender = new();

    public EntrustmentDecisionsListStatesTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("instadmin@test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        Services.AddSingleton<IScopedSender>(_sender);

        // Revoke opens the register's ConfirmDialog, a native <dialog>, through the browser (T366, M5).
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void WhileTheListIsRead_ThePageShowsSkeletons_NotLoadingText()
    {
        _sender.List = new TaskCompletionSource<IReadOnlyList<EntrustmentDecisionDto>>().Task;

        var cut = RenderComponent<DecisionsPage>();

        cut.FindAll(".skeleton").Should().NotBeEmpty("StatePanel's loading state");
        // T366: the only loading words are the visually hidden status line's, never text in the list's place.
        cut.Find("p.visually-hidden[role=status]").TextContent.Should().Be("Loading the STARs.");
        cut.FindAll(".state-panel-title").Should().BeEmpty();
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsStatePanelsAlert_InFixedWords()
    {
        // T366 (T329): a read has no refusal of its own to show; any failure is fixed words, logged.
        _sender.List = Task.FromException<IReadOnlyList<EntrustmentDecisionDto>>(Refusal("Choose a status the list knows."));

        var cut = RenderComponent<DecisionsPage>();

        var alert = cut.Find(".alert.alert-danger");
        alert.QuerySelector(".alert-row-text")!.TextContent.Trim().Should().Be(DecisionsPage.LoadFailed);
        alert.GetAttribute("role").Should().Be("alert");
        cut.FindAll(".skeleton").Should().BeEmpty("a failed read is not still loading");
        cut.Markup.Should().NotContain("Validation failed").And.NotContain("Choose a status");
    }

    [Fact]
    public void AnEmptyList_IsStatePanelsEmptyCard()
    {
        _sender.List = Task.FromResult<IReadOnlyList<EntrustmentDecisionDto>>([]);

        var cut = RenderComponent<DecisionsPage>();

        // T366 (C24): none issued, with nothing to filter.
        var empty = cut.Find(".detail-card--empty");
        empty.QuerySelector(".state-panel-title")!.TextContent.Should().Be("No STARs issued yet");
        empty.QuerySelector(".state-panel-copy")!.TextContent.Should().Be("When a committee ratifies an entrustment decision, its STAR is listed here.");
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void ARefusedRevocation_IsShownInItsOwnWords_AndTheListStays()
    {
        _sender.List = Task.FromResult<IReadOnlyList<EntrustmentDecisionDto>>([Decision(7, "PAED-003")]);
        _sender.Revoke = Refusal("Say why the decision is revoked, in no more than 1000 characters.");

        var cut = RenderComponent<DecisionsPage>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Revoke").Click();
        cut.Find("#revoke-reason").Change("Concern raised.");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Revoke the STAR").Click();

        // T366 (M4): in the dialog, under the reason, which is kept.
        cut.WaitForAssertion(() => cut.Find("dialog .alert.alert-danger").TextContent.Trim()
            .Should().Be("Say why the decision is revoked, in no more than 1000 characters."));
        cut.Find("#revoke-reason").GetAttribute("value").Should().Be("Concern raised.");
        cut.Markup.Should().NotContain("Validation failed");
        cut.FindAll("tbody tr").Should().HaveCount(1, "a refused action leaves the list in view");
    }

    private static ValidationException Refusal(string message)
        => new([new ValidationFailure("Field", message)]);

    private static EntrustmentDecisionDto Decision(int id, string code)
        => new(
            id, $"trainee-{id}", id, code, $"EPA {code}", true, 13, "3a", 3, new DateOnly(2026, 7, 2), null, 30, "chair-1",
            "Consistent across the window.", EntrustmentDecisionStatus.Active, null, null, null, null, [])
        {
            TraineeName = $"Trainee {id}"
        };

    private sealed class FakeSender : IScopedSender
    {
        public Task<IReadOnlyList<EntrustmentDecisionDto>> List { get; set; } = Task.FromResult<IReadOnlyList<EntrustmentDecisionDto>>([]);

        public Exception? Revoke { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request switch
            {
                ListEntrustmentDecisionsForAdminQuery => As<TResponse>(List),
                RevokeEntrustmentDecisionCommand when Revoke is not null => Task.FromException<TResponse>(Revoke),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

        // Awaiting a failed read rethrows its own exception, as the pipeline does.
        private static async Task<TResponse> As<TResponse>(Task<IReadOnlyList<EntrustmentDecisionDto>> list)
            => (TResponse)(object)await list;

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
