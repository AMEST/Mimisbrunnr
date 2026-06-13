using Xunit;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Execution;
using Mimisbrunnr.PageTemplates.Contracts;
using Mimisbrunnr.PageTemplates.Services;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.PageTemplates;

public class PageTemplateManagerTests
{
    private readonly IRepository<PageTemplate> _repository;
    private readonly PageTemplateManager _manager;

    public PageTemplateManagerTests()
    {
        QueryableAsyncExtensions.EnableFallback();
        _repository = A.Fake<IRepository<PageTemplate>>();
        _manager = new PageTemplateManager(_repository);
    }

    [Fact]
    public async Task Create_Should_Set_CreatedAndUpdated()
    {
        var template = new PageTemplate
        {
            Name = "Test",
            Content = "# Hello",
            Type = TemplateType.User,
            OwnerEmail = "user@test.com"
        };

        var result = await _manager.Create(template);

        using (new AssertionScope())
        {
            result.Created.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            result.Updated.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            result.CreatedBy.Email.Should().Be("user@test.com");
        }

        A.CallTo(() => _repository.Create(A<PageTemplate>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetById_Should_Return_Template_From_Repository()
    {
        var templates = new[]
        {
            new PageTemplate { Id = "123", Name = "Test" },
            new PageTemplate { Id = "456", Name = "Other" },
        }.AsQueryable();
        A.CallTo(() => _repository.GetAll()).Returns(templates);

        var result = await _manager.GetById("123");

        result.Should().NotBeNull();
        result.Id.Should().Be("123");
    }

    [Fact]
    public async Task GetById_Should_Throw_When_Not_Found()
    {
        A.CallTo(() => _repository.GetAll()).Returns(Enumerable.Empty<PageTemplate>().AsQueryable());

        await _manager.Invoking(m => m.GetById("missing"))
            .Should().ThrowAsync<PageTemplateNotFoundException>();
    }

    [Fact]
    public async Task Update_Should_Set_Updated_And_UpdatedBy()
    {
        var existing = new PageTemplate
        {
            Id = "1",
            Name = "Old",
            Content = "Old content",
            Updated = DateTime.UtcNow.AddDays(-1),
            UpdatedBy = new UserInfo { Email = "old@test.com" }
        };
        var templates = new[] { existing }.AsQueryable();
        A.CallTo(() => _repository.GetAll()).Returns(templates);

        var updateInfo = new UserInfo { Email = "new@test.com" };
        await _manager.Update("1", "New Name", "New content", updateInfo);

        using (new AssertionScope())
        {
            existing.Name.Should().Be("New Name");
            existing.Content.Should().Be("New content");
            existing.UpdatedBy.Email.Should().Be("new@test.com");
            existing.Updated.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        }

        A.CallTo(() => _repository.Update(existing, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Delete_Should_Remove_From_Repository()
    {
        var template = new PageTemplate { Id = "1" };
        var templates = new[] { template }.AsQueryable();
        A.CallTo(() => _repository.GetAll()).Returns(templates);

        await _manager.Delete("1");

        A.CallTo(() => _repository.Delete(template, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAll_Should_Return_From_Repository()
    {
        var templates = new[]
        {
            new PageTemplate { Name = "A" },
            new PageTemplate { Name = "B" }
        }.AsQueryable();

        A.CallTo(() => _repository.GetAll()).Returns(templates);

        var result = _manager.GetAll();

        result.Should().HaveCount(2);
    }
}
