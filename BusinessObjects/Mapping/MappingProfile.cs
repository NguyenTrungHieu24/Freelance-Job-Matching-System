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
            CreateMap<Category, CategoryDTO>();
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
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => 
                    src.JobSkills != null && src.JobSkills.Any()
                        ? src.JobSkills.Select(js => js.Skill).ToList() 
                        : new List<Skill>()))
                .ForMember(dest => dest.EmployerCompanyName, opt => opt.MapFrom(src => 
                    src.EmployerProfile != null ? src.EmployerProfile.CompanyName : ""))
                .ForMember(dest => dest.EmployerDescription, opt => opt.MapFrom(src => 
                    src.EmployerProfile != null ? src.EmployerProfile.Description : ""))
                .ForMember(dest => dest.EmployerEmail, opt => opt.MapFrom(src => 
                    src.EmployerProfile != null && !string.IsNullOrEmpty(src.EmployerProfile.Email)
                        ? src.EmployerProfile.Email 
                        : (src.EmployerProfile != null && src.EmployerProfile.Account != null ? src.EmployerProfile.Account.Email : "")))
                .ForMember(dest => dest.EmployerPhone, opt => opt.MapFrom(src => 
                    src.EmployerProfile != null ? src.EmployerProfile.Phone : ""))
                .ForMember(dest => dest.EmployerAddress, opt => opt.MapFrom(src => 
                    src.EmployerProfile != null ? src.EmployerProfile.Address : ""))
                .ForMember(dest => dest.EmployerLogo, opt => opt.MapFrom(src => 
                    src.EmployerProfile != null ? src.EmployerProfile.Logo : ""));

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
                    o => o.MapFrom(s => s.JobSkills.Select(x => x.Skill.Name)))
                .ForMember(
                    d => d.CompanyName,
                    o => o.MapFrom(s => s.EmployerProfile.CompanyName))
                .ForMember(
                    d => d.EmployerLogo,
                    o => o.MapFrom(s => s.EmployerProfile.Logo));

            CreateMap<User, UserDto>()
                .ForMember(
                    d => d.Name,
                    o => o.MapFrom(s => s.FullName))
                .ForMember(
                    d => d.Status,
                    o => o.MapFrom(s => s.IsActive ? 1 : 0))
                .ForMember(
                    d => d.Role,
                    o => o.MapFrom(s => s.Role.Name));

            CreateMap<Role, RoleDTO>();
            CreateMap<Application, ApplicationHistoryDto>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job.Title))
                .ForMember(dest => dest.Budget, opt => opt.MapFrom(src => src.Job.Budget))
                .ForMember(dest => dest.Deadline, opt => opt.MapFrom(src => src.Job.Deadline))
                .ForMember(dest => dest.EmployerName, opt => opt.MapFrom(src => 
                    src.Job.EmployerProfile != null && src.Job.EmployerProfile.Account != null 
                        ? src.Job.EmployerProfile.Account.FullName 
                        : ""))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => 
                    src.Job.EmployerProfile != null ? src.Job.EmployerProfile.CompanyName : ""))
                .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => 
                    src.Job.EmployerProfile != null ? src.Job.EmployerProfile.Logo : ""));
            CreateMap<Report, ReportDto>()
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => 
                    src.Reporter != null ? src.Reporter.FullName : ""))
                .ForMember(dest => dest.ReportedUserName, opt => opt.MapFrom(src => 
                    src.ReportedUser != null ? src.ReportedUser.FullName : ""))
                .ForMember(dest => dest.ResolverName, opt => opt.MapFrom(src => 
                    src.Resolver != null ? src.Resolver.FullName : ""));

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => 
                    src.Reviewer != null ? src.Reviewer.FullName : ""));
        }
    }
}
