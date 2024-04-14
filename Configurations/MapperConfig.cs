using AutoMapper;
using Wombat.Data;
using Wombat.Models;

namespace Wombat.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<AssessmentCategory, AssessmentCategoryVM>().ReverseMap();
            CreateMap<TextCriterion, TextCriterionVM>().ReverseMap();
            CreateMap<OptionCriterion, OptionCriterionVM>().ReverseMap();
            CreateMap<OptionSet, OptionSetVM>().ReverseMap();
            CreateMap<Option, OptionVM>().ReverseMap();
        }
    }
}
