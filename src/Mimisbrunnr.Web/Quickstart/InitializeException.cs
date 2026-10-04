namespace Mimisbrunnr.Web.Quickstart;

/// <summary>
/// Exception thrown when application initialization fails
/// </summary>
public class InitializeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InitializeException"/> class
    /// </summary>
    /// <param name="message">Message describing the initialization failure</param>
    public InitializeException(string message) : base(message)
    {

    }
}