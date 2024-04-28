using Wombat.Data;

namespace Wombat.Contracts
{
    public interface ILoggedAssessmentRepository : IGenericRepository<LoggedAssessment>
    {
        Task<List<LoggedAssessment>?> GetAssessmntsbyTraineeAsync(string id);
        Task<List<LoggedAssessment>?> GetAssessmntsbyAssessorAsync(string id);
    }
}
