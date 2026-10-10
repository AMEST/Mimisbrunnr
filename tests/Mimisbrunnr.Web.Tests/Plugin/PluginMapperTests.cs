using System.Text.Json;
using FluentAssertions;
using Mimisbrunnr.Integration.Plugin;
using Mimisbrunnr.Web.Mapping;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Mimisbrunnr.Web.Tests.Plugin;

public class PluginMapperTests
{
    [Fact]
    public void Should_PreservePageTemplates_WhenMappingPluginPackageAndResponse()
    {
        var model = new PluginModel { PluginIdentifier = "plugin", PageTemplates = [new PluginPageTemplateModel
        {
            TemplateIdentifier = "meeting", Name = "Meeting", Description = "Notes", Content = "# {{SpaceName}}"
        }] };
        var entity = model.ToEntity();
        entity.PageTemplates.Should().ContainSingle();
        entity.ToModel().PageTemplates.Should().BeEquivalentTo(model.PageTemplates);
        var restored = BsonSerializer.Deserialize<Mimisbrunnr.Wiki.Contracts.Plugin>(entity.ToBson());
        restored.PageTemplates.Should().BeEquivalentTo(entity.PageTemplates);
    }

    [Fact]
    public void Should_DefaultToEmptyPageTemplates_WhenReadingLegacyJsonAndMongoDocuments()
    {
        var model = JsonSerializer.Deserialize<PluginModel>("{\"PluginIdentifier\":\"legacy\"}");
        model.PageTemplates.Should().BeEmpty();
        model.ToEntity().PageTemplates.Should().BeEmpty();
        var plugin = BsonSerializer.Deserialize<Mimisbrunnr.Wiki.Contracts.Plugin>(new BsonDocument("PluginIdentifier", "legacy"));
        plugin.PageTemplates.Should().BeEmpty();
    }
}
