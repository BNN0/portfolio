using Luxottica.ApplicationServices.Commissioners;
using Luxottica.Core.Entities.Commissioners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luxottica.Controllers.Commissioners
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommissionerController : ControllerBase
    {
        private readonly ICommissionersAppService _commissionersService;
        private readonly ILogger<CommissionerController> _logger;
        public CommissionerController(ICommissionersAppService commissionersService, ILogger<CommissionerController> logger)
        {
            _commissionersService = commissionersService;
            _logger = logger;
        }

        [HttpGet("GetCommissionerStatus")]
        public async Task<ActionResult<object>> GetStatusOfFirstCommissioner()
        {
            try
            {
                List<Commissioner> commissioners = await _commissionersService.GetComissionnersAsync();

                if (commissioners.Any())
                {
                    Commissioner firstCommissioner = commissioners.First();
                    return Ok(new { Status = firstCommissioner.Status });
                }
                _logger.LogError($"ERROR SELECT COMMISSIONER IN CONTROLLER, Message: No Commissioner records found.");
                return NotFound(new { Message = "No Commissioner records found." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT COMMISSIONER IN CONTROLLER {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("ChangeCommissionerStatus")]
        public async Task<ActionResult> ChangeCommissionerStatus(bool newStatus)
        {
            try
            {
                Commissioner commissioner = await _commissionersService.GetFirstCommissionerAsync();

                if (commissioner != null)
                {
                    commissioner.Status = newStatus;
                    await _commissionersService.UpdateCommissionerAsync(commissioner);
                    return Ok(new { Message = "Commissioner status updated successfully." });
                }
                _logger.LogError($"ERROR SELECT COMMISSIONER IN CONTROLLER, Message: No Commissioner records found.");
                return NotFound(new { Message = "No Commissioner records found." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE COMMISSIONER IN CONTROLLER {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

    }
}
