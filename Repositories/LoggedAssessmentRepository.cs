using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wombat.Contracts;
using Wombat.Data;

namespace Wombat.Repositories
{
    public class LoggedAssessmentRepository : GenericRepository<LoggedAssessment>, ILoggedAssessmentRepository
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly IAssessmentContextRepository assessmentContextRepository;
        private readonly IOptionCriterionResponseRepository optionCriterionResponse;

        public LoggedAssessmentRepository( ApplicationDbContext context,
                                           UserManager<WombatUser> userManager,
                                           IAssessmentContextRepository assessmentContextRepository,
                                           IOptionCriterionResponseRepository optionCriterionResponse) : base(context)
        {
            this.userManager=userManager;
            this.assessmentContextRepository=assessmentContextRepository;
            this.optionCriterionResponse=optionCriterionResponse;
        }

        public override async Task<LoggedAssessment?> GetAsync(int? id)
        {
            var assessment = await base.GetAsync(id);

            if (assessment != null)
            {
                assessment.Trainee = await userManager.FindByIdAsync(assessment.TraineeId);
                assessment.Assessor = await userManager.FindByIdAsync(assessment.AssessorId);
                assessment.AssessmentContext = await assessmentContextRepository.GetAsync(assessment.AssessmentContextId);

                assessment.OptionCriterionResponses = await optionCriterionResponse.GetByAssessmentIdAsync(assessment.Id);
            }

            return assessment;
        }

        public override async Task<List<LoggedAssessment>?> GetAllAsync()
        {
            var assessments = await base.GetAllAsync();

            if( assessments != null )
            {
                foreach( var assessment in assessments )
                {
                    assessment.Trainee = await userManager.FindByIdAsync(assessment.TraineeId);
                    assessment.Assessor = await userManager.FindByIdAsync(assessment.AssessorId);
                    assessment.AssessmentContext = await assessmentContextRepository.GetAsync(assessment.AssessmentContextId);
                }
            }
            return assessments;
        }
    }
}
