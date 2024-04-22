using Wombat.Data;

namespace Wombat.Contracts
{
    public interface IOptionCriterionResponseRepository : IGenericRepository<OptionCriterionResponse>
    {
        Task<List<OptionCriterionResponse>?> GetByAssessmentIdAsync(int assessmentID);
    }
}