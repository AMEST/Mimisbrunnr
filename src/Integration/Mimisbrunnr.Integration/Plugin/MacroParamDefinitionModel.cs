namespace Mimisbrunnr.Integration.Plugin;

/// <summary>
/// Describes how a macro parameter should be edited and validated.
/// </summary>
public class MacroParamDefinitionModel
{
    public string Name { get; set; }
    public string Type { get; set; } = "text";
    public string Label { get; set; }
    public string Help { get; set; }
    public int? Rows { get; set; }
    public string Pattern { get; set; }
    public bool Required { get; set; }
    public bool Advanced { get; set; }
    public string Group { get; set; }
    public string[] Options { get; set; } = [];
    public string DefaultValue { get; set; }
}
