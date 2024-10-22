using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface ILoggedAssessmentRepository : IGenericRepository<LoggedAssessment>
    {
        Task<List<LoggedAssessment>?> GetAssessmentsByTraineeAsync(string id);
        Task<List<LoggedAssessment>?> GetAssessmentsByAssessorAsync(string id);
        Task<LoggedAssessment?> GetAssessmentByRequestAsync(int? id);
        Task<Dictionary<int, int>?> GetTotalAssessmentsPerEPAByTrainee(List<int> epaIds, string traineeId);
        Task<Dictionary<int, int>?> GetVisibleAssessmentsPerEPAByTrainee(List<int> epaIds, string traineeId);
        

        Task<Dictionary<int, int>?> GetVisibleScorePerEPAByTrainee(List<int> epaIds, string traineeId);
        
        Task<List<LoggedAssessment>?> GetAssessmentsByEPAAndTraineeAsync(int epaId, string traineeId);
        Task<List<LoggedAssessment>?> GetVisibleAssessmentsPerEPAByTrainee(int epaId, string traineeId);
    }
}
