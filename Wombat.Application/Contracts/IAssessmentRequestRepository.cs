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
        Task<List<AssessmentRequest>?> GetTraineePendingRequests(string traineeId);
        Task<List<AssessmentRequest>?> GetTraineePendingAssessments(string traineeId);
        Task<List<AssessmentRequest>?> GetTraineeCompletedAssessments(string traineeId);
        Task<List<AssessmentRequest>?> GetTraineeDeclinedRequests(string traineeId);

        Task<List<AssessmentRequest>?> GetAssessorPendingRequests(string assessorId);
        Task<List<AssessmentRequest>?> GetAssessorPendingAssessments(string assessorId);
        Task<List<AssessmentRequest>?> GetAssessorCompletedAssessments(string assessorId); 
        Task<List<AssessmentRequest>?> GetAssessorDeclinedRequests(string assessorId);
    }
}
