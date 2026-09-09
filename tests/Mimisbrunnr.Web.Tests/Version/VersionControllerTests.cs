using FluentAssertions;
using Mimisbrunnr.Web.Version;
using Microsoft.AspNetCore.Mvc;

namespace Mimisbrunnr.Web.Tests.Version;

public class VersionControllerTests
{
    [Fact]
    public void Should_ReturnOkObject_WhenVersionIsRequested()
    {
        var result = new VersionController().Get();

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }
}
