using car.api.Interfaces;
using car.api.Models;
using Dapper;
using System.Data;

namespace car.api.Repositories
{
    public class SalesmanRepository : ISalesmanRepository
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<SalesmanRepository> _logger;

        public SalesmanRepository(IDbConnection connection, ILogger<SalesmanRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<IEnumerable<Salesman>> GetAllAsync()
        {
            try
            {
                return await _connection.QueryAsync<Salesman>("SELECT * FROM Salesmen");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all salesmen");
                throw;
            }
        }

        public async Task<Salesman> GetByIdAsync(int id)
        {
            try
            {
                return await _connection.QuerySingleOrDefaultAsync<Salesman>(
                    "SELECT * FROM Salesmen WHERE Id = @Id", new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting salesman with id {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Sale>> GetSalesByMonthAsync(int month, int year)
        {
            try
            {
                return await _connection.QueryAsync<Sale>(
                    "SELECT * FROM Sales WHERE MONTH(SaleDate) = @Month AND YEAR(SaleDate) = @Year",
                    new { Month = month, Year = year });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales for month {Month} and year {Year}", month, year);
                throw;
            }
        }
    }
}
