using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.User
{
    public interface IUserService
    {
         Task<UserModel> FindByEmail(string email, UserInfo requestedBy);
    }
}