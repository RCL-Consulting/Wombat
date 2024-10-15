namespace Wombat.Common.Models
{
    public class EPAVM
    {
        public EPAVM()
        {
            Name = "";
            Description = "";
            Forms = new List<EPAFormVM>();
        }
        public int Id { get; set; }

        public int SpecialityId { get; set; }
        public int SubSpecialityId { get; set; }

        public SpecialityVM? Speciality { get; set; }
        public SubSpecialityVM? SubSpeciality { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public List<EPAFormVM>? Forms { get; set; }

        public static List<AssessmentFormVM> AvailableForms { get; set; } = new List<AssessmentFormVM>();

        public List<SpecialitySelectVM>? Specialities { get; set; }
        public List<SubSpecialitySelectVM>? SubSpecialities { get; set; }

    }
}
