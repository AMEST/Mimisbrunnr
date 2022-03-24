using Mimisbrunnr.Web.Authentication.Account;

namespace Mimisbrunnr.Web.Wiki;

public class SpacePermissionModel
{
    public UserModel User { get; set; }

    public GroupModel Group { get; set; }

    public bool IsAdmin { get; set; }

    public bool CanView { get; set; }

    public bool CanEdit { get; set; }

    public bool CanRemove { get; set; }
}