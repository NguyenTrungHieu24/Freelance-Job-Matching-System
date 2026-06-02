using API.Services.Auth;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobsController : BaseController
    {
        public JobsController(AppDbContext context, IMapper mapper, IUserService user) : base(context, mapper, user)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] FilterJobDTO filter) {
            var query = _context.Jobs
                .Include(x => x.Category)
                .Include(x => x.EmployerProfile)
                .ThenInclude(x => x.Account)
                .AsQueryable();

            query = ApplyPermission(query);

            #region Filters

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(x =>
                    x.Title.Contains(filter.Keyword) ||
                    x.Description.Contains(filter.Keyword));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
            }

            if (filter.EmployerProfileId.HasValue)
            {
                query = query.Where(x => x.EmployerProfileId == filter.EmployerProfileId.Value);
            }

            if (filter.MinBudget.HasValue)
            {
                query = query.Where(x => x.Budget >= filter.MinBudget.Value);
            }

            if (filter.MaxBudget.HasValue)
            {
                query = query.Where(x => x.Budget <= filter.MaxBudget.Value);
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= filter.CreatedTo.Value);
            }

            if (filter.DeadlineFrom.HasValue)
            {
                query = query.Where(x => x.Deadline >= filter.DeadlineFrom.Value);
            }

            if (filter.DeadlineTo.HasValue)
            {
                query = query.Where(x => x.Deadline <= filter.DeadlineTo.Value);
            }

            if (filter.Temperature.HasValue)
            {
                switch (filter.Temperature.Value)
                {
                    case JobTemperature.Cool:
                        query = query.Where(x => x.Applications.Count < 2);
                        break;

                    case JobTemperature.Warm:
                        query = query.Where(x => x.Applications.Count >= 2 && x.Applications.Count < 8);
                        break;

                    case JobTemperature.Hot:
                        query = query.Where(x => x.Applications.Count >= 8);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.EmployerKeyword))
            {
                query = query.Where(x => x.EmployerProfile.Account.FullName.Contains(filter.EmployerKeyword));
            }

            if (filter.SkillIds.Count > 0)
            {
                query = query.Where(x => x.JobSkills.Any(js => filter.SkillIds.Contains(js.SkillId)));
            }

            #endregion

            #region Sorting

            query = filter.SortBy?.ToLower() switch
            {
                "title" => filter.IsDescending
                    ? query.OrderByDescending(x => x.Title)
                    : query.OrderBy(x => x.Title),

                "budget" => filter.IsDescending
                    ? query.OrderByDescending(x => x.Budget)
                    : query.OrderBy(x => x.Budget),

                "deadline" => filter.IsDescending
                    ? query.OrderByDescending(x => x.Deadline)
                    : query.OrderBy(x => x.Deadline),

                _ => filter.IsDescending
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt)
            };

            #endregion

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ProjectTo<JobDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var result = new PaginateResult<JobDTO>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var query = _context.Jobs
                .Include(x => x.Category)
                .Include(x => x.EmployerProfile)
                .ThenInclude(x => x.Account)
                .AsQueryable();

            query = ApplyPermission(query);

            var job = await query.FirstOrDefaultAsync(x => x.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<JobDTO>(job));
        }

        private IQueryable<Job> ApplyPermission(IQueryable<Job> query)
        {
            switch (_user.Role.ToLower())
            {
                case "admin":
                    return query;

                case "employer":
                    return query.Where(x => x.EmployerProfile.AccountId == _user.UserId);

                case "freelancer":
                    return query.Where(x => x.Status == JobStatus.ACTIVE);

                default:
                    return query.Where(x => false);
            }
        }
    }
}
