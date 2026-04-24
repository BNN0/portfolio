using LuxotticaSorting.ApplicationServices.ContainerTypes;
using LuxotticaSorting.ApplicationServices.Shared.Dto.ContainerTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.ContainerTypes
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ContainerTypeController : ControllerBase
    {
        private readonly IContainerTypeAppService _containerTypeService;
        private readonly ILogger<ContainerTypeController> _logger;
        public ContainerTypeController(IContainerTypeAppService containerTypeService, ILogger<ContainerTypeController> logger)
        {
            _containerTypeService = containerTypeService;
            _logger = logger ;
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<ContainerTypeDto> containerType = await _containerTypeService.GetContainerTypesAsync();
                return Ok(containerType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAll in ContainerTypeController method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                ContainerTypeDto containerType = await _containerTypeService.GetContainerTypeByIdAsync(id);
                if (containerType == null)
                {
                    _logger.LogWarning($"ContainerType with ID {id} not found in ContainerTypeController method GetById.");
                    return NotFound(new { Message = $"ContainerType with ID {id} not found." });
                }
                return Ok(containerType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById in ContainerTypeController method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPost]
        public async Task<IActionResult> Post(ContainerTypeAddDto entity)
        {
            try
            {
                if (entity == null || string.IsNullOrWhiteSpace(entity.ContainerTypes))
                {
                    _logger.LogWarning("Invalid JSON Model in ContainerTypeController method Post");
                    return BadRequest(new { Message = "Invalid JSON Model." });
                }
                await _containerTypeService.AddContainerTypeAsync(entity);
                return Ok(new { Message = "Successfully registered ContainerType" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post in ContainerTypeController method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ContainerTypeAddDto entity)
        {
            try
            {
                if ( id == 0 ||entity == null || string.IsNullOrWhiteSpace(entity.ContainerTypes))
                {
                    _logger.LogWarning("Invalid JSON model in ContainerTypeController method Put");
                    return BadRequest(new { Message = "Invalid JSON model." });
                }
                await _containerTypeService.EditContainerTypeAsync(id, entity);
                return Ok(new { Message = "Successfully updated ContainerType" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Put in ContainerTypeController method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            ContainerTypeDto containerType = await _containerTypeService.GetContainerTypeByIdAsync(id);
            try
            {
                if (containerType != null)
                {
                    if (containerType.ContainerTypes == "G" || containerType.ContainerTypes == "T" || containerType.ContainerTypes == "H" || containerType.ContainerTypes == "P")
                    {
                        _logger.LogWarning("This type of container is G, T, P, H, or P, therefore it cannot be deleted.");
                        return BadRequest(new { Message = "This type of container is G, T, P, H, or P, therefore it cannot be deleted." });
                    }
                }

                if ( id == 0)
                {
                    _logger.LogWarning($"Container of id: {id} is null in ContainerTypeController method Delete") ;
                    return BadRequest(new { Message = $"Container of id: {id} is null" });
                }

                await _containerTypeService.DeleteContainerTypeAsync(id);
                return Ok(new { Message = "ContainerType successfully removed" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete in ContainerTypeController method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
