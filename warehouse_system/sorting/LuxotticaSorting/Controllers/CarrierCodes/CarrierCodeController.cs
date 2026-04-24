using LuxotticaSorting.ApplicationServices.CarriersCodes;
using LuxotticaSorting.ApplicationServices.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.Controllers.LogisticAgents;
using LuxotticaSorting.Core.CarrierCodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.CarrierCodes
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CarrierCodeController : ControllerBase
    {
        private readonly ICarrierCodeAppService _carrierCodeAppService;
        private readonly ILogger<CarrierCodeController> _logger;
        public CarrierCodeController(ICarrierCodeAppService carrierCodeAppService, ILogger<CarrierCodeController> logger)
        {
            _carrierCodeAppService = carrierCodeAppService;
            _logger = logger;

        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<CarrierCode> carrierCodes = await _carrierCodeAppService.GetCarrierCodesAsync();
                return Ok(carrierCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeController method GetAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
        
        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                CarrierCode carrierCode = await _carrierCodeAppService.GetCarrierCodeByIdAsync(id);
                if (carrierCode == null)
                {
                    return NotFound($"Carrier Code with ID {id} not found.");
                }
                return Ok(carrierCode);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeController method GetById failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPost()]
        public async Task<IActionResult> Post(CarrierCodeAddDto entity)
        {
            try
            {
                if (entity == null)
                {
                    return BadRequest("Invalid JSON Model!.");
                }

                await _carrierCodeAppService.AddCarrierCodeAsync(entity);
                return Ok(new { Message = "CarrierCode added successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeController method Post failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, CarrierCodeAddDto entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    return BadRequest("Invalid JSON model!.");
                }
                await _carrierCodeAppService.EditCarrierCodeAsync(id, entity);
                return Ok(new { Message = "CarrierCode edited successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeController method Put failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest("Id is null!.");
                }
                await _carrierCodeAppService.DeleteCarrierCodeAsync(id);
                return Ok(new { Message = "CarrierCode deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeController method Delete failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
