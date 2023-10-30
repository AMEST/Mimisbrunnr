using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Wiki;

public class PageModel
{
    public string Id { get; set; }

    public long Version {get; set;}

    public string SpaceKey { get; set; }

    public string Name { get; set; }

    public string Content { get; set; }

    public string PlainTextContent { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
    
    public UserModel CreatedBy { get; set; }
    
    public UserModel UpdatedBy { get; set; }

    public PageEditorTypeModel EditorType { get; set; }
}