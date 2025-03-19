using car.api.DTO;
using car.api.Models;

namespace car.api.Interfaces
{
    public interface ICarModelRepository
    {
        Task<IEnumerable<CarModel>> GetAllAsync(CarModelSearchDto searchDto);
        Task<CarModel> GetByIdAsync(int id);
        Task<int> CreateAsync(CarModel carModel);
        Task<bool> UpdateAsync(CarModel carModel);
        Task<bool> DeleteAsync(int id);
        Task<bool> AddImageAsync(CarModelImage image);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> SetDefaultImageAsync(int imageId, int carModelId);
    }
}
