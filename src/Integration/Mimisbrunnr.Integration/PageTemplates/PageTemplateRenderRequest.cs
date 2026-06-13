using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.PageTemplates;

public class PageTemplateRenderRequest
{
    [Required] public string TemplateId { get; set; }
    [Required] public string SpaceKey { get; set; }
}
