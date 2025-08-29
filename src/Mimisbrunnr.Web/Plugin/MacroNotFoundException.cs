public class MacroNotFoundException : Exception
{
    public MacroNotFoundException(string message = "Macro not found")
        : base(message)
    {
    }
}