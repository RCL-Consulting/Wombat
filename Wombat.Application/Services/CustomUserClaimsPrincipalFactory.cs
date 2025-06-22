using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Constants;
using Wombat.Data;

namespace Wombat.Application.Services
{
    public class CustomUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<WombatUser, IdentityRole>
    {
        private readonly ApplicationDbContext _dbContext;

        public CustomUserClaimsPrincipalFactory(
            UserManager<WombatUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ApplicationDbContext dbContext)
            : base(userManager, roleManager, optionsAccessor)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(WombatUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Add user ID claim if not present
            if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));

            // Fetch all roles
            var roles = await UserManager.GetRolesAsync(user);

            // Admin-level claims
            if (roles.Contains(Role.Administrator.ToStringValue()))
            {

                identity.AddClaim(new Claim(Claims.ManageAssessmentForms, "true"));
                identity.AddClaim(new Claim(Claims.ManageOptionSets, "true"));
                identity.AddClaim(new Claim(Claims.ManageEPAs, "true"));
                identity.AddClaim(new Claim(Claims.ManageSpecialities, "true"));
                identity.AddClaim(new Claim(Claims.ManageSubspecialities, "true"));
                identity.AddClaim(new Claim(Claims.ManageUsers, "true"));
                identity.AddClaim(new Claim(Claims.ManageRegistrationInvitations, "true"));
            }

            if (roles.Contains(Role.InstitutionalAdmin.ToStringValue()))
            {
                identity.AddClaim(new Claim(Claims.ManageAssessmentForms, "true"));
                identity.AddClaim(new Claim(Claims.ManageUsers, "true"));
                identity.AddClaim(new Claim(Claims.ManageRegistrationInvitations, "true"));
            }

            if (roles.Contains(Role.SpecialityAdmin.ToStringValue()))
            {
                identity.AddClaim(new Claim(Claims.ManageAssessmentForms, "true"));
                identity.AddClaim(new Claim(Claims.ManageUsers, "true"));
                identity.AddClaim(new Claim(Claims.ManageRegistrationInvitations, "true"));
            }

            if (roles.Contains(Role.SubSpecialityAdmin.ToStringValue()))
            {
                identity.AddClaim(new Claim(Claims.ManageAssessmentForms, "true"));
                identity.AddClaim(new Claim(Claims.ManageUsers, "true"));
                identity.AddClaim(new Claim(Claims.ManageRegistrationInvitations, "true"));
            }

            // Assessor-level claims
            if (roles.Contains(Role.Assessor.ToStringValue()))
            {
                identity.AddClaim(new Claim(Claims.LogAssessment, "true"));
                identity.AddClaim(new Claim(Claims.HandleAssessmentRequests, "true"));
            }

            // Coordinator-level claims
            if (roles.Contains(Role.Coordinator.ToStringValue()))
            {
                identity.AddClaim(new Claim(Claims.ApproveTrainee, "true"));
                identity.AddClaim(new Claim(Claims.ViewInstitutionPortfolios, "true"));
            }

            // Trainee-level claims
            if (roles.Contains(Role.Trainee.ToStringValue()))
            {
                identity.AddClaim(new Claim(Claims.RequestAssessment, "true"));
                identity.AddClaim(new Claim(Claims.ManageOwnPortfolio, "true"));
            }

            // Store approval status claim
            identity.AddClaim(new Claim(Claims.ApprovalStatus, user.ApprovalStatus.ToString()));

            return identity;
        }
    }
}
