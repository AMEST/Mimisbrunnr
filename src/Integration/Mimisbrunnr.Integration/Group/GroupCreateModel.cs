using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Group;

/// <summary>
/// Represents the data required to create a new group in the Mimisbrunnr system.
/// This model is used during group creation processes.
/// </summary>
public class GroupCreateModel
{
    [Required]
    /// <summary>
    /// The unique name for the new group.
    /// This is required and must be unique across the system.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A brief description explaining the group's purpose or function.
    /// This is optional but recommended for better group management.
    /// </summary>
    public string Description { get; set; }
}
