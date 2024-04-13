using AutoMapper;
using Wombat.Data;
using Wombat.Models;

namespace Wombat.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<Category, AssessmentCategoryVM>().ReverseMap();
            CreateMap<TextCriterion, TextCriterionVM>().ReverseMap();
            CreateMap<EnumCriterion, EnumCriterionVM>().ReverseMap();
        }
    }
}
