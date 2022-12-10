using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

public class SpaceUpdateModel
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    public bool? Public { get; set; }

    public string AvatarUrl { get; set; }
}