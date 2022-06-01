using Microsoft.Extensions.Logging;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.User
{
    internal class UserService : IUserService
    {
        private readonly IUserManager _userManager;
        private readonly ILogger<UserService> _logger;
        public UserService(IUserManager userManager, ILogger<UserService> logger)
        {
            _logger = logger;
            _userManager = userManager;

        }

        public async Task<IEnumerable<UserViewModel>> GetUsers(UserInfo requestedBy)
        {
            var users = await _userManager.GetUsers();
            return users.Select(x => x.ToViewModel());
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

        public async Task Disable(string email, UserInfo disabledBy)
        {
            var user = await _userManager.GetByEmail(email);
            await _userManager.Disable(user);
            _logger.LogInformation("User `{User}` disabled by `{RequestedBy}`", user.Email, disabledBy.Email);
        }

        public async Task Enable(string email, UserInfo enabledBy)
        {
            var user = await _userManager.GetByEmail(email);
            await _userManager.Enable(user);
            _logger.LogInformation("User `{User}` disabled by `{RequestedBy}`", user.Email, enabledBy.Email);
        }

        public async Task Promote(string email, UserInfo promotedBy)
        {
            var user = await _userManager.GetByEmail(email);
            await _userManager.ChangeRole(user, UserRole.Admin);
            _logger.LogInformation("User `{User}` disabled by `{RequestedBy}`", user.Email, promotedBy.Email);
        }

        public async Task Demote(string email, UserInfo demotedBy)
        {
            var user = await _userManager.GetByEmail(email);
            await _userManager.ChangeRole(user, UserRole.Employee);
            _logger.LogInformation("User `{User}` disabled by `{RequestedBy}`", user.Email, demotedBy.Email);
        }
    }
}