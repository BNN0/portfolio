using Luxottica.ApplicationServices.SecondLevelCameraAssignments;
using Luxottica.ApplicationServices.Shared.Dto.SecondLevelCamera;
using Luxottica.Controllers.PhysicalMaps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Luxottica.Controllers.SeconLevelCameraAssignment
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SecondLevelCameraController : ControllerBase
    {
        private readonly ISecondLevelCameraAppService _appService;
        private readonly ILogger<SecondLevelCameraController> _logger;
        public SecondLevelCameraController(ISecondLevelCameraAppService appService, ILogger<SecondLevelCameraController> logger)
        {
            _appService = appService;
            _logger = logger;
        }

        // GET api/<SecondLevelCameraController>/5
        [HttpGet]
        public async Task<SecondLevelCameraGetDto> Get()
        {
            try
            {
                var cameraAssignment = await _appService.GetSecondLevelCameraAsync();
                return cameraAssignment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT SecondLevelCamera IN MapPhysicalVirualSAPController {ex.Message}");
                throw new Exception("Get in SecondLevelCameraController unsuccessful");
            }
        }

        [HttpGet("GetCameraAssignmentInfo")]
        public async Task<SecondLevelCameraBundleDto> GetCameraAssignmentInfo()
        {
            try
            {
                var result = await _appService.GetCameraAssignmentInfo();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetCameraAssignmentInfo IN MapPhysicalVirualSAPController {ex.Message}");
                throw new Exception("GetCameraAssignmentInfo in SecondLevelCameraController unsuccessful");
            }
        }

        // DELETE api/<SecondLevelCameraController>/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            try
            {
                await _appService.DeleteSecondLevelCamerasAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE SecondLevelCamera Where Id = {id} IN MapPhysicalVirualSAPController {ex.Message}");
                throw new Exception("Delete in SecondLevelCameraController unsuccessful");
            }
        }

        [HttpPut("ChangeAssignment/{id}")]
        public async Task<IActionResult> ChangeCameraAssignment(int id)
        {
            try
            {
                var result = await _appService.ChangeSecondLevelCamera(id);
                if (result)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest($"{result}: The camera assignment cannot be changed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR ChangeAssignment with Id = {id} IN MapPhysicalVirualSAPController {ex.Message}");
                throw new Exception("ChangeCameraAssignment in SecondLevelCameraController unsuccessful");
            }
        }
    }
}
