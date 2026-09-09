using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Users;
using Mimisbrunnr.Users.Services;
using Mimisbrunnr.Web.Search;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Search;

public class SearchServiceTests
{
    [Fact]
    public async Task Should_ReturnOnlyVisibleSpaces_WhenEmployeeSearchesSpaces()
    {
        var spaces = A.Fake<ISpaceSearcher>();
        var display = A.Fake<ISpaceDisplayService>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => spaces.Search("space")).Returns(Task.FromResult(new[]
        {
            new Space { Id = "visible", Key = "VISIBLE" },
            new Space { Id = "hidden", Key = "HIDDEN" }
        }));
        A.CallTo(() => display.FindUserVisibleSpaces(user, null, null)).Returns(Task.FromResult<IEnumerable<Space>>([new Space { Id = "visible", Key = "VISIBLE" }]));
        var users = A.Fake<IUserManager>();
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));
        var service = new SearchService(A.Fake<IPermissionService>(), A.Fake<IPageSearcher>(), spaces, A.Fake<IUserSearcher>(), users, display, A.Fake<ISpaceManager>());

        var result = await service.SearchSpaces("space", user);

        result.Should().ContainSingle().Which.Key.Should().Be("VISIBLE");
    }

    [Fact]
    public async Task Should_ReturnEmpty_WhenNoPagesMatch()
    {
        var pages = A.Fake<IPageSearcher>();
        var users = A.Fake<IUserManager>();
        var permissions = A.Fake<IPermissionService>();
        A.CallTo(() => pages.Search("query")).Returns(Task.FromResult(Array.Empty<Page>()));
        var service = new SearchService(permissions, pages, A.Fake<ISpaceSearcher>(), A.Fake<IUserSearcher>(), users, A.Fake<ISpaceDisplayService>(), A.Fake<ISpaceManager>());

        var result = await service.SearchPages("query", new UserInfo { Email = "user@example.test" });

        result.Should().BeEmpty();
        A.CallTo(() => permissions.EnsureAnonymousAllowed(A<UserInfo>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => users.GetByEmail(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnOnlyFoundSpacePages_WhenAdminSearchesPages()
    {
        var pages = A.Fake<IPageSearcher>();
        var users = A.Fake<IUserManager>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => pages.Search("query")).Returns(Task.FromResult(new[]
        {
            new Page { Id = "p1", SpaceId = "s1", Name = "Found" },
            new Page { Id = "p2", SpaceId = "missing-space", Name = "Orphan" }
        }));
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        A.CallTo(() => spaces.GetByIds(A<string[]>._)).Returns(Task.FromResult(new[] { new Space { Id = "s1", Key = "DOCS" } }));
        var service = new SearchService(A.Fake<IPermissionService>(), pages, A.Fake<ISpaceSearcher>(), A.Fake<IUserSearcher>(), users, A.Fake<ISpaceDisplayService>(), spaces);

        var result = await service.SearchPages("query", user);

        result.Should().ContainSingle().Which.Should().Match<Mimisbrunnr.Integration.Wiki.PageModel>(x => x.Id == "p1" && x.SpaceKey == "DOCS");
    }

    [Fact]
    public async Task Should_ReturnOnlyVisiblePages_WhenEmployeeSearchesPages()
    {
        var pages = A.Fake<IPageSearcher>();
        var users = A.Fake<IUserManager>();
        var display = A.Fake<ISpaceDisplayService>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => pages.Search("query")).Returns(Task.FromResult(new[]
        {
            new Page { Id = "pub", SpaceId = "s1", Name = "Visible" },
            new Page { Id = "hidden", SpaceId = "s2", Name = "Hidden" }
        }));
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));
        A.CallTo(() => display.FindUserVisibleSpaces(user, null, null)).Returns(Task.FromResult<IEnumerable<Space>>(new[] { new Space { Id = "s1", Key = "PUB" } }));
        var service = new SearchService(A.Fake<IPermissionService>(), pages, A.Fake<ISpaceSearcher>(), A.Fake<IUserSearcher>(), users, display, A.Fake<ISpaceManager>());

        var result = await service.SearchPages("query", user);

        result.Should().ContainSingle().Which.Should().Match<Mimisbrunnr.Integration.Wiki.PageModel>(x => x.Id == "pub" && x.SpaceKey == "PUB");
    }

    [Fact]
    public async Task Should_ReturnEmpty_WhenNoSpacesMatch()
    {
        var spaces = A.Fake<ISpaceSearcher>();
        var users = A.Fake<IUserManager>();
        A.CallTo(() => spaces.Search("query")).Returns(Task.FromResult(Array.Empty<Space>()));
        var service = new SearchService(A.Fake<IPermissionService>(), A.Fake<IPageSearcher>(), spaces, A.Fake<IUserSearcher>(), users, A.Fake<ISpaceDisplayService>(), A.Fake<ISpaceManager>());

        var result = await service.SearchSpaces("query", new UserInfo { Email = "user@example.test" });

        result.Should().BeEmpty();
        A.CallTo(() => users.GetByEmail(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnAllFoundSpaces_WhenAdminSearchesSpaces()
    {
        var spaces = A.Fake<ISpaceSearcher>();
        var users = A.Fake<IUserManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => spaces.Search("query")).Returns(Task.FromResult(new[]
        {
            new Space { Id = "s1", Key = "DOCS", Name = "Docs" },
            new Space { Id = "s2", Key = "SALES", Name = "Sales" }
        }));
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        var service = new SearchService(A.Fake<IPermissionService>(), A.Fake<IPageSearcher>(), spaces, A.Fake<IUserSearcher>(), users, A.Fake<ISpaceDisplayService>(), A.Fake<ISpaceManager>());

        var result = await service.SearchSpaces("query", user);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_ReturnViewModels_WhenAdminSearchesUsers()
    {
        var users = A.Fake<IUserManager>();
        var userSearcher = A.Fake<IUserSearcher>();
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        A.CallTo(() => userSearcher.Search("alice")).Returns(Task.FromResult<IEnumerable<Mimisbrunnr.Users.User>>(new[] { new Mimisbrunnr.Users.User { Email = "alice@example.test", Name = "Alice", Role = UserRole.Admin } }));
        var service = new SearchService(A.Fake<IPermissionService>(), A.Fake<IPageSearcher>(), A.Fake<ISpaceSearcher>(), userSearcher, users, A.Fake<ISpaceDisplayService>(), A.Fake<ISpaceManager>());

        var result = await service.SearchUsers("alice", user);

        result.Should().ContainSingle().Which.As<UserViewModel>().IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnModels_WhenEmployeeSearchesUsers()
    {
        var users = A.Fake<IUserManager>();
        var userSearcher = A.Fake<IUserSearcher>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));
        A.CallTo(() => userSearcher.Search("alice")).Returns(Task.FromResult<IEnumerable<Mimisbrunnr.Users.User>>(new[] { new Mimisbrunnr.Users.User { Email = "alice@example.test", Name = "Alice", Role = UserRole.Employee } }));
        var service = new SearchService(A.Fake<IPermissionService>(), A.Fake<IPageSearcher>(), A.Fake<ISpaceSearcher>(), userSearcher, users, A.Fake<ISpaceDisplayService>(), A.Fake<ISpaceManager>());

        var result = await service.SearchUsers("alice", user);

        result.Should().ContainSingle().And.NotBeOfType<UserViewModel>();
    }
}
