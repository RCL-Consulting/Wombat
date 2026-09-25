using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T239: the entrustment scales list's Edit and Delete are named by their scale, under an "Actions" header; and an
/// Institutional admin, who changes no scale, is not shown the column at all. It was an empty header over a column of
/// blank cells.
/// </summary>
public sealed class EntrustmentScalesListActionsTests : TestContext
{
    private readonly EntrustmentScaleDto[] _scales =
    [
        new(1, "O-SCORE", "Operative", [new(1, 1, "1", null)]),
        new(2, "CPSA v11.1", "College ladder", [new(2, 1, "1", null), new(3, 2, "2", null)])
    ];

    public EntrustmentScalesListActionsTests()
    {
        Services.AddSingleton<IScopedSender>(new Sender(_scales));
    }

    [Fact]
    public void ForAnAdministrator_EachActionIsNamedByItsScale_UnderAnActionsHeader()
    {
        var cut = Render(WombatRoles.Administrator);

        cut.FindAll("thead th").Select(header => header.TextContent.Trim()).Should().Equal("Name", "Description", "Levels", "Actions");
        cut.Find("thead th:last-child span.visually-hidden").TextContent.Should().Be("Actions");

        var names = cut.FindAll("tbody .actions-cell a, tbody .actions-cell button")
            .Select(control => AccessibleNames.NameOf(cut, control))
            .ToList();
        names.Should().Equal("Edit O-SCORE", "Delete O-SCORE", "Edit CPSA v11.1", "Delete CPSA v11.1");
    }

    [Fact]
    public void ForAnInstitutionalAdmin_TheColumnIsNotRendered()
    {
        // No row offers them an action, so there is no column of blank cells under an unnamed header (DESIGN.md § Table
        // system, T226).
        var cut = Render(WombatRoles.InstitutionalAdmin);

        cut.FindAll("thead th").Select(header => header.TextContent.Trim()).Should().Equal("Name", "Description", "Levels");
        cut.FindAll("tbody tr").Should().OnlyContain(row => row.QuerySelectorAll("td").Length == 3);
        cut.FindAll("tbody a, tbody button").Should().BeEmpty();
    }

    private IRenderedComponent<EntrustmentScalesList> Render(string role)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(role);

        var cut = RenderComponent<EntrustmentScalesList>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == _scales.Length);
        return cut;
    }

    private sealed class Sender(IReadOnlyList<EntrustmentScaleDto> scales) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetEntrustmentScalesListQuery
                ? Task.FromResult((TResponse)(object)scales)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
