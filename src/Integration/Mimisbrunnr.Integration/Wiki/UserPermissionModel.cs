namespace Mimisbrunnr.Integration.Wiki;

public class UserPermissionModel
{
    public bool IsAdmin { get; set; }

    public bool CanView { get; set; }

    public bool CanEdit { get; set; }

    public bool CanRemove { get; set; }
}