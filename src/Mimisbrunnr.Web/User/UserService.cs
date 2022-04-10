using Mimisbrunner.Users;
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

        public async Task<UserModel> FindByEmail(string email, UserInfo requestedBy)
        {
            var user = await _userManager.FindByEmail(email);
            return user?.ToModel();
        }
    }
}