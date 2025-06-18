/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

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

            CreateMap<RegistrationInvitation, RegistrationInvitationVM>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.Roles)
                        ? new List<string>()
                        : src.Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()))
                .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution != null ? src.Institution.Name : ""))
                .ForMember(dest => dest.Speciality, opt => opt.MapFrom(src => src.Speciality != null ? src.Speciality.Name : null))
                .ForMember(dest => dest.SubSpeciality, opt => opt.MapFrom(src => src.SubSpeciality != null ? src.SubSpeciality.Name : null));


            CreateMap<RegistrationInvitation, RegisterFromInviteVM>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.Roles)
                        ? new List<string>()
                        : src.Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()));
                //.ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution != null ? src.Institution.Name : ""))
                //.ForMember(dest => dest.Speciality, opt => opt.MapFrom(src => src.Speciality != null ? src.Speciality.Name : null))
                //.ForMember(dest => dest.SubSpeciality, opt => opt.MapFrom(src => src.SubSpeciality != null ? src.SubSpeciality.Name : null));

        }
    }
}
