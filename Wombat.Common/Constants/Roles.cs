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

using Microsoft.Identity.Client;

namespace Wombat.Common.Constants
{
    public class Roles
    {
        public const string Administrator = "Administrator";
        public const string InstitutionalAdmin = "InstitutionalAdmin";
        public const string SpecialityAdmin = "SpecialityAdmin";
        public const string SubSpecialityAdmin = "SubSpecialityAdmin";
        public const string Assessor = "Assessor";
        public const string Coordinator = "Coordinator";
        public const string CommitteeMember = "CommitteeMember";
        public const string Trainee = "Trainee";
        public const string PendingTrainee = "PendingTrainee";

        /// <summary>
        /// Returns all defined roles (excluding Unassigned, unless needed)
        /// </summary>
        public static IEnumerable<string> AllForSystem()
        {
            yield return Administrator;
            yield return InstitutionalAdmin;
            yield return SpecialityAdmin;
            yield return SubSpecialityAdmin;
            yield return Assessor;
            yield return Coordinator;
            yield return CommitteeMember;
            yield return Trainee;
            yield return PendingTrainee;
        }

        public static IEnumerable<string> AllForDisplay()
        {
            yield return Administrator;
            yield return InstitutionalAdmin;
            yield return SpecialityAdmin;
            yield return SubSpecialityAdmin;
            yield return Assessor;
            yield return Coordinator;
            yield return CommitteeMember;
            yield return Trainee;
        }
    }
}
