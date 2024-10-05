using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Wombat.Data.EPA;

namespace Wombat.Data.Configurations.Entities
{
    public class CompetencyConfiguration : IEntityTypeConfiguration<Competency>
    {
        Dictionary<string, string> Competencies = new Dictionary<string, string>()
        {
            { "PC1", "History" },
            { "PC2", "Physical Exam" },
            { "PC3", "Organize and Prioritize Patient Care" },
            { "PC4", "Clinical Reasoning" },
            { "PC5", "Patient Management" },
            { "MK1", "Clinical Knowledge" },
            { "MK2", "Diagnostic Evaluation" },
            { "SBP1", "Patient Safety" },
            { "SBP2", "Quality Improvement" },
            { "SBP3", "System Navigation for Patient Centered Care – Coordination of Care" },
            { "SBP4", "System Navigation for Patient-Centered Care – Transitions in Care" },
            { "SBP5", "Population and Community Health" },
            { "SBP6", "Physician Role in Health Care Systems" },
            { "PBLI1", "Evidence-Based and Informed Practice" },
            { "PBLI2", "Reflective Practice and Commitment to Personal Growth" },
            { "P1", "Professional Behavior" },
            { "P2", "Ethical Principles" },
            { "P3", "Accountability/Conscientiousness" },
            { "P4", "Well-Being" },
            { "ICS1", "Patient- and Family-Centered Communication" },
            { "ICS2", "Interprofessional and Team Communication" },
            { "ICS3", "Communication within Health Care Systems" }
        };

        private Competency BuildCompetency(int id, string description, string code, bool displayRank)
        {
            return new Competency
            {
                Id = id,
                Description = description,
                Code = code,
                DisplayRank = displayRank
            };
        }

        public void Configure(EntityTypeBuilder<Competency> builder)
        {
           builder.HasData(
                BuildCompetency(1, Competencies["PC1"], "PC1", true),
                BuildCompetency(2, Competencies["PC2"], "PC2", true),
                BuildCompetency(3, Competencies["PC3"], "PC3", true),
                BuildCompetency(4, Competencies["PC4"], "PC4", true),
                BuildCompetency(5, Competencies["PC5"], "PC5", true),
                BuildCompetency(6, Competencies["MK1"], "MK1", true),
                BuildCompetency(7, Competencies["MK2"], "MK2", true),
                BuildCompetency(8, Competencies["SBP1"], "SBP1", true),
                BuildCompetency(9, Competencies["SBP2"], "SBP2", true),
                BuildCompetency(11, Competencies["SBP3"], "SBP3", true),
                BuildCompetency(12, Competencies["SBP4"], "SBP4", true),
                BuildCompetency(13, Competencies["SBP5"], "SBP5", true),
                BuildCompetency(14, Competencies["SBP6"], "SBP6", true),
                BuildCompetency(15, Competencies["PBLI1"], "PBLI1", true),
                BuildCompetency(16, Competencies["PBLI2"], "PBLI2", true),
                BuildCompetency(17, Competencies["P1"], "P1", true),
                BuildCompetency(18, Competencies["P2"], "P2", true),
                BuildCompetency(19, Competencies["P3"], "P3", true),
                BuildCompetency(20, Competencies["P4"], "P4", true),
                BuildCompetency(21, Competencies["ICS1"], "ICS1", true),
                BuildCompetency(22, Competencies["ICS2"], "ICS2", true),
                BuildCompetency(23, Competencies["ICS3"], "ICS3", true)
            );
        }
    }
}
