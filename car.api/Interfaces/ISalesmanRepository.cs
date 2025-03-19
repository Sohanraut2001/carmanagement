using car.api.DTO;
using car.api.Models;

namespace car.api.Interfaces
{
    public interface ISalesmanRepository
    {
        Task<IEnumerable<Salesman>> GetAllAsync();
        Task<Salesman> GetByIdAsync(int id);
        Task<IEnumerable<Sale>> GetSalesByMonthAsync(int month, int year);
    }

}
