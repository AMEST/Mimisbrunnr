namespace Mimisbrunnr.Web.Wiki;

public class PageTreeModel
{
    public PageModel Page { get; set; }
    
    public IEnumerable<PageTreeModel> Childs { get; set; }
}