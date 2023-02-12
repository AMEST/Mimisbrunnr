using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.User
{
    public interface IUserService
    {
        Task<IEnumerable<UserModel>> GetUsers(UserInfo requestedBy,int? offset = null);

        Task<UserViewModel> GetCurrent(UserInfo requestedBy);

        Task<IEnumerable<GroupModel>> GetUserGroups(string email, UserInfo requestedBy);

        Task<UserProfileModel> GetByEmail(string email, UserInfo requestedBy);

        Task<UserModel> CreateUser(UserCreateModel model, UserInfo createdBy);

        Task UpdateProfileInfo(string email, UserProfileUpdateModel model, UserInfo updatedBy);

        Task Disable(string email, UserInfo disabledBy);

        Task Enable(string email, UserInfo enabledBy);

        Task Promote(string email, UserInfo promotedBy)
        ;
        Task Demote(string email, UserInfo demotedBy);
    }
}