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

            // Mapping for Job
            CreateMap<Job, JobDto>()
                .ForMember(dest => dest.EmployerName, opt => opt.MapFrom(src => 
                    src.EmployerProfile != null && src.EmployerProfile.Account != null 
                        ? src.EmployerProfile.Account.FullName 
                        : (src.EmployerProfile != null ? src.EmployerProfile.CompanyName : "")))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => 
                    src.Category != null ? src.Category.Name : ""))
                .ForMember(dest => dest.ApplicationsCount, opt => opt.MapFrom(src =>
                    src.Applications != null ? src.Applications.Count : 0))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => 
                    src.JobSkills != null && src.JobSkills.Any()
                        ? src.JobSkills.Select(js => js.Skill).ToList() 
                        : new List<Skill>()));
            // Remove mappings for non-existing JobDTO to avoid confusion
        }
    }
}
