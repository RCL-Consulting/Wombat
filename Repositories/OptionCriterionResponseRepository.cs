﻿using Microsoft.EntityFrameworkCore;
using Wombat.Contracts;
using Wombat.Data;

namespace Wombat.Repositories
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
                .ToListAsync();

            return responses;
        }
    }
}