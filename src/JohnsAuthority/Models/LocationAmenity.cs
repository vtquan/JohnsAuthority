using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JohnsAuthority.Models
{
    public class LocationAmenity
    {
        [ForeignKey("Location")]
        public string LocationId { get; set; }
        public Location Location { get; set; }
        
        [ForeignKey("Amenity")]
        public int AmenityId { get; set; }
        public Amenity Amenity { get; set; }
    }
}
