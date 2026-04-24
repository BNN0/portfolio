using LuxotticaSorting.ApplicationServices.Containers;
using LuxotticaSorting.ApplicationServices.DivertLanes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.Controllers.DivertLanes;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.Containers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ContainerController : ControllerBase
    {
        private readonly IContainerAppService _containerAppService;
        private readonly ILogger<ContainerController> _logger;
        public ContainerController(IContainerAppService containerAppService, ILogger<ContainerController> logger)
        {
            _containerAppService = containerAppService;
            _logger = logger;

        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<ContainerDTO> containers = await _containerAppService.GetContainersAsync();
                return Ok(containers);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method GetAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpGet("GetTruckAll")]
        public async Task<IActionResult> GetTruckAll()
        {
            try
            {
                List<ContainerToShow> containers = await _containerAppService.GetContainersTruckAsync();
                if(containers != null)
                {
                    return Ok(containers);
                }
                else
                {
                    return Ok(new { Message = "There are no Truck containers" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method GetTruckAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpGet("GetGaylordAll")]
        public async Task<IActionResult> GetGaylordAll()
        {
            try
            {
                List<ContainerToShow> containers = await _containerAppService.GetContainersGaylordAsync();
                if (containers != null)
                {
                    return Ok(containers);
                }
                else
                {
                    return Ok(new { Message = "There are no Gaylord containers" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method GetGaylordAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                ContainerDTO containerDTO = await _containerAppService.GetContainerByIdAsync(id);
                if (containerDTO == null)
                {
                    _logger.LogWarning($"Containers with ID {id} not found in ContainerController method GetById");
                    return NotFound($"Containers with ID {id} not found.");
                }
                return Ok(containerDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method GeyById failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"Internal Server Error: {ex.Message}" });
            }
        }



        [HttpPost]
        public async Task<IActionResult> Post(ContainerAddDTO entity)
        {
            try
            {
                if (entity == null)
                {
                    return BadRequest("Invalid JSON Model in ContainerController method Post");
                }

                await _containerAppService.AddContainerAsync(entity);
                return Ok(new { Message = "Container added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method Post failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpPost("GayLord")]
        public async Task<IActionResult> PostGayLord(ContainerAddOneStepDTO entity)
        {
            try
            {
                if (entity == null)
                {
                    return BadRequest("Invalid JSON Model in ContainerController method PostGaylord");
                }

                await _containerAppService.AddContainerGayLordAsync(entity);
                return Ok(new { Message = "Container added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method PostGaylord failede, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost("Truck")]
        public async Task<IActionResult> PostTruck(ContainerAddOneStepDTO entity)
        {
            try
            {
                if (entity == null)
                {
                    return BadRequest("Invalid JSON Model in ContainerController method PostTruck");
                }

                await _containerAppService.AddContainerTruckAsync(entity);
                return Ok(new { Message = "Container added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method PostTruck failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost("ContainerToPrint")]
        public async Task<IActionResult> PostToPrint(ContainerToPrint entity)
        {
            try
            {
                if (entity == null)
                {
                    return BadRequest("Invalid JSON Model in ContainerController method PostPrint");
                }

                await _containerAppService.AddContainerToPrintAsync(entity);
                return Ok(new { Message = "Container added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method PostPrint failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ContainerAddDTO entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    _logger.LogWarning("Invalid JSON model in ContainerController method Put");
                    return BadRequest("Invalid JSON model in ContainerController method Put");
                }
                await _containerAppService.EditContainerAsync(id, entity);
                return Ok(new { Message = "Container edited successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method Put failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogWarning("Id is null in ContainerController method Delete");
                    return BadRequest("Id is null in ContainerController method Delete");
                }
                await _containerAppService.DeleteContainerAsync(id);
                return Ok(new { Message = "Container deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerController method Delete failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
