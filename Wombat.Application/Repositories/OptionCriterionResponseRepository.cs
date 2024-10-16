using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class OptionCriterionResponseRepository : GenericRepository<OptionCriterionResponse>, IOptionCriterionResponseRepository
    {
        public OptionCriterionResponseRepository( ApplicationDbContext context ) : base(context)
        {
        }

        public async Task<List<OptionCriterionResponse>?> GetByAssessmentIdAsync(int assessmentID)
        {
            var responses = await context.OptionCriterionResponses
                .Where(x => x.AssessmentId == assessmentID)
                .Include( o => o.Option)
                .Include(s => s.Criterion)
                .ThenInclude(sc => sc.OptionsSet)
                .ToListAsync();

            //if (specialities != null)
            //{
            //    foreach (var speciality in specialities)
            //    {
            //        var subSpecialities = context.Entry(speciality);

            //        subSpecialities.Collection(e => e.SubSpecialities)
            //             .Query()
            //             .Load();
            //    }
            //}

            return responses;
        }
    }
}
