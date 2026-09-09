using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class SpaceManagerTests
{
    public SpaceManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_FindSpaceCaseInsensitively_WhenGettingByKey()
    {
        var repository = A.Fake<IRepository<Space>>();
        var space = new Space { Key = "DOCS" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { space }.AsQueryable());

        var result = await new SpaceManager(repository, A.Fake<IPageManager>()).GetByKey("docs");

        result.Should().BeSameAs(space);
    }

    [Fact]
    public async Task Should_ReturnPublicAndPermittedSpaces_WhenFilteringByUser()
    {
        var repository = A.Fake<IRepository<Space>>();
        var publicSpace = new Space { Key = "PUBLIC", Type = SpaceType.Public, Permissions = [] };
        var privateSpace = new Space { Key = "PRIVATE", Type = SpaceType.Private, Permissions = [new Permission { User = new UserInfo { Email = "USER@EXAMPLE.TEST" } }] };
        var hiddenSpace = new Space { Key = "HIDDEN", Type = SpaceType.Private, Permissions = [] };
        publicSpace.UpdatePermissions();
        privateSpace.UpdatePermissions();
        hiddenSpace.UpdatePermissions();
        A.CallTo(() => repository.GetAll()).Returns(new[] { publicSpace, privateSpace, hiddenSpace }.AsQueryable());
        var manager = new SpaceManager(repository, A.Fake<IPageManager>());

        var result = await manager.GetAllWithPermissions(new UserInfo { Email = "USER@EXAMPLE.TEST" });

        result.Select(x => x.Key).Should().BeEquivalentTo(["PUBLIC", "PRIVATE"]);
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenAddingPermissionWithUserAndGroup()
    {
        var manager = new SpaceManager(A.Fake<IRepository<Space>>(), A.Fake<IPageManager>());
        var space = new Space { Permissions = [] };

        await manager.Invoking(x => x.AddPermission(space, new Permission { User = new UserInfo(), Group = new GroupInfo() }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ReturnOnlyPublicSpaces_WhenGettingPublicSpaces()
    {
        var repository = A.Fake<IRepository<Space>>();
        A.CallTo(() => repository.GetAll()).Returns(new[] { new Space { Key = "PUBLIC", Type = SpaceType.Public }, new Space { Key = "PRIVATE", Type = SpaceType.Private } }.AsQueryable());

        var spaces = await new SpaceManager(repository, A.Fake<IPageManager>()).GetPublicSpaces();

        spaces.Should().ContainSingle().Which.Key.Should().Be("PUBLIC");
    }

    [Fact]
    public async Task Should_ApplySkipAndTake_WhenGettingAll()
    {
        var repository = A.Fake<IRepository<Space>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Space { Key = "A" }, new Space { Key = "B" }, new Space { Key = "C" }
        }.AsQueryable());

        var spaces = await new SpaceManager(repository, A.Fake<IPageManager>()).GetAll(take: 2, skip: 1);

        spaces.Select(x => x.Key).Should().BeEquivalentTo(["B", "C"]);
    }

    [Fact]
    public async Task Should_ReturnGroupPermittedSpaces_WhenFilteringByGroups()
    {
        var repository = A.Fake<IRepository<Space>>();
        var devSpace = new Space { Key = "DEV", Type = SpaceType.Private, Permissions = [new Permission { Group = new GroupInfo { Name = "DEV" } }] };
        var lockedSpace = new Space { Key = "LOCKED", Type = SpaceType.Private, Permissions = [] };
        var publicSpace = new Space { Key = "PUB", Type = SpaceType.Public, Permissions = [] };
        devSpace.UpdatePermissions();
        lockedSpace.UpdatePermissions();
        publicSpace.UpdatePermissions();
        A.CallTo(() => repository.GetAll()).Returns(new[] { devSpace, lockedSpace, publicSpace }.AsQueryable());

        var result = await new SpaceManager(repository, A.Fake<IPageManager>()).GetAllWithPermissions(userGroups: ["DEV"]);

        result.Select(x => x.Key).Should().BeEquivalentTo(["DEV", "PUB"]);
    }

    [Fact]
    public async Task Should_ReturnNull_WhenGettingByUnknownKey()
    {
        var repository = A.Fake<IRepository<Space>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Space>().AsQueryable());

        var result = await new SpaceManager(repository, A.Fake<IPageManager>()).GetByKey("UNKNOWN");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_ReturnSpace_WhenGettingById()
    {
        var repository = A.Fake<IRepository<Space>>();
        var space = new Space { Id = "s1", Key = "DOCS" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { space }.AsQueryable());

        var result = await new SpaceManager(repository, A.Fake<IPageManager>()).GetById("s1");

        result.Should().BeSameAs(space);
    }

    [Fact]
    public async Task Should_ReturnOnlyRequestedSpaces_WhenGettingByIds()
    {
        var repository = A.Fake<IRepository<Space>>();
        A.CallTo(() => repository.GetAll()).Returns(new[] { new Space { Id = "s1" }, new Space { Id = "s2" }, new Space { Id = "s3" } }.AsQueryable());

        var result = await new SpaceManager(repository, A.Fake<IPageManager>()).GetByIds("s1", "s3");

        result.Select(x => x.Id).Should().BeEquivalentTo(["s1", "s3"]);
    }

    [Fact]
    public async Task Should_ReturnEmpty_WhenGettingByIdsWithoutArguments()
    {
        var repository = A.Fake<IRepository<Space>>();

        var result = await new SpaceManager(repository, A.Fake<IPageManager>()).GetByIds();

        result.Should().BeEmpty();
        A.CallTo(() => repository.GetAll()).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnPersonalSpaceWithEmailKey_WhenFindingPersonalSpace()
    {
        var repository = A.Fake<IRepository<Space>>();
        var personal = new Space { Key = "USER@TEST.COM", Type = SpaceType.Personal };
        A.CallTo(() => repository.GetAll()).Returns(new[] { personal }.AsQueryable());

        var result = await new SpaceManager(repository, A.Fake<IPageManager>()).FindPersonalSpace(new UserInfo { Email = "user@test.com" });

        result.Should().BeSameAs(personal);
    }

    [Fact]
    public async Task Should_ReturnMatchingSpaces_WhenFindingByName()
    {
        var repository = A.Fake<IRepository<Space>>();
        A.CallTo(() => repository.GetAll()).Returns(new[] { new Space { Name = "Dev Team" }, new Space { Name = "Sales" } }.AsQueryable());

        var result = await new SpaceManager(repository, A.Fake<IPageManager>()).FindByName("Team");

        result.Should().ContainSingle().Which.Name.Should().Be("Dev Team");
    }

    [Fact]
    public async Task Should_CreateSpaceWithHomePageAndAdminPermission()
    {
        var repository = A.Fake<IRepository<Space>>();
        var pages = A.Fake<IPageManager>();
        var owner = new UserInfo { Email = "owner@test.com" };
        var homePage = new Page { Id = "home1" };
        A.CallTo(() => pages.Create(A<string>._, "Dev", "# Description   ", owner, A<string>._)).Returns(homePage);

        var result = await new SpaceManager(repository, pages).Create("DEV", "Dev", "Description", SpaceType.Private, owner);

        result.Key.Should().Be("DEV");
        result.HomePageId.Should().Be("home1");
        result.Status.Should().Be(SpaceStatus.Actual);
        result.Permissions.Single().User.Should().BeSameAs(owner);
        result.Permissions.Single().IsAdmin.Should().BeTrue();
        A.CallTo(() => repository.Create(result, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_AddUserPermission_WhenAddingValidPermission()
    {
        var repository = A.Fake<IRepository<Space>>();
        var space = new Space { Type = SpaceType.Private, Permissions = [] };
        var permission = new Permission { User = new UserInfo { Email = "u@test.com" }, CanView = true };
        A.CallTo(() => repository.GetAll()).Returns(new[] { space }.AsQueryable());
        var manager = new SpaceManager(repository, A.Fake<IPageManager>());

        await manager.AddPermission(space, permission);

        space.Permissions.Should().Contain(permission);
        space.PermissionsFlat.Should().Contain("user_u@test.com");
        A.CallTo(() => repository.Update(space, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenAddingAdminToPersonalSpace()
    {
        var manager = new SpaceManager(A.Fake<IRepository<Space>>(), A.Fake<IPageManager>());
        var space = new Space
        {
            Type = SpaceType.Personal,
            Permissions = [new Permission { User = new UserInfo { Email = "admin@test.com" }, IsAdmin = true }]
        };

        await manager.Invoking(x => x.AddPermission(space, new Permission { User = new UserInfo { Email = "other@test.com" }, IsAdmin = true }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_RemoveUserPermission_WhenRemovingValidPermission()
    {
        var repository = A.Fake<IRepository<Space>>();
        var user = new UserInfo { Email = "u@test.com" };
        var space = new Space { Type = SpaceType.Private, Permissions = [new Permission { User = user, CanView = true }] };
        space.UpdatePermissions();
        A.CallTo(() => repository.GetAll()).Returns(new[] { space }.AsQueryable());
        var manager = new SpaceManager(repository, A.Fake<IPageManager>());

        await manager.RemovePermission(space, new Permission { User = user });

        space.Permissions.Should().BeEmpty();
        space.PermissionsFlat.Should().BeEmpty();
        A.CallTo(() => repository.Update(space, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenRemovingAdminFromPersonalSpace()
    {
        var manager = new SpaceManager(A.Fake<IRepository<Space>>(), A.Fake<IPageManager>());
        var space = new Space { Type = SpaceType.Personal, Permissions = [] };

        await manager.Invoking(x => x.RemovePermission(space, new Permission { User = new UserInfo { Email = "x@test.com" }, IsAdmin = true }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_UpdatePermission_ByRemovingAndAdding()
    {
        var repository = A.Fake<IRepository<Space>>();
        var user = new UserInfo { Email = "u@test.com" };
        var space = new Space { Type = SpaceType.Private, Permissions = [new Permission { User = user, CanView = false }] };
        A.CallTo(() => repository.GetAll()).Returns(new[] { space }.AsQueryable());
        var manager = new SpaceManager(repository, A.Fake<IPageManager>());

        await manager.UpdatePermission(space, new Permission { User = user, CanEdit = true, IsAdmin = true });

        A.CallTo(() => repository.Update(space, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_SetStatusArchived_WhenArchivingSpace()
    {
        var repository = A.Fake<IRepository<Space>>();
        var space = new Space { Status = SpaceStatus.Actual, Permissions = [] };

        await new SpaceManager(repository, A.Fake<IPageManager>()).Archive(space);

        space.Status.Should().Be(SpaceStatus.Archived);
        A.CallTo(() => repository.Update(space, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_SetStatusActual_WhenUnArchivingSpace()
    {
        var repository = A.Fake<IRepository<Space>>();
        var space = new Space { Status = SpaceStatus.Archived, Permissions = [] };

        await new SpaceManager(repository, A.Fake<IPageManager>()).UnArchive(space);

        space.Status.Should().Be(SpaceStatus.Actual);
        A.CallTo(() => repository.Update(space, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_RemoveHomePageAndSpace_WhenRemovingSpace()
    {
        var repository = A.Fake<IRepository<Space>>();
        var pages = A.Fake<IPageManager>();
        var space = new Space { Id = "s1", HomePageId = "home1" };
        var homePage = new Page { Id = "home1" };
        A.CallTo(() => pages.GetById("home1")).Returns(homePage);
        var manager = new SpaceManager(repository, pages);

        await manager.Remove(space);

        A.CallTo(() => pages.Remove(homePage, true)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Delete(space, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_SearchByNameAndDescriptionCaseInsensitively()
    {
        var repository = A.Fake<IRepository<Space>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Space { Name = "Knowledge Base", Description = "All docs" },
            new Space { Name = "Marketing", Description = "KNOWLEDGE transfer" },
            new Space { Name = "Sales", Description = "Pitching" }
        }.AsQueryable());

        var result = await new SpaceManager(repository, A.Fake<IPageManager>()).Search("knowledge");

        result.Select(x => x.Name).Should().BeEquivalentTo(["Knowledge Base", "Marketing"]);
    }
}
