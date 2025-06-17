using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Application.Contracts
{
    public interface IUserContextService
    {
        string? UserId { get; }

        // Admin permissions
        bool CanManageAssessmentForms { get; }
        bool CanManageOptionSets { get; }
        bool CanManageEPAs { get; }
        bool CanManageSpecialities { get; }
        bool CanManageSubspecialities { get; }
        bool CanManageUsers { get; }
        bool CanManageInvitations { get; }

        // Assessor permissions
        bool CanLogAssessment { get; }
        bool CanHandleAssessmentRequests { get; }

        // Coordinator permissions
        bool CanApproveTrainees { get; }
        bool CanViewPortfolios { get; }

        // Trainee permissions
        bool CanRequestAssessment { get; }
        bool CanManageOwnPortfolio { get; }

        // User state
        string? ApprovalStatus { get; }

        // Optional: Global admin shortcut
        bool IsGlobalAdmin { get; }
    }
}
