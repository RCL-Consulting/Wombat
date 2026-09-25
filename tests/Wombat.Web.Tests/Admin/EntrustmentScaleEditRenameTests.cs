using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T253: a rename refuses nothing. When a published form still names the scale by its old name, the save says what it did
/// to that form, as a warning beside the success; and a name the validators refuse is shown in its own words.
/// </summary>
public sealed class EntrustmentScaleEditRenameTests : TestContext
{
    private const int ScaleId = 902;

    private const string Warning =
        "The form of the activity type \"Local CbD\" (local_cbd, version 1) names this scale by its old name, \"Local " +
        "Ladder\", so since it became \"Local Ladder v2\" their scale fields have no ladder.";

    private static readonly EntrustmentScaleDto LocalLadder = new(ScaleId, "Local Ladder", null,
        [new EntrustmentLevelDto(9020, 1, "Low", null), new EntrustmentLevelDto(9021, 2, "High", null)]);

    private readonly ScaleSender _sender = new();

    public EntrustmentScaleEditRenameTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
        Services.AddSingleton<IScopedSender>(_sender);
    }

    [Fact]
    public void ARenameThatUnbindsAForm_IsSaved_AndTheWarningNamesTheForm_AsAStatus()
    {
        _sender.Respond = command => new UpdateEntrustmentScaleResult(LocalLadder with { Name = command.Name.Trim() }, Warning);
        var cut = RenderEditor();

        cut.Find("#scale-name").Change("Local Ladder v2");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Save").Click();

        cut.WaitForAssertion(() => cut.FindAll(".alert-warning").Should().ContainSingle());
        cut.Find(".alert-success").TextContent.Trim().Should().Be("Entrustment scale saved.");
        var warning = cut.Find(".alert-warning");
        warning.TextContent.Trim().Should().Be(Warning);
        warning.GetAttribute("role").Should().Be("status", "it reports what the save just did (DESIGN.md § Alerts)");
        cut.FindAll(".alert-danger").Should().BeEmpty("the save went through");
        _sender.Sent.Should().ContainSingle().Which.Name.Should().Be("Local Ladder v2");
    }

    [Fact]
    public void ARenameThatUnbindsNothing_SaysOnlyThatItSaved_AndASecondSaveClearsAnEarlierWarning()
    {
        _sender.Respond = command => new UpdateEntrustmentScaleResult(LocalLadder with { Name = command.Name.Trim() }, Warning);
        var cut = RenderEditor();
        cut.Find("#scale-name").Change("Local Ladder v2");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Save").Click();
        cut.WaitForAssertion(() => cut.FindAll(".alert-warning").Should().ContainSingle());

        _sender.Respond = command => new UpdateEntrustmentScaleResult(LocalLadder with { Name = command.Name.Trim() }, null);
        cut.Find("#scale-name").Change("Local Ladder v3");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Save").Click();

        cut.WaitForAssertion(() => cut.FindAll(".alert-warning").Should().BeEmpty());
        cut.Find(".alert-success").TextContent.Trim().Should().Be("Entrustment scale saved.");
    }

    [Fact]
    public void ANameTheValidatorsRefuse_IsShownInItsOwnWords()
    {
        _sender.Respond = _ => throw new ValidationException(
            [new ValidationFailure("Name", EntrustmentScaleBindings.ReservedNameRefusal)]);
        var cut = RenderEditor();

        cut.Find("#scale-name").Change("seed:cpsa:scale:v11.1");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Save").Click();

        cut.WaitForAssertion(() => cut.Find(".alert-danger").TextContent.Trim()
            .Should().Be(EntrustmentScaleBindings.ReservedNameRefusal, "not \"Validation failed: -- Name: …\""));
        cut.FindAll(".alert-success").Should().BeEmpty();
    }

    private IRenderedComponent<EntrustmentScaleEdit> RenderEditor()
    {
        var cut = RenderComponent<EntrustmentScaleEdit>(parameters => parameters.Add(page => page.Id, ScaleId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);
        return cut;
    }

    /// <summary>The editor's server: it loads one scale, and answers a save as the test says.</summary>
    private sealed class ScaleSender : IScopedSender
    {
        public Func<UpdateEntrustmentScaleCommand, UpdateEntrustmentScaleResult> Respond { get; set; } =
            _ => throw new NotSupportedException("No save expected.");

        public List<UpdateEntrustmentScaleCommand> Sent { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetEntrustmentScaleByIdQuery { Id: ScaleId }:
                    return Task.FromResult((TResponse)(object)LocalLadder);
                case UpdateEntrustmentScaleCommand command:
                    Sent.Add(command);
                    return Task.FromResult((TResponse)(object)Respond(command));
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
