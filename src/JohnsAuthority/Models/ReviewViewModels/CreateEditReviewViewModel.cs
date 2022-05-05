using System;
using System.ComponentModel.DataAnnotations;

namespace JohnsAuthority.Models.ReviewViewModels
{
    public class CreateEditReviewViewModel
    {
        [Required]
        public string LocationId { get; set; }
        [Required]
        public int ReviewId { get; set; }
        [Display(Name = "Location's Name")]
        public string LocationName { get; set; }
        [DataType(DataType.MultilineText)]
        [Required, StringLength(1000)]
        [Display(Name = "Content")]
        public string ReviewContent { get; set; }
        [Required]
        [Range(0, 5)]
        [Display(Name = "Score")]
        public float ReviewScore { get; set; }
        public DateTime Date { get; set; }

        public CreateEditReviewViewModel()
        {
        }

        public CreateEditReviewViewModel(string yelpId)
        {
            LocationId = yelpId;
        }
    }
}
