using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mono.TextTemplating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Models;

namespace Wombat.Data.Configurations.Entities
{
    public class OptionCriterionConfiguration : IEntityTypeConfiguration<OptionCriterion>
    {
        public void Configure(EntityTypeBuilder<OptionCriterion> builder)
        {
            builder.HasData(
                new OptionCriterion
                {
                    Id = 1,
                    Description = "Assessment rating",
                    OptionSetId = 2,
                    Rank = 1000,
                    AssessmentFormId = 1
                },
                new OptionCriterion
                {
                    Id = 2,
                    Description = "Briefly state at least one observation that supports the EPA rating you assigned",
                    OptionSetId = 1,
                    Rank = 1001,
                    AssessmentFormId = 1
                },
                new OptionCriterion
                {
                    Id = 3,
                    Description = 
                        "Briefly state at least one thing that needs to be demonstrated by the trainee "+
                        "to advance the EPA rating to the next level",
                    OptionSetId = 1,
                    Rank = 1002,
                    AssessmentFormId = 1
                },
                //EPA1
                new OptionCriterion
                {
                    Id = 4,
                    Description =
                        "Establishing and maintaining working relationships with the referring "+
                        "providers/agencies, marked by bidirectional communication",
                    OptionSetId = 3,
                    Rank = 1,
                    AssessmentFormId = 2
                },
                new OptionCriterion
                {
                    Id = 5,
                    Description =
                        "Clarifying and focusing the clinical question to be addressed",
                    OptionSetId = 3,
                    Rank = 2,
                    AssessmentFormId = 2
                },
                new OptionCriterion
                {
                    Id = 6,
                    Description =
                        "Gathering essential information from referring physician, organization, or "+
                        "health agency, as well as the patient(s) and family",
                    OptionSetId = 3,
                    Rank = 3,
                    AssessmentFormId = 2
                },
                new OptionCriterion
                {
                    Id = 7,
                    Description =
                        "Communicating findings and recommendations to the patient and family, and the "+
                        "source of the referral (i.e.,the requesting provider or health agency)",
                    OptionSetId = 3,
                    Rank = 4,
                    AssessmentFormId = 2
                },
                new OptionCriterion
                {
                    Id = 8,
                    Description =
                        "Demonstrating content expertise in one’s area of pediatrics to provide consultation",
                    OptionSetId = 3,
                    Rank = 5,
                    AssessmentFormId = 2
                },
                new OptionCriterion
                {
                    Id = 9,
                    Description =
                        "Navigating the relationship with the patient/family to be either supportive or "+
                        "directive (or some combination of the two) as needed over time",
                    OptionSetId = 3,
                    Rank = 6,
                    AssessmentFormId = 2
                },
                new OptionCriterion
                {
                    Id = 10,
                    Description = "Assessment rating",
                    OptionSetId = 2,
                    Rank = 1000,
                    AssessmentFormId = 2
                },
                new OptionCriterion
                {
                    Id = 11,
                    Description = "Briefly state at least one observation that supports the EPA rating you assigned",
                    OptionSetId = 1,
                    Rank = 1001,
                    AssessmentFormId = 2
                },
                new OptionCriterion
                {
                    Id = 12,
                    Description =
                        "Briefly state at least one thing that needs to be demonstrated by the trainee " +
                        "to advance the EPA rating to the next level",
                    OptionSetId = 1,
                    Rank = 1002,
                    AssessmentFormId = 2
                },
                //EPA2
                new OptionCriterion
                {
                    Id = 13,
                    Description =
                        "Applying knowledge in selection and interpretation of screening tools and tests "+
                        "(e.g., screens for growth and development, special senses, and medical conditions)",
                    OptionSetId = 3,
                    Rank = 1,
                    AssessmentFormId = 3
                },
                new OptionCriterion
                {
                    Id = 14,
                    Description =
                        "Engaging patients and families in shared decision-making for those screening tests "+
                        "that are not mandated by state law",
                    OptionSetId = 3,
                    Rank = 2,
                    AssessmentFormId = 3
                },
                new OptionCriterion
                {
                    Id = 15,
                    Description =
                        "Educating patients and families about the implications of the results to their overall "+
                        "health and care plan",
                    OptionSetId = 3,
                    Rank = 3,
                    AssessmentFormId = 3
                },
                new OptionCriterion
                {
                    Id = 16,
                    Description = "Assessment rating",
                    OptionSetId = 2,
                    Rank = 1000,
                    AssessmentFormId = 3
                },
                new OptionCriterion
                {
                    Id = 17,
                    Description = "Briefly state at least one observation that supports the EPA rating you assigned",
                    OptionSetId = 1,
                    Rank = 1001,
                    AssessmentFormId = 3
                },
                new OptionCriterion
                {
                    Id = 18,
                    Description =
                        "Briefly state at least one thing that needs to be demonstrated by the trainee " +
                        "to advance the EPA rating to the next level",
                    OptionSetId = 1,
                    Rank = 1002,
                    AssessmentFormId = 3
                },
                //EPA3
                new OptionCriterion
                {
                    Id = 19,
                    Description =
                        "Performing a physical examination to look for normal variations, abnormal signs and "+
                        "congenital anomalies",
                    OptionSetId = 3,
                    Rank = 1,
                    AssessmentFormId = 4
                },
                new OptionCriterion
                {
                    Id = 20,
                    Description =
                        "Identifying and applying key evidence-based guidelines for care of the newborn",
                    OptionSetId = 3,
                    Rank = 2,
                    AssessmentFormId = 4
                },
                new OptionCriterion
                {
                    Id = 21,
                    Description =
                        "Providing routine care, as well as addressing common problems that develop within "+
                        "the first 28 days of life",
                    OptionSetId = 3,
                    Rank = 3,
                    AssessmentFormId = 4
                },
                new OptionCriterion
                {
                    Id = 22,
                    Description =
                        "Using judgment to know when common problems can be handled at home, and arrange for "+
                        "discharge and follow up",
                    OptionSetId = 3,
                    Rank = 4,
                    AssessmentFormId = 4
                },
                new OptionCriterion
                {
                    Id = 23,
                    Description =
                        "Assessing maternal/family readiness to care for the infant post discharge",
                    OptionSetId = 3,
                    Rank = 5,
                    AssessmentFormId = 4
                },
                new OptionCriterion
                {
                    Id = 24,
                    Description =
                        "Transitioning care to the community practitioner",
                    OptionSetId = 3,
                    Rank = 6,
                    AssessmentFormId = 4
                },
                new OptionCriterion
                {
                    Id = 25,
                    Description =
                        "Demonstrating confidence that puts new parents at ease",
                    OptionSetId = 3,
                    Rank = 7,
                    AssessmentFormId = 4
                },
                new OptionCriterion
                {
                    Id = 26,
                    Description = "Assessment rating",
                    OptionSetId = 2,
                    Rank = 1000,
                    AssessmentFormId = 4
                },
                new OptionCriterion
                {
                    Id = 27,
                    Description = "Briefly state at least one observation that supports the EPA rating you assigned",
                    OptionSetId = 1,
                    Rank = 1001,
                    AssessmentFormId = 4
                },
                new OptionCriterion
                {
                    Id = 28,
                    Description =
                        "Briefly state at least one thing that needs to be demonstrated by the trainee " +
                        "to advance the EPA rating to the next level",
                    OptionSetId = 1,
                    Rank = 1002,
                    AssessmentFormId = 4
                },
                //EPA4
                new OptionCriterion
                {
                    Id = 29,
                    Description = 
                        "Assessing the severity of illness and using judgment as to whether immediate "+
                        "or emergency actions, stabilization, or transfer to a higher acuity facility are "+
                        "necessary for treatment of urgent or life-threatening problems",
                    OptionSetId = 3,
                    Rank = 1,
                    AssessmentFormId = 5
                },
                new OptionCriterion
                {
                    Id = 30,
                    Description =
                        "Gathering essential information through history, physical examination, and initial "+
                        "laboratory evaluation",
                    OptionSetId = 3,
                    Rank = 2,
                    AssessmentFormId = 5
                },
                new OptionCriterion
                {
                    Id = 31,
                    Description =
                        "Engaging in sound clinical reasoning that drives the development of an appropriate "+
                        "differential diagnosis to allow the indicated diagnostic tests to be performed",
                    OptionSetId = 3,
                    Rank = 3,
                    AssessmentFormId = 5
                },
                new OptionCriterion
                {
                    Id = 32,
                    Description =
                        "Knowing or acquiring knowledge of the evidence related to the primary problem and "+
                        "applying the evidence to the patient’s care in developing a diagnostic work - up and "+
                        "plans for management and follow up",
                    OptionSetId = 3,
                    Rank = 4,
                    AssessmentFormId = 5
                },
                new OptionCriterion
                {
                    Id = 33,
                    Description =
                        "Placing the patient at the center of all management decisions to provide patient "+
                        "and family centered care by engaging in bidirectional communication with patients and families",
                    OptionSetId = 3,
                    Rank = 5,
                    AssessmentFormId = 5
                },
                new OptionCriterion
                {
                    Id = 34,
                    Description = "Assessment rating",
                    OptionSetId = 2,
                    Rank = 1000,
                    AssessmentFormId = 5
                },
                new OptionCriterion
                {
                    Id = 35,
                    Description = "Briefly state at least one observation that supports the EPA rating you assigned",
                    OptionSetId = 1,
                    Rank = 1001,
                    AssessmentFormId = 5
                },
                new OptionCriterion
                {
                    Id = 36,
                    Description =
                        "Briefly state at least one thing that needs to be demonstrated by the trainee " +
                        "to advance the EPA rating to the next level",
                    OptionSetId = 1,
                    Rank = 1002,
                    AssessmentFormId = 5
                }
            );
        }
    }
}
