namespace Mimisbrunnr.Web.Wiki;

public class SpaceModel
{
    public string Key { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string HomePageId { get; set; }
    
    public SpaceTypeModel Type { get; set; }
}