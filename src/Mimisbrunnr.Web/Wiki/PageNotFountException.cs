namespace Mimisbrunnr.Web.Wiki;

public class PageNotFountException : Exception
{
    public PageNotFountException(string message = null)
        : base(message)
    {
        
    }
}