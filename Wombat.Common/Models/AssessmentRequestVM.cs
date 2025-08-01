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

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public enum AssessmentRequestStatus
    {
        Requested,
        Accepted,
        Declined,
        CancelledByTrainee,
        CancelledByAssessor,
        Completed
    }
    public class AssessmentRequestVM
    {
        public int Id { get; set; }

        [Display(Name = "Trainee")]
        public string TraineeDisplayName { get { return Trainee?.Name + " " + Trainee?.Surname + " (" + Trainee?.Email + ")"; } }

        [Display(Name = "EPA")]
        public string FullEPADisplayName { get { return EPA?.Name + " - " + EPA?.Description; } }

        [Display(Name = "EPA")]
        public string ShortEPADisplayName 
        { 
            get { 
                if(EPA!=null)
                    return EPA?.Name; 
                else
                    return "";
            }
        }

        public string AssessorNotes { get; set; } = "";

        [Display(Name = "Date requested")]
        public DateTime? DateRequested { get; set; }

        [Display(Name = "Date accepted")]
        public DateTime? DateAccepted { get; set; }

        [Display(Name = "Date declined")]
        public DateTime? DateDeclined { get; set; }

        [Display(Name = "Assessment date")]
        public DateTime? AssessmentDate { get; set; }

        [Display(Name = "Completion date")]
        public DateTime?CompletionDate { get; set; }

        public string TraineeId { get; set; } = "";
        public WombatUserVM? Trainee { get; set; }

        public string AssessorId { get; set; } = "";
        public WombatUserVM? Assessor { get; set; }

        public int EPAId { get; set; } = 0;
        public EPAVM? EPA { get; set; }

        public int? AssessmentFormId { get; set; } = 0;
        public AssessmentFormVM? AssessmentForm { get; set; }

        public string Notes { get; set; } = "";
    }
}
