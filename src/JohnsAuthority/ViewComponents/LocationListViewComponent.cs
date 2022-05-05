using JohnsAuthority.Data;
using JohnsAuthority.Helpers;
using JohnsAuthority.Models.LocationViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yelp.Api.Web.Models;

namespace JohnsAuthority.ViewComponents
{
    public class LocationListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public LocationListViewComponent(ApplicationDbContext context)
        {
            db = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(double latitude, double longitude)
        {
            var yelpClient = new Yelp.Api.Web.Client("CCff-aom5xwDJKsbDbe89g", "lq6irvMYDmgZKn2dibxGoSiiHiK9BF3THEH9C5MgzhhgQzBHERDjt2ob2M9qaB4g");
            var request = new SearchRequest
            {
                Latitude = latitude,
                Longitude = longitude,
                Term = "sushi",
                Radius = 8000,
                MaxResults = 30
            };
            var results = await yelpClient.SearchBusinessesAllAsync(request);
            var locations = new LocationDetailListViewModel(results, db);
            var vm = PaginatedList<LocationDetailViewModel>.Create(locations.LocationDetails.AsQueryable(), 1, 10);

            return View(vm);
        }
    }
}
