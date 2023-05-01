namespace Mimisbrunnr.Integration.Wiki;

public class CommentNotFoundException : Exception
{
    public CommentNotFoundException(string message = "Comment not found")
        : base(message)
    {
    }
}