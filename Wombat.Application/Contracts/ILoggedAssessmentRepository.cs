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

using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface ILoggedAssessmentRepository : IGenericRepository<LoggedAssessment>
    {
        Task<List<LoggedAssessment>?> GetAssessmentsByTraineeAsync(string id);
        Task<List<LoggedAssessment>?> GetAssessmentsByAssessorAsync(string id);
        Task<LoggedAssessment?> GetAssessmentByRequestAsync(int? id);
        Task<Dictionary<int, int>?> GetTotalAssessmentsPerEPAByTrainee(List<int> epaIds, string traineeId);
        Task<Dictionary<int, int>?> GetVisibleAssessmentsPerEPAByTrainee(List<int> epaIds, string traineeId);
        

        Task<Dictionary<int, int>?> GetVisibleScorePerEPAByTrainee(List<int> epaIds, string traineeId);
        
        Task<List<LoggedAssessment>?> GetAssessmentsByEPAAndTraineeAsync(int epaId, string traineeId);
        Task<List<LoggedAssessment>?> GetVisibleAssessmentsPerEPAByTrainee(int epaId, string traineeId);
    }
}
