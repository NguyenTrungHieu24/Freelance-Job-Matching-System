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

            CreateMap<Job, JobDTO>()
                .ForMember(
                    dest => dest.EmployerName,
                    opt => opt.MapFrom(src =>
                        src.EmployerProfile.Account.FullName))
                .ForMember(
                    dest => dest.CategoryName,
                    opt => opt.MapFrom(src =>
                        src.Category.Name));
            CreateMap<Job, JobDTO>()
                .ForMember(
                    d => d.CategoryName,
                    o => o.MapFrom(s => s.Category.Name))
                .ForMember(
                    d => d.EmployerName,
                    o => o.MapFrom(s => s.EmployerProfile.Account.FullName))
                .ForMember(
                    d => d.ApplicationsCount,
                    o => o.MapFrom(s => s.Applications.Count))
                .ForMember(
                    d => d.Skills,
                    o => o.MapFrom(s => s.JobSkills.Select(x => x.Job.Title)));
        }
    }
}
