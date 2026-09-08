using FakeItEasy;
using FluentAssertions;
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
}
