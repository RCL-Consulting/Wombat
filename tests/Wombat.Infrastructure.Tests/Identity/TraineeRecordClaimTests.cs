using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Identity;

/// <summary>
/// T252: completing a programme removes the Trainee role, so the claim sign-in issues to anyone holding a trainee profile
/// is what still admits a graduate to their own progress page. Built on the real Identity stack over the in-memory store,
/// with the app's own claims factory and policies.
/// </summary>
public sealed class TraineeRecordClaimTests : IDisposable
{
    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;

    public TraineeRecordClaimTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<WombatUserClaimsPrincipalFactory>();
        services.AddWombatAuthorization();

        _root = services.BuildServiceProvider();
        _scope = _root.CreateScope();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _root.Dispose();
    }

    private UserManager<WombatIdentityUser> Users => _scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();

    private ApplicationDbContext Db => _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    [Fact]
    public async Task AGraduateWhoLostTheTraineeRole_StillHoldsTheClaim_AndIsAdmittedToTheirProgress()
    {
        var graduate = await CreateUserAsync("graduate@kgk.test", roles: []);
        AddProfile(graduate.Id, profile => profile.Complete(new DateOnly(2026, 6, 30), today: new DateOnly(2026, 6, 30)));

        var principal = await SignInPrincipalAsync(graduate);

        principal.IsInRole(WombatRoles.Trainee).Should().BeFalse("completion removed the role");
        principal.HasClaim(WombatClaims.TraineeRecord, "true").Should().BeTrue();
        (await AdmittedToProgressAsync(principal)).Should().BeTrue();
    }

    [Fact]
    public async Task AWithdrawnTrainee_AndACurrentOne_HoldTheClaimToo()
    {
        var withdrawn = await CreateUserAsync("withdrawn@kgk.test", roles: [WombatRoles.Trainee]);
        AddProfile(withdrawn.Id, profile => profile.Deactivate(new DateOnly(2026, 8, 20), today: new DateOnly(2026, 8, 20)));
        var current = await CreateUserAsync("current@kgk.test", roles: [WombatRoles.Trainee]);
        AddProfile(current.Id, _ => { });

        foreach (var user in new[] { withdrawn, current })
        {
            var principal = await SignInPrincipalAsync(user);
            principal.HasClaim(claim => claim.Type == WombatClaims.TraineeRecord).Should().BeTrue(user.Email);
            (await AdmittedToProgressAsync(principal)).Should().BeTrue(user.Email);
        }
    }

    [Fact]
    public async Task SomeoneWhoNeverHeldAProfile_HasNoClaim_AndOnlyTheTraineeRoleAdmitsThem()
    {
        var assessor = await CreateUserAsync("assessor@kgk.test", roles: [WombatRoles.Assessor]);
        var pending = await CreateUserAsync("pending@kgk.test", roles: [WombatRoles.PendingTrainee]);
        var admittedWithoutAProfile = await CreateUserAsync("admitted@kgk.test", roles: [WombatRoles.Trainee]);
        AddProfile("someone-else", _ => { });

        foreach (var user in new[] { assessor, pending })
        {
            var principal = await SignInPrincipalAsync(user);
            principal.HasClaim(claim => claim.Type == WombatClaims.TraineeRecord).Should().BeFalse(user.Email);
            (await AdmittedToProgressAsync(principal)).Should().BeFalse(user.Email);
        }

        var trainee = await SignInPrincipalAsync(admittedWithoutAProfile);
        trainee.HasClaim(claim => claim.Type == WombatClaims.TraineeRecord).Should().BeFalse();
        (await AdmittedToProgressAsync(trainee)).Should().BeTrue("the Trainee role admits on its own, as before T252");
    }

    private async Task<WombatIdentityUser> CreateUserAsync(string email, string[] roles)
    {
        var user = new WombatIdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        (await Users.CreateAsync(user)).Succeeded.Should().BeTrue();

        var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            (await Users.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue();
        }

        return user;
    }

    private void AddProfile(string userId, Action<TraineeProfile> end)
    {
        var profile = new TraineeProfile
        {
            UserId = userId, InstitutionId = 1, CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2024, 1, 1), ExpectedCompletionDate = new DateOnly(2028, 1, 1),
            IsActive = true
        };
        end(profile);
        Db.TraineeProfiles.Add(profile);
        Db.SaveChanges();
    }

    /// <summary>The principal sign-in builds, through the factory the app registers (<c>AddClaimsPrincipalFactory</c>).</summary>
    private Task<ClaimsPrincipal> SignInPrincipalAsync(WombatIdentityUser user)
        => _scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<WombatIdentityUser>>().CreateAsync(user);

    private async Task<bool> AdmittedToProgressAsync(ClaimsPrincipal principal)
        => (await _scope.ServiceProvider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(principal, AuthorizationPolicies.TraineeOrFormerTrainee)).Succeeded;
}
