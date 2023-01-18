using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Favorites.Contracts;

public abstract class Favorite : IHasId<string>
{
    public string Id { get; set; }

    public string OwnerEmail { get; set; }
}