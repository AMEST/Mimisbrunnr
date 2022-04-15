using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Users;

public class Group : IHasId<string>
{
    public string Id { get; set; }
    
    public string Name { get; set; }

    public string Description { get; set; }
    
    public string[] OwnerEmails { get; set; }
}