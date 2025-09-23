namespace Mimisbrunnr.Integration.Group;

/// <summary>
/// Represents the data structure for updating group information.
/// This model is used when modifying existing group details in the Mimisbrunnr system.
/// </summary>
public class GroupUpdateModel
{
    /// <summary>
    /// The updated description explaining the group's purpose or function.
    /// This helps maintain accurate and relevant group information.
    /// </summary>
    public string Description { get; set; }
}
