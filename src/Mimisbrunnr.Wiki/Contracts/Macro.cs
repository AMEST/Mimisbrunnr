namespace Mimisbrunnr.Wiki.Contracts;

public class Macro
{
    public string MacroIdentifier { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string[] Params { get; set; }
    public string RenderUrl { get; set; }
    public string Template { get; set; }
    public bool SendUserToken { get; set; }
    public bool StoreParamsInDatabase { get; set; }
    public bool Disabled { get; internal set; }
}