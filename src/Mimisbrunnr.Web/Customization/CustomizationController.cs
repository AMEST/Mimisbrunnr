using System.Text;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Customization;

/// <summary>
/// Controller for managing UI customization
/// </summary>
[ApiController]
[Route("api/[controller]")]
[HandleCustomizationErrors]
public class CustomizationController: ControllerBase
{
    private readonly ICustomizationService _customizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomizationController"/> class
    /// </summary>
    /// <param name="customizationService">Service for managing UI customization</param>
    public CustomizationController(ICustomizationService customizationService)
    {
        _customizationService = customizationService;
    }

    /// <summary>
    /// Get custom CSS for the application
    /// </summary>
    /// <returns>Custom CSS file content</returns>
    [HttpGet("css")]
    public async Task<IActionResult> GetCustomCss()
    {
        var customCss = await _customizationService.GetCustomCss();
        return new FileContentResult(Encoding.UTF8.GetBytes(customCss ?? string.Empty), "text/css");
    }

    /// <summary>
    /// Get custom homepage configuration
    /// </summary>
    /// <returns>Custom homepage settings</returns>
    [HttpGet("homepage")]
    public async Task<IActionResult> GetCustomHomepage()
    {
        var homepage = await _customizationService.GetCustomHomepage(User?.ToInfo());
        return Ok(homepage);
    }
}
