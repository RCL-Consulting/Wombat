using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Wombat.Application.Contracts;
using Wombat.Common.Constants;

namespace Wombat.Application.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly ClaimsPrincipal _user;

        public UserContextService(IHttpContextAccessor accessor)
        {
            _user = accessor.HttpContext?.User ?? throw new InvalidOperationException("Missing user context");
        }

        public string? UserId => _user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Global override check
        public bool IsGlobalAdmin => _user.IsInRole(Role.Administrator.ToStringValue());

        public bool CanManageInstitutions => IsGlobalAdmin || HasClaim(Claims.ManageInstitutions);
        public bool CanManageAssessmentForms => IsGlobalAdmin || HasClaim(Claims.ManageAssessmentForms);

        public bool CanManageOptionSets => IsGlobalAdmin || HasClaim(Claims.ManageOptionSets);
        public bool CanManageEPAs => IsGlobalAdmin || HasClaim(Claims.ManageEPAs);
        public bool CanManageSpecialities => IsGlobalAdmin || HasClaim(Claims.ManageSpecialities);
        public bool CanManageSubspecialities => IsGlobalAdmin || HasClaim(Claims.ManageSubspecialities);
        public bool CanManageUsers => IsGlobalAdmin || HasClaim(Claims.ManageUsers);
        public bool CanManageInvitations => IsGlobalAdmin || HasClaim(Claims.ManageRegistrationInvitations);

        public bool CanLogAssessment => HasClaim(Claims.LogAssessment);
        public bool CanHandleAssessmentRequests => HasClaim(Claims.HandleAssessmentRequests);

        public bool CanApproveTrainees => HasClaim(Claims.ApproveTrainee);
        public bool CanViewPortfolios => HasClaim(Claims.ViewInstitutionPortfolios);

        public bool CanRequestAssessment => HasClaim(Claims.RequestAssessment);
        public bool CanManageOwnPortfolio => HasClaim(Claims.ManageOwnPortfolio);

        public string? ApprovalStatus =>
            _user.FindFirst(Claims.ApprovalStatus)?.Value;

        private bool HasClaim(string claimType) =>
            _user.HasClaim(claimType, "true");
    }

}
