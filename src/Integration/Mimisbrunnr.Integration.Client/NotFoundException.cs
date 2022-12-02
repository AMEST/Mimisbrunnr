namespace Mimisbrunnr.Integration.Client;

public class NotFoundException : MimisbrunnrClientException
{
    public NotFoundException()
    {
    }


    public NotFoundException(string message)
        : base(message)
    {    
    }
}