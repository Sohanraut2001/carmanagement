namespace car.api.DTO
{
    public class CarModelDto
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Class { get; set; }
        public string ModelName { get; set; }
        public string ModelCode { get; set; }
        public string Description { get; set; }
        public string Features { get; set; }
        public decimal Price { get; set; }
        public DateTime DateOfManufacturing { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public List<CarModelImageDto> Images { get; set; }
    }
}
