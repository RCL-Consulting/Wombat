using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T392 (flow 15, boards R2-E01–E05, R3-E03, E04): the entrustment scales list. A reader (the Institutional admin) is told
/// what the ladders are and who sets them (Q13, T326), sees the scales by name with their count of levels (S10), and an
/// empty list in her terms; the Administrator's subtitle and empty state as built. Loading says so in words (S19); a failed
/// read is fixed words with Try again (S7, T329), never the exception's. Until T392 every caller read "Define and maintain
/// …" and a failed read printed the exception.
/// </summary>
public sealed class EntrustmentScalesListPageTests : TestContext
{
    private static readonly EntrustmentScaleDto[] Scales =
    [
        new(1, "CPSA Paediatric Entrustment Scale v11.1", "Six rungs in four bands.", [new(1, 1, "1", null), new(2, 2, "2", null)]),
        new(2, "O-R Scale", "Standard 1-5 observation-to-entrustment scale.", [new(3, 1, "1", null)])
    ];

    private const string FocusHeading = "wombat.focusHeading";

    public EntrustmentScalesListPageTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void AReader_IsToldWhoSetsTheScales_AndReadsThemByName_WithNoActions()
    {
        var cut = Render(WombatRoles.InstitutionalAdmin, new Sender { Scales = Scales });

        Text(cut.Find("h1")).Should().Be("Entrustment scales");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference.");
        cut.FindAll(".header-container a, .header-container button").Should().BeEmpty("a reader creates no scale");
        cut.Find(".people-page.scales-page").Should().NotBeNull();

        var table = cut.Find("table");
        table.ClassList.Should().Contain("clinic-table--stack-wide");
        Text(table.QuerySelector("caption")!).Should().Be("Entrustment scales, by name");
        cut.FindAll("tbody th[scope='row']").Select(Text).Should().Equal("CPSA Paediatric Entrustment Scale v11.1", "O-R Scale");
        cut.FindAll("tbody tr").First().QuerySelectorAll("td").Select(cell => cell.GetAttribute("data-label"))
            .Should().Equal("Description", "Levels");
        cut.FindAll("tbody td[data-label='Levels']").Select(Text).Should().Equal("2", "1");
    }

    [Fact]
    public void TheAdministrator_KeepsTheBuiltSubtitle_AndCreateScale()
    {
        var cut = Render(WombatRoles.Administrator, new Sender { Scales = Scales });

        Text(cut.Find(".page-subtitle")).Should().Be(
            "Define and maintain the rating ladders used by assessment forms and committee reviews.");
        Text(cut.Find(".header-container a.btn-primary")).Should().Be("Create scale");
    }

    [Theory]
    [InlineData(WombatRoles.InstitutionalAdmin, "No scales yet A Wombat administrator has not set up an entrustment scale yet.")]
    [InlineData(WombatRoles.Administrator, "No scales yet Create the first entrustment scale to back assessment ratings and entrustment decisions.")]
    public void WithNoScales_TheEmptyStateIsInTheCallersTerms(string role, string words)
    {
        var cut = Render(role, new Sender { Scales = [] });

        Text(cut.Find(".detail-card--empty")).Should().Be(words);
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void WhileItLoads_ItSaysSo_AndOffersNothing()
    {
        var cut = Render(WombatRoles.Administrator, new Sender { Hang = true });

        var line = cut.Find(".loading-line");
        Text(line).Should().Be("Loading the entrustment scales.");
        line.GetAttribute("role").Should().Be("status");
        cut.FindAll(".loading-line").Should().ContainSingle();
        cut.FindAll(".header-container a").Should().BeEmpty("no action is offered until the read is in");
    }

    [Fact]
    public void AFailedRead_IsFixedWords_WithTryAgain_AndNoExceptionText()
    {
        var sender = new Sender { Failure = new InvalidOperationException("Npgsql: relation entrustment_scales does not exist") };
        var cut = Render(WombatRoles.InstitutionalAdmin, sender);

        var alert = cut.Find(".action-result .alert-danger");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load the entrustment scales. Nothing has changed. Try again, or come back in a few minutes.");
        Text(alert.QuerySelector("button")!).Should().Be("Try again");
        cut.Markup.Should().NotContain("Npgsql");
        cut.FindAll(".detail-card--empty, table").Should().BeEmpty("no empty state under a failure");

        sender.Failure = null;
        sender.Scales = Scales;
        alert.QuerySelector("button")!.Click();

        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(2));
        // R3-E04: Try again moves the focus to the reloaded page's h1.
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call => call.Identifier == FocusHeading));
    }

    [Fact]
    public void ADeleteThatFailsOtherThanByARefusal_IsFixedWords_NotTheExceptions()
    {
        var sender = new Sender { Scales = Scales, DeleteFailure = new TimeoutException("socket closed by 10.0.0.4") };
        var cut = Render(WombatRoles.Administrator, sender);

        cut.FindAll("tbody button").First().Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "The scale could not be deleted. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("10.0.0.4");
    }

    [Fact]
    public void ADeleteRefusedByItsHandler_SaysTheHandlersWords()
    {
        var sender = new Sender { Scales = Scales, DeleteFailure = new Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale.EntrustmentScaleDeleteRefusedException("This scale is in use by 2 curriculum items, so it cannot be deleted.") };
        var cut = Render(WombatRoles.Administrator, sender);

        cut.FindAll("tbody button").First().Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "This scale is in use by 2 curriculum items, so it cannot be deleted."));
    }

    [Fact]
    public void ADeleteFailingWithAnyOtherInvalidOperation_IsFixedWords_NeverTheExceptionsText()
    {
        // P6 (T392 build review), T329: a bug's InvalidOperationException is no refusal.
        var sender = new Sender { Scales = Scales, DeleteFailure = new InvalidOperationException("Sequence contains no elements") };
        var cut = Render(WombatRoles.Administrator, sender);

        cut.FindAll("tbody button").First().Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "The scale could not be deleted. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Sequence contains");
    }

    private IRenderedComponent<EntrustmentScalesList> Render(string role, Sender sender)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("caller@test");
        auth.SetRoles(role);
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<EntrustmentScalesList>();
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        public IReadOnlyList<EntrustmentScaleDto> Scales { get; set; } = [];
        public Exception? Failure { get; set; }
        public Exception? DeleteFailure { get; init; }
        public bool Hang { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetEntrustmentScalesListQuery)
            {
                if (Hang)
                {
                    return new TaskCompletionSource<TResponse>().Task;
                }

                return Failure is not null
                    ? Task.FromException<TResponse>(Failure)
                    : Task.FromResult((TResponse)(object)Scales);
            }

            if (request is Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale.DeleteEntrustmentScaleCommand)
            {
                return DeleteFailure is not null
                    ? Task.FromException<TResponse>(DeleteFailure)
                    : Task.FromResult(default(TResponse)!);
            }

            throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => request is Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale.DeleteEntrustmentScaleCommand && DeleteFailure is not null
                ? Task.FromException(DeleteFailure)
                : Task.CompletedTask;
    }
}
