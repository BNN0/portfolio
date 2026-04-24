using Luxottica.ApplicationServices.CameraAssignments;
using Luxottica.ApplicationServices.DivertOutboundLines;
using Luxottica.ApplicationServices.Shared.Dto.CameraAssignments;
using Luxottica.Core.Entities.CameraAssignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Luxottica.Controllers.CameraAssignments
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CameraAssignmentController : ControllerBase
    {
        private readonly ICameraAssignmentService _cameraAssignmentService;
        private readonly ILogger<CameraAssignmentController> _logger;
        public CameraAssignmentController(ICameraAssignmentService cameraAssignmentService, ILogger<CameraAssignmentController> logger)
        {
            _cameraAssignmentService = cameraAssignmentService;
            _logger = logger;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<CameraAssignmentDto> cameraAssignment = await _cameraAssignmentService.GetCameraAssignmentAsync();
                return Ok(cameraAssignment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT CameraAssignment in GetAll Controller {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                CameraAssignmentDto cameraAssignment = await _cameraAssignmentService.GetCameraAssignmentByIdAsync(id);
                if (cameraAssignment == null)
                {
                    _logger.LogError($"ERROR SELECT CameraAssignment where Id = {id}, Message: CameraAssignment with ID {id} not found.");
                    return NotFound($"CameraAssignment with ID {id} not found.");
                }
                return Ok(cameraAssignment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT CameraAssignment where Id = {id} GetById Controller Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(CameraAssignmentAddDto entity)
        {
            try
            {
                if (entity == null || entity.CameraId == 0 || entity.DivertLineId == 0)
                {
                    _logger.LogError($"ERROR INSERT CameraAssignment in CameraAssignmentController, Message: The entity cannot be null and must have valid values for CameraId and DivertLineId");
                    return StatusCode(500, new { message = "The entity cannot be null and must have valid values for CameraId and DivertLineId" });
                }
                if (await _cameraAssignmentService.CameraAssignmentExistsAsync(entity.DivertLineId))
                {
                    _logger.LogError($"ERROR INSERT CameraAssignment in CameraAssignmentController, Message: The Divert line has already been assigned to a camera.");
                    return StatusCode(500, new { message = "The Divert line has already been assigned to a camera." });
                }

                await _cameraAssignmentService.AddCameraAssignmentAsync(entity);
                return Ok(new { message = "Divert Line assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT CameraAssignment Controller Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CameraAssignmentAddDto model)
        {
            try
            {
                if (model == null || model.CameraId == 0 || model.DivertLineId == 0)
                {
                    _logger.LogError($"ERROR UPDATE CameraAssignment where Id = {id} Controller ,Message: The entity cannot be null and must have valid values for CameraId and DivertLineId.");
                    return BadRequest("The entity cannot be null and must have valid values for CameraId and DivertLineId.");
                }
                if (await _cameraAssignmentService.CameraAssignmentExistsEditAsync(model.DivertLineId, id))
                {
                    _logger.LogError($"ERROR UPDATE CameraAssignment where Id = {id} Controller ,Message: The Divert line has already been assigned to a camera.");
                    return StatusCode(500, "The Divert line has already been assigned to a camera.");
                }

                await _cameraAssignmentService.EditCameraAssignmentAsync(id, model);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE CameraAssignment where Id = {id} Controller Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _cameraAssignmentService.DeleteCameraAssignmentAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Delete CameraAssignment where Id = {id} Controller Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }
        [HttpGet("GetAllExtra")]
        public async Task<IActionResult> GetAllExtra()
        {
            try
            {
                List<CameraAssignmentGetAllDto> result = await _cameraAssignmentService.GetAllExtraAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Select CameraAssignment Method GetAllExtra Controller Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }
    }
}
