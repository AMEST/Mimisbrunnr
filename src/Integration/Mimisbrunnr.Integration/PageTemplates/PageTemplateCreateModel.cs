using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.PageTemplates;

public class PageTemplateCreateModel
{
    [Required] public string Name { get; set; }
    public string Description { get; set; }
    [Required] public string Content { get; set; }
    [Required] public string Type { get; set; }
    public string SpaceKey { get; set; }
}
