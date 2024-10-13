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

        public int SpecialityId { get; set; }
        public int SubSpecialityId { get; set; }

        public SubSpecialityVM? SubSpeciality { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public List<AssessmentTemplateVM>? Templates { get; set; }

        public List<SpecialitySelectVM>? Specialities { get; set; }
        public List<SubSpecialitySelectVM>? SubSpecialities { get; set; }

    }
}
