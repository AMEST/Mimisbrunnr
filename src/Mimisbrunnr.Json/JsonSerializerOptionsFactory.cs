using System.Text.Json;
using System.Text.Json.Serialization;
using Mimisbrunnr.Integration.Favorites;

namespace Mimisbrunnr.Json;

public static class JsonSerializerOptionsFactory
{
    static JsonSerializerOptionsFactory()
    {
        Default = new JsonSerializerOptions();
        Default.ApplyDefaults();
    }

    public static JsonSerializerOptions Default;

    public static void ApplyDefaults(this JsonSerializerOptions opt)
    {
        opt.Converters.Add(new JsonStringEnumConverter());
        opt.Converters.Add(new AbstractClassConverter<FavoriteModel>());
        opt.Converters.Add(new AbstractClassConverter<FavoriteCreateModel>());
    }
}