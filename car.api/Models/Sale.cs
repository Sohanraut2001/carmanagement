namespace car.api.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int SalesmanId { get; set; }
        public string CarClass { get; set; }
        public string Brand { get; set; }
        public int NumberOfCars { get; set; }
        public decimal CarPrice { get; set; }
    }
}
