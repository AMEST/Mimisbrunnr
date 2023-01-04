namespace Mimisbrunnr.Wiki.Contracts;


/// <summary>
///     User information contract
/// </summary>
public class UserInfo
{
    public string Email { get; set; }

    public string Name { get; set; }
    
    public string AvatarUrl { get; set; }

    public override bool Equals(object obj)
    {
        return obj is UserInfo userInfo && Email.Equals(userInfo.Email, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Email, Name, AvatarUrl);
    }
}