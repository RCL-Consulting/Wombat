using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface ILoggedAssessmentRepository : IGenericRepository<LoggedAssessment>
    {
        Task<List<LoggedAssessment>?> GetAssessmentsByTraineeAsync(string id);
        Task<List<LoggedAssessment>?> GetAssessmentsByAssessorAsync(string id);

        Task<LoggedAssessment?> GetAssessmentByRequestAsync(int? id);
    }
}
