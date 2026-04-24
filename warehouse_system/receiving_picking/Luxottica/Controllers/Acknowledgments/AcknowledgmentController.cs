using Luxottica.ApplicationServices.Acknowledgments;
using Luxottica.ApplicationServices.Shared.Dto.Acknowledgment;
using Luxottica.Controllers.Cameras;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.Core.Entities.ToteHdrs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Luxottica.Controllers.Acknowledgments
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AcknowledgmentController : ControllerBase
    {
        private readonly IAcknowledgmentAppService _acknowledgmentService;
        public AcknowledgmentController(IAcknowledgmentAppService acknowledgmentService)
        {
            _acknowledgmentService = acknowledgmentService;

        }

        //[Authorize(Roles = "API.ReadOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<Acknowledgment> acknowledgments = await _acknowledgmentService.GetAcknowledgmentsAsync();
                return Ok(acknowledgments);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while processing the request.",
                    Details = ex.Message
                };
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }


        //[Authorize(Roles = "API.ReadOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                Acknowledgment acknowledgment = await _acknowledgmentService.GetAcknowledgmentByIdAsync(id);

                if (acknowledgment == null)
                {
                    var errorResponse = new
                    {
                        Message = "Acknowledgment not found.",
                        Details = $"Acknowledgment with ID {id} does not exist."
                    };
                    return NotFound(errorResponse);
                }
                return Ok(acknowledgment);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while processing the request.",
                    Details = ex.Message
                };
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }


        //[Authorize(Roles = "API.ReadOnly")]
        [HttpPost]
        public async Task<IActionResult> Post(AcknowledgmentAddDTO entity)
        {
            try
            {

                if (entity.Status != "IN" && entity.Status != "NA")
                {
                    var errorResponse = new
                    {
                        Message = "Invalid status value.",
                        Details = "Status must be 'IN' or 'NA'."
                    };

                    return BadRequest(errorResponse);
                }

                if (string.IsNullOrEmpty(entity.ToteLpn))
                {
                    var errorResponse = new
                    {
                        Message = "Invalid ToteLPN value.",
                        Details = "CANNOT BE NULL"
                    };

                    return BadRequest(errorResponse);
                }
                var acknowledgment = await _acknowledgmentService.AddAcknowledgmentAsync(entity);
                return Ok(acknowledgment);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while processing the request.",
                    Details = ex.Message
                };
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }



        //[Authorize(Roles = "API.ReadOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, AcknowledgmentAddDTO model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.ToteLpn))
                {
                    var errorResponse = new
                    {
                        Message = "Invalid ToteLPN value.",
                        Details = "TOTELPN must have at least 10 characters."
                    };

                    return BadRequest(errorResponse);
                }

                if (model.Status != "IN" && model.Status != "NA")
                {
                    var errorResponse = new
                    {
                        Message = "Invalid status value.",
                        Details = "Status must be 'IN' or 'NA'."
                    };

                    return BadRequest(errorResponse);
                }
                await _acknowledgmentService.EditAcknowledgmentAsync(id, model);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                var errorResponse = new
                {
                    Message = "Acknowledgment not found.",
                    Details = ex.Message
                };
                return NotFound(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while processing the request.",
                    Details = ex.Message
                };
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
        //[Authorize(Roles = "API.ReadOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _acknowledgmentService.DeleteAcknowledgmentAsync(id);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                var errorResponse = new
                {
                    Message = "Acknowledgment not found.",
                    Details = ex.Message
                };
                return NotFound(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while processing the request.",
                    Details = ex.Message
                };
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }


        [HttpGet("/api/Acknowledgment/status/{status}")]
        public async Task<IActionResult> GetAllByStatus(string status)
        {
            try
            {
                List<Acknowledgment> acknowledgments = await _acknowledgmentService.GetAcknowledgmentsAsync();

                var acknowledgmentsWithStatus = acknowledgments
                    .Where(a => a.Status == status)
                    .ToList();

                if (acknowledgmentsWithStatus.Count > 0)
                {
                    return Ok(acknowledgmentsWithStatus);
                }
                else
                {
                    var errorResponse = new
                    {
                        Message = "No acknowledgments were found with the specified status.",
                        Status = status
                    };
                    return NotFound(errorResponse);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while processing the request.",
                    Details = ex.Message
                };
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpGet("/api/Acknowledgment/wave/{wave}")]
        public async Task<IActionResult> GetAllByWave(string wave)
        {
            try
            {
                List<Acknowledgment> acknowledgments = await _acknowledgmentService.GetAcknowledgmentsAsync();

                var acknowledgmentsWithWave = acknowledgments
                    .Where(a => a.WaveNr == wave)
                    .ToList();

                if (acknowledgmentsWithWave.Count > 0)
                {
                    return Ok(acknowledgmentsWithWave);
                }
                else
                {
                    var errorResponse = new
                    {
                        Message = "No acknowledgments were found with the specified wave.",
                        Wave = wave
                    };
                    return NotFound(errorResponse);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    Message = "An error occurred while processing the request.",
                    Details = ex.Message
                };
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        private bool IsNumeric(string value)
        {
            return value.All(char.IsDigit);
        }
        private static bool ValidateTimestampFormat(string timestamp)
        {
            string format = "yyyyMMddHHmmssfff";

            if (DateTime.TryParseExact(timestamp, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

