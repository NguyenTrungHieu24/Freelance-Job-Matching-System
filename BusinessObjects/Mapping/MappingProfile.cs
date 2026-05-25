using AutoMapper;
using BusinessObjects.DTOs;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add Mapping here: 

            CreateMap<Skill, SkillDTO>();

            // Mapping for personal info
            CreateMap<FreelancerProfile, FreelancerPersonalInfoDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Account.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email));

            // Mapping for CV, Portfolio
            CreateMap<FreelancerProfile, FreelancerCvDto>();
        }
    }
}
