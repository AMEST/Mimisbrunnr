namespace Mimisbrunnr.Integration.Wiki;

public class PageNotFoundException : Exception
{
    public PageNotFoundException(string message = null)
        : base(message)
    {
        
    }
}