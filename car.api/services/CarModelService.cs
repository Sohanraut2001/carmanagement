using car.api.DTO;
using car.api.Interfaces;
using car.api.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace car.api.services
{
    public class CarModelService : ICarModelService
    {
        private readonly ICarModelRepository _repository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CarModelService> _logger;

        public CarModelService(
            ICarModelRepository repository,
            IWebHostEnvironment environment,
            ILogger<CarModelService> logger)
        {
            _repository = repository;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IEnumerable<CarModelDto>> GetAllAsync(CarModelSearchDto searchDto)
        {
            var carModels = await _repository.GetAllAsync(searchDto);
            return carModels.Select(MapToDto);
        }

        public async Task<CarModelDto> GetByIdAsync(int id)
        {
            var carModel = await _repository.GetByIdAsync(id);
            return carModel != null ? MapToDto(carModel) : null;
        }

        public async Task<int> CreateAsync(CarModelDto carModelDto)
        {
            ValidateCarModel(carModelDto);
            var carModel = MapToEntity(carModelDto);
            return await _repository.CreateAsync(carModel);
        }

        public async Task<bool> UpdateAsync(CarModelDto carModelDto)
        {
            ValidateCarModel(carModelDto);
            var carModel = MapToEntity(carModelDto);
            return await _repository.UpdateAsync(carModel);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // First get the car model to find any images
            var carModel = await _repository.GetByIdAsync(id);
            if (carModel == null)
                return false;

            // Delete any physical image files
            foreach (var image in carModel.Images)
            {
                DeletePhysicalImage(image.ImageUrl);
            }

            // Delete the car model (and its images in DB via repository)
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> UploadImagesAsync(int carModelId, List<IFormFile> images, int defaultImageIndex)
        {
            if (images == null || !images.Any())
                throw new ArgumentException("No images provided");

            // Validate carModelId exists
            var carModel = await _repository.GetByIdAsync(carModelId);
            if (carModel == null)
                throw new ArgumentException($"Car model with id {carModelId} not found");

            // Validate images
            foreach (var image in images)
            {
                if (image.Length > 5 * 1024 * 1024) // 5MB
                    throw new ArgumentException($"Image {image.FileName} exceeds 5MB limit");

                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
                    throw new ArgumentException($"Invalid image format for {image.FileName}");
            }

            // Process each image
            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "carmodels", fileName);

                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Create the image record
                var carModelImage = new CarModelImage
                {
                    CarModelId = carModelId,
                    ImageUrl = $"/uploads/carmodels/{fileName}",
                    IsDefault = i == defaultImageIndex
                };

                await _repository.AddImageAsync(carModelImage);
            }

            return true;
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            // Get the car model images to find the one to delete
            var allCarModels = await _repository.GetAllAsync(new CarModelSearchDto());
            var image = allCarModels
                .SelectMany(cm => cm.Images)
                .FirstOrDefault(img => img.Id == imageId);

            if (image == null)
                return false;

            // Delete the physical file
            DeletePhysicalImage(image.ImageUrl);

            // Delete from database
            return await _repository.DeleteImageAsync(imageId);
        }

        public async Task<bool> SetDefaultImageAsync(int imageId, int carModelId)
        {
            return await _repository.SetDefaultImageAsync(imageId, carModelId);
        }

        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            // Return fixed brands as per requirement
            return new List<string> { "Audi", "Jaguar", "Land Rover", "Renault" };
        }

        public async Task<IEnumerable<string>> GetClassesAsync()
        {
            // Return fixed classes as per requirement
            return new List<string> { "A-Class", "B-Class", "C-Class" };
        }

        private void ValidateCarModel(CarModelDto carModelDto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(carModelDto.Brand))
                errors.Add("Brand is required");
            else if (!new[] { "Audi", "Jaguar", "Land Rover", "Renault" }.Contains(carModelDto.Brand))
                errors.Add("Invalid brand selection");

            if (string.IsNullOrWhiteSpace(carModelDto.Class))
                errors.Add("Class is required");
            else if (!new[] { "A-Class", "B-Class", "C-Class" }.Contains(carModelDto.Class))
                errors.Add("Invalid class selection");

            if (string.IsNullOrWhiteSpace(carModelDto.ModelName))
                errors.Add("Model name is required");

            if (string.IsNullOrWhiteSpace(carModelDto.ModelCode))
                errors.Add("Model code is required");
            else if (carModelDto.ModelCode.Length > 10)
                errors.Add("Model code cannot exceed 10 characters");
            else if (!Regex.IsMatch(carModelDto.ModelCode, "^[a-zA-Z0-9]*$"))
                errors.Add("Model code must contain only alphanumeric characters");

            if (string.IsNullOrWhiteSpace(carModelDto.Description))
                errors.Add("Description is required");

            if (string.IsNullOrWhiteSpace(carModelDto.Features))
                errors.Add("Features is required");

            if (carModelDto.Price <= 0)
                errors.Add("Price must be greater than zero");

            if (carModelDto.DateOfManufacturing == default)
                errors.Add("Date of manufacturing is required");

            if (errors.Any())
                throw new ValidationException("Validation failed");
        }

        private CarModelDto MapToDto(CarModel entity)
        {
            return new CarModelDto
            {
                Id = entity.Id,
                Brand = entity.Brand,
                Class = entity.Class,
                ModelName = entity.ModelName,
                ModelCode = entity.ModelCode,
                Description = entity.Description,
                Features = entity.Features,
                Price = entity.Price,
                DateOfManufacturing = entity.DateOfManufacturing,
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder,
                Images = entity.Images?.Select(i => new CarModelImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsDefault = i.IsDefault
                }).ToList() ?? new List<CarModelImageDto>()
            };
        }

        private CarModel MapToEntity(CarModelDto dto)
        {
            return new CarModel
            {
                Id = dto.Id,
                Brand = dto.Brand,
                Class = dto.Class,
                ModelName = dto.ModelName,
                ModelCode = dto.ModelCode,
                Description = dto.Description,
                Features = dto.Features,
                Price = dto.Price,
                DateOfManufacturing = dto.DateOfManufacturing,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder
            };
        }

        private void DeletePhysicalImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return;

                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "carmodels", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting physical image file {ImageUrl}", imageUrl);
                // Continue execution even if file deletion fails
            }
        }
    }

}
