using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Integration.Group;

namespace Mimisbrunnr.Web.Group;

/// <summary>
/// Service for managing user groups
/// </summary>
public interface IGroupService
{
     /// <summary>
     /// Get all groups matching the specified filter
     /// </summary>
     /// <param name="filter">Optional filter criteria</param>
     /// <param name="requestedBy">User requesting the list</param>
     /// <returns>List of groups</returns>
     Task<IEnumerable<GroupModel>> GetAll(GroupFilterModel filter, UserInfo requestedBy);
     
     /// <summary>
     /// Get a group by name
     /// </summary>
     /// <param name="name">Name of the group</param>
     /// <param name="requestedBy">User requesting the group</param>
     /// <returns>The group information</returns>
     Task<GroupModel> Get(string name, UserInfo requestedBy);

     /// <summary>
     /// Create a new group
     /// </summary>
     /// <param name="createModel">Group creation parameters</param>
     /// <param name="createdBy">User performing the creation</param>
     /// <returns>The created group</returns>
     Task<GroupModel> Create(GroupCreateModel createModel, UserInfo createdBy);

     /// <summary>
     /// Remove a group
     /// </summary>
     /// <param name="name">Name of the group</param>
     /// <param name="removedBy">User performing the removal</param>
     Task Remove(string name, UserInfo removedBy);

     /// <summary>
     /// Update a group
     /// </summary>
     /// <param name="name">Name of the group</param>
     /// <param name="model">Group update parameters</param>
     /// <param name="updatedBy">User performing the update</param>
     Task Update(string name, GroupUpdateModel model, UserInfo updatedBy);

     /// <summary>
     /// Get users in a group
     /// </summary>
     /// <param name="name">Name of the group</param>
     /// <param name="requestedBy">User requesting the list</param>
     /// <returns>List of users in the group</returns>
     Task<IEnumerable<UserModel>> GetUsers(string name, UserInfo requestedBy);

     /// <summary>
     /// Add a user to a group
     /// </summary>
     /// <param name="name">Name of the group</param>
     /// <param name="user">User to add</param>
     /// <param name="addedBy">User performing the operation</param>
     Task AddUserToGroup(string name, UserInfo user, UserInfo addedBy);

     /// <summary>
     /// Remove a user from a group
     /// </summary>
     /// <param name="name">Name of the group</param>
     /// <param name="user">User to remove</param>
     /// <param name="removedBy">User performing the operation</param>
     Task RemoveUserFromGroup(string name, UserInfo user, UserInfo removedBy);
}