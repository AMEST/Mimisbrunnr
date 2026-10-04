/// <summary>
/// Exception thrown when a requested plugin cannot be found
/// </summary>
public class PluginNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginNotFoundException"/> class
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public PluginNotFoundException(string message = "Plugin not found")
        : base(message)
    {
    }
}