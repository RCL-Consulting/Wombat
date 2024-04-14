using Microsoft.CodeAnalysis.Options;

namespace Wombat.Data
{
    public class Option : BaseEntity
    {
        public Option()
        {
            Description = "";
        }
        public string Description { get; set; }
        public int Rank { get; set; }

        public int OptionSetId { get; set; }
        public OptionSet? OptionSet { get; set; }

    }
}
