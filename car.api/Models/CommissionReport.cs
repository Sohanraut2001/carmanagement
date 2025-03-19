namespace car.api.Models
{
    public class CommissionReport
    {
        public string SalesmanName { get; set; }
        public decimal FixedCommission { get; set; }
        public decimal ClassCommission { get; set; }
        public decimal AdditionalCommission { get; set; }
        public decimal TotalCommission { get; set; }
        public Dictionary<string, BrandCommission> BrandCommissions { get; set; } = new Dictionary<string, BrandCommission>();
    }
}
