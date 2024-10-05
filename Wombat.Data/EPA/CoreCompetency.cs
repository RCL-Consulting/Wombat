namespace Wombat.Data.EPA
{
    public class CoreCompetency : BaseEntity
    {
        public CoreCompetency()
        {
            Description = "";
            Code = "";
            Competencies = new List<Competency>();
        }

        public string Description { get; set; }
        public string Code { get; set; }

        public List<Competency> Competencies { get; set; }
    }
}
