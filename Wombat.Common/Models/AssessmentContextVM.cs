namespace Wombat.Common.Models
{
    public class AssessmentContextVM
    {
        public AssessmentContextVM()
        {
            Description = "";
        }
        public int Id { get; set; }
        public string Description { get; set; }

        public AssessmentTemplateVM? AssessmentTemplate { get; set; }
        public int AssessmentTemplateId { get; set; }

        public List<AssessmentTemplateVM>? Templates { get; set; }

    }
}
