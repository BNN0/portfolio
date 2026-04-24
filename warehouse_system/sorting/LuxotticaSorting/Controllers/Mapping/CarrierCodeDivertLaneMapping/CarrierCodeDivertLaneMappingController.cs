using LuxotticaSorting.ApplicationServices.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.Controllers.LogisticAgents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.Mapping.CarrierCodeDivertLaneMapping
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CarrierCodeDivertLaneMappingController : ControllerBase
    {
        private readonly ICarrierCodeDivertLaneMappingAppService _carrierCodeDivertLaneMappingService;
        private readonly ILogger<CarrierCodeDivertLaneMappingController> _logger;
        public CarrierCodeDivertLaneMappingController(ICarrierCodeDivertLaneMappingAppService carrierCodeDivertLaneMappingService, ILogger<CarrierCodeDivertLaneMappingController> logger)
        {
            _carrierCodeDivertLaneMappingService = carrierCodeDivertLaneMappingService;
            _logger = logger;

        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<CarrierCodeDivertLaneMappingDto> CarrierCodeDivertLaneMapping = await _carrierCodeDivertLaneMappingService.GetCarrierCodeDivertLaneMappingAsync();
                return Ok(CarrierCodeDivertLaneMapping);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDIvertLaneMappingController in GetAll method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                CarrierCodeDivertLaneMappingDto CarrierCodeDivertLaneMapping = await _carrierCodeDivertLaneMappingService.GetCarrierCodeDivertLaneMappingByIdAsync(id);
                if (CarrierCodeDivertLaneMapping == null)
                {
                    _logger.LogWarning($"CarrierCodeDivertLaneMapping with ID {id} not found.");
                    return NotFound($"CarrierCodeDivertLaneMapping with ID {id} not found.");
                }
                return Ok(CarrierCodeDivertLaneMapping);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDIvertLaneMappingController in GetById method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(CarrierCodeDivertLaneMappingAddDto entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Invalid JSON Model in CarrierCodeDIvertLaneMappingController method Post");
                    return BadRequest("Invalid JSON Model!.");
                }

                await _carrierCodeDivertLaneMappingService.AddCarrierCodeDivertLaneMappingAsync(entity);
                var response = new { Message = "Assignment created successfully" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post in CarrierCodeDIvertLaneMappingController method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, CarrierCodeDivertLaneMappingEditDto entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    _logger.LogWarning("Invalid ID or model in CarrierCodeDIvertLaneMappingController method Put");
                    return BadRequest("Invalid JSON model!.");
                }
                await _carrierCodeDivertLaneMappingService.EditCarrierCodeDivertLaneMappingAsync(id, entity);
                var response = new { Message = "Assignment updated successfully" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Put in CarrierCodeDIvertLaneMappingController method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id == null)
                {
                    _logger.LogWarning($"Id is null in CarrierCodeDIvertLaneMappingController method Delete");
                    return BadRequest("Id is null!.");
                }
                await _carrierCodeDivertLaneMappingService.DeleteCarrierCodeDivertLaneMappingAsync(id);
                var response = new { Message = "Assignment deleted successfully" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDIvertLaneMappingController in Delete method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
        [HttpGet("GetAllCombinedData")]
        public async Task<IActionResult> GetAllCombinedData()
        {
            try
            {
                var combinedDataList = await _carrierCodeDivertLaneMappingService.GetCombinedDataAsync();
                return Ok(combinedDataList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDIvertLaneMappingController in GetAllCombinedData method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

    }
}
