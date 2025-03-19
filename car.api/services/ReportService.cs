using car.api.DTO;
using car.api.Interfaces;
using car.api.Models;

namespace car.api.services
{
    public class ReportService : IReportService
    {
        private readonly ISalesmanRepository _salesmanRepository;
        private readonly ICarModelRepository _carModelRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            ISalesmanRepository salesmanRepository,
            ICarModelRepository carModelRepository,
            ILogger<ReportService> logger)
        {
            _salesmanRepository = salesmanRepository;
            _carModelRepository = carModelRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CommissionReport>> GenerateCommissionReportsAsync(int month, int year)
        {
            try
            {
                var salesmen = await _salesmanRepository.GetAllAsync();
                var sales = await _salesmanRepository.GetSalesByMonthAsync(month, year);
                var carModels = await _carModelRepository.GetAllAsync(new CarModelSearchDto());

                var commissionReports = new List<CommissionReport>();

                foreach (var salesman in salesmen)
                {
                    var salesmanSales = sales.Where(s => s.SalesmanId == salesman.Id);
                    var report = new CommissionReport
                    {
                        SalesmanName = salesman.Name,
                        FixedCommission = 0,
                        ClassCommission = 0,
                        AdditionalCommission = 0,
                        TotalCommission = 0,
                        BrandCommissions = new Dictionary<string, BrandCommission>()
                    };

                    // Initialize brand commissions
                    foreach (var brand in new[] { "Audi", "Jaguar", "Land Rover", "Renault" })
                    {
                        report.BrandCommissions[brand] = new BrandCommission
                        {
                            Brand = brand,
                            FixedCommission = 0,
                            ClassACommission = 0,
                            ClassBCommission = 0,
                            ClassCCommission = 0,
                            AdditionalCommission = 0,
                            TotalCommission = 0
                        };
                    }

                    // Calculate commissions for each sale
                    foreach (var sale in salesmanSales)
                    {
                        var carModel = carModels.FirstOrDefault(cm =>
                            cm.Brand == sale.Brand && cm.Class == sale.CarClass);

                        if (carModel == null)
                            continue;

                        var brandCommission = report.BrandCommissions[sale.Brand];
                        decimal fixedCommission = 0;
                        decimal classCommission = 0;
                        decimal additionalCommission = 0;

                        // Brand-wise fixed commission
                        switch (sale.Brand)
                        {
                            case "Audi":
                                if (carModel.Price > 25000)
                                    fixedCommission = 800;
                                break;
                            case "Jaguar":
                                if (carModel.Price > 35000)
                                    fixedCommission = 750;
                                break;
                            case "Land Rover":
                                if (carModel.Price > 30000)
                                    fixedCommission = 850;
                                break;
                            case "Renault":
                                if (carModel.Price > 20000)
                                    fixedCommission = 400;
                                break;
                        }

                        // Class-wise commission percentage
                        decimal commissionRate = 0;
                        switch (sale.CarClass)
                        {
                            case "A-Class":
                                switch (sale.Brand)
                                {
                                    case "Audi": commissionRate = 0.08m; break;
                                    case "Jaguar": commissionRate = 0.06m; break;
                                    case "Land Rover": commissionRate = 0.07m; break;
                                    case "Renault": commissionRate = 0.05m; break;
                                }
                                // For A-Class, check if additional 2% applies
                                if (salesman.LastYearSales > 500000 && sale.CarClass == "A-Class")
                                {
                                    additionalCommission = sale.NumberOfCars * carModel.Price * 0.02m;
                                }
                                break;
                            case "B-Class":
                                switch (sale.Brand)
                                {
                                    case "Audi": commissionRate = 0.06m; break;
                                    case "Jaguar": commissionRate = 0.05m; break;
                                    case "Land Rover": commissionRate = 0.05m; break;
                                    case "Renault": commissionRate = 0.03m; break;
                                }
                                break;
                            case "C-Class":
                                switch (sale.Brand)
                                {
                                    case "Audi": commissionRate = 0.04m; break;
                                    case "Jaguar": commissionRate = 0.03m; break;
                                    case "Land Rover": commissionRate = 0.04m; break;
                                    case "Renault": commissionRate = 0.02m; break;
                                }
                                break;
                        }

                        classCommission = sale.NumberOfCars * carModel.Price * commissionRate;

                        // Update brand commission
                        brandCommission.FixedCommission += fixedCommission * sale.NumberOfCars;

                        switch (sale.CarClass)
                        {
                            case "A-Class":
                                brandCommission.ClassACommission += classCommission;
                                break;
                            case "B-Class":
                                brandCommission.ClassBCommission += classCommission;
                                break;
                            case "C-Class":
                                brandCommission.ClassCCommission += classCommission;
                                break;
                        }

                        brandCommission.AdditionalCommission += additionalCommission;
                        brandCommission.TotalCommission = brandCommission.FixedCommission +
                            brandCommission.ClassACommission + brandCommission.ClassBCommission +
                            brandCommission.ClassCCommission + brandCommission.AdditionalCommission;

                        // Update total commission
                        report.FixedCommission += fixedCommission * sale.NumberOfCars;
                        report.ClassCommission += classCommission;
                        report.AdditionalCommission += additionalCommission;
                        // Update total commission
                        report.FixedCommission += fixedCommission * sale.NumberOfCars;
                        report.ClassCommission += classCommission;
                        report.AdditionalCommission += additionalCommission;
                        report.TotalCommission += (fixedCommission * sale.NumberOfCars) + classCommission + additionalCommission;
                    }

                    // Add the report to the list
                    commissionReports.Add(report);
                }

                return commissionReports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating commission reports for {Month}/{Year}", month, year);
                throw; // Re-throw the exception to be handled by the caller
            }
        }
    }
}
