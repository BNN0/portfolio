using Luxottica.ApplicationServices.DivertOutboundLines;
using Luxottica.ApplicationServices.HighwayPickingLanes;
using Luxottica.ApplicationServices.Shared.Dto.DivertOutboundLines;
using Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes;
using Luxottica.Controllers.HighwayPickingLanes;
using Luxottica.Models.DivertOutboundLines;
using Luxottica.Models.HighwayPickingLanes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Luxottica.Controllers.DivertOutboundLines
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DivertOutboundLineController : ControllerBase
    {
        private readonly IDivertOutboundLine _divertOutboundLineService;
        private readonly ILogger<DivertOutboundLineController> _logger;

        public DivertOutboundLineController(IDivertOutboundLine divertOutboundLineService, ILogger<DivertOutboundLineController> logger)
        {
            _divertOutboundLineService = divertOutboundLineService;
            _logger = logger;
        }

        [HttpGet("LimitDivertOutboundLine")]
        public async Task<ActionResult<object>> GetLimitsDivertOutboundLine()
        {
            List<DivertOutboundLineDTO> divertOutboundLine = await _divertOutboundLineService.GetDivertOutboundLineAsync();
            if (!divertOutboundLine.Any())
            {
                await _divertOutboundLineService.AddDivertOutboundLineAsync();
            }
            try
            {

                divertOutboundLine = await _divertOutboundLineService.GetDivertOutboundLineAsync();
                if (divertOutboundLine.Any())
                {
                    DivertOutboundLineDTO divertOutboundLineData = divertOutboundLine.First();
                    var divertOutboundLines = new DivertOutboundResponse
                    {
                        MaxToteDivertOutbound = divertOutboundLineData.MultiTotes,
                        MaxTotesLPTUMachine1 = divertOutboundLineData.MaxTotesLPTUMachine1,
                        MaxTotesSPTAMachine1 = divertOutboundLineData.MaxTotesSPTAMachine1,
                        MaxTotesSPTAMachine2 = divertOutboundLineData.MaxTotesSPTAMachine2,
                    };
                    return Ok(divertOutboundLines);
                }
                _logger.LogError($"ERROR SELECT DivertOutboundLine IN CONTROLLER, Message: No divertOutboundLine records found.");
                return NotFound(new { Message = "No divertOutboundLine records found." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT DivertOutboundLine IN CONTROLLER {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("CommissionerPresent/LimitDivertOutboundLine")]
        public async Task<ActionResult<object>> GetLimitsDivertOutboundLinePresent()
        {
            List<DivertOutboundLineDTO> divertOutboundLine = await _divertOutboundLineService.GetDivertOutboundLineAsync();
            if (!divertOutboundLine.Any())
            {
                await _divertOutboundLineService.AddDivertOutboundLineAsync();
            }
            try
            {
                var response = await _divertOutboundLineService.GetDivertOutboundLineLimitsPresentAsync();
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT DivertOutboundLine PRESENT IN CONTROLLER MESSAGE {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPut("CommissionerPresent/UpdateLimitDivertOutboundLine")]
        public async Task<IActionResult> UpdateLimitsDivertOutboundLinePresent(DivertOutboundLineRequestDto request)
        {
            List<DivertOutboundLineDTO> divertOutboundLine = await _divertOutboundLineService.GetDivertOutboundLineAsync();
            if (!divertOutboundLine.Any())
            {
                await _divertOutboundLineService.AddDivertOutboundLineAsync();
            }
            try
            {
                await _divertOutboundLineService.UpdateLimitsPresent(request);
                return Ok(new { Message = $"Successfully edited limits in divertOutboundLine Commissioner Present." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE DivertOutboundLine PRESENT IN CONTROLLER Message {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPut("EditLimits")]
        public async Task<IActionResult> EditLimits(DivertOutboundLineRequestDto request)
        {
            List<DivertOutboundLineDTO> divertOutboundLine = await _divertOutboundLineService.GetDivertOutboundLineAsync();
            if (!divertOutboundLine.Any())
            {
                await _divertOutboundLineService.AddDivertOutboundLineAsync();
            }
            try
            {
                await _divertOutboundLineService.UpdateLimits(request);
                return Ok(new { Message = $"Successfully edited limits in divertOutboundLine." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE DivertOutboundLine IN CONTROLLER MESSAGE {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }

        }
    }
}
