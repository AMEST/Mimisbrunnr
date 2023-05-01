namespace Mimisbrunnr.Web.Infrastructure;

public class UserHasNotPermissionException : Exception
{
    public UserHasNotPermissionException(string message = "User has no permissions for this action")
        : base(message)
    {
    }
}