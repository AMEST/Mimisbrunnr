/// <summary>
/// Exception thrown when a requested macro cannot be found
/// </summary>
public class MacroNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MacroNotFoundException"/> class
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public MacroNotFoundException(string message = "Macro not found")
        : base(message)
    {
    }
}