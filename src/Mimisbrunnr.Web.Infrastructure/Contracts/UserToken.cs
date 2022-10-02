using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Infrastructure.Contracts;

public class UserToken : IHasId<string>
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public DateTime Created { get; set; }

    public DateTime Expired { get; set; }

    public bool Revoked { get; set; }
}