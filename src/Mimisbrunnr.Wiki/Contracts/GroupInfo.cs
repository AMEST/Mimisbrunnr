namespace Mimisbrunnr.Wiki.Contracts;

/// <summary>
///     Group information contract
/// </summary>
public class GroupInfo
{
    /// <summary>
    ///     Group name
    /// </summary>
    public string Name { get; set; }   
    
    public override bool Equals(object obj)
    {
        return obj is GroupInfo groupInfo && Name.Equals(groupInfo.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name);
    }
}