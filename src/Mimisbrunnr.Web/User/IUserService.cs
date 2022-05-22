using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.User
{
    public interface IUserService
    {
        Task<UserViewModel> GetCurrent(UserInfo requestedBy);

        Task<UserProfileModel> GetByEmail(string email, UserInfo requestedBy);
    }
}