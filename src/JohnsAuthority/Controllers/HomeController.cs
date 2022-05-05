using JohnsAuthority.Data;
using JohnsAuthority.Helpers;
using JohnsAuthority.Models;
using JohnsAuthority.Models.LocationViewModels;
using JohnsAuthority.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yelp.Api.Web.Models;


namespace JohnsAuthority.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(int page = 1)
        {
            ViewData["Page"] = page;

            return View();
        }

        public IActionResult Contact()
        {
            var vm = new ContactMessage();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact([Bind("ContactMessageID,Email,Message")] ContactMessage contactMessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contactMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(contactMessage);
        }

        public IActionResult Error()
        {
            return View();
        }
        
        [Authorize(Policy = "Moderators")]
        public IActionResult Moderator()
        {
            return View();
        }

        [Authorize(Policy = "Administrators")]
        public IActionResult Administrator()
        {
            return View();
        }

        //find all locations that are a certain distance from the given coordinate and return a paged list view of the locations
        public async Task<IActionResult> LocationListPartial(float lat, float lng, int page = 1, int pageSize = 10)
        {
            var userCoordinate = new Coordinate { lat = lat, lng = lng };

            var yelpClient = new Yelp.Api.Web.Client("CCff-aom5xwDJKsbDbe89g", "lq6irvMYDmgZKn2dibxGoSiiHiK9BF3THEH9C5MgzhhgQzBHERDjt2ob2M9qaB4g");
            var request = new SearchRequest
            {
                Latitude = lat,
                Longitude = lng,
                Term = "",
                Radius = 8000,
                MaxResults = 10,
                OpenNow = true
            };
            var results = await yelpClient.SearchBusinessesAllAsync(request);
            var vm = new LocationDetailListViewModel(results, _context);
            vm.LocationDetails
                .OrderBy(m => GeolocationServices.CalculateDistanceInMiles(new Coordinate { lat = m.Coordinate.lat, lng = m.Coordinate.lng }, userCoordinate))
                .ToList();
            return PartialView("_LocationDetailListViewModel", vm);
        }
    }
}
