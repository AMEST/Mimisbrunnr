using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Users;

public class User : IHasId<string>
{
    public string Id { get; set; }
    
    public bool Enable { get; internal set; } = true;
    
    public string Email { get; internal set; }

    public string Name { get; set; }
    
    public string AvatarUrl { get; set; }
    
    public UserRole Role { get; internal set; }
}