using Microsoft.AspNetCore.Identity;
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
    public class AssessmentRequestRepository : GenericRepository<AssessmentRequest>, IAssessmentRequestRepository
    {
        private readonly UserManager<WombatUser> userManager;

        public AssessmentRequestRepository( ApplicationDbContext context,
                                            UserManager<WombatUser> userManager ) : base(context)
        {
            this.userManager = userManager;
        }

        public async Task<List<AssessmentRequest>?> GetTraineePendingAssessments(string traineeId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.TraineeId == traineeId && r.DateAccepted != null && r.DateDeclined == null && r.CompletionDate == null)
                .Include(r => r.Trainee)
                .ToListAsync();

            foreach (var item in requests)
            {
                item.EPA = await context.EPAs
                    .Where(e => e.Id == item.EPAId)
                    .Include(e => e!.SubSpeciality)
                    .ThenInclude(s => s!.Speciality)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetTraineeCompletedAssessments(string traineeId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.TraineeId == traineeId && r.DateAccepted != null && r.DateDeclined == null && r.CompletionDate != null)
                .Include(r => r.Trainee)
                .ToListAsync();

            foreach (var item in requests)
            {
                item.EPA = await context.EPAs
                    .Where(e => e.Id == item.EPAId)
                    .Include(e => e!.SubSpeciality)
                    .ThenInclude(s => s!.Speciality)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetTraineeDeclinedRequests(string traineeId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.TraineeId == traineeId && r.DateAccepted == null && r.DateDeclined != null)
                .Include(r => r.Trainee)
                .ToListAsync();

            foreach (var item in requests)
            {
                item.EPA = await context.EPAs
                    .Where(e => e.Id == item.EPAId)
                    .Include(e => e!.SubSpeciality)
                    .ThenInclude(s => s!.Speciality)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetTraineePendingRequests(string traineeId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.TraineeId == traineeId && r.DateAccepted == null && r.DateDeclined == null)
                .Include(r => r.Trainee)
                .ToListAsync();

            foreach (var item in requests)
            {
                item.EPA = await context.EPAs
                    .Where(e => e.Id == item.EPAId)
                    .Include(e => e!.SubSpeciality)
                    .ThenInclude(s => s!.Speciality)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetAssessorPendingAssessments(string assessorId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.AssessorId == assessorId && r.DateAccepted != null && r.DateDeclined == null && r.CompletionDate==null)
                .Include(r => r.Trainee)
                .ToListAsync();

            foreach (var item in requests)
            {
                item.EPA = await context.EPAs
                    .Where(e => e.Id == item.EPAId)
                    .Include(e => e!.SubSpeciality)
                    .ThenInclude(s => s!.Speciality)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            
            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetAssessorCompletedAssessments(string assessorId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.AssessorId == assessorId && r.DateAccepted != null && r.DateDeclined == null && r.CompletionDate != null)
                .Include(r => r.Trainee)
                .ToListAsync();

            foreach (var item in requests)
            {
                item.EPA = await context.EPAs
                    .Where(e => e.Id == item.EPAId)
                    .Include(e => e!.SubSpeciality)
                    .ThenInclude(s => s!.Speciality)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetAssessorDeclinedRequests(string assessorId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.AssessorId == assessorId && r.DateAccepted == null && r.DateDeclined != null)
                .Include(r => r.Trainee)
                .ToListAsync();

            foreach (var item in requests)
            {
                item.EPA = await context.EPAs
                    .Where(e => e.Id == item.EPAId)
                    .Include(e => e!.SubSpeciality)
                    .ThenInclude(s => s!.Speciality)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetAssessorPendingRequests(string assessorId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.AssessorId == assessorId && r.DateAccepted == null&& r.DateDeclined == null)
                .Include(r => r.Trainee)
                .ToListAsync();

            foreach (var item in requests)
            {
                item.EPA = await context.EPAs
                    .Where(e => e.Id == item.EPAId)
                    .Include(e => e!.SubSpeciality)
                    .ThenInclude(s => s!.Speciality)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }

            return requests;
        }

        public override async Task<AssessmentRequest?> GetAsync(int? id)
        {
            var request = await context.AssessmentRequests
                .Where(r => r.Id == id)
                .Include(r => r.Trainee)
                .FirstOrDefaultAsync();

            if (request != null)
            {
                request.EPA = await context.EPAs
                    .Where(e => e.Id == request.EPAId)
                    .Include(e => e!.SubSpeciality)
                    .ThenInclude(s => s!.Speciality)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            return request;
        }
    }
}
