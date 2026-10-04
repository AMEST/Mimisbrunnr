/// <summary>
/// Exception thrown when a remote macro fails to render
/// </summary>
public class RemoteMacroRenderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteMacroRenderException"/> class
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public RemoteMacroRenderException(string message = "Error render remote macro") : base(message){}
}