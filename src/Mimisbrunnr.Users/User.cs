using System.Text.Json.Serialization;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Users;

public class User : IHasId<string>
{
    public User()
    {

    }

    [JsonConstructor]
    public User(string id, bool enable, string email, string name, string avatarUrl, UserRole role)
    {
        Id = id;
        Enable = enable;
        Name = name;
        Email = email;
        AvatarUrl = avatarUrl;
        Role = role;
    }
    public string Id { get; set; }

    public bool Enable { get; internal set; } = true;

    public string Email { get; internal set; }

    public string Name { get; set; }

    public string AvatarUrl { get; set; }

    public UserRole Role { get; internal set; }
}