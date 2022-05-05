using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JohnsAuthority.Models.LocationViewModels
{
    public class EditLocationViewModel
    {
        public LocationDetailViewModel Location { get; set; }

        public List<AssignedAmenityData> AssignedAmenities { get; set; }

        public EditLocationViewModel()
        {
            AssignedAmenities = new List<AssignedAmenityData>();
        }
    }
}
