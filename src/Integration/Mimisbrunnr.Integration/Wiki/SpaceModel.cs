using System.Text.Json.Serialization;

namespace Mimisbrunnr.Integration.Wiki;

public class SpaceModel
{
    public string Key { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string HomePageId { get; set; }

    public string AvatarUrl { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpaceTypeModel Type { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpaceStatusModel Status { get; set; }
}