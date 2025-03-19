namespace car.api.Models
{
    public class BrandCommission
    {
        public string Brand { get; set; }
        public decimal FixedCommission { get; set; }
        public decimal ClassACommission { get; set; }
        public decimal ClassBCommission { get; set; }
        public decimal ClassCCommission { get; set; }
        public decimal AdditionalCommission { get; set; }
        public decimal TotalCommission { get; set; }
    }
}
