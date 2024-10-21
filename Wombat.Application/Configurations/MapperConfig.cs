using AutoMapper;
using Wombat.Data;
using Wombat.Common.Models;

namespace Wombat.Application.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<AssessmentForm, AssessmentFormVM>().ReverseMap();
            CreateMap<OptionCriterion, OptionCriterionVM>().ReverseMap();
            CreateMap<OptionSet, OptionSetVM>().ReverseMap();
            CreateMap<Option, OptionVM>().ReverseMap();
            CreateMap<EPA, EPAVM>().ReverseMap();
            CreateMap<LoggedAssessment, LoggedAssessmentVM>().ReverseMap();
            CreateMap<OptionCriterionResponse, OptionCriterionResponseVM>().ReverseMap();
            CreateMap<WombatUser, WombatUserVM>().ReverseMap();
            CreateMap<Speciality, SpecialityVM>().ReverseMap();
            CreateMap<SubSpeciality, SubSpecialityVM>().ReverseMap();
            CreateMap<Speciality, SpecialitySelectVM>().ReverseMap();
            CreateMap<SubSpeciality, SubSpecialitySelectVM>().ReverseMap();
            CreateMap<Institution, InstitutionVM>().ReverseMap();
            CreateMap<EPAForm, EPAFormVM>().ReverseMap();
            CreateMap<EPACurriculum, EPACurriculumVM>().ReverseMap();
            CreateMap<AssessmentRequest, AssessmentRequestVM>().ReverseMap();
        }
    }
}
