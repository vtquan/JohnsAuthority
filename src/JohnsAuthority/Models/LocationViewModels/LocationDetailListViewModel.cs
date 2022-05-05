using JohnsAuthority.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yelp.Api.Web.Models;

namespace JohnsAuthority.Models.LocationViewModels
{
    public class LocationDetailListViewModel
    {
        public List<LocationDetailViewModel> LocationDetails { get; set; }

        public LocationDetailListViewModel(SearchResponse results, ApplicationDbContext _context)
        {
            var locations = new List<LocationDetailViewModel>();
            foreach (var business in results.Businesses)
            {
                locations.Add(new LocationDetailViewModel(business, _context));
            }

            LocationDetails = locations;
        }

        public LocationDetailListViewModel(SearchResponse results, IQueryable<Location> locations)
        {
            var locationDetailList = new List<LocationDetailViewModel>();
            foreach (var location in locations)
            {
                locationDetailList.Add(new LocationDetailViewModel(results.Businesses.First(b => String.Equals(b.Id, location.Id)), location));
            }
            LocationDetails = locationDetailList;
        }
    }
}
