using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.DataImport;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Host.Services;

namespace Mimisbrunnr.Web.Tests.Host;

public class ConfluenceDataImportServiceTests
{
    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenSpaceIsNull()
    {
        var service = new ConfluenceDataImportService(
            new FakeWikiService(),
            NullLogger<ConfluenceDataImportService>.Instance);

        await service.Invoking(x => x.ImportSpace(null, new MemoryStream()))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_InvalidDataException_WhenImportArchiveIsInvalid()
    {
        var service = new ConfluenceDataImportService(
            new FakeWikiService(),
            NullLogger<ConfluenceDataImportService>.Instance);
        var space = new SpaceModel { Key = "SPACE", HomePageId = "home" };

        await service.Invoking(x => x.ImportSpace(space, new MemoryStream([1, 2, 3])))
            .Should().ThrowAsync<InvalidDataException>();
    }

    private sealed class FakeWikiService : IWikiService
    {
        public Task<SpaceModel> GetSpaceByKey(string key) => throw new NotSupportedException();
        public Task<PageModel> GetPageById(string id) => throw new NotSupportedException();
        public Task<PageModel> CreatePage(PageCreateModel model) => throw new NotSupportedException();
        public Task UpdatePage(string pageId, PageUpdateModel model) => throw new NotSupportedException();
        public Task UploadAttachment(string pageId, Stream content, string name) => throw new NotSupportedException();
    }
}
