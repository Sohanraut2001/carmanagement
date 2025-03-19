using car.api.DTO;
using car.api.Models;

namespace car.api.Interfaces
{
    public interface ICarModelService
    {
        Task<IEnumerable<CarModelDto>> GetAllAsync(CarModelSearchDto searchDto);
        Task<CarModelDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CarModelDto carModelDto);
        Task<bool> UpdateAsync(CarModelDto carModelDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> UploadImagesAsync(int carModelId, List<IFormFile> images, int defaultImageIndex);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> SetDefaultImageAsync(int imageId, int carModelId);
        Task<IEnumerable<string>> GetBrandsAsync();
        Task<IEnumerable<string>> GetClassesAsync();
    }

    
}
