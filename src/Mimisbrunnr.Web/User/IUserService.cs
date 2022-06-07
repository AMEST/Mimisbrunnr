using Mimisbrunnr.Web.Group;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.User
{
    public interface IUserService
    {
        Task<IEnumerable<UserModel>> GetUsers(UserInfo requestedBy);

        Task<UserViewModel> GetCurrent(UserInfo requestedBy);

        Task<IEnumerable<GroupModel>> GetUserGroups(string email, UserInfo requestedBy);

        Task<UserProfileModel> GetByEmail(string email, UserInfo requestedBy);

        Task Disable(string email, UserInfo disabledBy);

        Task Enable(string email, UserInfo enabledBy);

        Task Promote(string email, UserInfo promotedBy)
        ;
        Task Demote(string email, UserInfo demotedBy);
    }
}