using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.User
{
    /// <summary>
    /// Service for managing users
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get a list of users
        /// </summary>
        /// <param name="requestedBy">User requesting the list</param>
        /// <param name="offset">Pagination offset</param>
        /// <returns>List of users</returns>
        Task<IEnumerable<UserModel>> GetUsers(UserInfo requestedBy,int? offset = null);

        /// <summary>
        /// Get the current authenticated user
        /// </summary>
        /// <param name="requestedBy">Current user</param>
        /// <returns>Current user view model</returns>
        Task<UserViewModel> GetCurrent(UserInfo requestedBy);

        /// <summary>
        /// Get groups for a specific user
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="requestedBy">User requesting the groups</param>
        /// <returns>List of groups the user belongs to</returns>
        Task<IEnumerable<GroupModel>> GetUserGroups(string email, UserInfo requestedBy);

        /// <summary>
        /// Get a user profile by email
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="requestedBy">User requesting the profile</param>
        /// <returns>User profile information</returns>
        Task<UserProfileModel> GetByEmail(string email, UserInfo requestedBy);

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="model">User creation parameters</param>
        /// <param name="createdBy">User performing the creation</param>
        /// <returns>The created user</returns>
        Task<UserModel> CreateUser(UserCreateModel model, UserInfo createdBy);

        /// <summary>
        /// Update user profile information
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="model">Profile update data</param>
        /// <param name="updatedBy">User performing the update</param>
        Task UpdateProfileInfo(string email, UserProfileUpdateModel model, UserInfo updatedBy);

        /// <summary>
        /// Disable a user account
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="disabledBy">User performing the operation</param>
        Task Disable(string email, UserInfo disabledBy);

        /// <summary>
        /// Enable a user account
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="enabledBy">User performing the operation</param>
        Task Enable(string email, UserInfo enabledBy);

        /// <summary>
        /// Promote a user to admin
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="promotedBy">User performing the operation</param>
        Task Promote(string email, UserInfo promotedBy)
        ;
        /// <summary>
        /// Demote a user from admin role
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="demotedBy">User performing the operation</param>
        Task Demote(string email, UserInfo demotedBy);
    }
}