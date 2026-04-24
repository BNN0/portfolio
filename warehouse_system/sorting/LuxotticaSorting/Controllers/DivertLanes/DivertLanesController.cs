using LuxotticaSorting.ApplicationServices.BoxTypes;
using LuxotticaSorting.ApplicationServices.DivertLanes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.Controllers.BoxTypes;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.DivertLanes
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DivertLanesController : ControllerBase
    {
        private readonly IDivertLanesAppService _divertLanesAppService;
        private readonly ILogger<DivertLanesController> _logger;
        public DivertLanesController(IDivertLanesAppService divertLanesAppService, ILogger<DivertLanesController> logger)
        {
            _divertLanesAppService = divertLanesAppService;
            _logger = logger;

        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<DivertLanesDTO> divertLanes = await _divertLanesAppService.GetDivertLanesAsync();
                return Ok(divertLanes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method GetAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("GetDivertLanesToPrintTruck")]
        public async Task<IActionResult> GetAllDivertLanesTruckToPrint()
        {
            try
            {
                List<DivertLanesDTO> divertLanes = await _divertLanesAppService.GetDivertLanesToPrintTruckAsync();
                return Ok(divertLanes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method GetAllDivertLanesTruckToPrint failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("GetDivertLanesToPrintGaylord")]
        public async Task<IActionResult> GetAllDivertLanesGaylordToPrint()
        {
            try
            {
                List<DivertLanesDTO> divertLanes = await _divertLanesAppService.GetDivertLanesToPrintGaylordAsync();
                return Ok(divertLanes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method GetAllDivertLanesGaylordToPrint failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                DivertLanesDTO divertLane = await _divertLanesAppService.GetDivertLaneByIdAsync(id);
                if (divertLane == null)
                {
                    _logger.LogWarning($"DivertLane with ID {id} not found in DivertLanesController method GetById");
                    return NotFound($"DivertLane with ID {id} not found.");
                }
                return Ok(divertLane);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method GetById failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("GetAllCreation")]
        public async Task<IActionResult> GetAllCreation()
        {
            try
            {
                List<DivertLanesCreationDTO> divertLanes = await _divertLanesAppService.GetDivertLanesCreationAsync();
                return Ok(divertLanes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method GetAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("GetByIdCreation/{id}")]
        public async Task<IActionResult> GetByIdCreation(int id)
        {
            try
            {
                DivertLanesCreationDTO divertLane = await _divertLanesAppService.GetDivertLaneCreationByIdAsync(id);
                if (divertLane == null)
                {
                    _logger.LogWarning($"DivertLane Creation with {id} not found in DivertLanesController method GetByIdCreation");
                    return NotFound($"DivertLane Creation with ID {id} not found.");
                }
                return Ok(divertLane);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method GetByIdCreation failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpPost]
        public async Task<IActionResult> Post(DivertLanesAddDto entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Invalid JSON Model in DivertLanesController method Post");
                    return BadRequest("Invalid JSON Model!.");
                }

                await _divertLanesAppService.AddDivertLaneAsync(entity);
                return Ok(new { Message = "DivertLane added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method Post failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
        
        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPost("Creation")]
        public async Task<IActionResult> PostCreation(DivertLanesAddCreationDTO entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Invalid JSON Model in DivertLanesController method PostCreation");
                    return BadRequest("Invalid JSON Model!.");
                }

                await _divertLanesAppService.AddDivertLaneCreationAsync(entity);
                return Ok(new { Message = "DivertLane added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method Post failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, DivertLanesAddDto entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    _logger.LogWarning("Invalid ID or model in DivertLanesController method Put");
                    return BadRequest("Invalid JSON model!.");
                }
                await _divertLanesAppService.EditDivertLaneAsync(id, entity);
                return Ok(new { Message = "DivertLane edited successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method Put failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPut("creation/{id}")]
        public async Task<IActionResult> PutCreation(int id, DivertLanesAddCreationDTO entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    _logger.LogWarning("Invalid ID or model in DivertLanesController method PutCreation");
                    return BadRequest("Invalid JSON model!.");
                }
                await _divertLanesAppService.EditDivertLaneCreationAsync(id, entity);
                return Ok(new { Message = "DivertLane Creation edited successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method PutCreation failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogWarning($"Id is null in DivertLanesController method Delete");
                    return BadRequest("Id is null!.");
                }
                await _divertLanesAppService.DeleteDivertLaneAsync(id);
                return Ok(new { Message = "DivertLane deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesController method Delete for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
