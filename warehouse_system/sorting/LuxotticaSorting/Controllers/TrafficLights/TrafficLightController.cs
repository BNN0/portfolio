using LuxotticaSorting.ApplicationServices.Shared.DTO.TrafficLights;
using LuxotticaSorting.ApplicationServices.TrafficLights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LuxotticaSorting.Controllers.TrafficLights
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TrafficLightController : ControllerBase
    {
        private readonly ITrafficLightAppService _trafficService;
        private readonly ILogger<TrafficLightController> _logger;

        public TrafficLightController(ITrafficLightAppService trafficService, ILogger<TrafficLightController> logger)
        {
            _trafficService = trafficService;
            _logger = logger;
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("TruckLine2")]
        public async Task<IActionResult> GetStatusTrafficLightLine2()
        {
            try
            {
                TrafficLightDataDto trafficlightLine2 = await _trafficService.GetStatusLightLine2();
                return Ok(trafficlightLine2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In TrafficLightControllermethod GetStatusTrafficLightLine2 failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("TruckLine4")]
        public async Task<IActionResult> GetStatusTrafficLightLine4()
        {
            try
            {
                TrafficLightDataDto trafficlightLine2 = await _trafficService.GetStatusLightLine4();
                return Ok(trafficlightLine2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In TrafficLightController GetStatusTrafficLightLine4 failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
