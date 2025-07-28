namespace Mimisbrunnr.Integration.Group;

/// <summary>
/// Represents the criteria for filtering and paginating groups in the Mimisbrunnr system.
/// This model is used when querying groups with specific conditions.
/// </summary>
public class GroupFilterModel
{
    /// <summary>
    /// Filters groups by the owner's email address.
    /// When specified, only groups owned by this user will be returned.
    /// </summary>
    public string OwnerEmail { get; set; }

    /// <summary>
    /// The number of items to skip before starting to return results.
    /// Used for pagination to retrieve subsequent pages of groups.
    /// </summary>
    public int? Offset { get; set; }
}
