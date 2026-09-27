using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Colleges;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesForInstitution;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Navigation;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T335, flow 01 (R2-Rules § 3): a College's specialities carry the College on their trail. The Administrator reached them
/// from Colleges, so the College is the crumb between (Home › Colleges › the College › Specialities); the College admin's
/// own Specialities opens this page, so the College is its own crumb (Home › Specialities › the College).
/// </summary>
public sealed class SpecialitiesListTrailTests : TestContext
{
    private const int CollegeId = 2;
    private const string College = "College of Paediatricians of South Africa";

    [Fact]
    public void ForTheAdministrator_TheCollegeIsBetweenColleges_AndTheList()
        => Crumbs(Render(WombatRoles.Administrator)).Should().Equal(
            ("Home", "/"), ("Colleges", "/admin/colleges"), (College, "/admin/colleges/2"), ("Specialities", null));

    [Fact]
    public void ForTheCollegeAdmin_TheCollegeIsThePagesOwnCrumb_UnderSpecialities()
        => Crumbs(Render(WombatRoles.CollegeAdmin)).Should().Equal(
            ("Home", "/"), ("Specialities", "/admin/specialities"), (College, null));

    private IRenderedComponent<SpecialitiesList> Render(string role)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{role}@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, $"{role}-1"));
        Services.AddSingleton<IScopedSender>(new CollegeSender());

        var acting = ActingRoleResolver.Resolve(role, [role]);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "Test"));
        var cut = RenderComponent<SpecialitiesList>(parameters => parameters
            .Add(page => page.CollegeId, CollegeId)
            .AddCascadingValue(acting)
            .AddCascadingValue(NavItems.For(acting, user))
            .AddCascadingValue(new RouteData(typeof(SpecialitiesList), new Dictionary<string, object?> { ["CollegeId"] = CollegeId })));
        cut.WaitForAssertion(() => cut.Markup.Should().Contain(College));
        return cut;
    }

    private static List<(string Label, string? Href)> Crumbs(IRenderedComponent<SpecialitiesList> cut)
        => cut.FindAll("nav[aria-label='Breadcrumb'] li")
            .Select(li => (li.TextContent.Trim(), li.QuerySelector("a")?.GetAttribute("href")))
            .ToList();

    private sealed class CollegeSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request switch
            {
                GetCollegeByIdQuery => Task.FromResult((TResponse)(object)new CollegeDto(CollegeId, College, "CPSA", null, true, DateTime.UnixEpoch)),
                GetSpecialitiesForInstitutionQuery => Task.FromResult((TResponse)(object)Array.Empty<SpecialityDto>()),
                _ => throw new NotSupportedException(request.GetType().Name),
            };

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
