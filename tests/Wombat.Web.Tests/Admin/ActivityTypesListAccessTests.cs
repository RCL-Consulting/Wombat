using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T300: the activity types list offers Edit only on a row the caller may write (<c>ActivityTypeAdminListItemDto.CanWrite</c>,
/// the rule the commands refuse by) and View on the rest, both to the same page and each named by its row (T239); and New
/// activity type only to a caller with a scope to create in (<c>ActivityTypeAdminListDto.CanCreate</c>). Before T300 every
/// one of the 22 rows offered Prof Mbatha Edit (Steps 1.24 and 1.31), and she could write ten of them at most.
/// </summary>
public sealed class ActivityTypesListAccessTests : TestContext
{
    public ActivityTypesListAccessTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("caller@test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "caller-1"));
    }

    [Fact]
    public void AWritableRow_OffersEdit_AndARowSheMayOnlyRead_OffersView_BothToItsPage()
    {
        var cut = Render(canCreate: true,
            Row(11, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityScope.Speciality, 5, canWrite: false),
            Row(23, "kgk_teaching_log", "KGK Teaching Session Log", ActivityScope.Institution, 7, canWrite: true));

        var college = RowAction(cut, 11);
        college.TextContent.Trim().Should().Be("View");
        college.GetAttribute("aria-label").Should().Be("View Mini-CEX (Paediatrics)");
        college.GetAttribute("href").Should().Be("/admin/activity-types/11");

        var own = RowAction(cut, 23);
        own.TextContent.Trim().Should().Be("Edit");
        own.GetAttribute("aria-label").Should().Be("Edit KGK Teaching Session Log");
        own.GetAttribute("href").Should().Be("/admin/activity-types/23");
    }

    [Fact]
    public void ACallerWithAScopeToCreateIn_IsOfferedNewActivityType()
    {
        var cut = Render(canCreate: true, Row(11, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityScope.Speciality, 5, canWrite: false));

        cut.FindAll("a").Should().ContainSingle(link => link.TextContent.Trim() == "New activity type")
            .Which.GetAttribute("href").Should().Be("/admin/activity-types/new");
    }

    [Fact]
    public void ACallerWithNoScopeToCreateIn_IsNotOfferedNewActivityType()
    {
        var cut = Render(canCreate: false, Row(11, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityScope.Speciality, 5, canWrite: false));

        cut.FindAll("a").Should().NotContain(link => link.TextContent.Trim() == "New activity type");
        cut.FindAll("a").Should().NotContain(link => link.GetAttribute("href") == "/admin/activity-types/new");
    }

    /// <summary>
    /// Each row's scope reads the target the query names, as the editor reads a stored one. The page resolves no names of
    /// its own, so the sender here answers nothing but the list: it did, from the caller's own institutions and
    /// disciplines, and every other College's row read "Speciality · #1" (T291 item 5), to the CollegeAdmin T300 admits
    /// as much as to an InstitutionalAdmin.
    /// </summary>
    [Fact]
    public void EveryRowsScope_ReadsTheTargetTheQueryNames()
    {
        var cut = Render(canCreate: true,
            Row(11, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityScope.Speciality, 5, canWrite: false, "Paediatrics"),
            Row(12, "mini_cex", "Mini-CEX", ActivityScope.Speciality, 1, canWrite: false, "Internal Medicine"),
            Row(13, "neonatal_log", "Neonatal log", ActivityScope.SubSpeciality, 9, canWrite: false, "Paediatrics / Neonatology"),
            Row(14, "kgk_teaching_log", "KGK Teaching Session Log", ActivityScope.Institution, 7, canWrite: true, "Kalafong"),
            Row(15, "reflection", "Reflection", ActivityScope.Global, null, canWrite: false, null),
            Row(16, "orphan", "Orphan", ActivityScope.Speciality, 99, canWrite: false, null));

        ScopeCell(cut, 11).Should().Be("Speciality · Paediatrics");
        ScopeCell(cut, 12).Should().Be("Speciality · Internal Medicine", "another College's discipline is named too");
        ScopeCell(cut, 13).Should().Be("Sub-speciality · Paediatrics / Neonatology");
        ScopeCell(cut, 14).Should().Be("Institution · Kalafong");
        ScopeCell(cut, 15).Should().Be("Global");
        ScopeCell(cut, 16).Should().Be("Speciality · #99", "only a target that no longer exists reads by its id");
    }

    private IRenderedComponent<ActivityTypesList> Render(bool canCreate, params ActivityTypeAdminListItemDto[] rows)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(new ActivityTypeAdminListDto(rows, canCreate)));
        var cut = RenderComponent<ActivityTypesList>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == rows.Length);
        return cut;
    }

    private static AngleSharp.Dom.IElement RowAction(IRenderedFragment cut, int id)
        => cut.FindAll("tbody a.btn").Single(link => link.GetAttribute("href") == $"/admin/activity-types/{id}");

    /// <summary>The Scope cell, the third, of the row whose action opens this type.</summary>
    private static string ScopeCell(IRenderedFragment cut, int id)
        => cut.FindAll("tbody tr")
            .Single(row => row.QuerySelector($"a[href='/admin/activity-types/{id}']") is not null)
            .QuerySelectorAll("td")[2].TextContent.Trim();

    private static ActivityTypeAdminListItemDto Row(
        int id, string key, string name, ActivityScope scope, int? scopeId, bool canWrite, string? scopeTargetName = null)
        => new(id, key, name, null, scope, scopeId, 1, true, false, null, canWrite, scopeTargetName);

    private sealed class FakeSender(ActivityTypeAdminListDto list) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                ListActivityTypesAdminQuery => list,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
