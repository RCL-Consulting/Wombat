using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Epas;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T158: what the EPA page says about deactivating, and that the Deactivate button asks first.
/// </summary>
/// <remarks>
/// Since T158 deactivating an EPA takes its items off every progress page and stops their credit, in every institution
/// that uses the curriculum. The Status field must say what that means, where a screen reader on the checkbox hears it,
/// and the Deactivate button must go through a ConfirmDialog, as DESIGN.md asks of every destructive action.
/// </remarks>
public sealed class EpaEditDeactivationTests : TestContext
{
    private const int EpaId = 3;

    public EpaEditDeactivationTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    [Fact]
    public void TheStatusCheckbox_IsDescribedByWhatInactiveMeans()
    {
        var cut = RenderPage(new FakeSender());

        var describedBy = cut.Find("#epa-active").GetAttribute("aria-describedby");
        describedBy.Should().Be("epa-active-help");

        var help = Text(cut.Find($"#{describedBy}"));
        help.Should().Contain("cannot be chosen for a new activity and is not a target on any progress page or dashboard")
            .And.Contain("still does after the EPA is reactivated unless an administrator rebuilds curriculum progress");
        help.Should().NotContain("every progress page.",
            "the rating trajectory still shows evidence recorded against an inactive EPA; only its targets go");
    }

    [Fact]
    public void Deactivate_OpensTheDialog_AndSendsNothingUntilConfirmed()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        var button = PageButton(cut, "Deactivate");
        button.ClassList.Should().NotContain("btn-danger", "the red button belongs only in the dialog's footer");

        button.Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty("opening the dialog must not deactivate the EPA");
        Text(cut.Find("dialog")).Should().Contain("stops being a target on every progress page and dashboard");
    }

    [Fact]
    public void ConfirmingTheDialog_DeactivatesTheEpa()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        PageButton(cut, "Deactivate").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Deactivate").Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<DeactivateEpaCommand>()
            .Which.Id.Should().Be(EpaId);
        Text(cut.Find(".alert.alert-success")).Should().Be("EPA deactivated.");
    }

    private IRenderedComponent<EpaEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<EpaEdit>(parameters => parameters.Add(page => page.Id, EpaId));
        cut.WaitForState(() => cut.FindAll("#epa-active").Count > 0);

        return cut;
    }

    private static IElement PageButton(IRenderedFragment cut, string label)
        => cut.FindAll("button").Single(button => button.Closest("dialog") is null && Text(button) == label);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender : IScopedSender
    {
        private static readonly EpaDto Epa = new(
            EpaId, 7, "General Paediatrics", "CMSA", "PAED-003", "Resuscitating a child", null, null, EpaCategory.Core,
            IsActive: true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        public List<IRequest> Commands { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                GetInstitutionsListQuery => Array.Empty<InstitutionDto>(),
                GetSpecialitiesListQuery => Array.Empty<SpecialityDto>(),
                GetSubSpecialitiesListQuery => Array.Empty<SubSpecialityDto>(),
                GetEpaByIdQuery query when query.Id == EpaId => Epa,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Commands.Add(request);
            return Task.CompletedTask;
        }
    }
}
