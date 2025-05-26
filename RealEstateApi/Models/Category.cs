namespace RealEstateApi.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<Listing>? Listings { get; set; }
    }
}
