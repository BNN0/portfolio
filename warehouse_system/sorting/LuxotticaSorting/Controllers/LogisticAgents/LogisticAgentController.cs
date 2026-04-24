using LuxotticaSorting.ApplicationServices.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace LuxotticaSorting.Controllers.LogisticAgents
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LogisticAgentController : ControllerBase
    {
        private readonly ILogisticAgentAppService _logisticAgentService;
        private readonly ILogger<LogisticAgentController> _logger;
        public LogisticAgentController(ILogisticAgentAppService logisticAgentService, ILogger<LogisticAgentController> logger)
        {
            _logisticAgentService = logisticAgentService;
            _logger = logger;

        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List < LogisticAgentDto > logisticAgents = await _logisticAgentService.GetLogisticAgentsAsync();
                return Ok(logisticAgents);
            }catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentController GetAll method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                LogisticAgentDto logisticAgent = await _logisticAgentService.GetLogisticAgentByIdAsync(id);
                if(logisticAgent == null)
                {
                    _logger.LogWarning($"LogisticAgent with ID {id} not found");
                    return NotFound(new { Message = $"LogisticAgent with ID {id} not found." });
                }
                return Ok(logisticAgent);
            }
            catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentController GetById method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPost]
        public async Task<IActionResult> Post(LogisticAgentAddDto entity)
        {
            try
            {
                if ( entity == null || string.IsNullOrWhiteSpace(entity.LogisticAgents))
                { 
                    _logger.LogWarning("Invalid JSON Model in LogisticAgentController method Post");
                    return BadRequest(new { Message = "Invalid JSON Model!." });
                }

                await _logisticAgentService.AddLogisticAgentAsync(entity);
                return Ok(new { Message = "Successfully registered LogisticAgent" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentController Post method failed: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, LogisticAgentAddDto entity)
        {
            try
            {
                if( id == 0 || entity == null || string.IsNullOrWhiteSpace(entity.LogisticAgents))
                {
                    _logger.LogWarning("Invalid request: Invalid ID or model method Put LogisticAgentController");
                    return BadRequest(new { Message = "Invalid JSON model!." });
                }
                await _logisticAgentService.EditLogisticAgentAsync(id, entity);
                return Ok(new { Message = "Successfully updated LogisticAgent" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentController Put method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if ( id == 0)
                {
                    _logger.LogWarning($"Id is null in LogisticAgentController method Delete");
                    return BadRequest(new { Message = "Id is null!." });
                }
                await _logisticAgentService.DeleteLogisticAgentAsync(id);
                return Ok(new { Message = "LogisticAgent successfully removed" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentController Delete method failed for ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
