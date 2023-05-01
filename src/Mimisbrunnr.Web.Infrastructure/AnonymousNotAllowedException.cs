namespace Mimisbrunnr.Web.Infrastructure;

public class AnonymousNotAllowedException : Exception
{
    public AnonymousNotAllowedException(string message = "Anonymous access disabled. Authorize needed.")
        : base(message)
    {
    }
}