using LuxotticaSorting.ApplicationServices.Mapping.BoxTypeDivertLane;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypeDivertLaneMapping;
using LuxotticaSorting.Core.Mapping.BoxTypeDivertLane;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LuxotticaSorting.Controllers.Mapping.BoxType_DivertLane
{
    [Route("api/[controller]")]
    [Authorize]    
    [ApiController]
    public class BoxTypeDivertLaneMappingController : ControllerBase
    {
        private readonly IBoxTypeDivertLaneAppService _boxTypeDivertLaneAppService;
        private readonly ILogger<BoxTypeDivertLaneMappingController> _logger;

        public BoxTypeDivertLaneMappingController(IBoxTypeDivertLaneAppService boxTypeDivertLaneAppService, ILogger<BoxTypeDivertLaneMappingController> logger)
        {
            _boxTypeDivertLaneAppService = boxTypeDivertLaneAppService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<BoxTypeDivertLaneMapping>> GetAll()
        {
            try
            {
                List<BoxTypeDivertLaneMapping> result = await _boxTypeDivertLaneAppService.GetAllBoxTypeDivertLaneMappingsAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAll in BoxTypeDivertLaneMappingController unsuccessful. Error: {ex.Message}");
                throw new Exception("GetAll in BoxTypeDivertLaneMappingController unsuccessful");
            }
        }

        [HttpGet("MenuView")]
        public async Task<IActionResult> GetAllMenuView()
        {
            try
            {
                List<BoxTypeDivertLaneMenuView> result = await _boxTypeDivertLaneAppService.GetAllBoxTypeDivertLaneViewAsync();
                if (result == null)
                {
                    _logger.LogWarning("There is a logical conflict in the mapping in one or more records in BoxTypeDivertLaneMappingController method GetAllMenuView ");
                    return BadRequest(new { Message = "There is a logical conflict in the mapping in one or more records." });
                }
                return Ok(result);
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAll in BoxTypeDivertLaneMenuViewController unsuccessful. Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }


        [HttpGet("{id}")]
        public async Task<BoxTypeDivertLaneMapping> GetById(int id)
        {
            try
            {
                BoxTypeDivertLaneMapping result = await _boxTypeDivertLaneAppService.GetBoxTypeDivertLaneMappingAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById in BoxTypeDivertLaneMappingController unsuccessful. Error: {ex.Message}");
                throw new Exception("GetById in BoxTypeDivertLaneMappingController unsuccessful");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(BoxTypeDivertLaneMappingAddDto entity)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _boxTypeDivertLaneAppService.AddBoxTypeDivertLaneMappingAsync(entity);

                    if(result != null)
                    {
                        return Ok(new { Message = "BoxType Divertlane Mapping added successfully" });
                    }
                    _logger.LogWarning("Post in BoxTypeDivertLaneMappingController unsuccessful");
                    return BadRequest("Post in BoxTypeDivertLaneMappingController unsuccessful");
                }
                _logger.LogWarning("Post in BoxTypeDivertLaneMappingController unsuccessful due to invalid model state");
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post in BoxTypeDivertLaneMappingController unsuccessful. Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });

            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, BoxTypeDivertLaneMappingAddDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _boxTypeDivertLaneAppService.EditBoxTypeDivertLaneMappingAsync(id, model);
                    if( result != null )
                    {
                        return Ok(result);
                    }
                    _logger.LogWarning("Result is null in BoxTypeDivertLaneMappingController method Put");
                    return BadRequest("Edit mapping unsuccessful");
                }
                _logger.LogWarning("Put in BoxTypeDivertLaneMappingController unsuccessful due to invalid model state");
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Put in BoxTypeDivertLaneMappingController unsuccessful. Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _boxTypeDivertLaneAppService.DeleteBoxTypeDivertLaneMappingAsync(id);

                if (result)
                {
                    return Ok(new { Message = "BoxType Divertlane Mapping Deleted" });
                }
                _logger.LogWarning("Boxtype es false in BoxTypeDivertLaneMappingController method Delete");
                return BadRequest("Delete in BoxTypeDivertLaneMappingController unsuccessful");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete in BoxTypeDivertLaneMappingController unsuccessful. Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
