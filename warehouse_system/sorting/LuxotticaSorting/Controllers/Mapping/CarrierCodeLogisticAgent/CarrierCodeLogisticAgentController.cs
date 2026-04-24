using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeLogisticAgent;
using LuxotticaSorting.Controllers.Mapping.CarrierCodeDivertLaneMapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.Mapping.CarrierCodeLogisticAgent
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CarrierCodeLogisticAgentController : ControllerBase
    {
        private readonly ICarrierCodeLogisticAgentAppService _carrierCodeLogisticAgentService;
        private readonly ILogger<CarrierCodeLogisticAgentController> _logger;
        public CarrierCodeLogisticAgentController(ICarrierCodeLogisticAgentAppService carrierCodeLogisticAgentService, ILogger<CarrierCodeLogisticAgentController> logger)
        {
            _carrierCodeLogisticAgentService = carrierCodeLogisticAgentService;
            _logger = logger;

        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<CarrierCodeLogisticAgentDto> CarrierCodeLogisticAgent = await _carrierCodeLogisticAgentService.GetCarrierCodeLogisticAgentAsync();
                return Ok(CarrierCodeLogisticAgent);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeLogisticAgentController in GetAll method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                CarrierCodeLogisticAgentDto CarrierCodeLogisticAgent = await _carrierCodeLogisticAgentService.GetCarrierCodeLogisticAgentByIdAsync(id);
                if (CarrierCodeLogisticAgent == null)
                {
                    _logger.LogWarning($"CarrierCodeDivertLaneMapping with ID {id} not found.");
                    return NotFound($"CarrierCodeDivertLaneMapping with ID {id} not found.");
                }
                return Ok(CarrierCodeLogisticAgent);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById in CarrierCodeLogisticAgentController method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(CarrierCodeLogisticAgentAddDto entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Invalid JSON Model in CarrierCodeLogisticAgentController method Post");
                    return BadRequest("Invalid JSON Model!.");
                }

                await _carrierCodeLogisticAgentService.AddCarrierCodeLogisticAgentAsync(entity);
                var response = new { Message = "Assignment created successfully" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post in CarrierCodeLogisticAgentController method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, CarrierCodeLogisticAgentAddDto entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    _logger.LogWarning("Invalid request: Invalid ID or model in CarrierCodeLogisticAgentController method Put");
                    return BadRequest("Invalid JSON model!.");
                }
                await _carrierCodeLogisticAgentService.EditCarrierCodeLogisticAgentAsync(id, entity);
                var response = new { Message = "Assignment updated successfully" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Put in CarrierCodeLogisticAgentController method failed for ID {id}: {ex.Message}");
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
                    _logger.LogWarning($"Id is null in CarrierCodeLogisticAgentController method Delete");
                    return BadRequest("Id is null!.");
                }
                await _carrierCodeLogisticAgentService.DeleteCarrierCodeLogisticAgentAsync(id);
                var response = new { Message = "Assignment deleted successfully" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete in CarrierCodeLogisticAgentController method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("GetCombinedData")]
        public async Task<IActionResult> GetCombinedData()
        {
            try
            {
                var combinedData = await _carrierCodeLogisticAgentService.GetCombinedDataAsync();
                return Ok(combinedData);
            }
            catch (Exception ex)
            {
                
                _logger.LogError($"GetCombinedData in CarrierCodeLogisticAgentController method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

    }
}
