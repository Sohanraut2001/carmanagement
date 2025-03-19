using car.api.Models;

namespace car.api.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<CommissionReport>> GenerateCommissionReportsAsync(int month, int year);
    }
}
