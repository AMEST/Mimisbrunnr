namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Tree structure model for wiki pages
/// </summary>
public class PageTreeModel
{
    /// <summary>
    /// Page model
    /// </summary>
    public PageModel Page { get; set; }

    /// <summary>
    /// Child pages in the tree structure
    /// </summary>
    public IEnumerable<PageTreeModel> Childs { get; set; }
}
