using RealEstateApi.Models;

namespace RealEstateApi.Repositories
{
    public class ListingRepository : GenericRepository<Listing>
    {
        public ListingRepository(AppDbContext context) : base(context)
        {
        }

        // Listing'e özel ekstra metodlar buraya yazılır.
    }
}
