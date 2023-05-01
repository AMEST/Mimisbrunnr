namespace Mimisbrunnr.Integration.Group;

public class GroupNotFoundException : Exception
{
    public GroupNotFoundException(string message = "Group not found")
        : base(message)
    {
    
    }
}