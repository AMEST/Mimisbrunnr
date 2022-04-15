namespace Mimisbrunnr.Wiki.Contracts;

public class Permission
{
    public UserInfo User { get; set; }

    public GroupInfo Group { get; set; }

    public bool IsAdmin { get; set; }

    public bool CanView { get; set; }

    public bool CanEdit { get; set; }

    public bool CanRemove { get; set; }
}