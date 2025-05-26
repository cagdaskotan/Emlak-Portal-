namespace RealEstateApi.DTOs
{
    public class ListingUpdateDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? CategoryId { get; set; }
        public int? CityId { get; set; }
        public string? Address { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
