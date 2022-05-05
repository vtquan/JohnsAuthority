using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace JohnsAuthority.Models
{
    public class Location
    {
        public string Id { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Image> Images { get; set; }
        [Display(Name = "Amenities")]
        public List<LocationAmenity> LocationAmenities { get; set; }

        public Location()
        {
            Reviews = new List<Review>();
            Images = new List<Image>();
            LocationAmenities = new List<LocationAmenity>();
        }

        public float GetRating()
        {
            if ((Reviews?.Count() ?? 0) == 0)
            {
                return 0;
            }
            var sumScore = Reviews.Sum(r => r.Score);
            float averageScore = sumScore / Reviews.Count();
            return averageScore;
        }

        public string PrintAmenities()
        {
            var sb = new StringBuilder();
            foreach (var locationAmenity in LocationAmenities)
            {
                if (locationAmenity == LocationAmenities.Last())
                {
                    sb.Append($"{locationAmenity.Amenity.Name}");
                }
                else
                {
                    sb.Append($"{locationAmenity.Amenity.Name} | ");
                }
            }

            return sb.ToString();
        }

        public ICollection<Amenity> GetAmenties()
        {
            return LocationAmenities.Select(m => m.Amenity).ToList();
        }
    }
}
