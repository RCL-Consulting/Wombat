using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Identity;

public static class AuthorizationPolicies
{
    public const string RequireCollegeScope = nameof(RequireCollegeScope);
    public const string RequireInstitutionScope = nameof(RequireInstitutionScope);
    public const string RequireSpecialityScope = nameof(RequireSpecialityScope);
    public const string RequireSubSpecialityScope = nameof(RequireSubSpecialityScope);
    public const string RequireSpecialityAdminForCurrentInstitution = nameof(RequireSpecialityAdminForCurrentInstitution);
    public const string RequireSubSpecialityAdminForCurrentInstitution = nameof(RequireSubSpecialityAdminForCurrentInstitution);
    public const string AdministratorOrInstitutionalAdmin = nameof(AdministratorOrInstitutionalAdmin);
    public const string AdministratorOrCollegeAdmin = nameof(AdministratorOrCollegeAdmin);

    /// <summary>
    /// National EPA/curriculum catalogue pages. A CollegeAdmin authors the national core, an
    /// InstitutionalAdmin views the adopted catalogue and manages institution-local extras, an
    /// Administrator sees everything. Handlers enforce the fine-grained per-row scope (T091).
    /// </summary>
    public const string NationalCatalogueAccess = nameof(NationalCatalogueAccess);

    public static IServiceCollection AddWombatAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, ScopeClaimRequirementHandler>();

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            foreach (var role in WombatRoles.All)
            {
                options.AddPolicy(role, policy => policy.RequireRole(role));
            }

            options.AddPolicy(RequireCollegeScope, policy =>
                policy.Requirements.Add(new ScopeClaimRequirement(WombatClaims.CollegeId)));

            options.AddPolicy(RequireInstitutionScope, policy =>
                policy.Requirements.Add(new ScopeClaimRequirement(WombatClaims.InstitutionId)));

            options.AddPolicy(RequireSpecialityScope, policy =>
                policy.Requirements.Add(new ScopeClaimRequirement(WombatClaims.SpecialityId)));

            options.AddPolicy(RequireSubSpecialityScope, policy =>
                policy.Requirements.Add(new ScopeClaimRequirement(WombatClaims.SubSpecialityId)));

            options.AddPolicy(RequireSpecialityAdminForCurrentInstitution, policy =>
            {
                policy.RequireRole(WombatRoles.SpecialityAdmin);
                policy.Requirements.Add(new ScopeClaimRequirement(WombatClaims.InstitutionId));
                policy.Requirements.Add(new ScopeClaimRequirement(WombatClaims.SpecialityId));
            });

            options.AddPolicy(RequireSubSpecialityAdminForCurrentInstitution, policy =>
            {
                policy.RequireRole(WombatRoles.SubSpecialityAdmin);
                policy.Requirements.Add(new ScopeClaimRequirement(WombatClaims.InstitutionId));
                policy.Requirements.Add(new ScopeClaimRequirement(WombatClaims.SubSpecialityId));
            });

            options.AddPolicy(AdministratorOrInstitutionalAdmin, policy =>
                policy.RequireRole(WombatRoles.Administrator, WombatRoles.InstitutionalAdmin));

            options.AddPolicy(AdministratorOrCollegeAdmin, policy =>
                policy.RequireRole(WombatRoles.Administrator, WombatRoles.CollegeAdmin));

            options.AddPolicy(NationalCatalogueAccess, policy =>
                policy.RequireRole(WombatRoles.Administrator, WombatRoles.CollegeAdmin, WombatRoles.InstitutionalAdmin));
        });

        return services;
    }

    private sealed class ScopeClaimRequirement : IAuthorizationRequirement
    {
        public ScopeClaimRequirement(string claimType)
        {
            ClaimType = claimType;
        }

        public string ClaimType { get; }
    }

    private sealed class ScopeClaimRequirementHandler : AuthorizationHandler<ScopeClaimRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeClaimRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == requirement.ClaimType))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
