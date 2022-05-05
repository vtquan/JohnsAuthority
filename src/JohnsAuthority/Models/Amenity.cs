using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JohnsAuthority.Models
{
    public class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Display(Name = "Locations")]
        public ICollection<LocationAmenity> LocationAmenities { get; set; }
    }
}
