using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Wiki;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class SpaceControllerTests
{
    [Fact]
    public async Task Should_ReturnBadRequest_WhenImportModelIsMissing()
    {
        var controller = new SpaceController(A.Fake<ISpaceService>(), A.Fake<Mimisbrunnr.DataImport.IDataImportService>());
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        controller.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

        var result = await controller.Import();

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task Should_ForwardPagination_WhenSpacesAreRequested()
    {
        var service = A.Fake<ISpaceService>();
        A.CallTo(() => service.GetAll(null, 5, 2)).Returns(Task.FromResult<SpaceModel[]>([]));
        var controller = new SpaceController(service, A.Fake<Mimisbrunnr.DataImport.IDataImportService>());

        var result = await controller.GetAll(5, 2);

        result.Should().BeOfType<OkObjectResult>();
        A.CallTo(() => service.GetAll(null, 5, 2)).MustHaveHappenedOnceExactly();
    }
}
