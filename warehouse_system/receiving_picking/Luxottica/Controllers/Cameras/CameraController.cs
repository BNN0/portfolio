using Luxottica.ApplicationServices.Cameras;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.Core.Entities.Cameras;
using Luxottica.DataAccess.Repositories.Camera;
using Luxottica.DataAccess.Repositories.CameraAssignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Luxottica.Controllers.Cameras
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CameraController : ControllerBase
    {
        private readonly ICameraAppService _cameraService;
        private readonly ILogger<CameraController> _logger;

        public CameraController(ICameraAppService cameraService, ILogger<CameraController> logger)
        {
            _cameraService = cameraService;
            _logger = logger;
        }

        [HttpGet()]
        public async Task<IEnumerable<CameraDTO>> GetAll()
        {
            try
            {
                List<CameraDTO> cameras = await _cameraService.GetCamerasAsync();
                return cameras;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT CAMERA {ex.Message}");
                throw new Exception("GetAll in CameraController unsuccessful");
            }

        }

        [HttpGet("{id}")]
        public async Task<CameraDTO> GetById(int id)
        {
            try
            {
                CameraDTO camera = await _cameraService.GetCameraByIdAsync(id);
                return camera;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT WHERE Id = {id} CAMERA {ex.Message}");
                throw new Exception("GetById in CameraController unsuccessful");
            }

        }

        [HttpPost]
        public async Task<Int32> Post(CameraDTO entity)
        {
            try
            {
                var camera = await _cameraService.AddCameraAsync(entity);
                return camera;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT CAMERA {ex.Message}");
                throw new Exception("Post in CameraController unsuccessful");
            }

        }


        [HttpPost("newCamera")]
        public async Task<IActionResult> AddNewCamera()
        {
            try
            {

                await _cameraService.AddNewCamera();
                return Ok(new { message = "New camera added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT NEW CAMERA IN CONTROLLER {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }



        //[Authorize(Roles = "API.ReadOnly")]
        [HttpPut("{id}")]
        public async Task Put(int id, CameraDTO model)
        {
            try
            {
                model.Id = id;
                await _cameraService.EditCameraAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE CAMERA WHERE Id = {id} IN CONTROLLER MESSAGE {ex.Message}");
                throw new Exception("Put in CameraController unsuccessful");
            }

        }
        //[Authorize(Roles = "API.ReadOnly")]
        [HttpDelete("last")]
        public async Task<IActionResult> DeleteLastRecord()
        {
            try
            {
                await _cameraService.DeleteLastRecord();
                return Ok(new { message = "Last record deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE LAST RECORD IN CONTROLLER MESSAGE {ex.Message}");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {

                await _cameraService.DeleteCameraAsync(id);
                return Ok(new { message = "Camera deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE WHERE Id= {id} IN CONTROLLER MESSAGE {ex.Message}");
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
