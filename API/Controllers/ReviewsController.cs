using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    [Authorize]
    public class ReviewsController : BaseController
    {
        public ReviewsController(AppDbContext context, IMapper mapper, IUserService user)
            : base(context, mapper, user)
        {
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewerId = _user.UserId;
            var revieweeId = dto.RevieweeId;

            if (reviewerId == revieweeId)
                return BadRequest("You cannot review yourself.");

            // Verify they have completed a job together
            var completedAppExists = await _context.Applications
                .Include(a => a.Job)
                    .ThenInclude(j => j.EmployerProfile)
                .Include(a => a.FreelancerProfile)
                .AnyAsync(a => a.Status == ApplicationStatus.COMPLETED &&
                    ((a.FreelancerProfile.AccountId == reviewerId && a.Job.EmployerProfile.AccountId == revieweeId) ||
                     (a.FreelancerProfile.AccountId == revieweeId && a.Job.EmployerProfile.AccountId == reviewerId)));

            if (!completedAppExists)
            {
                return BadRequest("You can only review users you have completed jobs with.");
            }

            // Check if already reviewed for this pair
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ReviewerId == reviewerId && r.RevieweeId == revieweeId);

            if (alreadyReviewed)
            {
                return BadRequest("You have already reviewed this user.");
            }

            var review = new Review
            {
                ReviewerId = reviewerId,
                RevieweeId = revieweeId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            var reviewerName = await _context.Users
                .Where(u => u.Id == reviewerId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync() ?? "một người dùng";

            _context.Reviews.Add(review);
            
            // Generate notification for reviewee
            _context.Notifications.Add(new Notification
            {
                AccountId = revieweeId,
                Content = $"Bạn nhận được một đánh giá mới ({dto.Rating} sao) từ {reviewerName}."
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Review submitted successfully" });
        }

        // GET: api/reviews/user/{userId}
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserReviews(int userId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.RevieweeId == userId)
                .ToListAsync();

            var dtos = _mapper.Map<List<ReviewDto>>(reviews);
            return Ok(dtos);
        }
    }
}
