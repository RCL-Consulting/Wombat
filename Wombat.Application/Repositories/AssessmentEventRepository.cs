using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class AssessmentEventRepository : GenericRepository<AssessmentEvent>, IAssessmentEventRepository
    {

        public AssessmentEventRepository(ApplicationDbContext context ) : base(context)
        {
        }

        public async Task<List<AssessmentEvent>> GetEventsForRequestAsync(int requestId)
        {
            return await context.AssessmentEvents
                .Where(e => e.AssessmentRequestId == requestId)
                .Include(e => e.Actor)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AssessmentEvent>> GetEventsForAssessmentAsync(int loggedAssessmentId)
        {
            return await context.AssessmentEvents
                .Where(e => e.LoggedAssessmentId == loggedAssessmentId)
                .Include(e => e.Actor)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }
    }
}
