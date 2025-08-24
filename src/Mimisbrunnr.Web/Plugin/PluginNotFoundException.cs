public class PluginNotFoundException : Exception
{
    public PluginNotFoundException(string message = "Plugin not found")
        : base(message)
    {
    }
}