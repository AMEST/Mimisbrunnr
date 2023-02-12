namespace Mimisbrunnr.Web.Infrastructure;

public class UserHasNotPermissionException : Exception
{
    public UserHasNotPermissionException()
    {
    }

    public UserHasNotPermissionException(string message)
        : base(message)
    {
    }
}