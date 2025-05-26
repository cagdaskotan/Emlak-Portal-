namespace RealEstateApi.DTOs
{
    public class ListingDto : BaseDto
    {
        public string? UserId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<IFormFile>? Files { get; set; }
        public int CategoryId { get; set; }
        public CategoryDto? Category { get; set; }
        public List<ListingImageDto>? Images { get; set; }
        public string? PhoneNumber { get; set; }
        public int CityId { get; set; }
        public CityDTO? City { get; set; }
        public string Address { get; set; }

    }
}
