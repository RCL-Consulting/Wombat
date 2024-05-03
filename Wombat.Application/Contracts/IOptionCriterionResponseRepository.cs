using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface IOptionCriterionResponseRepository : IGenericRepository<OptionCriterionResponse>
    {
        Task<List<OptionCriterionResponse>?> GetByAssessmentIdAsync(int assessmentID);
    }
}