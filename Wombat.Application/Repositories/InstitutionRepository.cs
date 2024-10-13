using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class InstitutionRepository : GenericRepository<Institution>, IInstitutionRepository
    {

        public InstitutionRepository(ApplicationDbContext context) : base(context)
        {
        }

    }
}
