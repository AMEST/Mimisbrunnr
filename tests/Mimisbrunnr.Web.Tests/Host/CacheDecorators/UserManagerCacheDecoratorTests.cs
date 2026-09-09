using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Host.Services.CacheDecorators;
using Mimisbrunnr.Web.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Mimisbrunnr.Web.Tests.Host.CacheDecorators;

public class UserManagerCacheDecoratorTests
{
    [Fact]
    public async Task Should_ReturnCachedUser_WhenExistsInCacheByEmail()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var user = new Users.User { Id = "u1", Email = "user@test.com", Name = "Test" };
        await cache.SetAsync("user_cache_email_user@test.com", user, new DistributedCacheEntryOptions());
        var decorator = new UserManagerCacheDecorator(inner, cache);

        var result = await decorator.GetByEmail("user@test.com");

        result.Should().BeEquivalentTo(user);
        A.CallTo(() => inner.GetByEmail("user@test.com")).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnCachedUser_WhenExistsInCacheById()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var user = new Users.User { Id = "u1", Email = "user@test.com" };
        await cache.SetAsync("user_cache_id_u1", user, new DistributedCacheEntryOptions());
        var decorator = new UserManagerCacheDecorator(inner, cache);

        var result = await decorator.GetById("u1");

        result.Should().BeEquivalentTo(user);
        A.CallTo(() => inner.GetById("u1")).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_CallInnerAndCache_WhenEmailCacheMiss()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var user = new Users.User { Id = "u1", Email = "user@test.com" };
        A.CallTo(() => inner.GetByEmail("user@test.com")).Returns(user);
        var decorator = new UserManagerCacheDecorator(inner, cache);

        var result = await decorator.GetByEmail("user@test.com");

        result.Should().BeEquivalentTo(user);
        A.CallTo(() => inner.GetByEmail("user@test.com")).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Users.User>("user_cache_email_user@test.com")).Should().NotBeNull();
        (await cache.GetAsync<Users.User>("user_cache_id_u1")).Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ReturnNull_WhenEmailIsEmpty()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var decorator = new UserManagerCacheDecorator(inner, cache);

        var result = await decorator.GetByEmail("");

        result.Should().BeNull();
        A.CallTo(() => inner.GetByEmail(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnNull_WhenEmailIsNull()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var decorator = new UserManagerCacheDecorator(inner, cache);

        var result = await decorator.GetByEmail(null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_ClearCache_WhenDisablingUser()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var user = new Users.User { Id = "u1", Email = "user@test.com" };
        await cache.SetAsync("user_cache_email_user@test.com", user, new DistributedCacheEntryOptions());
        await cache.SetAsync("user_cache_id_u1", user, new DistributedCacheEntryOptions());
        var decorator = new UserManagerCacheDecorator(inner, cache);

        await decorator.Disable(user);

        A.CallTo(() => inner.Disable(user)).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Users.User>("user_cache_email_user@test.com")).Should().BeNull();
        (await cache.GetAsync<Users.User>("user_cache_id_u1")).Should().BeNull();
    }

    [Fact]
    public async Task Should_ClearCache_WhenEnablingUser()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var user = new Users.User { Id = "u1", Email = "user@test.com" };
        await cache.SetAsync("user_cache_email_user@test.com", user, new DistributedCacheEntryOptions());
        var decorator = new UserManagerCacheDecorator(inner, cache);

        await decorator.Enable(user);

        A.CallTo(() => inner.Enable(user)).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Users.User>("user_cache_email_user@test.com")).Should().BeNull();
    }

    [Fact]
    public async Task Should_ClearCache_WhenChangingRole()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var user = new Users.User { Id = "u1", Email = "user@test.com" };
        await cache.SetAsync("user_cache_email_user@test.com", user, new DistributedCacheEntryOptions());
        var decorator = new UserManagerCacheDecorator(inner, cache);

        await decorator.ChangeRole(user, UserRole.Admin);

        A.CallTo(() => inner.ChangeRole(user, UserRole.Admin)).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Users.User>("user_cache_email_user@test.com")).Should().BeNull();
    }

    [Fact]
    public async Task Should_ClearCache_WhenUpdatingUserInfo()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var user = new Users.User { Id = "u1", Email = "user@test.com" };
        await cache.SetAsync("user_cache_email_user@test.com", user, new DistributedCacheEntryOptions());
        var decorator = new UserManagerCacheDecorator(inner, cache);

        await decorator.UpdateUserInfo(user);

        A.CallTo(() => inner.UpdateUserInfo(user)).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Users.User>("user_cache_email_user@test.com")).Should().BeNull();
    }

    [Fact]
    public async Task Should_CacheUser_WhenAdding()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var user = new Users.User { Id = "u1", Email = "new@test.com" };
        A.CallTo(() => inner.GetByEmail("new@test.com")).Returns(user);
        var decorator = new UserManagerCacheDecorator(inner, cache);

        var result = await decorator.Add("new@test.com", "New", "avatar", UserRole.Employee);

        result.Should().BeEquivalentTo(user);
        (await cache.GetAsync<Users.User>("user_cache_email_new@test.com")).Should().NotBeNull();
        (await cache.GetAsync<Users.User>("user_cache_id_u1")).Should().NotBeNull();
    }

    [Fact]
    public async Task Should_DelegateGetUsers_ToInner()
    {
        var inner = A.Fake<IUserManager>();
        var cache = new FakeDistributedCache();
        var decorator = new UserManagerCacheDecorator(inner, cache);

        await decorator.GetUsers(0);

        A.CallTo(() => inner.GetUsers(0)).MustHaveHappenedOnceExactly();
    }
}