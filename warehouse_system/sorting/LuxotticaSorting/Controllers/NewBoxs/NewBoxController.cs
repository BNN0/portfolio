using LuxotticaSorting.ApplicationServices.MappingSorter;
using LuxotticaSorting.ApplicationServices.NewBoxs;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters;
using LuxotticaSorting.ApplicationServices.Shared.DTO.NewBoxs;
using LuxotticaSorting.Controllers.MappingSorters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.NewBoxs
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class NewBoxController : ControllerBase
    {
        private readonly INewBoxAppService _newBoxAppService;
        private readonly ILogger<NewBoxController> _logger;
        public NewBoxController(INewBoxAppService newBoxAppService, ILogger<NewBoxController> logger)
        {
            _newBoxAppService = newBoxAppService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(NewBoxAddDto entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Invalid JSON Model in NewBoxController method Post");
                    return BadRequest("Invalid JSON Model!.");
                }

                await _newBoxAppService.AddNewBoxAsync(entity);
                var response = new { Message = "Assignment created successfully" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In NewBoxController method Post failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
        }
    }
}
