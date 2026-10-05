using System.Text.Json;
using Mimisbrunnr.Json;

namespace Mimisbrunnr.Web.Mcp.Internal;

/// <summary>
/// Serializes tool results using the same JSON conventions as the REST API.
/// </summary>
internal static class ToolJson
{
    private static readonly JsonSerializerOptions Options = JsonSerializerOptionsFactory.Default;

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static string Ok() => "{\"ok\":true}";
}
