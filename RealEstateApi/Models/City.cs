using RealEstateApi.Models;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Listing> Listings { get; set; }
}
