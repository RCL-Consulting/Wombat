namespace Wombat.Common.Models
{
    public class EPAVM
    {
        public EPAVM()
        {
            Name = "";
            Description = "";
        }
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public List<AssessmentTemplateVM>? Templates { get; set; }

    }
}
