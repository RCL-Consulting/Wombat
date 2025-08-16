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
    public enum Role
    {
        Administrator,
        InstitutionalAdmin,
        SpecialityAdmin,
        SubSpecialityAdmin,
        Assessor,
        Coordinator,
        CommitteeMember,
        Trainee,
        PendingTrainee
    }
    public static class RoleStrings
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
    }

    public static class RoleHelper
    {
        private static readonly Dictionary<Role, string> _roleToString = new()
        {
            [Role.Administrator] = RoleStrings.Administrator,
            [Role.InstitutionalAdmin] = RoleStrings.InstitutionalAdmin,
            [Role.SpecialityAdmin] = RoleStrings.SpecialityAdmin,
            [Role.SubSpecialityAdmin] = RoleStrings.SubSpecialityAdmin,
            [Role.Assessor] = RoleStrings.Assessor,
            [Role.Coordinator] = RoleStrings.Coordinator,
            [Role.CommitteeMember] = RoleStrings.CommitteeMember,
            [Role.Trainee] = RoleStrings.Trainee,
            [Role.PendingTrainee] = RoleStrings.PendingTrainee
        };

        private static readonly Dictionary<string, Role> _stringToRole =
            _roleToString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        private static readonly Dictionary<Role, string> _displayNames = new()
        {
            [Role.Administrator] = "System Administrator",
            [Role.InstitutionalAdmin] = "Institution Administrator",
            [Role.SpecialityAdmin] = "Speciality Administrator",
            [Role.SubSpecialityAdmin] = "Subspeciality Administrator",
            [Role.Assessor] = "Assessor",
            [Role.Coordinator] = "Coordinator",
            [Role.CommitteeMember] = "Committee Member",
            [Role.Trainee] = "Trainee",
            [Role.PendingTrainee] = "Pending Trainee"
        };

        public static string ToStringValue(this Role role) => _roleToString[role];

        public static Role? FromStringValue(string value) =>
            _stringToRole.TryGetValue(value, out var role) ? role : null;

        public static string GetDisplayName(this Role role) => _displayNames[role];

        public static string? GetDisplayName(string roleString) =>
            FromStringValue(roleString) is Role role ? _displayNames[role] : null;

        public static IEnumerable<Role> AllRoles => _roleToString.Keys;

        public static IEnumerable<Role> DisplayRoles =>
            AllRoles.Where(r => r != Role.PendingTrainee);
    }

    public static class RoleHierarchy
    {
        private static readonly Dictionary<string, int> RoleRanks = new()
        {
            { Role.Administrator.ToStringValue(), 100 },
            { Role.InstitutionalAdmin.ToStringValue(), 90 },
            { Role.SpecialityAdmin.ToStringValue(), 80 },
            { Role.SubSpecialityAdmin.ToStringValue(), 70 },
            { Role.Coordinator.ToStringValue(), 60 },
            { Role.CommitteeMember.ToStringValue(), 50 },
            { Role.Assessor.ToStringValue(), 40 },
            { Role.Trainee.ToStringValue(), 30 },
            { Role.PendingTrainee.ToStringValue(), 10 }
        };

        public static int GetRank(string role) =>
            RoleRanks.TryGetValue(role, out var rank) ? rank : 0;

        public static bool CanAssign(string currentUserRole, string targetRole) =>
            GetRank(currentUserRole) >= GetRank(targetRole);
    }
}
