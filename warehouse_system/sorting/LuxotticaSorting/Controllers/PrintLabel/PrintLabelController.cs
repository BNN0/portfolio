using LuxotticaSorting.ApplicationServices.Containers;
using LuxotticaSorting.ApplicationServices.PrintLabel;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.Controllers.Containers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace LuxotticaSorting.Controllers.PrintLabel
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PrintLabelController : ControllerBase
    {
        private readonly IPrintLabelAppService _printLabelAppService;
        private readonly ILogger<PrintLabelController> _logger;
        public PrintLabelController(IPrintLabelAppService printLabelAppService, ILogger <PrintLabelController> logger)
        {
            _printLabelAppService = printLabelAppService;
            _logger = logger;

        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<ZebraConfigurationDTO> conf = await _printLabelAppService.GetZebraConfigurationsAsync();
                return Ok(conf);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In PrintLabelController method GetAll failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                ZebraConfigurationDTO confDTO = await _printLabelAppService.GetZebraConfigurationByIdAsync(id);
                if (confDTO == null)
                {
                    _logger.LogWarning($"ZebraConfiguration with ID {id} not found, in PrintLabelController method GetById");
                    return NotFound($"ZebraConfiguration with ID {id} not found.");
                }
                return Ok(confDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In PrintLabelController method GetById failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }



        [HttpPost("NewZebraConfiguration")]
        public async Task<IActionResult> Post2(ZebraConfigurationAddDTO entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Invalid JSON Model in PrintLabelController method Post2.");
                    return BadRequest("Invalid JSON Model!.");
                }

                await _printLabelAppService.AddZebraConfigurationAsync(entity);
                return Ok(new { Message = "ZebraConfiguration added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post ZebraConfiguration method failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ZebraConfigurationAddDTO entity)
        {
            try
            {
                if (id == null || entity == null)
                {
                    _logger.LogWarning("Invalid request: Invalid ID or model in PrintLabelController method Put.");
                    return BadRequest("Invalid JSON model!.");
                }
                await _printLabelAppService.EditZebraConfigurationAsync(id, entity);
                return Ok(new { Message = "ZebraConfiguration edited successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"In PrintLabelController method Put failed for ID {id}, error: {ex.Message}");
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
                    _logger.LogWarning($"Id is null in PrintLabelController method Delete");
                    return BadRequest("Id is null!.");
                }
                await _printLabelAppService.DeleteZebraConfigurationAsync(id);
                return Ok(new { Message = "ZebraConfiguration deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"In PrintLabelController method Delete failed for {id}, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(PrintLabelDTO printLabelDTO)
        {
            try
            {
                if (printLabelDTO == null)
                {
                    _logger.LogWarning("Invalid JSON Model in PrintLabelController method Post");
                    return BadRequest("Invalid JSON Model!.");
                }

                var labelPrinted = await _printLabelAppService.PrintLabelService(printLabelDTO);

                if (labelPrinted == true)
                {
                    return Ok(new { Message = "Label printed successfully." });
                }
                else
                {
                    _logger.LogWarning("The printer was unable to print the label due to a communication error in PrintLabelController method Post");
                    return BadRequest(new { Message = "The printer was unable to print the label due to a communication error." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"In PrintLabelController method Post failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost("PrintLabelManual")]
        public async Task<IActionResult> PrintLabelManual(PrintManualDTO printLabelDTO)
        {
            try
            {
                if (printLabelDTO == null)
                {
                    _logger.LogWarning("Invalid JSON Model in PrintLabelController method PrintLabelManual");
                    return BadRequest("Invalid JSON Model!.");
                }

                var labelPrinted = await _printLabelAppService.PrintLabelManualService(printLabelDTO);

                if (labelPrinted == true)
                {
                    return Ok(new { Message = "Label printed successfully." });
                }
                else
                {
                    _logger.LogWarning("The printer was unable to print the label due to a communication error in PrintLabelController method PrintLaberManual");
                    return BadRequest(new { Message = "The printer was unable to print the label due to a communication error." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"In PrintLabelController method PrintLaberManual failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
        [HttpPost("PrintLabelReprint")]
        public async Task<IActionResult> RePrintLabel(PrintLabelReprint printLabelDTO)
        {
            try
            {
                if (printLabelDTO == null)
                {
                    _logger.LogWarning("Invalid JSON Model in PrintLabelController method RePrintLabel.");
                    return BadRequest("Invalid JSON Model!.");
                }

                var labelPrinted = await _printLabelAppService.PrintLabelReprint(printLabelDTO);

                if (labelPrinted == true)
                {
                    return Ok(new { Message = "Label printed successfully." });
                }
                else
                {
                    _logger.LogWarning("The printer was unable to print the label due to a communication error in PrintLabelController method RePrintLabel");
                    return BadRequest(new { Message = "The printer was unable to print the label due to a communication error." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"In PrintLabelController method RePrintLabel failed, error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
