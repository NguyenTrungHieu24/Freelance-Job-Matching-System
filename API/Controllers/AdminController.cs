using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("/api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : BaseController
{
    // GET
    public AdminController(AppDbContext context, IMapper mapper, IUserService user) : base(context, mapper, user)
    {
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports([FromQuery] ReportStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .Include(r => r.Resolver)
            .AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(r => r.Status  == status.Value);
        }
        
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var dto = _mapper.Map<List<ReportDto>>(items);
        var rs = new PaginateResult<ReportDto>
        {
            Items = dto,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };
        return Ok(rs);
    }

    [HttpPut("reports/{id}/status")]
    public async Task<IActionResult> UpdateReportStatus(int id, [FromBody] int status)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null) return NotFound();

        report.Status = (ReportStatus)status;
        
        if ((ReportStatus)status == ReportStatus.RESOLVED || (ReportStatus)status == ReportStatus.REJECTED)
        {
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolvedBy = _user.UserId;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}