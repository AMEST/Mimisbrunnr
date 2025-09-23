using System.Text.Json.Serialization;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Wiki space model
/// </summary>
public class SpaceModel
{
    /// <summary>
    /// Unique space key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Space name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Space description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// ID of the home page
    /// </summary>
    public string HomePageId { get; set; }

    /// <summary>
    /// URL of the space avatar
    /// </summary>
    public string AvatarUrl { get; set; }

    /// <summary>
    /// Type of the space
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpaceTypeModel Type { get; set; }

    /// <summary>
    /// Current status of the space
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpaceStatusModel Status { get; set; }
}
