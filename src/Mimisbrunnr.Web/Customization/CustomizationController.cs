using System.Text;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Customization
{
    [ApiController]
    [Route("api/[controller]")]
    [HandleCustomizationErrors]
    public class CustomizationController: ControllerBase
    {
        private readonly ICustomizationService _customizationService;

        public CustomizationController(ICustomizationService customizationService)
        {
            _customizationService = customizationService;
        }

        [HttpGet("css")]
        public async Task<IActionResult> GetCustomCss()
        {
            var customCss = await _customizationService.GetCustomCss();
            return new FileContentResult(Encoding.UTF8.GetBytes(customCss ?? string.Empty), "text/css");
        }

        [HttpGet("homepage")]
        public async Task<IActionResult> GetCustomHomepage()
        {
            var homepage = await _customizationService.GetCustomHomepage(UserMapper.Instance.ToInfo(User));
            return Ok(homepage);
        }
    }
}