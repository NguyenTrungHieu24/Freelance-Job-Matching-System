using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = null!;
        public int RevieweeId { get; set; }
        public string RevieweeName { get; set; } = null!;
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        [Required]
        public int RevieweeId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please enter a comment")]
        [MaxLength(1000)]
        public string Comment { get; set; } = null!;
    }
}
