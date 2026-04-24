using LuxotticaSorting.ApplicationServices.PrintLabel;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ZebraHistorial;
using LuxotticaSorting.ApplicationServices.ZebraHistorial;
using LuxotticaSorting.Controllers.PrintLabel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.ZebraHistorial
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ZebraHistorialController : ControllerBase
    {
        private readonly IZebraHistorialAppService _zebraHistorialAppService;
        private readonly ILogger<ZebraHistorialController> _logger;
        public ZebraHistorialController(IZebraHistorialAppService zebraHistorialAppService, ILogger<ZebraHistorialController> logger)
        {
            _zebraHistorialAppService = zebraHistorialAppService;
            _logger = logger;

        }
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<ZebraHistorialDTO> conf = await _zebraHistorialAppService.GetZebraHistorialsAsync();
                return Ok(conf);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In ZebraHistorialController mehtod GetAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("GetToReprintGaylord")]
        public async Task<IActionResult> GetToRePrintAll()
        {
            try
            {
                List<ZebraHistorialData> conf = await _zebraHistorialAppService.GetZebraHistorialsToRePrintAsync();
                return Ok(conf);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In ZebraHistorialController method GetRePrintAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("GetGaylord")]
        public async Task<IActionResult> GetGaylordAll()
        {
            try
            {
                List<ZebraHistorialData> conf = await _zebraHistorialAppService.GetZebraHistorialGaylordAsync();
                return Ok(conf);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In ZebraHistorialController method GetGaylordAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("GetTruck")]
        public async Task<IActionResult> GetTruckAll()
        {
            try
            {
                List<ZebraHistorialData> conf = await _zebraHistorialAppService.GetZebraHistorialTruckAsync();
                return Ok(conf);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In ZebraHistorialController method GetTruckAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }



        [HttpGet("GetCombinatedData")]
        public async Task<IActionResult> GetCombinatedDataAll()
        {
            try
            {
                List<ZebraHistorialData> conf = await _zebraHistorialAppService.GetZebraHistorialDataCombinatedAsync();
                return Ok(conf);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In ZebraHistorialController method GetAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                ZebraHistorialDTO confDTO = await _zebraHistorialAppService.GetZebraHistorialByIdAsync(id);
                if (confDTO == null)
                {
                    _logger.LogWarning($"ZebraHistorials with ID {id} not found, in ZebraHistorialController method GetById");
                    return NotFound($"ZebraHistorials with ID {id} not found.");
                }
                return Ok(confDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In ZebraHistorialController method GetById for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpDelete("DeleteZebraHistorials")]
        public async Task<IActionResult> Delete()
        {
            try
            {
                await _zebraHistorialAppService.DeleteZebraHistorialsAsync();
                return Ok(new { Message = "ZebraHistorial deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"In ZebraHistorialController method Delete failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
