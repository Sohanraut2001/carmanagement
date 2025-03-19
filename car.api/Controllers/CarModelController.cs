using car.api.DTO;
using car.api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace car.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarModelController : ControllerBase
    {
        private readonly ICarModelService _carModelService;
        private readonly ILogger<CarModelController> _logger;

        public CarModelController(ICarModelService carModelService, ILogger<CarModelController> logger)
        {
            _carModelService = carModelService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarModelDto>>> GetAll([FromQuery] CarModelSearchDto searchDto)
        {
            var carModels = await _carModelService.GetAllAsync(searchDto);
            return Ok(carModels);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CarModelDto>> GetById(int id)
        {
            var carModel = await _carModelService.GetByIdAsync(id);

            if (carModel == null)
                return NotFound();

            return Ok(carModel);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<int>> Create([FromBody] CarModelDto carModelDto)
        {
            var id = await _carModelService.CreateAsync(carModelDto);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] CarModelDto carModelDto)
        {
            if (id != carModelDto.Id)
                return BadRequest();

            var result = await _carModelService.UpdateAsync(carModelDto);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _carModelService.DeleteAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/images")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UploadImages(int id, [FromForm] int defaultImageIndex, [FromForm] List<IFormFile> images)
        {
            if (images == null || !images.Any())
                return BadRequest("No images provided");

            var result = await _carModelService.UploadImagesAsync(id, images, defaultImageIndex);
            return Ok(result);
        }

        [HttpDelete("images/{imageId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var result = await _carModelService.DeleteImageAsync(imageId);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{carModelId}/images/{imageId}/setDefault")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> SetDefaultImage(int carModelId, int imageId)
        {
            var result = await _carModelService.SetDefaultImageAsync(imageId, carModelId);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IEnumerable<string>>> GetBrands()
        {
            var brands = await _carModelService.GetBrandsAsync();
            return Ok(brands);
        }

        [HttpGet("classes")]
        public async Task<ActionResult<IEnumerable<string>>> GetClasses()
        {
            var classes = await _carModelService.GetClassesAsync();
            return Ok(classes);
        }
    }
}
