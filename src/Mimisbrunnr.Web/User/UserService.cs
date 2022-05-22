using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.User
{
    internal class UserService : IUserService
    {
        private readonly IUserManager _userManager;
        public UserService(IUserManager userManager)
        {
            _userManager = userManager;

        }

        public async Task<UserViewModel> GetCurrent(UserInfo requestedBy)
        {
            var user = await _userManager.GetByEmail(requestedBy?.Email);
            return user?.ToViewModel();
        }

        public async Task<UserProfileModel> GetByEmail(string email, UserInfo requestedBy)
        {
            var user = await _userManager.GetByEmail(email);
            return user?.ToProfileModel();
        }
    }
}