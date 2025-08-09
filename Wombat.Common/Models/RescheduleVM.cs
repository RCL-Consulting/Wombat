// Wombat.Common.Models/RescheduleVM.cs
using System;
using System.ComponentModel.DataAnnotations;
using Wombat.Common.Constants;

public class RescheduleVM
{
    public int Id { get; set; }

    [Display(Name = "Current status")]
    public AssessmentRequestStatus Status { get; set; }

    [Display(Name = "Current date/time")]
    public DateTime? CurrentAssessmentDate { get; set; }

    [Required]
    [Display(Name = "New date/time")]
    public DateTime NewAssessmentDate { get; set; }

    // Optional note to include in the event/notification (service can append old/new)
    [StringLength(500)]
    [Display(Name = "Note (optional)")]
    public string? Message { get; set; }

    // For showing the right warning text in the view
    public bool IsTrainee { get; set; }
    public bool IsAssessor { get; set; }
}
