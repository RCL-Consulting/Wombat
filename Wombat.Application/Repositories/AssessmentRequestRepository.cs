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
                .Where(r => r.TraineeId == traineeId &&
                            r.DateAccepted != null &&
                            r.DateDeclined == null &&
                            r.CompletionDate != null)
                .Include(r => r.Trainee)
                .Include(r => r.LoggedAssessment)
                    .ThenInclude(la => la.OptionCriterionResponses!)
                        .ThenInclude(o => o.Option)
                .Include(r => r.LoggedAssessment)
                    .ThenInclude(la => la.OptionCriterionResponses!)
                        .ThenInclude(o => o.Criterion)
                .Include(r => r.EPA)
                    .ThenInclude(e => e.SubSpeciality)
                        .ThenInclude(s => s.Speciality)
                .AsNoTracking()
                .ToListAsync();

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
