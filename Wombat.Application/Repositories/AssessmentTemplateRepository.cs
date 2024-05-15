using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class AssessmentTemplateRepository : GenericRepository<AssessmentTemplate>, IAssessmentTemplateRepository
    {
        private readonly IOptionSetRepository optionSetRepository;

        public AssessmentTemplateRepository( ApplicationDbContext context,
                                             IOptionSetRepository optionSetRepository) : base(context)
        {
            this.optionSetRepository=optionSetRepository;
        }

        public override async Task<AssessmentTemplate?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var assessmentTemplate = await base.GetAsync(id);

            if (assessmentTemplate!=null)
            {
                var OptionCriteria = context.Entry(assessmentTemplate);

                OptionCriteria.Collection(e => e.OptionCriteria)
                     .Query()
                     .OrderBy(c => c.Rank)
                     .Load();

                foreach (var optionCriterion in assessmentTemplate.OptionCriteria)
                {
                    optionCriterion.OptionsSet = await optionSetRepository.GetAsync(optionCriterion.OptionSetId);
                }

                return assessmentTemplate;
            }

            return null;
        }
    }
}
