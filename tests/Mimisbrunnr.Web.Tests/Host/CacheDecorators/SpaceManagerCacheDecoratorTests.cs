using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Web.Host.Services.CacheDecorators;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Mimisbrunnr.Web.Tests.Host.CacheDecorators;

public class SpaceManagerCacheDecoratorTests
{
    [Fact]
    public async Task Should_ReturnCachedSpace_WhenExistsInCacheByKey()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var space = new Space { Id = "s1", Key = "DOCS" };
        await cache.SetAsync("space_cache_key_DOCS", space, new DistributedCacheEntryOptions());
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        var result = await decorator.GetByKey("DOCS");

        result.Should().BeEquivalentTo(space);
        A.CallTo(() => inner.GetByKey("DOCS")).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnCachedSpace_WhenExistsInCacheById()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var space = new Space { Id = "s1", Key = "DOCS" };
        await cache.SetAsync("space_cache_id_s1", space, new DistributedCacheEntryOptions());
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        var result = await decorator.GetById("s1");

        result.Should().BeEquivalentTo(space);
        A.CallTo(() => inner.GetById("s1")).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_CallInnerAndCache_WhenCacheMiss()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var space = new Space { Id = "s1", Key = "DOCS" };
        A.CallTo(() => inner.GetByKey("DOCS")).Returns(space);
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        var result = await decorator.GetByKey("DOCS");

        result.Should().BeEquivalentTo(space);
        A.CallTo(() => inner.GetByKey("DOCS")).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Space>("space_cache_key_DOCS")).Should().NotBeNull();
        (await cache.GetAsync<Space>("space_cache_id_s1")).Should().NotBeNull();
    }

    [Fact]
    public async Task Should_CreateSpaceAndCacheIt()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var space = new Space { Id = "s1", Key = "NEW" };
        A.CallTo(() => inner.Create("NEW", "New Space", "desc", SpaceType.Public, A<UserInfo>._)).Returns(space);
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        var result = await decorator.Create("NEW", "New Space", "desc", SpaceType.Public, new UserInfo { Email = "a@b.c" });

        result.Should().BeEquivalentTo(space);
        (await cache.GetAsync<Space>("space_cache_key_NEW")).Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ClearCache_WhenUpdatingSpace()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var space = new Space { Id = "s1", Key = "DOCS" };
        await cache.SetAsync("space_cache_key_DOCS", space, new DistributedCacheEntryOptions());
        await cache.SetAsync("space_cache_id_s1", space, new DistributedCacheEntryOptions());
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        await decorator.Update(space);

        A.CallTo(() => inner.Update(space)).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Space>("space_cache_key_DOCS")).Should().BeNull();
        (await cache.GetAsync<Space>("space_cache_id_s1")).Should().BeNull();
    }

    [Fact]
    public async Task Should_ClearCache_WhenRemovingSpace()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var space = new Space { Id = "s1", Key = "DOCS" };
        await cache.SetAsync("space_cache_key_DOCS", space, new DistributedCacheEntryOptions());
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        await decorator.Remove(space);

        A.CallTo(() => inner.Remove(space)).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Space>("space_cache_key_DOCS")).Should().BeNull();
    }

    [Fact]
    public async Task Should_ClearCache_WhenArchivingSpace()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var space = new Space { Id = "s1", Key = "DOCS" };
        await cache.SetAsync("space_cache_key_DOCS", space, new DistributedCacheEntryOptions());
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        await decorator.Archive(space);

        A.CallTo(() => inner.Archive(space)).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Space>("space_cache_key_DOCS")).Should().BeNull();
    }

    [Fact]
    public async Task Should_ClearCache_WhenAddingPermission()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var space = new Space { Id = "s1", Key = "DOCS" };
        await cache.SetAsync("space_cache_key_DOCS", space, new DistributedCacheEntryOptions());
        var permission = new Permission { User = new UserInfo { Email = "a@b.c" }, CanView = true };
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        await decorator.AddPermission(space, permission);

        A.CallTo(() => inner.AddPermission(space, permission)).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Space>("space_cache_key_DOCS")).Should().BeNull();
    }

    [Fact]
    public async Task Should_NotThrow_WhenSpaceIsNullOnGet()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        A.CallTo(() => inner.GetByKey("MISS")).Returns<Space>(null);
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        var result = await decorator.GetByKey("MISS");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_DelegateGetAll_ToInner()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        await decorator.GetAll(10, 0);

        A.CallTo(() => inner.GetAll(10, 0)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ClearCache_WhenUpdatingPermission()
    {
        var inner = A.Fake<ISpaceManager>();
        var cache = new FakeDistributedCache();
        var space = new Space { Id = "s1", Key = "DOCS" };
        await cache.SetAsync("space_cache_key_DOCS", space, new DistributedCacheEntryOptions());
        var permission = new Permission { User = new UserInfo { Email = "a@b.c" }, CanView = true };
        var decorator = new SpaceManagerCacheDecorator(inner, cache);

        await decorator.UpdatePermission(space, permission);

        A.CallTo(() => inner.UpdatePermission(space, permission)).MustHaveHappenedOnceExactly();
        (await cache.GetAsync<Space>("space_cache_key_DOCS")).Should().BeNull();
    }
}