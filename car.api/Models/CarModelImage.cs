namespace car.api.Models
{
    public class CarModelImage
    {
        public int Id { get; set; }
        public int CarModelId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsDefault { get; set; }
    }
}
