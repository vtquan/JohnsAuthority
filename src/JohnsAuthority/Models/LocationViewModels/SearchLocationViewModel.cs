using JohnsAuthority.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JohnsAuthority.Models.LocationViewModels
{
    public class SearchLocationViewModel
    {
        public string Description { get; set; }
        public string Location { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        [Display(Name = "Within")]
        public float WithinDistance { get; set; }
        public bool IsOpen { get; set; }
        public int Page { get; set; }
        public List<AssignedAmenityData> AssignedAmenities { get; set; }
        public PaginatedList<LocationDetailViewModel> Businesses { get; set; }

        public SearchLocationViewModel()
        {
            WithinDistance = 5;
        }
    }

    public class AssignedAmenityData
    {
        public int AmenityId { get; set; }
        public string AmenityName { get; set; }
        public bool Assigned { get; set; }
    }
}
