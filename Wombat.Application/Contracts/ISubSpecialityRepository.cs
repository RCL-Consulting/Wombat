using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface ISubSpecialityRepository : IGenericRepository<SubSpeciality>
    {
        Task<List<SubSpeciality>?> GetSubSpecialitiesBySpecialityAsync(int id);
    }
}
