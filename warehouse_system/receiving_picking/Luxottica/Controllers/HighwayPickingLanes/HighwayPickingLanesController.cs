using Luxottica.ApplicationServices.HighwayPickingLanes;
using Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes;
using Luxottica.Controllers.DivertLines;
using Luxottica.Core.Entities.Commissioners;
using Luxottica.Core.Entities.HighwayPikingLanes;
using Luxottica.Models.HighwayPickingLanes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Luxottica.Controllers.HighwayPickingLanes
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HighwayPickingLanesController : ControllerBase
    {
        private readonly IHighWayPickingLanesAppService _highwayPickingService;
        private readonly ILogger<HighwayPickingLanesController> _logger;
        public HighwayPickingLanesController(IHighWayPickingLanesAppService highwayPickingService, ILogger<HighwayPickingLanesController> logger)
        {
            _highwayPickingService = highwayPickingService;
            _logger = logger;
        }

        [HttpGet("LimitHighwayPickingLanes")]
        public async Task<ActionResult<object>> GetLimitsHighway()
        {
            List<HighwayPickingLanesDTO> highwayPikingLanes = await _highwayPickingService.GetHighwayPickingLaneAsync();
            if (!highwayPikingLanes.Any())
            {
                await _highwayPickingService.AddHighwayAsync();
            }
            try
            {
                if (highwayPikingLanes.Any())
                {
                    HighwayPickingLanesDTO highwayData = highwayPikingLanes.First();
                    var highway = new HighwayResponse
                    {
                        MaxToteHighway = highwayData.MultiTotes,
                        MaxTotesLPTUMachine1 = highwayData.MaxTotesLPTUMachine1,
                        MaxTotesSPTAMachine1 = highwayData.MaxTotesSPTAMachine1,
                        MaxTotesSPTAMachine2 = highwayData.MaxTotesSPTAMachine2,
                    };
                    return Ok(highway);
                }
                _logger.LogError($"ERROR SELECT HighwayPickingLanes IN CONTROLLER, Message: No HighwayPicking records found.");
                return NotFound(new { Message = "No HighwayPicking records found." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT HighwayPickingLanes IN CONTROLLER {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPut("EditLimits")]
        public async Task<IActionResult> EditLimits(HighwayPickingRequest request)
        {
            List<HighwayPickingLanesDTO> highwayPikingLanes = await _highwayPickingService.GetHighwayPickingLaneAsync();
            if (!highwayPikingLanes.Any())
            {
                await _highwayPickingService.AddHighwayAsync();
            }
            try
            {
                await _highwayPickingService.UpdateLimits(request);
                return Ok(new { Message = $"Successfully edited limits in HighwayPicking." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Update HighwayPickingLanes IN CONTROLLER {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }

        }


    }
}

