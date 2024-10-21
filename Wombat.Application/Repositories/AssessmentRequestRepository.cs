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
    //internal class AssessmentRequestRepository
    //{
    //}
    public class AssessmentRequestRepository : GenericRepository<AssessmentRequest>, IAssessmentRequestRepository
    {
        //IEPARepository EPARepository;
        //ISpecialityRepository specialityRepository;
        //ISubSpecialityRepository subSpecialityRepository;

        public AssessmentRequestRepository( ApplicationDbContext context/*,
                                            IEPARepository EPARepository,
                                            ISpecialityRepository specialityRepository,
                                            ISubSpecialityRepository subSpecialityRepository */) : base(context)
        {
            //this.EPARepository = EPARepository;
            //this.specialityRepository = specialityRepository;
            //this.subSpecialityRepository = subSpecialityRepository;
        }

        public async Task<List<AssessmentRequest>?> GetRequestsMadeByTraineeAndAccepted(string traineeId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.TraineeId == traineeId && r.DateAccepted != null && r.DateDeclined == null)
                .ToListAsync();

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetRequestsMadeByTraineeAndDeclined(string traineeId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.TraineeId == traineeId && r.DateAccepted == null && r.DateDeclined != null)
                .ToListAsync();

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetRequestsMadeByTraineeAndWaitingApproval(string traineeId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.TraineeId == traineeId && r.DateAccepted == null && r.DateDeclined == null)
                .ToListAsync();

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetRequestsMadeOfAssessorAndAccepted(string assessorId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.AssessorId == assessorId && r.DateAccepted != null && r.DateDeclined == null)
                .Include(r => r.Trainee)
                .Include(r => r.EPA)
                .ThenInclude(e => e!.SubSpeciality)
                .ThenInclude(s => s!.Speciality)
                .ToListAsync();

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetRequestsMadeOfAssessorAndDeclined(string assessorId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.AssessorId == assessorId && r.DateAccepted == null && r.DateDeclined != null)
                .Include(r => r.Trainee)
                .Include(r => r.EPA)
                .ThenInclude(e => e!.SubSpeciality)
                .ThenInclude(s => s!.Speciality)
                .ToListAsync();

            return requests;
        }

        public async Task<List<AssessmentRequest>?> GetRequestsMadeOfAssessorAndWaitingApproval(string assessorId)
        {
            var requests = await context.AssessmentRequests
                .Where(r => r.AssessorId == assessorId && r.DateAccepted == null&& r.DateDeclined == null)
                .Include(r => r.Trainee)
                .Include(r => r.EPA)
                .ThenInclude(e => e!.SubSpeciality)
                .ThenInclude(s => s!.Speciality)
                .ToListAsync();

            return requests;
        }

        public override async Task<AssessmentRequest?> GetAsync(int? id)
        {
            var request = await context.AssessmentRequests
                .Where(r => r.Id == id)
                .Include(r => r.Trainee)
                .Include(r => r.EPA)
                .ThenInclude(e => e!.SubSpeciality)
                .ThenInclude(s => s!.Speciality)
                .FirstOrDefaultAsync();

            return request;
        }
    }
}
