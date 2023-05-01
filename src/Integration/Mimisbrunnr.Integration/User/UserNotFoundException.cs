namespace Mimisbrunnr.Integration.User;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string message = "User not found")
        : base(message)
    {
    }
}