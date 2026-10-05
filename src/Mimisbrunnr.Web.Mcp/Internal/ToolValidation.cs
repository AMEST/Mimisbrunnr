using System.ComponentModel.DataAnnotations;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Mcp.Internal;

/// <summary>
/// Validates models passed to tools. MCP calls bypass the MVC pipeline, so the
/// DataAnnotations validation performed there must be replicated explicitly.
/// </summary>
internal static class ToolValidation
{
    public static void EnsureValid(object model)
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true))
            return;

        var message = string.Join("; ", results
            .Where(r => r.ErrorMessage is not null)
            .Select(r => r.ErrorMessage));

        throw new McpException($"validation_failed: {message}");
    }

    public static void EnsureNotEmpty(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new McpException($"validation_failed: {parameterName} is required.");
    }
}
