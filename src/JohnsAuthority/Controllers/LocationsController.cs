using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JohnsAuthority.Data;
using JohnsAuthority.Models;
using JohnsAuthority.Models.LocationViewModels;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using JohnsAuthority.Services;
using Microsoft.AspNetCore.Identity;
using JohnsAuthority.Helpers;
using Yelp.Api.Web.Models;
using Optional;
using Optional.Unsafe;
using FreeGeoIPCore;

namespace JohnsAuthority.Controllers
{
    public class LocationsController : Controller
    {
        private IHostingEnvironment _environment;

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public LocationsController(ApplicationDbContext context, IHostingEnvironment environment, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        // GET: Locations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Location.ToListAsync());
        }

        public async Task<IActionResult> Search(SearchLocationViewModel vm, int page = 1)
        {
            ViewData["page"] = page;
            vm.Page = page;

            if (vm.AssignedAmenities == null)
            {
                vm.AssignedAmenities = new List<AssignedAmenityData>();

                var Amenities = await _context.Amenity.ToListAsync();
                foreach (var amenity in Amenities)
                {
                    vm.AssignedAmenities.Add(new AssignedAmenityData { AmenityId = amenity.Id, AmenityName = amenity.Name, Assigned = false });
                }
            }
            return View(vm);
        }
        
        [HttpPost]
        //find all locations that are a certain distance from the given coordinate and return a paged list view of the locations
        public async Task<IActionResult> SearchResultPartial([FromBody] SearchLocationViewModel vm)
        {
            var yelpClient = new Yelp.Api.Web.Client("CCff-aom5xwDJKsbDbe89g", "lq6irvMYDmgZKn2dibxGoSiiHiK9BF3THEH9C5MgzhhgQzBHERDjt2ob2M9qaB4g");
            var request = new SearchRequest
            {
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                Term = vm.Description,
                Location = vm.Location,
                Radius = (int)GeolocationServices.ConvertMileToMeter(vm.WithinDistance), //in meters
                MaxResults = 30,
                OpenNow = vm.IsOpen
            };

            var results = await yelpClient.SearchBusinessesAllAsync(request);
            var locations = new LocationDetailListViewModel(results, _context);

            //Filter out only those with required amenities and reorder locations by how close they are
            var userCoordinate = new Coordinate { lat = vm.Latitude, lng = vm.Longitude };
            var selectedAmenityIds = vm.AssignedAmenities.Where(m => m.Assigned == true).Select(m => m.AmenityId).ToList();
            if(selectedAmenityIds.Count > 0)
            {
                var selectedAmenities = new List<Amenity>();
                foreach (var id in selectedAmenityIds)
                {
                    selectedAmenities.Add(await _context.Amenity.FindAsync(id));
                }

                locations.LocationDetails = locations.LocationDetails
                    .Where(m => selectedAmenities.Except(m.GetAmenties()).Count() == 0)
                    .OrderBy(m => GeolocationServices.CalculateDistanceInMiles(new Coordinate { lat = m.Coordinate.lat, lng = m.Coordinate.lng }, userCoordinate))
                    .ToList();
            }
            else
            {
                locations.LocationDetails = locations.LocationDetails
                    .OrderBy(m => GeolocationServices.CalculateDistanceInMiles(new Coordinate { lat = m.Coordinate.lat, lng = m.Coordinate.lng }, userCoordinate))
                    .ToList();
            }

            vm.Businesses = PaginatedList<LocationDetailViewModel>.Create(locations.LocationDetails.AsQueryable(), vm.Page, 10);

            return PartialView("_LocationListPartial", vm);
        }

        [AllowAnonymous]
        // GET: Locations/Details/5
        public async Task<IActionResult> Details(
            string id,
            int? page)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var vm = await GetLocationDetailViewModel(id: id, page: page ?? 1);

            if (vm.HasValue)
            {
                return View(vm.ValueOrFailure("Locations/Images: Unable to to find location for id: " + id));
            }
            else
            {
                return NotFound();
            }
        }

        // GET: Locations/Edit/5
        [Authorize()]
        public async Task<IActionResult> Edit(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var location = await GetLocationDetailViewModel(id: id);

            if (location.HasValue)
            {
                var vm = new EditLocationViewModel { Location = location.ValueOrFailure("Locations/Edit: Unable to to find location for id: " + id) };

                var Amenities = await _context.Amenity.ToListAsync();

                if (Amenities != null)
                {
                    foreach (var amenity in Amenities)
                    {
                        vm.AssignedAmenities.Add(new AssignedAmenityData { AmenityId = amenity.Id, AmenityName = amenity.Name, Assigned = location.ValueOrFailure("Locations/Edit: Unable to to find location for id: " + id).LocationAmenities.Any(m => m.Amenity == amenity) });
                    }
                }

                return View(vm);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize()]
        public async Task<IActionResult> Edit(string id, EditLocationViewModel vm)
        {
            if (id != vm.Location.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalLocation = await _context.Location
                        .Include(m => m.LocationAmenities)
                            .ThenInclude(m => m.Amenity)
                        .SingleOrDefaultAsync(m => m.Id == id);

                    if (originalLocation.LocationAmenities != null)
                    {
                        // Remove all amenities that is not assigned
                        foreach (var locationAmenity in originalLocation.LocationAmenities.ToList())
                        {
                            if (!vm.AssignedAmenities.Exists(m => m.Assigned && m.AmenityName == locationAmenity.Amenity.Name))
                                originalLocation.LocationAmenities.Remove(locationAmenity);
                        }

                        // Add all newly assigned amenities
                        foreach (var amenityData in vm.AssignedAmenities.Where(m => m.Assigned))
                        {
                            if (!originalLocation.LocationAmenities.Exists(m => m.Amenity.Name == amenityData.AmenityName))
                            {
                                var newLocationAmenity = new LocationAmenity
                                {
                                    LocationId = originalLocation.Id,
                                    Location = originalLocation,
                                    AmenityId = amenityData.AmenityId,
                                    Amenity = _context.Amenity.ToList().Find(p => p.Id == amenityData.AmenityId)
                                };

                                originalLocation.LocationAmenities.Add(newLocationAmenity);
                            }
                        }
                    }

                    _context.Update(originalLocation);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", new { id = vm.Location.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(vm.Location.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(vm);
        }

        [AllowAnonymous]
        // GET: Locations/Details/5
        public async Task<IActionResult> Images(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var vm = await GetLocationDetailViewModel(id);

            if (vm.HasValue)
            {
                return View(vm.ValueOrFailure("Locations/Images: Error with GetLocationDetailViewModel"));

            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(string id, ICollection<IFormFile> files)
        {
            var location = await _context.Location.Include(m => m.Images).SingleOrDefaultAsync(m => String.Equals(m.Id, id));

            if (location == null)
            {
                var yelpClient = new YelpApi().Client;
                var business = await yelpClient.GetBusinessAsync(id);
                if (business == null)
                {
                    return NotFound();
                }

                location = new Models.Location { Id = id };
                _context.Add(location);
                await _context.SaveChangesAsync();
            }
            
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = file.FileName;
                    var generatedName = DateTime.Now.ToString("yyyyHHmmss.ffff");
                    var fileType = System.IO.Path.GetExtension(fileName);
                    var newFileName = generatedName + fileType;
                    if (CheckIfFileTypeIsImage(fileType))
                    {
                        var newImage = new Image { Name = fileName, GeneratedName = generatedName, FileType = fileType };
                        var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                        newImage.User = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);

                        try
                        {
                            var fileManager = new FileManager();

                            var newImagePath = await fileManager.UploadFile(file, newFileName);

                            if (!String.IsNullOrEmpty(newImagePath))
                            {
                                newImage.Path = newImagePath;
                            }

                            location.Images.Add(newImage);
                            _context.Update(location);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("File", ex.Message);
                            return RedirectToAction("Images", new { id = id });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("File", "Make sure that the file uploaded is an image file.");
                        return RedirectToAction("Images", new { id = id });
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = id });
        }

        private Boolean CheckIfFileTypeIsImage(string fileType)
        {
            var result = false;
            switch (fileType.ToLower())
            {
                case ".jpg":
                case ".png":
                case ".jpeg":
                    {
                        result = true;
                        break;
                    }
            }

            return result;
        }

        private bool LocationExists(string id)
        {
            return _context.Location.Any(e => String.Equals(e.Id, id));
        }

        // Get LocationDetailViewModel by Id. If location doesn't exist, create a new location
        private async Task<Option<LocationDetailViewModel>> GetLocationDetailViewModel(string id, int pageSize = 10, int page = 1)
        {
            var yelpClient = new YelpApi().Client;
            var business = await yelpClient.GetBusinessAsync(id);
            if (business == null)
            {
                return Option.None<LocationDetailViewModel>();
            }

            var location = await _context.Location
                .Include(m => m.LocationAmenities)
                    .ThenInclude(m => m.Amenity)
                .Include(m => m.Reviews)
                    .ThenInclude(m => m.User)
                .Include(m => m.Images)
                .SingleOrDefaultAsync(m => String.Equals(m.Id, id));

            LocationDetailViewModel vm;

            if (location == null)
            {
                location = new Models.Location { Id = id };
                _context.Add(location);
                await _context.SaveChangesAsync();

                vm = new LocationDetailViewModel(business: business, location: location, pageSize: pageSize);
            }
            else
            {
                vm = new LocationDetailViewModel(business: business, location: location);
            }

            return Option.Some(vm);
        }

        //Save location to cookies
        public void SaveLocationToCookie(float lat, float lng)
        {
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(5)
            };
            Response.Cookies.Append("Latitude", lat.ToString(), options);
            Response.Cookies.Append("Longitude", lng.ToString(), options);
        }

        [HttpGet("api/locations")]
        public IActionResult GetLocations()
        {
            return Ok(_context.Location.ToList());
        }
    }
}