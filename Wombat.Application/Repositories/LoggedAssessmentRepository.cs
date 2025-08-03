/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class LoggedAssessmentRepository : GenericRepository<LoggedAssessment>, ILoggedAssessmentRepository
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly IEPARepository EPARepository;
        private readonly IOptionCriterionResponseRepository optionCriterionResponse;

        public LoggedAssessmentRepository( ApplicationDbContext context,
                                           UserManager<WombatUser> userManager,
                                           IEPARepository EPA,
                                           IOptionCriterionResponseRepository optionCriterionResponse) : base(context)
        {
            this.userManager=userManager;
            this.EPARepository=EPA;
            this.optionCriterionResponse=optionCriterionResponse;
        }

        public async Task<List<LoggedAssessment>> GetByRequestIdsAsync(List<int> requestIds)
        {
            return await context.LoggedAssessments
                .Include(a => a.OptionCriterionResponses)
                .Where(a => a.AssessmentRequestId != null && requestIds.Contains(a.AssessmentRequestId.Value))
                .ToListAsync();
        }

        public override async Task<LoggedAssessment?> GetAsync(int? id)
        {
            var assessment = await base.GetAsync(id);

            if (assessment != null)
            {
                assessment.Trainee = await userManager.FindByIdAsync(assessment.TraineeId);
                assessment.Assessor = await userManager.FindByIdAsync(assessment.AssessorId);
                assessment.EPA = await EPARepository.GetAsync(assessment.EPAId);

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
                    assessment.EPA = await EPARepository.GetAsync(assessment.EPAId);
                }
            }
            return assessments;
        }

        public async Task<List<LoggedAssessment>?> GetAssessmentsByTraineeAsync(string id)
        {
            var assessments = await context.LoggedAssessments
                .Where(x => x.TraineeId == id)
                .ToListAsync();

            if (assessments != null)
            {
                foreach (var assessment in assessments)
                {
                    assessment.Trainee = await userManager.FindByIdAsync(assessment.TraineeId);
                    assessment.Assessor = await userManager.FindByIdAsync(assessment.AssessorId);
                    assessment.EPA = await EPARepository.GetAsync(assessment.EPAId);
                }
            }
            return assessments;
        }

        public async Task<List<LoggedAssessment>?> GetAssessmentsByAssessorAsync(string id)
        {
            var assessments = await context.LoggedAssessments
                .Where(x => x.AssessorId == id)
                .ToListAsync();

            if (assessments != null)
            {
                foreach (var assessment in assessments)
                {
                    assessment.Trainee = await userManager.FindByIdAsync(assessment.TraineeId);
                    assessment.Assessor = await userManager.FindByIdAsync(assessment.AssessorId);
                    assessment.EPA = await EPARepository.GetAsync(assessment.EPAId);
                }
            }
            return assessments;
        }

        public async Task<LoggedAssessment?> GetAssessmentByRequestAsync(int? id)
        {
            var assessment = await context.LoggedAssessments
                .Where(x => x.AssessmentRequestId == id)
                .FirstOrDefaultAsync();

            if (assessment != null)
            {
                assessment.Trainee = await userManager.FindByIdAsync(assessment.TraineeId);
                assessment.Assessor = await userManager.FindByIdAsync(assessment.AssessorId);
                assessment.EPA = await EPARepository.GetAsync(assessment.EPAId);

                assessment.OptionCriterionResponses = await optionCriterionResponse.GetByAssessmentIdAsync(assessment.Id);
            }
            return assessment;
        }

        public async Task<Dictionary<int, int>?> GetTotalAssessmentsPerEPAByTrainee(List<int> epaIds, string traineeId)
        {
            var Counts = await context.LoggedAssessments
                .Where(x => epaIds.Contains(x.EPAId) && x.TraineeId == traineeId)
                .GroupBy(x => x.EPAId)
                .Select(x => new { EPAId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.EPAId, x => x.Count);

            return Counts;
        }

        public async Task<Dictionary<int, int>?> GetVisibleAssessmentsPerEPAByTrainee(List<int> epaIds, string traineeId)
        {
            var Counts = await context.LoggedAssessments
                .Where(x => epaIds.Contains(x.EPAId) && x.TraineeId == traineeId && x.AssessmentIsPublic==true)
                .GroupBy(x => x.EPAId)
                .Select(x => new { EPAId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.EPAId, x => x.Count);

            return Counts;
        }

        public async Task<Dictionary<int, int>?> GetVisibleScorePerEPAByTrainee(List<int> epaIds, string traineeId)
        {
            // Fetch all public assessments for the given EPAs and trainee in a single query
            var publicAssessments = await context.LoggedAssessments
                .Where(x => epaIds.Contains(x.EPAId) && x.TraineeId == traineeId && x.AssessmentIsPublic)
                .Select(x => new { x.EPAId, x.Id })
                .ToListAsync();

            // Fetch ranks for all relevant assessments in one go
            var assessmentRanks = await context.OptionCriterionResponses
                .Include(x => x.Criterion)
                .Include(x => x.Option)
                .Where(x => publicAssessments.Select(p => p.Id).Contains(x.AssessmentId) &&
                            x.Criterion.OptionSetId == OptionSet.kEPAScaleId)
                .GroupBy(x => x.AssessmentId)
                .Select(group => new
                {
                    AssessmentId = group.Key,
                    MaxRank = group.Sum(x => x.Option.Rank)
                })
                .ToListAsync();

            // Create a dictionary to store the highest rank per EPA
            var result = publicAssessments
                .GroupBy(x => x.EPAId)
                .Select(epaGroup => new
                {
                    EPAId = epaGroup.Key,
                    HighestRank = epaGroup
                        .Join(assessmentRanks,
                              epa => epa.Id,
                              rank => rank.AssessmentId,
                              (epa, rank) => rank.MaxRank)
                        .DefaultIfEmpty(0)  // In case there are no ranks for the EPA
                        .Max()
                })
                .ToDictionary(x => x.EPAId, x => x.HighestRank);

            return result;
        }

        public async Task<List<LoggedAssessment>?> GetAssessmentsByEPAAndTraineeAsync(int epaId, string traineeId)
        {
            var assessments = await context.LoggedAssessments
                .Where(x => x.EPAId == epaId && x.TraineeId == traineeId)
                .ToListAsync();

            foreach (var item in assessments)
            {
                item.EPA = await EPARepository.GetAsync(epaId);
                item.Assessor = await userManager.FindByIdAsync(item.AssessorId);

                item.OptionCriterionResponses = await optionCriterionResponse.GetByAssessmentIdAsync(item.Id);
            }

            return assessments;
        }

        public async Task<List<LoggedAssessment>?> GetVisibleAssessmentsPerEPAByTrainee(int epaId, string traineeId)
        {
            var assessments = await context.LoggedAssessments
                .Where(x => x.EPAId == epaId && x.TraineeId == traineeId && x.AssessmentIsPublic==true)
                .ToListAsync();

            foreach (var item in assessments)
            {
                item.EPA = await EPARepository.GetAsync(epaId);
                item.Assessor = await userManager.FindByIdAsync(item.AssessorId);

                item.OptionCriterionResponses = await optionCriterionResponse.GetByAssessmentIdAsync(item.Id);
            }

            return assessments;
        }

    }
}
