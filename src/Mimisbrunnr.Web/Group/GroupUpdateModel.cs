using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Group;

public class GroupUpdateModel
{
    [Required]
    public string Name { get; set; }

    public string Description { get; set; }
}