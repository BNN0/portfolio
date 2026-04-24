using LuxotticaSorting.ApplicationServices.MultiBoxWaves;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MultiBoxWaves;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.MultiBoxWaves
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MultiBoxWaveController : ControllerBase
    {
        private readonly IMultiBoxWaveAppService _multiBoxWaveAppService;
        private readonly ILogger<MultiBoxWaveController> _logger;

        public MultiBoxWaveController(IMultiBoxWaveAppService multiBoxWaveAppService, ILogger<MultiBoxWaveController> logger)
        {
            _multiBoxWaveAppService = multiBoxWaveAppService;
            _logger = logger;
        }

        [HttpPost()]
        public async Task<IActionResult> AddMultiBoxWave([FromBody] MultiBoxWavesAddDto multiBoxWaves)
        {
            try
            {
                var result = await _multiBoxWaveAppService.AddMultiBoxWavesAsync(multiBoxWaves);

                if (result.Item2)
                {
                    return Ok(new { Added = result.Item1, Completed = result.Item2 });
                }
                else
                {
                    _logger.LogWarning("Failed to add MultiBoxWave in MultiBoxWaveController");
                    return BadRequest(new { Added = result.Item1, Completed = result.Item2 });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MultiBoxWaveController method AddMultiBoxWave : {ex.Message}");
                return StatusCode(500, new { Message = $"Error in AddMultiBoxWave" });
            }
        }

        [HttpPost("ConfirmMultiBoxWave")]
        public async Task<IActionResult> ConfirmMultiBoxWave([FromBody] MultiBoxWavesAddDto multiBoxWaves)
        {
            try
            {
                var result = await _multiBoxWaveAppService.ConfirmMultiBoxWaveAsync(multiBoxWaves);
                if (result.Item2)
                {
                    return Ok(new { Added = result.Item1, Completed = result.Item2 });
                }
                else
                {
                    _logger.LogWarning("Failed to add ConfirmMultiBoxWave in MultiBoxWaveController");
                    return BadRequest(new { Added = result.Item1, Completed = result.Item2 });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MultiBoxWaveController method ConfirmMultiBoxWave: {ex.Message}");
                return StatusCode(500, new { Message = $"Error in ConfirmMultiBoxWave" });
            }
        }

        [HttpGet("{confirmationNumber}")]
        public async Task<IActionResult> GetMultiBoxWave(string confirmationNumber)
        {
            try
            {
                var result = await _multiBoxWaveAppService.GetMultiBoxWavesAsync(confirmationNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MultiBoxWaveController method GetMultiBoxWave: {ex.Message}");
                return StatusCode(500, new { Message = $"Error in GetMultiBoxWave" });
            }
        }

        [HttpDelete("{multiBoxWavesId}")]
        public async Task<IActionResult> DeleteMultiBoxWave(int multiBoxWavesId)
        {
            try
            {
                await _multiBoxWaveAppService.DeleteMultiBoxWavesAsync(multiBoxWavesId);
                return Ok(new { Message = $"MultiBoxWave with ID {multiBoxWavesId} deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MultiBoxWaveController method DeleteMultiBoxWave: {ex.Message}");
                return StatusCode(500, new { Message = $"Error in DeleteMultiBoxWave" });
            }
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllMultiBoxWaves()
        {
            try
            {
                var result = await _multiBoxWaveAppService.GetAllMultiBoxWavesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MultiBoxWaveController method GetAllMultiBoxWaves: {ex.Message}");
                return StatusCode(500, new { Message = $"Error in GetAllMultiBoxWaves" });
            }
        }

        [HttpGet("GetAllBoxMultiBoxWave")]
        public async Task<IActionResult> GetAllBoxMultiBoxWaveAsync()
        {
            try
            {
                var result = await _multiBoxWaveAppService.GetAllBoxMultiBoxWaveAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MultiBoxWaveController method GetAllMultiBoxWaves: {ex.Message}");
                return StatusCode(500, new { Message = $"Error in GetAllMultiBoxWaves" });
            }
        }

        [HttpPost("ManualConfirmationMultiBox")]
        public async Task<IActionResult> ManualConfirmationMultiBoxWaves(MultiBoxWaveConfirmationDto manualConfirmation)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(manualConfirmation.BoxId))
                {
                    _logger.LogWarning("BoxId field is null, please enter a value in MultiBoxWaveController method ManualConfirmationMultiBoxWaves");
                    return BadRequest(new { Message = "BoxId field is null, please enter a value" });
                }
                if (string.IsNullOrWhiteSpace(manualConfirmation.ConfirmationNumber))
                {
                    _logger.LogWarning("ConfirmationNumber field is null, please enter a value in MultiBoxWaveController method ManualConfirmationMultiBoxWaves");
                    return BadRequest(new { Message = "ConfirmationNumber field is null, please enter a value" });
                }
                var result = await _multiBoxWaveAppService.ManualConfirmationMultiBoxWavesForTrucks(manualConfirmation);
                if (result)
                {
                    return Ok(new { Message = $"Confirmation Box: {manualConfirmation.BoxId} is successful" });
                }
                else
                {
                    _logger.LogWarning($"Confirmation Box: {manualConfirmation.BoxId} is unsuccessful in MultiBoxWaveController method ManualConfirmationMultiBoxWaves");
                    return BadRequest(new { Message = $"Confirmation Box: {manualConfirmation.BoxId} is unsuccessful,try again" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"method ManualConfirmationMultiBoxWaves in MultiBoxWaveController failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost("MaxCountQtyConfiguration")]
        public async Task<IActionResult> MaxCountQtyConfiguration(MaxCountQtyAddDto MaxCountQty)
        {
            try
            {
                if (MaxCountQty == null)
                {
                    _logger.LogWarning("BoxId field is null, please enter a value in MultiboxWaveController method MaxCountQtyConfiguration");
                    return BadRequest(new { Message = "BoxId field is null, please enter a value" });
                }
                if (MaxCountQty.maxCountQty <= 0)
                {
                    _logger.LogWarning("Please enter a value higher to zero, in MultiboxWaveController method MaxCountQtyConfiguration");
                    return BadRequest(new { Message = "Please enter a value higher to zero" });
                }
                var result = await _multiBoxWaveAppService.MaxCountQtyConfiguration(MaxCountQty.maxCountQty);
                if (result != null)
                {
                    return Ok(new { Message = $"Successfully updated the limit of multiboxes by order." });
                }
                else
                {
                    _logger.LogWarning("Get information about MaxCountQtyConfiguration is unsuccessful, in MultiboxWaveController method MaxCountQtyConfiguration");
                    return BadRequest(new { Message = $"MaxCountQtyConfiguration: Get information about MaxCountQtyConfiguration is unsuccessful,try again" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"In MultiboxWaveController,MaxCountQtyConfiguration method failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("GetMaxCountQtyConfiguration")]
        public async Task<IActionResult> GetMaxCountQtyConfiguration()
        {
            try
            {
                var result = await _multiBoxWaveAppService.GetMaxCountQtyConfiguration();
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Get information about GetMaxCountQtyConfiguration is unsuccessful, in MultiboxWaveController method MaxCountQtyConfiguration");
                    return BadRequest(new { Message = $"GetMaxCountQtyConfiguration: Get information about GetMaxCountQtyConfiguration is unsuccessful,try again" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"In MultiboxWaveController, GetMaxCountQtyConfiguration method failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
