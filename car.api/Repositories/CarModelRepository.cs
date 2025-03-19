using car.api.DTO;
using car.api.Interfaces;
using car.api.Models;
using Dapper;
using System.Data;
using System.Text;

namespace car.api.Repositories
{
    public class CarModelRepository : ICarModelRepository
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<CarModelRepository> _logger;

        public CarModelRepository(IDbConnection connection, ILogger<CarModelRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<IEnumerable<CarModel>> GetAllAsync(CarModelSearchDto searchDto)
        {
            try
            {
                var query = new StringBuilder(@"
                SELECT cm.*, cmi.Id as ImageId, cmi.ImageUrl, cmi.IsDefault 
                FROM CarModels cm
                LEFT JOIN CarModelImages cmi ON cmi.CarModelId = cm.Id
                WHERE 1=1");

                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(searchDto.ModelName))
                {
                    query.Append(" AND cm.ModelName LIKE @ModelName");
                    parameters.Add("ModelName", $"%{searchDto.ModelName}%");
                }

                if (!string.IsNullOrEmpty(searchDto.ModelCode))
                {
                    query.Append(" AND cm.ModelCode LIKE @ModelCode");
                    parameters.Add("ModelCode", $"%{searchDto.ModelCode}%");
                }

                string orderBy = searchDto.SortBy switch
                {
                    "DateOfManufacturing" => "cm.DateOfManufacturing",
                    "SortOrder" => "cm.SortOrder",
                    _ => "cm.DateOfManufacturing"
                };

                query.Append($" ORDER BY {orderBy} {(searchDto.SortDescending ? "DESC" : "ASC")}");

                var carModelDictionary = new Dictionary<int, CarModel>();

                var result = await _connection.QueryAsync<CarModel, CarModelImage, CarModel>(
                    query.ToString(),
                    (carModel, image) =>
                    {
                        if (!carModelDictionary.TryGetValue(carModel.Id, out var currentCarModel))
                        {
                            currentCarModel = carModel;
                            currentCarModel.Images = new List<CarModelImage>();
                            carModelDictionary.Add(currentCarModel.Id, currentCarModel);
                        }

                        if (image != null)
                        {
                            currentCarModel.Images.Add(image);
                        }

                        return currentCarModel;
                    },
                    parameters,
                    splitOn: "ImageId"
                );

                return carModelDictionary.Values;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting car models");
                throw;
            }
        }

        public async Task<CarModel> GetByIdAsync(int id)
        {
            try
            {
                var query = @"
                SELECT cm.*, cmi.Id as ImageId, cmi.ImageUrl, cmi.IsDefault 
                FROM CarModels cm
                LEFT JOIN CarModelImages cmi ON cmi.CarModelId = cm.Id
                WHERE cm.Id = @Id";

                var carModelDictionary = new Dictionary<int, CarModel>();

                var result = await _connection.QueryAsync<CarModel, CarModelImage, CarModel>(
                    query,
                    (carModel, image) =>
                    {
                        if (!carModelDictionary.TryGetValue(carModel.Id, out var currentCarModel))
                        {
                            currentCarModel = carModel;
                            currentCarModel.Images = new List<CarModelImage>();
                            carModelDictionary.Add(currentCarModel.Id, currentCarModel);
                        }

                        if (image != null)
                        {
                            currentCarModel.Images.Add(image);
                        }

                        return currentCarModel;
                    },
                    new { Id = id },
                    splitOn: "ImageId"
                );

                return carModelDictionary.Values.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting car model with id {Id}", id);
                throw;
            }
        }

        public async Task<int> CreateAsync(CarModel carModel)
        {
            try
            {
                var query = @"
                INSERT INTO CarModels (Brand, Class, ModelName, ModelCode, Description, Features, Price, DateOfManufacturing, IsActive, SortOrder)
                VALUES (@Brand, @Class, @ModelName, @ModelCode, @Description, @Features, @Price, @DateOfManufacturing, @IsActive, @SortOrder);
                SELECT CAST(SCOPE_IDENTITY() as int)";

                return await _connection.QuerySingleAsync<int>(query, carModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating car model");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(CarModel carModel)
        {
            try
            {
                var query = @"
                UPDATE CarModels 
                SET Brand = @Brand,
                    Class = @Class,
                    ModelName = @ModelName,
                    ModelCode = @ModelCode,
                    Description = @Description,
                    Features = @Features,
                    Price = @Price,
                    DateOfManufacturing = @DateOfManufacturing,
                    IsActive = @IsActive,
                    SortOrder = @SortOrder
                WHERE Id = @Id";

                var affectedRows = await _connection.ExecuteAsync(query, carModel);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating car model with id {Id}", carModel.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                // First delete related images
                await _connection.ExecuteAsync("DELETE FROM CarModelImages WHERE CarModelId = @Id", new { Id = id });

                // Then delete the car model
                var affectedRows = await _connection.ExecuteAsync("DELETE FROM CarModels WHERE Id = @Id", new { Id = id });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting car model with id {Id}", id);
                throw;
            }
        }

        public async Task<bool> AddImageAsync(CarModelImage image)
        {
            try
            {
                var query = @"
                INSERT INTO CarModelImages (CarModelId, ImageUrl, IsDefault)
                VALUES (@CarModelId, @ImageUrl, @IsDefault)";

                var affectedRows = await _connection.ExecuteAsync(query, image);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image for car model with id {Id}", image.CarModelId);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            try
            {
                var affectedRows = await _connection.ExecuteAsync("DELETE FROM CarModelImages WHERE Id = @Id", new { Id = imageId });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image with id {Id}", imageId);
                throw;
            }
        }

        public async Task<bool> SetDefaultImageAsync(int imageId, int carModelId)
        {
            try
            {
                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        // First reset all images for this car model
                        await _connection.ExecuteAsync("UPDATE CarModelImages SET IsDefault = 0 WHERE CarModelId = @CarModelId",
                            new { CarModelId = carModelId }, transaction);

                        // Then set the selected image as default
                        var affectedRows = await _connection.ExecuteAsync("UPDATE CarModelImages SET IsDefault = 1 WHERE Id = @Id",
                            new { Id = imageId }, transaction);

                        transaction.Commit();
                        return affectedRows > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default image with id {Id} for car model {CarModelId}", imageId, carModelId);
                throw;
            }
        }
    }
}
