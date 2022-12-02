namespace Mimisbrunnr.Integration.Wiki;

public class PageTreeModel
{
    public PageModel Page { get; set; }
    
    public IEnumerable<PageTreeModel> Childs { get; set; }
}