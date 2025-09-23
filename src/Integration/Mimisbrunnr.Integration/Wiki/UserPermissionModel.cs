namespace Mimisbrunnr.Integration.Wiki;

public class UserPermissionModel
{
    /// <summary>
    ///     Is user admin of space
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    ///     Can user view pages
    /// </summary>
    public bool CanView { get; set; }

    /// <summary>
    ///     Can user edit pages
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    ///     Can user remove pages
    /// </summary>
    public bool CanRemove { get; set; }
}