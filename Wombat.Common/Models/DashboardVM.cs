﻿/*Copyright (C) 2024 RCL Consulting
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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class DashboardVM
    {
        public WombatUserVM User { get; set; }

        public List<EPAVM> EPAList { get; set; }

        [Display(Name = "Declined requests")]
        public int NumberOfRequestsDeclined { get; set; } = 0;

        [Display(Name = "Pending requests")]
        public int NumberOfRequestsMade { get; set; } = 0;

        [Display(Name = "Pending assessments")]
        public int NumberOfPendingAssessments { get; set; } = 0;

        [Display(Name = "Completed assessments")]
        public int NumberOfCompletedAssessments { get; set; } = 0;

        public Dictionary<int, int> TotalAssessmentsPerEPA { get; set; } = new();
        public Dictionary<int, int> VisibleAssessmentsPerEPA { get; set; } = new();

        // New: rating tracking
        public Dictionary<int, int> ExpectedRatingPerEPA { get; set; } = new();
        public Dictionary<int, int> HighestRatingPerEPA { get; set; } = new();

        public Dictionary<int, int> LatestRatingPerEPA { get; set; } = new();

        public int MonthsInTraining { get; set; } = 0;

        // Optional: future extension
        public Dictionary<int, int> MonthsInTrainingPerEPA { get; set; } = new();

        public string UserName { get; set; }
        public CoordinatorDashboardVM Coordinator { get; set; }

        // Administrator statistics
        public int NumberOfInstitutions { get; set; }
        public int NumberOfSpecialities { get; set; }
        public int NumberOfSubSpecialities { get; set; }
        public int NumberOfAssessmentForms { get; set; }
        public int NumberOfEPAs { get; set; }
        public int NumberOfUsers { get; set; }
        public int NumberOfInvitations { get; set; }

        public List<InstitutionVM> RecentInstitutions { get; set; } = new();

        public List<AssessmentRequestVM> AcceptedRequests { get; set; } = new();

        public List<AssessmentRequestVM> PendingRequests { get; set; } = new();
        public List<AssessmentRequestVM> DeclinedRequests { get; set; } = new();
    }
}
