using Luxottica.ApplicationServices.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.Scanlogs;
using Luxottica.Controllers.DivertLines;
using Luxottica.Core.Entities.Scanlogs;
using Luxottica.Models.ScanlogsInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Luxottica.Controllers.ScanlogsRecivingPicking
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ScanlogsReceivingPickingController : ControllerBase
    {
        private readonly IScanlogsAppService _scanlogsAppService;
        private readonly ILogger<DivertLineController> _logger;

        public ScanlogsReceivingPickingController(IScanlogsAppService scanlogsAppService, ILogger<DivertLineController> logger)
        {
            _scanlogsAppService = scanlogsAppService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin, SuperUser")]
        // GET: api/<ScanlogsReceivingPickingController>
        [HttpGet]
        public async Task<IActionResult> GetAllScanlogs()
        {
            try
            {
                List<ScanlogsReceivingPicking> result = await _scanlogsAppService.GetAllScanlogsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT SCANLOGS CONTROLLER {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }        
    }
}
