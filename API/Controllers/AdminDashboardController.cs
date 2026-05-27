using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Common.Admin;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    public class AdminDashboardController : BaseController
    {
        public AdminDashboardController(AppDbContext context, IMapper mapper, IUserService user) : base(context, mapper, user)
        {
        }

        #region User Stats

        [HttpGet("user-stats")]
        public async Task<IActionResult> UserStats(DashboardRangeType range = DashboardRangeType.ThisMonth)
        {
            var fromDate = GetFromDate(range);

            var totalUsers = await _context.Users
                .CountAsync();

            var totalFreelancers = await _context.Users
                .CountAsync(x =>
                    x.RoleId == (int)RoleEnum.FREELANCER);

            var totalEmployers = await _context.Users
                .CountAsync(x =>
                    x.RoleId == (int)RoleEnum.EMPLOYER);

            var totalNewUsers = await _context.Users
                .CountAsync(x =>
                    x.CreatedAt >= fromDate);

            var totalNewFreelancers = await _context.Users
                .CountAsync(x =>
                    x.RoleId == (int)RoleEnum.FREELANCER &&
                    x.CreatedAt >= fromDate);

            var totalNewEmployers = await _context.Users
                .CountAsync(x =>
                    x.RoleId == (int)RoleEnum.EMPLOYER &&
                    x.CreatedAt >= fromDate);

            return Ok(new UserStats
            {
                TotalUsers = totalUsers,
                TotalFreelancers = totalFreelancers,
                TotalEmployers = totalEmployers,

                TotalNewUsers = totalNewUsers,
                TotalNewFreelancers = totalNewFreelancers,
                TotalNewEmployers = totalNewEmployers
            });
        }

        #endregion

        #region Job Stats

        [HttpGet("job-stats")]
        public async Task<IActionResult> JobStats(
            DashboardRangeType range = DashboardRangeType.ThisMonth)
        {
            var fromDate = GetFromDate(range);

            var totalJobs = await _context.Jobs
                .CountAsync();

            var activeJobs = await _context.Jobs.CountAsync(x => x.Status == JobStatus.ACTIVE);

            var closedJobs = await _context.Jobs.CountAsync(x => x.Status == JobStatus.CLOSED);

            var newJobs = await _context.Jobs
                .CountAsync(x =>
                    x.CreatedAt >= fromDate);

            return Ok(new JobStats
            {
                TotalJobs = totalJobs,
                ActiveJobs = activeJobs,
                ClosedJobs = closedJobs,
                NewJobs = newJobs
            });
        }

        #endregion

        #region Application Stats

        [HttpGet("application-stats")]
        public async Task<IActionResult> ApplicationStats(
            DashboardRangeType range = DashboardRangeType.ThisMonth)
        {
            var fromDate = GetFromDate(range);

            var totalApplications = await _context.Applications
                .CountAsync();

            var totalMatches = await _context.Applications.CountAsync(x => x.Status == ApplicationStatus.ACCEPTED);

            var newApplications = await _context.Applications
                .CountAsync(x => x.AppliedAt >= fromDate);

            double matchRate = 0;

            if (totalApplications > 0)
            {
                matchRate =
                    (double)totalMatches / totalApplications * 100;
            }

            return Ok(new ApplicationStats
            {
                TotalApplications = totalApplications,
                TotalMatches = totalMatches,
                NewApplications = newApplications,
                MatchRate = Math.Round(matchRate, 2)
            });
        }

        #endregion

        #region Revenue Stats

        [HttpGet("revenue-stats")]
        public async Task<IActionResult> RevenueStats(DashboardRangeType range = DashboardRangeType.ThisMonth)
        {
            var fromDate = GetFromDate(range);

            // TODO:
            // Replace bằng bảng Payments thật sau này

            decimal totalRevenue = 0;

            decimal revenueInRange = 0;

            int totalTransactions = 0;

            return Ok(new RevenueStats
            {
                TotalRevenue = totalRevenue,
                RevenueInRange = revenueInRange,
                TotalTransactions = totalTransactions
            });
        }

        #endregion

        #region Helpers

        private static DateTime GetFromDate(DashboardRangeType range)
        {
            return range switch
            {
                DashboardRangeType.Today => DateTime.UtcNow.Date,

                DashboardRangeType.ThisWeek => DateTime.UtcNow.AddDays(-7),

                DashboardRangeType.ThisMonth => DateTime.UtcNow.AddMonths(-1),

                DashboardRangeType.ThisYear => DateTime.UtcNow.AddYears(-1),

                _ => DateTime.UtcNow.AddMonths(-1)
            };
        }

        #endregion

        #region User Growth

        [HttpGet("user-growth")]
        public async Task<IActionResult> UserGrowth(DashboardRangeType range = DashboardRangeType.ThisMonth)
        {
            var fromDate = GetFromDate(range);

            var users = await _context.Users
                .Where(x => x.CreatedAt >= fromDate)
                .ToListAsync();

            var result = users
                .GroupBy(x => x.CreatedAt.Date)
                .Select(x => new UserGrowthChartItem
                {
                    Label = x.Key.ToString("dd/MM"),

                    Freelancers = x.Count(y =>
                        y.RoleId == (int)RoleEnum.FREELANCER),

                    Employers = x.Count(y =>
                        y.RoleId == (int)RoleEnum.EMPLOYER)
                })
                .OrderBy(x => x.Label)
                .ToList();

            return Ok(result);
        }

        #endregion

        #region Job Growth

        [HttpGet("job-growth")]
        public async Task<IActionResult> JobGrowth(
            DashboardRangeType range = DashboardRangeType.ThisMonth)
        {
            var fromDate = GetFromDate(range);

            var jobs = await _context.Jobs
                .Where(x => x.CreatedAt >= fromDate)
                .ToListAsync();

            var result = jobs
                .GroupBy(x => x.CreatedAt.Date)
                .Select(x => new JobGrowthChartItem
                {
                    Label = x.Key.ToString("dd/MM"),

                    TotalJobs = x.Count(),

                    ActiveJobs = x.Count(y => y.Status == JobStatus.ACTIVE),

                    ClosedJobs = x.Count(y => y.Status == JobStatus.CLOSED)
                })
                .OrderBy(x => x.Label)
                .ToList();

            return Ok(result);
        }

        #endregion

        #region Recent Users

        [HttpGet("recent-users")]
        public async Task<IActionResult> RecentUsers()
        {
            var users = await _context.Users
                .Include(x => x.Role)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new RecentUserItem
                {
                    UserId = x.Id,
                    FullName = x.FullName,
                    Role = x.Role.Name,
                    CreatedAt = x.CreatedAt,
                })
                .ToListAsync();

            return Ok(users);
        }

        #endregion

        #region Recent Jobs

        [HttpGet("recent-jobs")]
        public async Task<IActionResult> RecentJobs()
        {
            var jobs = await _context.Jobs
                .Include(x => x.EmployerProfile)
                    .ThenInclude(x => x.Account)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new RecentJobItem
                {
                    JobId = x.Id,

                    Title = x.Title,

                    EmployerName = x.EmployerProfile.Account.FullName,

                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(jobs);
        }

        #endregion
    }
}