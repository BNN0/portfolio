using LuxotticaSorting.ApplicationServices.MappingSorter;
using LuxotticaSorting.ApplicationServices.Shared.Dto.MappingSorters;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.MappingSorters
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class MappingSorterController : ControllerBase
    {
        private readonly IMappingSorterAppService _mappingSorterAppService;
        private readonly ILogger<MappingSorterController> _logger;
        public MappingSorterController(IMappingSorterAppService mappingSorterAppService, ILogger<MappingSorterController> logger)
        {
            _mappingSorterAppService = mappingSorterAppService;
            _logger = logger;

        }
        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<MappingSorterDto> MappingSorter = await _mappingSorterAppService.GetMappingSorterAsync();
                return Ok(MappingSorter);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAll in MappingSorterController, method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                MappingSorterDto MappingSorter = await _mappingSorterAppService.GetMappingSorterByIdAsync(id);
                if (MappingSorter == null)
                {
                    _logger.LogWarning($"MappingSorter with ID {id} not found in MappingSorterController method GetById.");
                    return NotFound($"MappingSorter with ID {id} not found.");
                }
                return Ok(MappingSorter);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById in MappingSorterController, method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {
                await _mappingSorterAppService.AddMappingSorterAsync();
                var response = new { Message = "Assignment created successfully" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post in MappingSorterController method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, MappingSorterAddDto entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    _logger.LogWarning("Invalid ID or model in MappingSorterController method Put.");
                    return BadRequest("Invalid JSON model!.");
                }
                await _mappingSorterAppService.EditMappingSorterAsync(id, entity);
                var response = new { Message = "Assignment updated successfully" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Put in MappingSorterController method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("GetAllCombinedData")]
        public async Task<IActionResult> GetAllCombinedData()
        {
            try
            {
                var combinedDataList = await _mappingSorterAppService.GetCombinedDataAsync();
                return Ok(combinedDataList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllCombinedData in MappingSorterController, method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost("MappingTruck")]
        public async Task<IActionResult> PostMapingTruck(MappingSorterAddDivertContainerTyDto mappingDivertLaneContainerTypeTruck)
        {
            try
            {
                if (mappingDivertLaneContainerTypeTruck.DivertLaneId == 0)
                {
                    _logger.LogWarning("DivertLane Invalid in MappingSorterController method PostMapingTruck");
                    return BadRequest(new { Message = "DivertLane Invalid." });
                }
                await _mappingSorterAppService.AddMappingDivertLaneAndContainerTypeTruck(mappingDivertLaneContainerTypeTruck);
                return Ok(new { Message = "Successfully registered new Mapping Divertlane and container type for truck" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"PostMapingTruck in MappingSorterController, method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost("MappingGaylord")]
        public async Task<IActionResult> PostMapingGaylord(MappingSorterAddDivertContainerTyDto mappingDivertLaneContainerTypeGaylord)
        {
            try
            {
                if (mappingDivertLaneContainerTypeGaylord.DivertLaneId == 0)
                {
                    _logger.LogWarning("DivertLane Invalid in MappingSorterController method PostMapingGaylord");
                    return BadRequest(new { Message = "DivertLane Invalid." });
                }
                await _mappingSorterAppService.AddMappingDivertLaneAndContainerTypeGaylord(mappingDivertLaneContainerTypeGaylord);
                return Ok(new { Message = "Successfully registered new Mapping Divertlane and container type for gaylord" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"PostMapingGaylord in MappingSorterController, method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpDelete("DeleteDataContainerTypeInMappingSorter/{id}")]
        public async Task<IActionResult> DeleteDataContainerType(int id)
        {
            try
            {
                if (id == null)
                {
                    _logger.LogWarning($"Id is null in MappingSorterController method DeleteDataContainerType");
                    return BadRequest("Id is null!.");
                }
                await _mappingSorterAppService.DeleteDataContainerTypeInMappingSorter(id);
                return Ok(new { Message = "Assignment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteDataContainerType in MappingSorterController, method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("GetOnlyDivertLaneAndContainerType")]
        public async Task<IActionResult> GetOnlyDivertLaneAndContainerTypeData()
        {
            try
            {
                var onlyDivertandContainerType = await _mappingSorterAppService.GetOnlyDivertLaneAndConainerType();
                return Ok(onlyDivertandContainerType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetOnlyDivertLaneAndContainerTypeData in MappingSorterController, method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

    }
}
