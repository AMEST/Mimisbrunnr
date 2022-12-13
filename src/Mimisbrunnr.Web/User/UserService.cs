using Microsoft.Extensions.Logging;
using Mimisbrunnr.Users;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.User
{
    internal class UserService : IUserService
    {
        private readonly IUserManager _userManager;
        private readonly IUserGroupManager _userGroupManager;
        private readonly ILogger<UserService> _logger;
        public UserService(
            IUserManager userManager,
            IUserGroupManager userGroupManager,
            ILogger<UserService> logger
         )
        {
            _logger = logger;
            _userManager = userManager;
            _userGroupManager = userGroupManager;
        }

        public async Task<IEnumerable<UserModel>> GetUsers(UserInfo requestedBy, int? offset = null)
        {
            var requestedByUser = await _userManager.GetByEmail(requestedBy.Email);
            var users = await _userManager.GetUsers(offset);

            return requestedByUser.Role == UserRole.Admin
                ? users.Select(x => x.ToViewModel())
                : users.Select(x => x.ToModel());
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

        public async Task<IEnumerable<GroupModel>> GetUserGroups(string email, UserInfo requestedBy)
        {
            if (string.IsNullOrEmpty(email)) 
                return Array.Empty<GroupModel>();

            var user = await _userManager.GetByEmail(email);
            var requestedByUser = requestedBy.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                ? user
                : await _userManager.GetByEmail(requestedBy.Email);

            if(!requestedBy.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                && requestedByUser.Role != UserRole.Admin)
                return Array.Empty<GroupModel>();

            var groups = await _userGroupManager.GetUserGroups(user);
            return groups.Select(x => x.ToModel());
        }

        public async Task UpdateProfileInfo(string email, UserProfileUpdateModel model, UserInfo updatedBy)
        {
            var user = await _userManager.GetByEmail(email);
            var updatedByUser = updatedBy.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                ? user
                : await _userManager.GetByEmail(updatedBy.Email);

            if(!updatedBy.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                && updatedByUser.Role != UserRole.Admin)
                throw new UserHasNotPermissionException();
            
            user.Website = model.Website;
            user.Post = model.Post;
            user.Department = model.Department;
            user.Organization = model.Organization;
            user.Location = model.Location;

            await _userManager.UpdateUserInfo(user);
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