namespace RealEstateApi.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }                  // Yorum ID
        public string Content { get; set; }          // Yoruma yazılan metin
        public int ListingId { get; set; }           // Hangi ilana ait olduğunu belirtir

        // Bu alanlar sadece response sırasında otomatik atanır
        public string? UserId { get; set; }          // Yorumu yapan kullanıcı
        public string? UserName { get; set; }        // Yorumu yapan kullanıcının kullanıcı adı
        public DateTime? CreatedAt { get; set; }     // Oluşturulma zamanı
    }
}
