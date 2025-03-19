using car.api.Interfaces;
using car.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace car.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("commission")]
        public async Task<ActionResult<IEnumerable<CommissionReport>>> GetCommissionReport([FromQuery] int month, [FromQuery] int year)
        {
            if (month < 1 || month > 12)
                return BadRequest("Month must be between 1 and 12");

            if (year < 2000 || year > 2050)
                return BadRequest("Year must be between 2000 and 2050");

            var reports = await _reportService.GenerateCommissionReportsAsync(month, year);
            return Ok(reports);
        }
    }
}
