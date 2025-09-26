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
    }
}
