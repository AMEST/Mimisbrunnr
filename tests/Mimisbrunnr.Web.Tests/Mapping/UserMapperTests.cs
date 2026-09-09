using System.Security.Claims;
using FluentAssertions;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Tests.Mapping;

public class UserMapperTests
{
    #region ToInfo(ClaimsPrincipal)

    [Fact]
    public void Should_ReturnUserInfo_WhenPrincipalHasEmailClaim()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Email, "User@Example.Test"), new Claim(ClaimTypes.Name, "John")]));

        var result = principal.ToInfo();

        result.Should().NotBeNull();
        result.Email.Should().Be("user@example.test");
        result.Name.Should().Be("John");
    }

    [Fact]
    public void Should_ReturnNull_WhenPrincipalHasNoEmail()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "John")]));

        var result = principal.ToInfo();

        result.Should().BeNull();
    }

    [Fact]
    public void Should_ReturnNull_WhenPrincipalIsNull()
    {
        var result = ((ClaimsPrincipal)null).ToInfo();

        result.Should().BeNull();
    }

    [Fact]
    public void Should_FallbackToEmailClaimType_WhenClaimTypesEmailMissing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("email", "Fallback@Test.Com")]));

        var result = principal.ToInfo();

        result.Should().NotBeNull();
        result.Email.Should().Be("fallback@test.com");
    }

    [Fact]
    public void Should_MapAvatarUrl_WhenPictureClaimExists()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Email, "a@b.c"), new Claim("picture", "https://img.test/a.png")]));

        var result = principal.ToInfo();

        result.AvatarUrl.Should().Be("https://img.test/a.png");
    }

    [Fact]
    public void Should_MapNameFromIdentity_WhenNameClaimExists()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Email, "a@b.c"), new Claim(ClaimTypes.Name, "John")], "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var result = principal.ToInfo();

        result.Should().NotBeNull();
        result.Name.Should().Be("John");
    }

    [Fact]
    public void Should_FallbackToNameClaim_WhenIdentityNameIsNull()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Email, "a@b.c"), new Claim("name", "Jane")]));

        var result = principal.ToInfo();

        result.Name.Should().Be("Jane");
    }

    #endregion
}
