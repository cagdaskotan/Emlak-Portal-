namespace RealEstateApi.Models
{
    public class ListingImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }

        public int ListingId { get; set; }
        public Listing Listing { get; set; }
    }

}
