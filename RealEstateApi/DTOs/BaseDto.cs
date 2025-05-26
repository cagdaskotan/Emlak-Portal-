namespace RealEstateApi.DTOs
{
    public class BaseDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; } = DateTime.Now;
    }
}
