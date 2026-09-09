using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Web.Host.Services.CacheDecorators;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Mimisbrunnr.Web.Tests.Host.CacheDecorators;

public class PageManagerCacheDecoratorTests
{
    [Fact]
    public async Task Should_ReturnCachedPage_WhenExistsInCache()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        var page = new Page { Id = "p1", Name = "Test" };
        await cache.SetAsync("page_manager_cache_id_p1", page, new DistributedCacheEntryOptions());
        var decorator = new PageManagerCacheDecorator(inner, cache);

        var result = await decorator.GetById("p1");

        result.Should().BeEquivalentTo(page);
        A.CallTo(() => inner.GetById("p1")).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_CallInnerAndCache_WhenCacheMiss()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        var page = new Page { Id = "p1", Name = "Test", SpaceId = "s1" };
        A.CallTo(() => inner.GetById("p1")).Returns(page);
        var decorator = new PageManagerCacheDecorator(inner, cache);

        var result = await decorator.GetById("p1");

        result.Should().BeEquivalentTo(page);
        A.CallTo(() => inner.GetById("p1")).MustHaveHappenedOnceExactly();
        var cached = await cache.GetAsync<Page>("page_manager_cache_id_p1");
        cached.Should().BeEquivalentTo(page);
    }

    [Fact]
    public async Task Should_ReturnCachedPages_WhenSpacePagesExistInCache()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        var pages = new[] { new Page { Id = "p1" }, new Page { Id = "p2" } };
        await cache.SetAsync("page_manager_cache_all_s1", pages, new DistributedCacheEntryOptions());
        var space = new Space { Id = "s1" };
        var decorator = new PageManagerCacheDecorator(inner, cache);

        var result = await decorator.GetAllOnSpace(space);

        result.Should().BeEquivalentTo(pages);
        A.CallTo(() => inner.GetAllOnSpace(space)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ClearCache_WhenUpdatingPage()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        var page = new Page { Id = "p1", SpaceId = "s1" };
        await cache.SetAsync("page_manager_cache_id_p1", page, new DistributedCacheEntryOptions());
        await cache.SetAsync("page_manager_cache_all_s1", new[] { page }, new DistributedCacheEntryOptions());
        var decorator = new PageManagerCacheDecorator(inner, cache);

        await decorator.Update(page, new UserInfo { Email = "a@b.c" });

        A.CallTo(() => inner.Update(page, A<UserInfo>._)).MustHaveHappenedOnceExactly();
        var cached = await cache.GetAsync<Page>("page_manager_cache_id_p1");
        cached.Should().BeNull();
    }

    [Fact]
    public async Task Should_ClearCache_WhenRemovingPage()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        var page = new Page { Id = "p1", SpaceId = "s1" };
        await cache.SetAsync("page_manager_cache_id_p1", page, new DistributedCacheEntryOptions());
        var decorator = new PageManagerCacheDecorator(inner, cache);

        await decorator.Remove(page, false);

        A.CallTo(() => inner.Remove(page, false)).MustHaveHappenedOnceExactly();
        var cached = await cache.GetAsync<Page>("page_manager_cache_id_p1");
        cached.Should().BeNull();
    }

    [Fact]
    public async Task Should_DelegateCreateAndClearCache()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        var created = new Page { Id = "p1", SpaceId = "s1" };
        A.CallTo(() => inner.Create("s1", "Name", "Content", A<UserInfo>._, A<string>._)).Returns(created);
        var decorator = new PageManagerCacheDecorator(inner, cache);

        var result = await decorator.Create("s1", "Name", "Content", new UserInfo { Email = "a@b.c" });

        result.Should().Be(created);
        A.CallTo(() => inner.Create("s1", "Name", "Content", A<UserInfo>._, A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotThrow_WhenPageIsNullOnGet()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        A.CallTo(() => inner.GetById("missing")).Returns<Page>(null);
        var decorator = new PageManagerCacheDecorator(inner, cache);

        var result = await decorator.GetById("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_DelegateFindByName_ToInner()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        var decorator = new PageManagerCacheDecorator(inner, cache);

        await decorator.FindByName("test");

        A.CallTo(() => inner.FindByName("test")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DelegateGetAllVersions_ToInner()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        var page = new Page { Id = "p1" };
        var decorator = new PageManagerCacheDecorator(inner, cache);

        await decorator.GetAllVersions(page);

        A.CallTo(() => inner.GetAllVersions(page)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ClearCache_WhenMovingPage()
    {
        var inner = A.Fake<IPageManager>();
        var cache = new FakeDistributedCache();
        var source = new Page { Id = "p1", SpaceId = "s1" };
        var dest = new Page { Id = "p2", SpaceId = "s2" };
        var moved = new Page { Id = "p2", SpaceId = "s2" };
        A.CallTo(() => inner.Move(source, dest, true)).Returns(moved);
        var decorator = new PageManagerCacheDecorator(inner, cache);

        await decorator.Move(source, dest);

        A.CallTo(() => inner.Move(source, dest, true)).MustHaveHappenedOnceExactly();
    }
}