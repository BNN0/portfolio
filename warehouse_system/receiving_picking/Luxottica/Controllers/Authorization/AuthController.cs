using Luxottica.ApplicationServices.Shared.Config;
using Luxottica.Auth;
using Luxottica.Models.Login;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Security.Claims;
using Luxottica.ApplicationServices.Commissioners;

namespace Luxottica.Controllers.Authorization
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IJwtIssuerOptions _jwtOptions;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOptions<JwtTokenValidationSettings> _jwtConfig;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IJwtIssuerOptions jwtOptions,
            RoleManager<IdentityRole> roleManager,
            IOptions<JwtTokenValidationSettings> jwtConfig,
            ILogger<AuthController>  logger
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtOptions = jwtOptions;
            _roleManager = roleManager;
            _jwtConfig = jwtConfig;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (model == null)
            {
                _logger.LogError($"ERROR LOGIN IN AuthController SERVICE, Message: Invalid client request");
                return BadRequest("Invalid client request");
            }
            var user = await _userManager.FindByEmailAsync(model.UserName);
            if (user == null)
            {
                _logger.LogError($"ERROR LOGIN IN AuthController SERVICE, Message: Unauthorized");
                return Unauthorized();
            }
            var resuslt = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (user == null || !(await _signInManager.PasswordSignInAsync(user, model.Password, false, false)).Succeeded)
                return Unauthorized();
            var tokenString = await CreateJwtTokenAsync(user);
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(new[] { new { Token = tokenString } }),
                ContentType = "application/json",
            };
            // var result = new ContentResult() { Content = tokenString, ContentType = "application/text" };
            return result;

        }

        private async Task<String> CreateJwtTokenAsync(IdentityUser user)
        {
            var claims = new List<Claim>(new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iss,_jwtOptions.Issuer),

                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,user.UserName),

                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email,user.Email),

                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti,await _jwtOptions.JtiGenerator()),

                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat,_jwtOptions.IssuedAt.ToUnixEpochDate().ToString().ToString() , ClaimValueTypes.String)
            });

            claims.AddRange(await _userManager.GetClaimsAsync(user));

            var roleNames = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaim = new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, _jwtOptions.Issuer);
                    claims.Add(roleClaim);
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    claims.AddRange(roleClaims);
                }
            }
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expires,
                signingCredentials: _jwtOptions.SigningCredentials
                );
            var result = new JwtSecurityTokenHandler().WriteToken(jwt);
            return result;
        }
    }
}
