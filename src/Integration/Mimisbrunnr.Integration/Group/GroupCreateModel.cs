using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Group;

public class GroupCreateModel
{
    [Required]
    public string Name { get; set; }

    public string Description { get; set; }
}