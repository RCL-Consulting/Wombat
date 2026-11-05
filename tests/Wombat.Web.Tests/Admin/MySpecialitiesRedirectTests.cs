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
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetSpecialityById;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T394 (flow 16, lane A): the College admin's Specialities sends him to his College's tree, or says his account names no
/// College (S12, R2-A21), never Colleges, which refuses him (T291 item 2); the Administrator goes to Colleges. A
/// speciality's old Sub-specialities address sends to its section of the tree, or is Speciality unavailable (Q1, Q7, N2).
/// Home's card says Q12's sentence over four links in its footer (Q17, R2-A01).
/// </summary>
public sealed class MySpecialitiesRedirectTests : TestContext
{
    [Fact]
    public void ACollegeAdmin_IsSentToHisCollegesTree()
    {
        SignIn(WombatRoles.CollegeAdmin, new Claim(WombatClaimTypes.CollegeId, "2"));
        var navigation = Services.GetRequiredService<NavigationManager>();

        RenderComponent<MySpecialitiesRedirect>();

        navigation.Uri.Should().EndWith("/admin/colleges/2/specialities");
    }

    [Fact]
    public void ACollegeAdminWhoseAccountNamesNoCollege_IsToldSo_NoListNoCreate_NotSentToColleges()
    {
        SignIn(WombatRoles.CollegeAdmin);
        var navigation = Services.GetRequiredService<NavigationManager>();

        var cut = RenderComponent<MySpecialitiesRedirect>();

        Text(cut.Find("h1")).Should().Be("Specialities");
        Text(cut.Find(".detail-card--empty .state-panel-title")).Should().Be("No College on your account");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            "Your account names no College. Ask the platform administrator to add it.");
        cut.FindAll("a.btn, table").Should().BeEmpty();
        navigation.Uri.Should().NotContain("/admin/colleges");
    }

    [Fact]
    public void TheAdministrator_IsSentToColleges()
    {
        SignIn(WombatRoles.Administrator);
        var navigation = Services.GetRequiredService<NavigationManager>();

        RenderComponent<MySpecialitiesRedirect>();

        navigation.Uri.Should().EndWith("/admin/colleges");
    }

    [Fact]
    public void TheOldSubSpecialitiesAddress_SendsToItsSpecialitysSectionOfTheTree()
    {
        SignIn(WombatRoles.CollegeAdmin, new Claim(WombatClaimTypes.CollegeId, "2"));
        Services.AddSingleton<IScopedSender>(new SpecialitySender(new SpecialityDto(3, 2, "Neurology", null, true)));
        var navigation = Services.GetRequiredService<NavigationManager>();

        RenderComponent<SubSpecialitiesList>(parameters => parameters.Add(page => page.SpecialityId, 3));

        navigation.Uri.Should().EndWith("/admin/colleges/2/specialities#speciality-3");
    }

    [Fact]
    public void TheOldSubSpecialitiesAddress_ForASpecialityTheCallerCannotOpen_IsSpecialityUnavailable()
    {
        SignIn(WombatRoles.CollegeAdmin, new Claim(WombatClaimTypes.CollegeId, "2"));
        Services.AddSingleton<IScopedSender>(new SpecialitySender(null));

        var cut = RenderComponent<SubSpecialitiesList>(parameters => parameters.Add(page => page.SpecialityId, 3));

        Text(cut.Find("h1")).Should().Be("Speciality unavailable");
        Text(cut.Find(".detail-card--empty div.state-panel-copy")).Should().Be("This speciality does not exist, or you cannot open it.");
        Text(cut.Find(".detail-card--empty .form-actions a")).Should().Be("Go to Specialities");
        cut.Find(".detail-card--empty .form-actions a").QuerySelector("svg").Should().BeNull();
    }

    [Fact]
    public void TheCollegeAdminsHomeCard_SaysWhatSheKeeps_OverFourLinksInItsFooter()
    {
        var cut = RenderComponent<CollegeAdminDashboard>();

        Text(cut.Find(".dashboard-card-title")).Should().Be("National catalogue");
        Text(cut.Find(".dashboard-card-body p")).Should().Be(
            "Your College's specialities, EPAs, curricula and activity types. An institution can adopt a curriculum version once you open it.");
        cut.FindAll(".dashboard-card-footer a").Select(link => (Text(link), link.GetAttribute("href"))).Should().Equal(
            ("Specialities", "/admin/specialities"), ("EPAs", "/admin/epas"), ("Curricula", "/admin/curricula"),
            ("Activity types", "/admin/activity-types"));
        cut.FindAll(".dashboard-card-footer a").Should().OnlyContain(link => link.ClassList.Contains("btn"));
    }

    private void SignIn(string role, params Claim[] claims)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{role}@test");
        auth.SetRoles(role);
        auth.SetClaims([new Claim(ClaimTypes.NameIdentifier, $"{role}-1"), .. claims]);
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class SpecialitySender(SpecialityDto? speciality) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetSpecialityByIdQuery
                ? Task.FromResult((TResponse)(object?)speciality!)
                : throw new NotSupportedException(request.GetType().Name);

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
