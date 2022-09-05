using Mimisbrunnr.Web.User;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Group;

public interface IGroupService
{
     Task<IEnumerable<GroupModel>> GetAll(GroupFilterModel filter, UserInfo requestedBy);
     
     Task<GroupModel> Get(string name, UserInfo requestedBy);

     Task<GroupModel> Create(GroupCreateModel createModel, UserInfo createdBy);

     Task Remove(string name, UserInfo removedBy);

     Task Update(string name, GroupUpdateModel model, UserInfo updatedBy);

     Task<IEnumerable<UserModel>> GetUsers(string name, UserInfo requestedBy);

     Task AddUserToGroup(string name, UserInfo user, UserInfo addedBy);

     Task RemoveUserFromGroup(string name, UserInfo user, UserInfo removedBy);
}