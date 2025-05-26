namespace RealEstateApi.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public string UserId { get; set; }  // ilişkili kullanıcı
        public AppUser User { get; set; }

        public int ListingId { get; set; }  // ilişkili ilan
        public Listing Listing { get; set; }
    }
}
