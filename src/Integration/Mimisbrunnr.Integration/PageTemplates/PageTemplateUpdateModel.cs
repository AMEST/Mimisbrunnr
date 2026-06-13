using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.PageTemplates;

public class PageTemplateUpdateModel
{
    [Required] public string Name { get; set; }
    public string Description { get; set; }
    [Required] public string Content { get; set; }
}
