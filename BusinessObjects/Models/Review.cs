using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public int ReviewerId { get; set; }

        public int RevieweeId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; }

        [ForeignKey("ReviewerId")]
        public User Reviewer { get; set; }

        [ForeignKey("RevieweeId")]
        public User Reviewee { get; set; }
    }
}
