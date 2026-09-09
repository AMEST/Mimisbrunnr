using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;
using Skidbladnir.Storage.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class AttachmentManagerTests
{
    public AttachmentManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task ShouldThrow_ArgumentNullException_WhenAttachmentNameIsEmpty()
    {
        var manager = new AttachmentManager(A.Fake<IRepository<Attachment>>(), A.Fake<IStorage>());

        await manager.Invoking(x => x.Upload(new Page { Id = "page" }, new MemoryStream(), "", new UserInfo()))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ShouldThrow_ArgumentNullException_WhenAttachmentContentIsNull()
    {
        var manager = new AttachmentManager(A.Fake<IRepository<Attachment>>(), A.Fake<IStorage>());

        await manager.Invoking(x => x.Upload(new Page { Id = "page" }, null, "file.txt", new UserInfo()))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ShouldReturnNull_WhenAttachmentDoesNotExist()
    {
        var repository = A.Fake<IRepository<Attachment>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Attachment>().AsQueryable());
        var storage = A.Fake<IStorage>();
        var manager = new AttachmentManager(repository, storage);

        var result = await manager.GetAttachmentContent(new Page { Id = "page" }, "missing.txt");

        result.Should().BeNull();
        A.CallTo(() => storage.DownloadFileAsync(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ShouldNotDelete_WhenAttachmentDoesNotExist()
    {
        var repository = A.Fake<IRepository<Attachment>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Attachment>().AsQueryable());
        var storage = A.Fake<IStorage>();
        var manager = new AttachmentManager(repository, storage);

        await manager.Remove(new Page { Id = "page" }, "missing.txt");

        A.CallTo(() => storage.DeleteAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => repository.Delete(A<Attachment>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnAttachmentContent_WhenDownloadingExistingAttachment()
    {
        var repository = A.Fake<IRepository<Attachment>>();
        var attachment = new Attachment { Id = "a1", PageId = "page", Name = "file.txt", Path = "attachments/x" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { attachment }.AsQueryable());
        var storage = A.Fake<IStorage>();
        var content = new MemoryStream();
        A.CallTo(() => storage.DownloadFileAsync("attachments/x")).Returns(new DownloadResult(null, content));
        var manager = new AttachmentManager(repository, storage);

        var result = await manager.GetAttachmentContent(new Page { Id = "page" }, "file.txt");

        result.Should().BeSameAs(content);
    }

    [Fact]
    public async Task Should_ReturnNull_WhenFileMissingInStorage()
    {
        var repository = A.Fake<IRepository<Attachment>>();
        var attachment = new Attachment { Id = "a1", PageId = "page", Name = "file.txt", Path = "attachments/x" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { attachment }.AsQueryable());
        var storage = A.Fake<IStorage>();
        A.CallTo(() => storage.DownloadFileAsync("attachments/x")).Returns((DownloadResult)null);
        var manager = new AttachmentManager(repository, storage);

        var result = await manager.GetAttachmentContent(new Page { Id = "page" }, "file.txt");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_DeleteFileAndAttachment_WhenRemovingByName()
    {
        var repository = A.Fake<IRepository<Attachment>>();
        var attachment = new Attachment { Id = "a1", PageId = "page", Name = "file.txt", Path = "attachments/x" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { attachment }.AsQueryable());
        var storage = A.Fake<IStorage>();
        var manager = new AttachmentManager(repository, storage);

        await manager.Remove(new Page { Id = "page" }, "file.txt");

        A.CallTo(() => storage.DeleteAsync("attachments/x")).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Delete(attachment, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteFilesAndAttachments_WhenRemovingAll()
    {
        var repository = A.Fake<IRepository<Attachment>>();
        var attachment1 = new Attachment { Id = "a1", PageId = "page", Name = "1.txt", Path = "attachments/1" };
        var attachment2 = new Attachment { Id = "a2", PageId = "page", Name = "2.txt", Path = "attachments/2" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { attachment1, attachment2 }.AsQueryable());
        var storage = A.Fake<IStorage>();
        var manager = new AttachmentManager(repository, storage);

        await manager.RemoveAll(new Page { Id = "page" });

        A.CallTo(() => storage.DeleteAsync("attachments/1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => storage.DeleteAsync("attachments/2")).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Delete(attachment1, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Delete(attachment2, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_UploadFile_WhenNoAttachmentExists()
    {
        var repository = A.Fake<IRepository<Attachment>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Attachment>().AsQueryable());
        var storage = A.Fake<IStorage>();
        var content = new MemoryStream();
        var manager = new AttachmentManager(repository, storage);

        await manager.Upload(new Page { Id = "page" }, content, "file.txt", new UserInfo { Email = "user@example.test" });

        A.CallTo(() => storage.UploadFileAsync(content, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Create(A<Attachment>.That.Matches(x => x.PageId == "page" && x.Name == "file.txt"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReplaceExistingAttachment_WhenUploadingSameName()
    {
        var repository = A.Fake<IRepository<Attachment>>();
        var existing = new Attachment { Id = "a1", PageId = "page", Name = "file.txt", Path = "attachments/old" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { existing }.AsQueryable());
        var storage = A.Fake<IStorage>();
        var content = new MemoryStream();
        var manager = new AttachmentManager(repository, storage);

        await manager.Upload(new Page { Id = "page" }, content, "file.txt", new UserInfo { Email = "user@example.test" });

        A.CallTo(() => storage.DeleteAsync("attachments/old")).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Delete(existing, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => storage.UploadFileAsync(content, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Create(A<Attachment>.That.Matches(x => x.PageId == "page" && x.Name == "file.txt"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
