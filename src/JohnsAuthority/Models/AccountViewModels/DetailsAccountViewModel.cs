using JohnsAuthority.Helpers;

namespace JohnsAuthority.Models.AccountViewModels
{
    public class DetailsAccountViewModel
    {
        public ApplicationUser User { get; set; }
        public PaginatedList<Review> Reviews { get; set; }
    }
}
