using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Mimisbrunnr.Integration.Plugin;

namespace Mimisbrunnr.Web.Tests.Plugin;

public class MacroModelTests
{
    [Fact]
    public void Should_Throw_ValidationError_WhenParamDefinitionTypeIsUnsupported()
    {
        var model = CreateValidMacro();
        model.ParamDefinitions = [new MacroParamDefinitionModel { Name = "name", Type = "bogus" }];

        Validate(model).Should().Contain(x => x.ErrorMessage.Contains("Unsupported param types"));
    }

    [Fact]
    public void Should_Throw_ValidationError_WhenParamDefinitionNamesAreDuplicated()
    {
        var model = CreateValidMacro();
        model.ParamDefinitions =
        [
            new MacroParamDefinitionModel { Name = "name" },
            new MacroParamDefinitionModel { Name = "name" }
        ];

        Validate(model).Should().Contain(x => x.ErrorMessage.Contains("must be unique"));
    }

    [Fact]
    public void Should_Throw_ValidationError_WhenSelectParamHasNoOptions()
    {
        var model = CreateValidMacro();
        model.ParamDefinitions = [new MacroParamDefinitionModel { Name = "mode", Type = "select" }];

        Validate(model).Should().Contain(x => x.ErrorMessage.Contains("must define options"));
    }

    [Fact]
    public void Should_NotThrow_ValidationError_WhenParamDefinitionsAreValid()
    {
        var model = CreateValidMacro();
        model.ParamDefinitions =
        [
            new MacroParamDefinitionModel { Name = "name", Type = "text", Required = true },
            new MacroParamDefinitionModel { Name = "mode", Type = "select", Options = ["view", "edit"] }
        ];

        Validate(model).Should().BeEmpty();
    }

    private static MacroModel CreateValidMacro()
        => new()
        {
            MacroIdentifier = "mm.test.macro",
            RenderUrl = "https://example.com/render"
        };

    private static ValidationResult[] Validate(MacroModel model)
        => model.Validate(new ValidationContext(model)).ToArray();
}
