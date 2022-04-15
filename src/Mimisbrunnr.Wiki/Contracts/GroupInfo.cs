namespace Mimisbrunnr.Wiki.Contracts;

public class GroupInfo
{
    public string Name { get; set; }   
    
    public override bool Equals(object obj)
    {
        return obj is GroupInfo groupInfo && Name.Equals(groupInfo.Name, StringComparison.OrdinalIgnoreCase);
    }
}