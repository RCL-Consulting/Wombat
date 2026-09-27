using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T211: the curricula list links each admin only to pages they can open and act on. Every row is a curriculum the caller
/// can open (the list and the read share <c>CurriculumAdminScope.Openable</c>, held in the Application tests), so Items is
/// on every row. Edit, and Create in the header, lead to the curriculum's own page, whose every command is the College's:
/// they are offered where the query says the caller may change the curriculum, and to those that page's policy admits.
/// </summary>
/// <remarks>
/// Before T211 every row offered Edit and Items, and the header Create, to everyone. An InstitutionalAdmin's Items link
/// led to not-found, and their Edit to a form whose every Save was refused; a CollegeAdmin's Edit and Create led to a page
/// that turned them away.
/// </remarks>
public sealed class CurriculaListScopeTests : TestContext
{
    private const string CreatePagePolicy = "AdministratorOrCollegeAdmin";
    private const string ProgressPagePolicy = "Administrator";

    [Fact]
    public void AnInstitution_IsOfferedTheItemsOfEachAdoptedCurriculum_AndNothingToCreateOrEdit()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderList([Curriculum(5, canEditCurriculum: false)]);

        Links(cut).Should().Equal([("Items", "/admin/curricula/5/items")],
            "an institution opens an adopted curriculum's items, and changes nothing else about it");
    }

    [Fact]
    public void TheCollege_IsOfferedCreate_AndEditAndItemsOnEachCurriculum()
    {
        SignIn(WombatRoles.CollegeAdmin, CreatePagePolicy);
        var cut = RenderList([Curriculum(5, canEditCurriculum: true), Curriculum(6, canEditCurriculum: true)]);

        Links(cut).Should().Equal(
            ("Create curriculum", "/admin/curricula/new"),
            ("Edit", "/admin/curricula/5"),
            ("Items", "/admin/curricula/5/items"),
            ("Edit", "/admin/curricula/6"),
            ("Items", "/admin/curricula/6/items"));
    }

    // T335, flow 01 (R2-Shell-Admin, R2-Rules § 3): the Administrator's Home drops its Maintenance card, so Curriculum
    // progress is reached from Curricula, the item it lights. Offered to those its page's policy admits.
    [Fact]
    public void TheAdministrator_IsOfferedCurriculumProgress_InTheHeader()
    {
        SignIn(WombatRoles.Administrator, CreatePagePolicy, ProgressPagePolicy);
        var cut = RenderList([Curriculum(5, canEditCurriculum: true)]);

        Links(cut).Should().Contain(("Curriculum progress", "/admin/curriculum-progress"));
        cut.Find(".header-container .actions-cell a[href='/admin/curriculum-progress']").ClassList.Should().Contain("btn-outline");
    }

    [Theory]
    [InlineData(WombatRoles.CollegeAdmin)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    public void NoOneElse_IsOfferedCurriculumProgress(string role)
    {
        SignIn(role, CreatePagePolicy);

        Links(RenderList([Curriculum(5, canEditCurriculum: true)])).Select(link => link.Href).Should().NotContain("/admin/curriculum-progress");
    }

    [Fact]
    public void TheCurriculumProgressPage_AdmitsThoseTheListOffersItTo()
        => typeof(Wombat.Web.Components.Pages.Admin.CurriculumProgress.CurriculumProgressRebuild)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Select(attribute => attribute.Policy)
            .Should().Equal(ProgressPagePolicy);

    [Fact]
    public void AnInstitutionWithNothingAdopted_IsPointedAtAdoption_NotAtCreatingACurriculum()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderList([]);

        cut.Find(".detail-card--empty").TextContent.Should().Contain("Your institution has not adopted a curriculum yet.")
            .And.NotContain("Create a curriculum");
        Links(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheCurriculumsOwnPage_AdmitsThoseTheListOffersItTo()
    {
        // Create and Edit lead to CurriculumEdit, whose create, update and clone commands are the College's
        // (CanAccessCollege). Before T211 it admitted AdministratorOrInstitutionalAdmin: an InstitutionalAdmin reached a
        // form whose every Save was refused, and the CollegeAdmin who could save it was turned away at the door.
        typeof(CurriculumEdit).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Select(attribute => attribute.Policy)
            .Should().Equal(CreatePagePolicy);
    }

    [Fact]
    public void TheRowsNameTheirSpecialityAndSubSpeciality_FromTheCurriculum()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderList([Curriculum(5, canEditCurriculum: false)]);

        var cells = cut.FindAll("tbody td").Select(cell => cell.TextContent.Trim()).ToList();
        cells.Should().ContainInOrder("Paediatric EPA Curriculum", "11.1", "CMSA", "Paediatrics", "General Paediatrics");
    }

    private void SignIn(string role, params string[] policies)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{role}@test");
        auth.SetRoles(role);
        auth.SetPolicies(policies);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, $"{role}-1"));
    }

    private IRenderedComponent<CurriculaList> RenderList(IReadOnlyList<CurriculumDto> curricula)
    {
        Services.AddSingleton<IScopedSender>(new ListSender(curricula));

        var cut = RenderComponent<CurriculaList>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private static List<(string Text, string? Href)> Links(IRenderedComponent<CurriculaList> cut)
        => cut.FindAll("a").Select(link => (link.TextContent.Trim(), link.GetAttribute("href"))).ToList();

    private static CurriculumDto Curriculum(int id, bool canEditCurriculum)
        => new(id, 2, 7, "Paediatrics", "General Paediatrics", "CMSA", "Paediatric EPA Curriculum", "11.1",
            new DateOnly(2026, 1, 1), null, true, true, [], null)
        {
            CanEditCurriculum = canEditCurriculum
        };

    private sealed class ListSender(IReadOnlyList<CurriculumDto> curricula) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetCurriculaListQuery
                ? Task.FromResult((TResponse)(object)curricula)
                : throw new NotSupportedException($"The list asks for nothing else: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
