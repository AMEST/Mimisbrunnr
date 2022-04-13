using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.User
{
    public interface IUserService
    {
         Task<UserModel> GetByEmail(string email, UserInfo requestedBy);
    }
}