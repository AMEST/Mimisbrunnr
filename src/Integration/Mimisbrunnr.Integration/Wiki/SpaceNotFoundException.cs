namespace Mimisbrunnr.Integration.Wiki;

public class SpaceNotFoundException : Exception
{
    public SpaceNotFoundException(string message = "Space not found")
        : base(message)
    {
    }
}