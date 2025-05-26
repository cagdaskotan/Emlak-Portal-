using RealEstateApi.Models;

namespace RealEstateApi.Repositories
{
    public class CommentRepository : GenericRepository<Category>
    {
        public CommentRepository(AppDbContext context) : base(context)
        {
        }

        // Comment'e özel ekstra metodlar buraya yazılır.
    }
}
