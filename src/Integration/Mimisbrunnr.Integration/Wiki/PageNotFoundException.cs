namespace Mimisbrunnr.Integration.Wiki;

public class PageNotFoundException : Exception
{
    public PageNotFoundException(string message = "Page not found")
        : base(message)
    {
    }
}