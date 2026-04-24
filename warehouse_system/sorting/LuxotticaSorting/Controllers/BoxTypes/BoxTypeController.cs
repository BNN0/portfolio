using LuxotticaSorting.ApplicationServices.BoxTypes;
using LuxotticaSorting.ApplicationServices.CarriersCodes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.Controllers.CarrierCodes;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.BoxTypes
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BoxTypeController : ControllerBase
    {
        private readonly IBoxTypeAppService _boxTypeAppService;
        private readonly ILogger<BoxTypeController> _logger;
        public BoxTypeController(IBoxTypeAppService boxTypeAppService, ILogger<BoxTypeController> logger)
        {
            _boxTypeAppService = boxTypeAppService;
            _logger = logger;

        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<BoxType> boxTypes = await _boxTypeAppService.GetBoxTypesAsync();
                return Ok(boxTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAll BoxTypes method failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                BoxType boxType = await _boxTypeAppService.GetBoxTypeByIdAsync(id);
                if (boxType == null)
                {
                    return NotFound($"BoxType with ID {id} not found.");
                }
                return Ok(boxType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById BoxType method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPost]
        public async Task<IActionResult> Post(BoxTypesAddDTO entity)
        {
            try
            {
                if (entity == null)
                {
                    return BadRequest("Invalid JSON Model!.");
                }

                await _boxTypeAppService.AddBoxTypeAsync(entity);
                return Ok(new { Message = "BoxType added successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post BoxType method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, BoxTypesAddDTO entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    return BadRequest("Invalid JSON model ");
                }
                await _boxTypeAppService.EditBoxTypeAsync(id, entity);
                return Ok(new { Message = "BoxType edited successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeController method Put failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest(new { Success = false, Message = "Id is null!" });
                }
                await _boxTypeAppService.DeleteBoxTypeAsync(id);
                return Ok(new { Message = "BoxType deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeController method Delete failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new {Message = $"{ex.Message}" });
            }
        }

    }
}
