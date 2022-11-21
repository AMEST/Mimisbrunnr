namespace Mimisbrunnr.Integration.Client;

public class MimisbrunnrClientException : Exception
{
    public MimisbrunnrClientException(string message = null)
        : base(message)
    {
    }
}