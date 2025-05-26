using Microsoft.AspNetCore.Identity;

namespace RealEstateApi.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }

        // Bir kullanıcının birçok ilanı olabilir.
        public ICollection<Listing> Listings { get; set; }
        public string? UserPhoto { get; set; } = "/UserPhotos/user.png";
    }
}
