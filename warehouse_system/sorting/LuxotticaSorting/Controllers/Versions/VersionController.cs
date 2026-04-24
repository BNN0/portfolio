using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Luxottica.Controllers.Versions
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VersionController : ControllerBase
    {
        private readonly ILogger<VersionController> _logger;

        public VersionController(ILogger<VersionController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get()
        {
            try
            {
                string gitCommand = "git describe --tags --abbrev=0";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = $"/c {gitCommand}"
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    process.WaitForExit();

                    string latestTag = process.StandardOutput.ReadToEnd().Trim();
                    if (string.IsNullOrEmpty(latestTag))
                    {
                        _logger.LogError($"Error not version");
                        return BadRequest(new { Message = "Not version" });
                    }
                    return Ok(new { Message = $"Version {latestTag}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting Tags version in VersionController. Error: {ex.Message}");
                return StatusCode(500, new { Message = $"Internal Server Error: {ex.Message}" });
            }
        }

        //get para obtener todas las versiones de git

        [HttpGet("versions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                string gitCommand = "git tag -l";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = $"/c {gitCommand}"
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    process.WaitForExit();

                    string latestTags = process.StandardOutput.ReadToEnd().Trim();
                    if (string.IsNullOrEmpty(latestTags))
                    {
                        _logger.LogError($"Error not versions");
                        return BadRequest(new { Message = "No versions available" });
                    }
                    string formattedTags = string.Join(", ", latestTags.Split('\n', StringSplitOptions.RemoveEmptyEntries));
                    return Ok(new { Message = $"Versions: {formattedTags}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting Tags version in VersionController. Error: {ex.Message}");
                return StatusCode(500, new { Message = $"Internal Server Error: {ex.Message}" });
            }
        }
    }
}
