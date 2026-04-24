using LuxotticaSorting.ApplicationServices.LaneStatus;
using LuxotticaSorting.ApplicationServices.Shared.DTO.LaneStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LuxotticaSorting.Controllers.LaneStatus
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LaneStatusController : ControllerBase
    {
        private readonly ILaneStatusAppService _laneStatusAppService;
        private readonly ILogger<LaneStatusController> _logger;

        public LaneStatusController(ILaneStatusAppService laneStatusAppService, ILogger<LaneStatusController> logger)
        {
            _laneStatusAppService = laneStatusAppService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(LaneStatusDto entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Invalid JSON Model in LaneStatusController method post.");
                    return BadRequest(new { Message = "Invalid JSON Model!" });
                }

                await _laneStatusAppService.UpdateLaneStatus(entity);
                return Ok(new { Message = "LaneStatus added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post LaneStatus method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

    }
}
