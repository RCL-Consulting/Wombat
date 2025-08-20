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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Wombat.Application.Contracts;
using Wombat.Data;
using Wombat.Common.Constants;

namespace Wombat.Application.Repositories
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> AndAlso<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var param = Expression.Parameter(typeof(T), "r");

            var body = Expression.AndAlso(
                Expression.Invoke(expr1, param),
                Expression.Invoke(expr2, param)
            );

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

    public class AssessmentRequestRepository : GenericRepository<AssessmentRequest>, IAssessmentRequestRepository
    {
        private readonly UserManager<WombatUser> userManager;

        public AssessmentRequestRepository( ApplicationDbContext context,
                                            UserManager<WombatUser> userManager ) : base(context)
        {
            this.userManager = userManager;
        }

        public async Task<List<AssessmentRequest>?> GetPendingRequestsAsync(Expression<Func<AssessmentRequest, bool>> predicate)
        {
            var now = DateTime.UtcNow;

            var requests = await context.AssessmentRequests
                .Where(predicate)
                .Where(r => r.Status == AssessmentRequestStatus.Requested &&
                    (!r.AssessmentDate.HasValue || r.AssessmentDate > now)
                )
                .Include(r => r.Trainee)
                .Include(r => r.Assessor)
                .Include(r => r.EPA)
                    .ThenInclude(e => e!.SubSpeciality)
                        .ThenInclude(s => s!.Speciality)
                .AsNoTracking()
                .ToListAsync();

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetPendingAssessmentsAsync(Expression<Func<AssessmentRequest, bool>> predicate)
        {
            var now = DateTime.UtcNow;

            Expression<Func<AssessmentRequest, bool>> pendingPredicate = r =>
                r.Status == AssessmentRequestStatus.Accepted &&
                r.CompletionDate == null &&
                r.AssessmentDate > now;

            var combined = predicate.AndAlso(pendingPredicate);

            var requests = await context.AssessmentRequests
                .Where(combined)
                .Include(r => r.Trainee)
                .Include(r => r.Assessor)
                .Include(r => r.EPA)
                    .ThenInclude(e => e.SubSpeciality)
                        .ThenInclude(s => s.Speciality)
                .AsNoTracking()
                .ToListAsync();

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetExpiredRequestsAsync(Expression<Func<AssessmentRequest, bool>> predicate)
        {
            var now = DateTime.UtcNow;

            var expiredPredicate = predicate.AndAlso(r =>
                r.Status == AssessmentRequestStatus.Requested &&
                r.AssessmentDate.HasValue &&
                r.AssessmentDate <= now
            );

            var requests = await context.AssessmentRequests
                .Where(expiredPredicate)
                .Include(r => r.Trainee)
                .Include (r => r.Assessor)
                .Include(r => r.EPA)
                    .ThenInclude(e => e!.SubSpeciality)
                        .ThenInclude(s => s!.Speciality)
                .AsNoTracking()
                .ToListAsync();

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetNotConductedAssessmentsAsync(Expression<Func<AssessmentRequest, bool>> predicate)
        {
            var now = DateTime.UtcNow;

            var notConductedPredicate = predicate.AndAlso(r =>
               r.Status == AssessmentRequestStatus.Accepted &&
                r.CompletionDate == null &&
                r.AssessmentDate.HasValue &&
                r.AssessmentDate <= now
            );

            var requests = await context.AssessmentRequests
                .Where(notConductedPredicate)
                .Include(r => r.Trainee)
                .Include(r => r.Assessor)
                .Include(r => r.EPA)
                    .ThenInclude(e => e!.SubSpeciality)
                        .ThenInclude(s => s!.Speciality)
                .AsNoTracking()
                .ToListAsync();

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetCompletedAssessmentsAsync(Expression<Func<AssessmentRequest, bool>> userPredicate)
        {
            // Common predicate for completed assessments
            Expression<Func<AssessmentRequest, bool>> completedPredicate = r =>
                r.Status == AssessmentRequestStatus.Completed;

            // Combine predicates
            var combined = userPredicate.AndAlso(completedPredicate);

            // Query with full eager loading
            var requests = await context.AssessmentRequests
                .Where(combined)
                .Include(r => r.Trainee)
                .Include(r => r.Assessor)
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

        public async Task<List<AssessmentRequest>?> GetDeclinedRequestsAsync(Expression<Func<AssessmentRequest, bool>> userPredicate)
        {
            // Common filter for declined requests
            Expression<Func<AssessmentRequest, bool>> declinedPredicate = r =>
                r.Status == AssessmentRequestStatus.Declined;

            // Combine user and common predicates
            var combined = userPredicate.AndAlso(declinedPredicate);

            // Query with eager loading
            var requests = await context.AssessmentRequests
                .Where(combined)
                .Include(r => r.Trainee)
                .Include(r => r.Assessor)
                .Include(r => r.EPA)
                    .ThenInclude(e => e.SubSpeciality)
                        .ThenInclude(s => s.Speciality)
                .AsNoTracking()
                .ToListAsync();

            return requests;
        }

        public override async Task<AssessmentRequest?> GetAsync(int? id)
        {
            if (id is null) return null;

            var qry = context.AssessmentRequests
                .Where(r => r.Id == id)
                .Include(r => r.Trainee)
                .Include(r => r.Assessor)
                .Include(r => r.AssessmentForm) // <-- add this
                .Include(r => r.LoggedAssessment)
                    .ThenInclude(la => la.OptionCriterionResponses!)
                        .ThenInclude(o => o.Option)
                .Include(r => r.LoggedAssessment)
                    .ThenInclude(la => la.OptionCriterionResponses!)
                        .ThenInclude(o => o.Criterion)
                .Include(r => r.EPA)
                    .ThenInclude(e => e.SubSpeciality)
                        .ThenInclude(s => s.Speciality)
                .AsSplitQuery(); // safer with multiple includes

            return await qry.FirstOrDefaultAsync();
        }
    }
}
