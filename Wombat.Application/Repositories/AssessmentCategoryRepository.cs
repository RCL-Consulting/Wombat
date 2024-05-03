using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class AssessmentCategoryRepository : GenericRepository<AssessmentCategory>, IAssessmentCategoryRepository
    {
        private readonly IOptionSetRepository optionSetRepository;

        public AssessmentCategoryRepository( ApplicationDbContext context,
                                             IOptionSetRepository optionSetRepository) : base(context)
        {
            this.optionSetRepository=optionSetRepository;
        }

        public override async Task<AssessmentCategory?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var assessmentCategory = await base.GetAsync(id);

            if (assessmentCategory!=null)
            {
                var OptionCriteria = context.Entry(assessmentCategory);

                OptionCriteria.Collection(e => e.OptionCriteria)
                     .Query()
                     .OrderBy(c => c.Rank)
                     .Load();

                foreach (var optionCriterion in assessmentCategory.OptionCriteria)
                {
                    optionCriterion.OptionsSet = await optionSetRepository.GetAsync(optionCriterion.OptionSetId);
                }

                return assessmentCategory;
            }

            return null;
        }
    }
}
