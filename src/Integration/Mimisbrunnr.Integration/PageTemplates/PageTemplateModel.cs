using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.PageTemplates;

public class PageTemplateModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string Type { get; set; }
    public string OwnerEmail { get; set; }
    public string SpaceId { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public UserModel CreatedBy { get; set; }
    public UserModel UpdatedBy { get; set; }
}
