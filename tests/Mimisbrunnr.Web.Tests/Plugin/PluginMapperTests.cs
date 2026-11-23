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

    [Fact]
    public void Should_PreserveParamDefinitions_WhenMappingMacroPackageAndResponse()
    {
        var model = new PluginModel
        {
            PluginIdentifier = "plugin",
            Macros = [new MacroModel
            {
                MacroIdentifier = "mm.test.macro",
                RenderUrl = "https://example.com/render",
                SharedCode = "MimisbrunnrPluginApi.helpers = {};",
                ParamDefinitions =
                [
                    new MacroParamDefinitionModel
                    {
                        Name = "name",
                        Type = "text",
                        Label = "Name",
                        Required = true,
                        DefaultValue = "diagram"
                    },
                    new MacroParamDefinitionModel
                    {
                        Name = "mode",
                        Type = "select",
                        Options = ["view", "edit"]
                    }
                ]
            }]
        };
        var entity = model.ToEntity();
        entity.Macros.Should().ContainSingle();
        entity.Macros[0].ParamDefinitions.Should().HaveCount(2);
        entity.Macros[0].SharedCode.Should().Be(model.Macros[0].SharedCode);
        entity.ToModel().Macros[0].ParamDefinitions.Should().BeEquivalentTo(model.Macros[0].ParamDefinitions);
        entity.ToModel().Macros[0].SharedCode.Should().Be(model.Macros[0].SharedCode);
        var restored = BsonSerializer.Deserialize<Mimisbrunnr.Wiki.Contracts.Plugin>(entity.ToBson());
        restored.Macros[0].ParamDefinitions.Should().BeEquivalentTo(entity.Macros[0].ParamDefinitions);
        restored.Macros[0].SharedCode.Should().Be(entity.Macros[0].SharedCode);
    }

    [Fact]
    public void Should_DefaultToEmptyParamDefinitions_WhenReadingLegacyJsonAndMongoDocuments()
    {
        var model = JsonSerializer.Deserialize<MacroModel>("{\"MacroIdentifier\":\"legacy\"}");
        model.ParamDefinitions.Should().BeEmpty();
        model.SharedCode.Should().BeNull();
        var macro = BsonSerializer.Deserialize<Mimisbrunnr.Wiki.Contracts.Macro>(new BsonDocument("MacroIdentifier", "legacy"));
        macro.ParamDefinitions.Should().BeEmpty();
        macro.SharedCode.Should().BeNull();
    }
}
