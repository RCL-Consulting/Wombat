using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Constants
{
    public static class Claims
    {
        // Admin operations
        public const string ManageInstitutions = "manage_institutions";
        public const string ManageOptionSets = "manage_option_sets";
        public const string ManageAssessmentForms = "manage_assessment_forms";
        public const string ManageEPAs = "manage_epas";
        public const string ManageSpecialities = "manage_specialities";
        public const string ManageSubspecialities = "manage_subspecialities";
        public const string ManageUsers = "manage_users";
        public const string ManageRegistrationInvitations = "manage_registration_invitations";

        // Assessor
        public const string LogAssessment = "log_assessment";
        public const string HandleAssessmentRequests = "handle_assessment_requests";

        // Coordinator
        public const string ApproveTrainee = "approve_trainee";
        public const string ViewInstitutionPortfolios = "view_institution_portfolios";

        // Trainee
        public const string RequestAssessment = "request_assessment";
        public const string ManageOwnPortfolio = "manage_own_portfolio";

        // User state
        public const string ApprovalStatus = "approval_status";
    }
}
