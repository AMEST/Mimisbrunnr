namespace Mimisbrunnr.Integration.Group;

/// <summary>
/// Represents a group entity in the Mimisbrunnr system.
/// Groups are used to organize users and manage permissions collectively.
/// </summary>
public class GroupModel
{
    /// <summary>
    /// The unique name identifying the group.
    /// Used for display purposes and must be unique across the system.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A brief description explaining the group's purpose or function.
    /// Helps users understand the group's role in the system.
    /// </summary>
    public string Description { get; set; }
}
