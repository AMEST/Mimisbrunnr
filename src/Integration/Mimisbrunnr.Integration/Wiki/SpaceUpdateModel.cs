using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Model for updating wiki space properties
/// </summary>
public class SpaceUpdateModel
{
    /// <summary>
    /// New space name (required)
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// New space description (required)
    /// </summary>
    [Required]
    public string Description { get; set; }

    /// <summary>
    /// Flag indicating if space should be public
    /// </summary>
    public bool? Public { get; set; }

    /// <summary>
    /// URL of the space avatar
    /// </summary>
    public string AvatarUrl { get; set; }
}
