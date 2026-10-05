namespace Mimisbrunnr.Integration.Wiki;

public class PageVersionNotFoundException : Exception
{
    public PageVersionNotFoundException(string message = "Page version not found")
        : base(message)
    {
    }
}
