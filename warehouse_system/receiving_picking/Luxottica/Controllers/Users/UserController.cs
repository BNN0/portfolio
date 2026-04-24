using Luxottica.ApplicationServices.Shared.Dto.Users;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.ApplicationServices.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Luxottica.Controllers.Users
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserAppService _userAppService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserAppService userAppService, ILogger<UserController> logger)
        {
            _userAppService = userAppService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<UserDto> users = await _userAppService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT ToteInformation IN UserController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                UserDto user = await _userAppService.GetUserAsync(id);
                if (user == null)
                {
                    _logger.LogError($"ERROR SELECT ToteInformation WHERE Id = {id} IN UserController, Message: User with Id: {id} not found!..");
                    return NotFound($"User with Id: {id} not found!..");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT ToteInformation WHERE Id = {id} IN UserController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Post(NewUserDto value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value.PhoneNumber) || string.IsNullOrWhiteSpace(value.Password) || string.IsNullOrWhiteSpace(value.Email)
                    || string.IsNullOrWhiteSpace(value.UserName) || value == null)
                {
                    _logger.LogError($"ERROR INSERT ToteInformation IN UserController, Message: The entity cannot be null, all field are required");
                    return BadRequest("The entity cannot be null, all field are required");
                }
                await _userAppService.AddUserAsync(value);
                return Ok(new { message = "User added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT ToteInformation IN UserController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, EditUserDto value)
        {
            try
            {
                await _userAppService.EditUserAsync(id, value);
                return Ok(new { message = "User edited successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE ToteInformation WHERE Id = {id} IN UserController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                string authorizationHeader = HttpContext.Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return BadRequest("Missing Authorization header");
                }

                int bearerIndex = authorizationHeader.IndexOf("Bearer", StringComparison.OrdinalIgnoreCase);

                if (bearerIndex == -1)
                {
                    return BadRequest("Invalid Authorization header");
                }

                string token = authorizationHeader.Substring(bearerIndex + "Bearer".Length).Trim();
                string email = await ExtractClaimFromTokenAsync(token, "email");
                List<UserDto> users = await _userAppService.GetUsersAsync();
                UserDto actualUser = users.FirstOrDefault(x => x.Email == email);
                if (actualUser.Id != id && actualUser != null)
                {
                    await _userAppService.DeleteUserAsync(id);
                    return Ok(new { message = "User deleted successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Cannot delete the same user with which the session has been initiated." });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                List<RolesNameDto> roles = await _userAppService.GetRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetRoles IN UserController {ex.Message}");
                return StatusCode(500, new { error = $"{ex.Message}" });
            }
        }

        private async Task<string> ExtractClaimFromTokenAsync(string token, string claimType)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken != null)
            {
                return await Task.FromResult(jsonToken.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value);
            }

            return null;
        }

    }
}
