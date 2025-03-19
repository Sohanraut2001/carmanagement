namespace car.api.DTO
{
    public class CarModelSearchDto
    {
        public string ModelName { get; set; }
        public string ModelCode { get; set; }
        public string SortBy { get; set; } = "DateOfManufacturing";
        public bool SortDescending { get; set; } = true;
    }
}
