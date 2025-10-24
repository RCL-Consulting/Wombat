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
            CreateMap<AssessmentForm, AssessmentFormVM>()
                .ForMember(vm => vm.InstitutionId, o => o.MapFrom(s => s.InstitutionId))
                .ForMember(vm => vm.SpecialityId, o => o.MapFrom(s => s.SpecialityId))
                .ForMember(vm => vm.SubSpecialityId, o => o.MapFrom(s => s.SubSpecialityId))
                // (optional) if you show names in the VM:
                .ForMember(vm => vm.InstitutionName,  o => o.MapFrom(s => s.Institution.Name))
                .ForMember(vm => vm.SpecialityName,   o => o.MapFrom(s => s.Speciality.Name))
                .ForMember(vm => vm.SubSpecialityName,o => o.MapFrom(s => s.SubSpeciality.Name));

            CreateMap<AssessmentFormVM, AssessmentForm>()
                // write only the FKs; DO NOT map navs on updates from a form
                .ForMember(d => d.InstitutionId, o => o.MapFrom(vm => vm.InstitutionId))
                .ForMember(d => d.SpecialityId, o => o.MapFrom(vm => vm.SpecialityId))
                .ForMember(d => d.SubSpecialityId, o => o.MapFrom(vm => vm.SubSpecialityId))
                .ForMember(d => d.Institution, o => o.Ignore())
                .ForMember(d => d.Speciality, o => o.Ignore())
                .ForMember(d => d.SubSpeciality, o => o.Ignore());

            CreateMap<OptionCriterion, OptionCriterionVM>().ReverseMap();

            CreateMap<OptionSet, OptionSetVM>()
                .ForMember(vm => vm.InstitutionId, o => o.MapFrom(s => s.InstitutionId))
                .ForMember(vm => vm.SpecialityId, o => o.MapFrom(s => s.SpecialityId))
                .ForMember(vm => vm.SubSpecialityId, o => o.MapFrom(s => s.SubSpecialityId))
                .ReverseMap()
                .ForMember(d => d.Institution, o => o.Ignore())
                .ForMember(d => d.Speciality, o => o.Ignore())
                .ForMember(d => d.SubSpeciality, o => o.Ignore());

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
            CreateMap<AssessmentEvent, AssessmentEventVM>().ReverseMap();

            // Entity -> VM: map both BaseStatus (persisted) and Status (derived/display)
            CreateMap<AssessmentRequest, AssessmentRequestVM>()
                .ForMember(d => d.BaseStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(d => d.Status, opt => opt.MapFrom(src => src.GetDisplayStatus(DateTime.UtcNow)));

            // VM -> Entity: never write the derived display status back
            CreateMap<AssessmentRequestVM, AssessmentRequest>()
                .ForMember(e => e.Status, opt => opt.Ignore())
                .ForMember(e => e.StatusChangedAt, opt => opt.Ignore());

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

        }
    }
}
