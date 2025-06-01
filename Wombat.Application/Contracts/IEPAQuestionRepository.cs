using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface IEPAQuestionRepository : IGenericRepository<STARItem>
    {
        Task<List<STARItem>> GetByEPAIdAsync(int epaId);
        Task<List<EPA>> GetAllWithSpecialitiesAndQuestionCountsAsync();
    }

}
