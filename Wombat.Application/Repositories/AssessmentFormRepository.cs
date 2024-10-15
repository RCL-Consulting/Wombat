using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class AssessmentFormRepository : GenericRepository<AssessmentForm>, IAssessmentFormRepository
    {
        private readonly IOptionSetRepository optionSetRepository;

        public AssessmentFormRepository( ApplicationDbContext context,
                                         IOptionSetRepository optionSetRepository ) : base(context)
        {
            this.optionSetRepository=optionSetRepository;
        }

        public override async Task<AssessmentForm?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var assessmentForm = await base.GetAsync(id);

            if (assessmentForm != null)
            {
                var OptionCriteria = context.Entry(assessmentForm);

                OptionCriteria.Collection(e => e.OptionCriteria)
                     .Query()
                     .OrderBy(c => c.Rank)
                     .Load();

                foreach (var optionCriterion in assessmentForm.OptionCriteria)
                {
                    optionCriterion.OptionsSet = await optionSetRepository.GetAsync(optionCriterion.OptionSetId);
                }

                return assessmentForm;
            }

            return null;
        }
    }
}
