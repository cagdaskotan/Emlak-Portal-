namespace RealEstateApi.DTOs
{
    public class FavoriteDTO
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int ListingId { get; set; }

        public ListingDto? Listing { get; set; }
    }
}
