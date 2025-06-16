using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface ISTARApplicationFormRepository : IGenericRepository<STARApplicationForm>
    {
        Task<STARApplicationForm?> GetByEPAIdAsync(int epaId);
    }

}
