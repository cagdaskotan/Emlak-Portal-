namespace RealEstateApi.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İlişkiler
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int ListingId { get; set; }
        public Listing Listing { get; set; }
    }

}
