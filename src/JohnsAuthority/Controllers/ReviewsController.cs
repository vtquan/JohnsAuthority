using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JohnsAuthority.Data;
using JohnsAuthority.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using JohnsAuthority.Models.ReviewViewModels;
using Microsoft.AspNetCore.Authorization;
using Yelp.Api.Web.Models;
using JohnsAuthority.Services;

namespace JohnsAuthority.Controllers
{
    [RequireHttps]
    [Authorize()]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        [Authorize(Policy = "Moderators")]
        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            return View(await _context.Review.ToListAsync());
        }

        [Authorize(Policy = "Moderators")]
        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review.SingleOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: Reviews/Create/5
        public async Task<IActionResult> Create(string id)
        {
            var yelpClient = new YelpApi().Client;
            var business = await yelpClient.GetBusinessAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            var vm = new CreateEditReviewViewModel(yelpId:id);
            vm.LocationName = business.Name;

            return View(vm);
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEditReviewViewModel vm)
        {
            var location = await _context.Location.Include(r => r.Reviews).SingleOrDefaultAsync(m => m.Id == vm.LocationId);
            if (location == null)
            {
                location = new Models.Location { Id = vm.LocationId };
                _context.Add(location);
                await _context.SaveChangesAsync();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var newReview = new Models.Review { Content = vm.ReviewContent, Score = vm.ReviewScore };

                    var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    newReview.User = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);

                    location.Reviews.Add(newReview);
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Location.Any(e => e.Id == location.Id))
                    {
                        return NotFound();
                    }
                }
                return RedirectToAction("Details","Locations", new { id = location.Id });
            }
            return View(vm);
        }

        // GET: Reviews/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
            
        //    var review = await _context.Review.Include(m => m.User).Include(m => m.Location).SingleOrDefaultAsync(m => m.Id == id);

        //    if (review == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    if (review?.User?.Id != userId)
        //    {
        //        return RedirectToAction("AccessDenied", "Account");
        //    }

        //    var vm = new CreateEditReviewViewModel(id ?? 1) {
        //        LocationId = review.Location.Id,
        //        //LocationName = review.Location.Name,
        //        ReviewId = review.Id,
        //        ReviewContent = review.Content,
        //        ReviewScore = review.Score };

        //    return View(vm);
        //}

        //// POST: Reviews/Edit
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(CreateEditReviewViewModel vm)
        //{
        //    var editedReview = await _context.Review.Include(m => m.User).SingleOrDefaultAsync(m => m.Id == vm.ReviewId);
        //    if (editedReview == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    if (editedReview?.User?.Id != userId)
        //    {
        //        return RedirectToAction("AccessDenied", "Account");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            editedReview.Content = vm.ReviewContent;
        //            editedReview.Score = vm.ReviewScore;

        //            _context.Update(editedReview);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!_context.Review.Any(e => e.Id == editedReview.Id))
        //            {
        //                return NotFound();
        //            }
        //        }
        //        return RedirectToAction("Details", "Locations", new { id = vm.LocationId });
        //    }
        //    return View(vm);
        //}
        
        // GET: Reviews/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var review = await _context.Review.Include(m => m.User).Include(m => m.Location).SingleOrDefaultAsync(m => m.Id == id);

        //    if (review == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    if (review?.User?.Id != userId)
        //    {
        //        return RedirectToAction("AccessDenied", "Account");
        //    }

        //    var vm = new CreateEditReviewViewModel(id ?? 1)
        //    {
        //        LocationId = review.Location.Id,
        //        //LocationName = review.Location.Name,
        //        ReviewId = review.Id,
        //        ReviewContent = review.Content,
        //        ReviewScore = review.Score,
        //        Date = review.Date
        //    };

        //    return View(vm);
        //}
        
        //// POST: Reviews/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(CreateEditReviewViewModel vm)
        //{
        //    var review = await _context.Review.Include(m => m.User).SingleOrDefaultAsync(m => m.Id == vm.ReviewId);

        //    var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    if (review?.User?.Id != userId)
        //    {
        //        return RedirectToAction("AccessDenied", "Account");
        //    }

        //    _context.Review.Remove(review);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Details", "Locations", new { id = vm.LocationId });
        //}

        private bool ReviewExists(int id)
        {
            return _context.Review.Any(e => e.Id == id);
        }
    }
}
