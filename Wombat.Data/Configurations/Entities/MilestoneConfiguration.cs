using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Wombat.Data.EPA;

namespace Wombat.Data.Configurations.Entities
{
    public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
    {
        private Dictionary<string, string> Milestones = new Dictionary<string, string>
        {
            { "PC1.NA", "Not Yet Assessable." },
            { "PC1.0", "Not Yet Completed Level 1." },
            { "PC1.1", "Gathers information strictly following a template." },
            { "PC1.2", "Adapts template to filter and prioritize pertinent positives and negatives based on broad diagnostic categories or possible diagnoses." },
            { "PC1.3", "Filters, prioritizes, and synthesizes the history to develop a differential diagnosis in real-time for uncomplicated or typical presentations." },
            { "PC1.4", "Filters, prioritizes, and synthesizes the history to develop a differential diagnosis in real time for complicated or atypical presentations." },
            { "PC1.5", "Recognizes and probes subtle clues from patients and families; distinguishes nuances among diagnoses to efficiently drive further information gathering." },
            { "PC2.NA", "Not Yet Assessable." },
            { "PC2.0", "Not Yet Completed Level 1." },
            { "PC2.1", "Performs fundamental physical examination.  Performs a rote physical examination using a strict head-to-toe approach." },
            { "PC2.2", "Performs complete physical examination and identifies variants and abnormal findings.  Performs a physical examination considering appropriate adaptation for age and development." },
            { "PC2.3", "Performs complete or focused physical examination, as indicated, and interprets normal variants and abnormal findings.  Performs a physical examination with consistent use of a developmentally appropriate approach." },
            { "PC2.4", "Performs complete or focused physical examination, as indicated, and selects advanced maneuvers to distinguish between diagnoses.  Performs a physical examination using strategies to maximize patient cooperation and comfort." },
            { "PC2.5", "Detects, pursues, and integrates key physical examination findings to distinguish nuances among competing, often similar diagnoses.  Performs a physical examination that consistently and positively engages the patient." },
            { "PC3.NA", "Not Yet Assessable." },
            { "PC3.0", "Not Yet Completed Level 1." },
            { "PC3.1", "Completes tasks for an individual patient, when prompted." },
            { "PC3.2", "Organizes patient care responsibilities by focusing on individual (rather than multiple) patients." },
            { "PC3.3", "Organizes and prioritizes the simultaneous care of patients with efficiency." },
            { "PC3.4", "Organizes, prioritizes, and delegates patient care responsibilities even when patient volume approaches the capacity of the individual or facility; anticipates and triages urgent and emergent issues." },
            { "PC3.5", "Serves as a role model and coach for patient care responsibilities." }
        };

        private Dictionary<string, int> Ranks = new Dictionary<string, int>
        {
            { "NA", -1 },
            { "0", 0 },
            { "1", 1 },
            { "2", 2 },
            { "3", 3 },
            { "4", 4 },
            { "5", 5 }
        };

        private Dictionary<string, int> CompetencyId = new Dictionary<string, int>
        {
            { "PC1", 1 },
            { "PC2", 2 },
            { "PC3", 3 },
            { "PC4", 4 },
            { "PC5", 5 },
            { "MK1", 6 },
            { "MK2", 7 },
            { "SBP1", 8 },
            { "SBP2", 9 },
            { "SBP3", 11 },
            { "SBP4", 12 },
            { "SBP5", 13 },
            { "SBP6", 14 },
            { "PBLI1", 15 },
            { "PBLI2", 16 },
            { "P1", 17 },
            { "P2", 18 },
            { "P3", 19 },
            { "P4", 20 },
            { "ICS1", 21 },
            { "ICS2", 22 },
            { "ICS3", 23 },
        };

        private Milestone BuildMilestone(int id, string description, int rank, int competencyId)
        {
            return new Milestone
            {
                Id = id,
                Description = description,
                Rank = rank,
                CompetencyId = competencyId
            };
        }

        public void Configure(EntityTypeBuilder<Milestone> builder)
        {
            builder.HasData(

                BuildMilestone(1, Milestones["PC1.NA"], Ranks["NA"], CompetencyId["PC1"]),
                BuildMilestone(1, Milestones["PC1.0"], Ranks["0"], CompetencyId["PC1"]),
                BuildMilestone(1, Milestones["PC1.1"], Ranks["1"], CompetencyId["PC1"]),
                BuildMilestone(1, Milestones["PC1.2"], Ranks["2"], CompetencyId["PC1"]),
                BuildMilestone(1, Milestones["PC1.3"], Ranks["3"], CompetencyId["PC1"]),
                BuildMilestone(1, Milestones["PC1.4"], Ranks["4"], CompetencyId["PC1"]),
                BuildMilestone(1, Milestones["PC1.5"], Ranks["5"], CompetencyId["PC1"]),

                BuildMilestone(2, Milestones["PC2.NA"], Ranks["NA"], CompetencyId["PC2"]),
                BuildMilestone(2, Milestones["PC2.0"], Ranks["0"], CompetencyId["PC2"]),
                BuildMilestone(2, Milestones["PC2.1"], Ranks["1"], CompetencyId["PC2"]),
                BuildMilestone(2, Milestones["PC2.2"], Ranks["2"], CompetencyId["PC2"]),
                BuildMilestone(2, Milestones["PC2.3"], Ranks["3"], CompetencyId["PC2"]),
                BuildMilestone(2, Milestones["PC2.4"], Ranks["4"], CompetencyId["PC2"]),
                BuildMilestone(2, Milestones["PC2.5"], Ranks["5"], CompetencyId["PC2"]),

                BuildMilestone(2, Milestones["PC3.NA"], Ranks["NA"], CompetencyId["PC3"]),
                BuildMilestone(2, Milestones["PC3.0"], Ranks["0"], CompetencyId["PC3"]),
                BuildMilestone(2, Milestones["PC3.1"], Ranks["1"], CompetencyId["PC3"]),
                BuildMilestone(2, Milestones["PC3.2"], Ranks["2"], CompetencyId["PC3"]),
                BuildMilestone(2, Milestones["PC3.3"], Ranks["3"], CompetencyId["PC3"]),
                BuildMilestone(2, Milestones["PC3.4"], Ranks["4"], CompetencyId["PC3"]),
                BuildMilestone(2, Milestones["PC3.5"], Ranks["5"], CompetencyId["PC3"])
            );
        }
    }
}
