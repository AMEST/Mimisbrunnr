using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Plugin;
/// <summary>
/// Represents a macro model used for defining and validating macros in the system.
/// </summary>
public class MacroModel : IValidatableObject
{
    /// <summary>
    /// Gets or sets the unique identifier for the macro.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string MacroIdentifier { get; set; }
    /// <summary>
    /// Gets or sets the name of the macro.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the description of the macro.
    /// </summary>
    [MaxLength(1024)]
    public string Description { get; set; }
    /// <summary>
    /// Icon link or data string (data:image/png;base64,....)
    /// </summary>
    public string Icon { get; set; }
    /// <summary>
    /// Gets or sets the parameters for the macro.
    /// </summary>
    public string[] Params { get; set; }
    /// <summary>
    /// Optional schema describing how macro params are edited.
    /// Keys match parameter names.
    /// </summary>
    public MacroParamDefinitionModel[] ParamDefinitions { get; set; } = [];
    /// <summary>
    /// Default values for params
    /// </summary>
    public IDictionary<string, string> DefaultValues { get; set; } = new Dictionary<string, string>();
    /// <summary>
    /// Gets or sets the URL for rendering the macro externally.
    /// </summary>
    public string RenderUrl { get; set; }
    /// <summary>
    /// Gets or sets the template for rendering the macro internally.
    /// </summary>
    public string Template { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to send the user token with the macro request.
    /// </summary>
    public bool SendUserToken { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to store macro parameters in the database.
    /// </summary>
    public bool StoreParamsInDatabase { get; set; }
    /// <summary>
    /// Custom params editor HTML
    /// </summary>
    public string CustomParamsEditor { get; set; }
    /// <summary>
    /// Shared JavaScript injected into both the custom params editor and the rendered macro.
    /// </summary>
    public string SharedCode { get; set; }

    /// <summary>
    /// Validates the macro model to ensure it is configured correctly.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if ((!string.IsNullOrWhiteSpace(RenderUrl) && !string.IsNullOrWhiteSpace(Template))
            || (string.IsNullOrWhiteSpace(RenderUrl) && string.IsNullOrWhiteSpace(Template)))
            yield return new ValidationResult("Only one field must be configured", [nameof(RenderUrl), nameof(Template)]);
        if (!string.IsNullOrWhiteSpace(RenderUrl) && !RenderUrl.ToLowerInvariant().StartsWith("https://") && !RenderUrl.ToLowerInvariant().StartsWith("http://"))
            yield return new ValidationResult("RenderUrl must be a url to external render api", [nameof(RenderUrl)]);
        char[] paramsSymbolsBlackList = ['\'', '"', '>', '<', '|', '=', '&'];
        if (Params is not null && Params.Any(x => string.IsNullOrWhiteSpace(x) || x.IndexOfAny(paramsSymbolsBlackList) >= 0))
            yield return new ValidationResult("Any params contains restricted symbols", [nameof(Params)]);
        if (ParamDefinitions is not null)
        {
            if (ParamDefinitions.Any(x => x is null || string.IsNullOrWhiteSpace(x.Name) || x.Name.IndexOfAny(paramsSymbolsBlackList) >= 0))
                yield return new ValidationResult("Any param definition name is empty or contains restricted symbols", [nameof(ParamDefinitions)]);
            if (ParamDefinitions.Where(x => x is not null).Select(x => x.Name).Distinct().Count() != ParamDefinitions.Count(x => x is not null))
                yield return new ValidationResult("Param definition names must be unique", [nameof(ParamDefinitions)]);
            string[] allowedTypes = ["text", "textarea", "code", "json", "number", "boolean", "select", "date", "attachment"];
            var invalidTypes = ParamDefinitions
                .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Type) && !allowedTypes.Contains(x.Type.ToLowerInvariant()))
                .Select(x => x.Type)
                .Distinct()
                .ToArray();
            if (invalidTypes.Length > 0)
                yield return new ValidationResult($"Unsupported param types: {string.Join(", ", invalidTypes)}", [nameof(ParamDefinitions)]);
            if (ParamDefinitions.Any(x => x is not null && x.Type?.ToLowerInvariant() == "select" && (x.Options is null || x.Options.Length == 0)))
                yield return new ValidationResult("Select params must define options", [nameof(ParamDefinitions)]);
        }
    }
}
