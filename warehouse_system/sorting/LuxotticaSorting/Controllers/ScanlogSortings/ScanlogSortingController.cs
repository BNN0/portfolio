using LuxotticaSorting.ApplicationServices.PrintLabel;
using LuxotticaSorting.ApplicationServices.ScanlogSortings;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.Controllers.PrintLabel;
using LuxotticaSorting.Core.ScanLogSortings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.ScanlogSortings
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScanlogSortingController : ControllerBase
    {
        private readonly IScanlogSortingAppService _scanlogSorting;
        private readonly ILogger<ScanlogSortingController> _logger;
        public ScanlogSortingController(IScanlogSortingAppService scanlogSortingAppService, ILogger<ScanlogSortingController> logger)
        {
            _scanlogSorting = scanlogSortingAppService;
            _logger = logger;

        }
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<ScanLogSorting> c = await _scanlogSorting.GetScanLogSortingAsync();
                return Ok(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In ScanlogSortingController method GetAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

    }
}
