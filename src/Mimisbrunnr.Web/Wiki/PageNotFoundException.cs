namespace Mimisbrunnr.Web.Wiki;

public class PageNotFoundException : Exception
{
    public PageNotFoundException(string message = null)
        : base(message)
    {
        
    }
}