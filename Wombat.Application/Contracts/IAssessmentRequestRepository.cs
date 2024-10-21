using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface IAssessmentRequestRepository : IGenericRepository<AssessmentRequest>
    {
        Task<List<AssessmentRequest>?> GetRequestsMadeByTraineeAndWaitingApproval(string traineeId);
        Task<List<AssessmentRequest>?> GetRequestsMadeByTraineeAndAccepted(string traineeId);
        Task<List<AssessmentRequest>?> GetRequestsMadeByTraineeAndDeclined(string traineeId);

        Task<List<AssessmentRequest>?> GetRequestsMadeOfAssessorAndWaitingApproval(string assessorId);
        Task<List<AssessmentRequest>?> GetRequestsMadeOfAssessorAndAccepted(string assessorId);
        Task<List<AssessmentRequest>?> GetRequestsMadeOfAssessorAndDeclined(string assessorId);
    }
}
