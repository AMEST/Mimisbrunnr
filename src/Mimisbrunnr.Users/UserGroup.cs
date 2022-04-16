using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Users;

public class UserGroup : IHasId<string>
{
    public string Id { get; set; }
    
    public string UserId { get; set; }
    
    public string GroupId { get; set; }
}