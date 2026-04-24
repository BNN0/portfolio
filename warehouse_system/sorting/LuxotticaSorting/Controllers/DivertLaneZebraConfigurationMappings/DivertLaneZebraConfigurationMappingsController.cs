using LuxotticaSorting.ApplicationServices.DivertLanes;
using LuxotticaSorting.ApplicationServices.Mapping.DivertLaneZebraConfigurations;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLaneZebraConfigurationMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.Controllers.DivertLanes;
using LuxotticaSorting.Core.Mapping.DivertLaneZebraConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.DivertLaneZebraConfigurationMappings
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DivertLaneZebraConfigurationMappingsController : ControllerBase
    {
        private readonly IDivertLaneZebraConfigurationAppService _divertLanesAppService;
        private readonly ILogger<DivertLaneZebraConfigurationMappingsController> _logger;
        public DivertLaneZebraConfigurationMappingsController(IDivertLaneZebraConfigurationAppService divertLanesAppService,
            ILogger<DivertLaneZebraConfigurationMappingsController> logger)
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
                List<DivertLaneZebraConfigurationMappingDTO> conf = await _divertLanesAppService.GetDivertLaneZebraConfigurationMappingAsync();  
                return Ok(conf);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAll DivertLaneZebraConfigurationMappings method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("GetCombinatedData")]
        public async Task<IActionResult> GetCombinatedData()
        {
            try
            {
                List<DivertLanesZebraConfigurationCombinated> conf = await _divertLanesAppService.GetDivertLaneZebraConfigurationMappingCombinatedDataAsync();
                return Ok(conf);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetCombinatedData method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                DivertLaneZebraConfigurationMappingDTO confDTO = await _divertLanesAppService.GetDivertLaneZebraConfigurationMappingByIdAsync(id);
                if (confDTO == null)
                {
                    return NotFound($"DivertLaneZebraConfigurationMapping with ID {id} not found.");
                }
                return Ok(confDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById DivertLaneZebraConfigurationMappings method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpPost()]
        public async Task<IActionResult> Post(DivertLaneZebraConfigurationMappingAddDTO entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Invalid JSON Model!.");
                    return BadRequest("Invalid JSON Model!.");
                }

                await _divertLanesAppService.AddDivertLaneZebraConfigurationMappingAsync(entity);
                return Ok(new { Message = "DivertLaneZebraConfigurationMapping added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post DivertLaneZebraConfigurationMapping method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, DivertLaneZebraConfigurationMappingAddDTO entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    _logger.LogWarning("Invalid request: Invalid ID or model in Put method DivertLaneZebra.");
                    return BadRequest("Invalid JSON model!.");
                }
                await _divertLanesAppService.EditDivertLaneZebraConfigurationMappingAsync(id, entity);
                return Ok(new { Message = "DivertLaneZebraConfigurationMapping edited successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Put DivertLaneZebraConfigurationMapping method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }


        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogWarning($"Id is null in DivertLaneZebraConfigurationMapping method failed");
                    return BadRequest("Id is null!.");
                }
                await _divertLanesAppService.DeleteDivertLaneZebraConfigurationMappingAsync(id);
                return Ok(new { Message = "DivertLaneZebraConfigurationMapping deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete DivertLaneZebraConfigurationMapping method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
