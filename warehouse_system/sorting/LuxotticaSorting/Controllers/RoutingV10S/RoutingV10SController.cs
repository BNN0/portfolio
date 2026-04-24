using LuxotticaSorting.ApplicationServices.RoutingV10;
using LuxotticaSorting.ApplicationServices.Shared.Dto.RoutingV10S;
using LuxotticaSorting.Core.ScanLogSortings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.RoutingV10S
{
    [Authorize]
    [Route("api/Routing")]
    [ApiController]
    public class RoutingV10SController : ControllerBase
    {
        private readonly IRoutingAppService _routingService;
        private readonly ILogger<RoutingV10SController> _logger;
        public RoutingV10SController(IRoutingAppService routingService, ILogger<RoutingV10SController> logger)
        {
            _routingService = routingService;
            _logger = logger;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<RoutingV10Dto> boxes = await _routingService.GetBoxesInformationRoutingAsync();
                return Ok(boxes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In RoutingV10SController method GetAll unsuccessful, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("orderInformationSAP/{boxId}")]
        public async Task<IActionResult> GetById(string boxId, int trackingId)
        {
            try
            {
                RoutingV10Dto boxType = await _routingService.GetOrdersBoxIdSAPInformation(boxId, trackingId);
                if (boxType == null)
                {
                    _logger.LogWarning($"Box SAP with ID {boxId} not found in RoutingV10SController method GetById");
                    return NotFound(new { Message = $"Box SAP with ID {boxId} not found." });
                }

                return Ok(new { Message = "A result was found and the information was added to the corresponding table." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"In RoutingV10SController method GetById failed for {boxId}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }





    }
}
