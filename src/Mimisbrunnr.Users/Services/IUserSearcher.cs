namespace Mimisbrunnr.Users.Services;

public interface IUserSearcher
{
    Task<IEnumerable<User>> Search(string text);
}