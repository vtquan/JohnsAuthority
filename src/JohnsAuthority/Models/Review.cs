using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JohnsAuthority.Models
{
    public class Review
    {
        public int Id { get; set; }
        [Required]
        [Range(0, 5)]
        public float Score { get; set; }
        [DataType(DataType.MultilineText)]
        [Required, StringLength(1000)]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public Location Location { get; set; }
        public ApplicationUser User { get; set; }

        public Review()
        {
            Date = DateTime.Now;
            Location = new Location();
        }
    }
}
