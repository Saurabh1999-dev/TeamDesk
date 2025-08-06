using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TeamDesk.DTOs;
using TeamDesk.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class CaptchaController : ControllerBase
{
    private readonly ICaptchaService _captchaService;
    private readonly ILogger<CaptchaController> _logger;

    public CaptchaController(ICaptchaService captchaService, ILogger<CaptchaController> logger)
    {
        _captchaService = captchaService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a CAPTCHA image and stores the code in the session.
    /// </summary>
    /// <returns>PNG image of the CAPTCHA.</returns>
    [HttpGet("generate")]
    public IActionResult Generate()
    {
        try
        {
            var code = _captchaService.GenerateCaptchaCode();
            HttpContext.Session.SetString("CaptchaCode", code);
            var imageBytes = _captchaService.GenerateCaptchaImage(code);

            _logger.LogInformation("CAPTCHA generated and stored in session.");
            return File(imageBytes, "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CAPTCHA");
            return StatusCode(500, new { success = false, message = "Error generating CAPTCHA" });
        }
    }

    /// <summary>
    /// Validates a user-submitted CAPTCHA code against the one stored in session.
    /// </summary>
    /// <param name="request">CAPTCHA input provided by the user.</param>
    /// <returns>Validation result (success/failure).</returns>
    [HttpPost("validate")]
    public IActionResult Validate([FromBody] CaptchaValidationRequest request)
    {
        try
        {
            var storedCode = HttpContext.Session.GetString("CaptchaCode");
            if (_captchaService.ValidateCaptcha(request.CaptchaInput, storedCode))
            {
                _logger.LogInformation("CAPTCHA validated successfully.");
                return Ok(new { success = true });
            }

            _logger.LogWarning("Invalid CAPTCHA entered.");
            return BadRequest(new { success = false, message = "Invalid CAPTCHA" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CAPTCHA");
            return StatusCode(500, new { success = false, message = "Error validating CAPTCHA" });
        }
    }

}
