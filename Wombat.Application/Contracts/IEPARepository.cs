﻿using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface IEPARepository : IGenericRepository<EPA>
    {
        Task<List<EPA>?> GetEPAListBySubspeciality(int id);
    }

}
