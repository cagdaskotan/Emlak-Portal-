using RealEstateApi.Models;

namespace RealEstateApi.Repositories
{
    public class CategoryRepository : GenericRepository<Category>
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        // Category'e özel ekstra metodlar buraya yazılır.
    }
}
