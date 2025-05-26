namespace RealEstateApi.Models
{
    public class Listing : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
        public string Address { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<ListingImage> Images { get; set; }
    }
}
