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
        public AssessmentRequestRepository( ApplicationDbContext context ) : base(context)
        {
        }
    }
}
