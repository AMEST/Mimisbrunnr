using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Quickstart;

namespace Mimisbrunnr.Web.Tests.Quickstart;

public class QuickstartControllerTests
{
    [Fact]
    public async Task Should_ReturnInitializationState_WhenStatusIsRequested()
    {
        var service = A.Fake<IQuickstartService>();
        A.CallTo(() => service.IsInitialized()).Returns(Task.FromResult(true));

        var result = await new QuickstartController(service).GetInitializeStatus();

        result.Should().BeOfType<OkObjectResult>();
        result.As<OkObjectResult>().Value.As<InitializeState>().IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenInitializationFails()
    {
        var service = A.Fake<IQuickstartService>();
        A.CallTo(() => service.Initialize(A<QuickstartModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .ThrowsAsync(new InitializeException("already initialized"));

        var result = await new QuickstartController(service).Initialize(new QuickstartModel());

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(400);
    }
}
