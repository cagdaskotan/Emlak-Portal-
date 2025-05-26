using RealEstateApi.Models;

namespace RealEstateApi.Repositories
{
    public class FavoriteRepository : GenericRepository<Category>
    {
        public FavoriteRepository(AppDbContext context) : base(context)
        {
        }

        // Favorite'e özel ekstra metodlar buraya yazılır.
    }
}
