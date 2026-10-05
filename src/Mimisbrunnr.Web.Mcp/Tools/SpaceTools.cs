using System.ComponentModel;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Mimisbrunnr.Web.Mcp.Tools;

/// <summary>
/// MCP tools for working with wiki spaces and their permissions.
/// </summary>
[McpServerToolType]
public sealed class SpaceTools
{
    private readonly ISpaceService _spaces;
    private readonly ToolContext _context;
    private readonly ToolExecutor _executor;

    public SpaceTools(ISpaceService spaces, ToolContext context, ToolExecutor executor)
    {
        _spaces = spaces;
        _context = context;
        _executor = executor;
    }

    [McpServerTool(Name = "list_spaces", ReadOnly = true, Idempotent = true)]
    [Description("Returns the list of wiki spaces available to the current user.")]
    public Task<string> ListSpaces(
        [Description("Maximum number of spaces to return.")] int? take = null,
        [Description("Number of spaces to skip for pagination.")] int? skip = null)
        => _executor.Execute("list_spaces", async () =>
        {
            var spaces = await _spaces.GetAll(_context.GetUser(), take, skip);
            return ToolJson.Serialize(spaces);
        });

    [McpServerTool(Name = "create_space")]
    [Description("Creates a new wiki space. Type must be one of: Personal, Private, Public.")]
    public Task<string> CreateSpace(
        [Description("Unique space key (alphanumeric, @, ., -, _; max 64 chars).")] string key,
        [Description("Space name.")] string name,
        [Description("Space type: Personal, Private or Public.")] string type,
        [Description("Optional space description.")] string description = null)
        => _executor.Execute("create_space", async () =>
        {
            ToolValidation.EnsureNotEmpty(key, nameof(key));
            ToolValidation.EnsureNotEmpty(name, nameof(name));

            var model = new SpaceCreateModel
            {
                Key = key,
                Name = name,
                Type = ParseSpaceType(type),
                Description = description
            };
            ToolValidation.EnsureValid(model);

            var space = await _spaces.Create(model, _context.GetUser());
            return ToolJson.Serialize(space);
        });

    [McpServerTool(Name = "get_space_permission", ReadOnly = true, Idempotent = true)]
    [Description("Returns the current user's permissions for the specified space.")]
    public Task<string> GetSpacePermission(
        [Description("Space key.")] string spaceKey)
        => _executor.Execute("get_space_permission", async () =>
        {
            var permission = await _spaces.GetPermission(spaceKey, _context.GetUser());
            return ToolJson.Serialize(permission);
        });

    [McpServerTool(Name = "list_space_permissions", ReadOnly = true, Idempotent = true)]
    [Description("Returns all permissions configured for the specified space. Requires space admin rights.")]
    public Task<string> ListSpacePermissions(
        [Description("Space key.")] string spaceKey)
        => _executor.Execute("list_space_permissions", async () =>
        {
            var permissions = await _spaces.GetSpacePermissions(spaceKey, _context.GetUser());
            return ToolJson.Serialize(permissions);
        });

    [McpServerTool(Name = "set_space_permission")]
    [Description("Adds or updates a permission for a user or group in the specified space. Requires space admin rights.")]
    public Task<string> SetSpacePermission(
        [Description("Space key.")] string spaceKey,
        [Description("Principal type: User or Group.")] string principalType,
        [Description("User email (for User) or group name (for Group).")] string principalId,
        [Description("Grants space admin rights.")] bool isAdmin = false,
        [Description("Allows viewing pages.")] bool canView = false,
        [Description("Allows editing pages.")] bool canEdit = false,
        [Description("Allows removing pages.")] bool canRemove = false)
        => _executor.Execute("set_space_permission", async () =>
        {
            var model = BuildPermission(principalType, principalId, isAdmin, canView, canEdit, canRemove);
            ToolValidation.EnsureValid(model);

            var user = _context.GetUser();
            var existing = await _spaces.GetSpacePermissions(spaceKey, user);
            var alreadyExists = existing.Any(p => MatchesPrincipal(p, principalType, principalId));

            if (alreadyExists)
                await _spaces.UpdatePermission(spaceKey, model, user);
            else
                await _spaces.AddPermission(spaceKey, model, user);

            return ToolJson.Serialize(model);
        });

    [McpServerTool(Name = "remove_space_permission", Destructive = true, Idempotent = true)]
    [Description("Removes a permission for a user or group from the specified space. Requires space admin rights.")]
    public Task<string> RemoveSpacePermission(
        [Description("Space key.")] string spaceKey,
        [Description("Principal type: User or Group.")] string principalType,
        [Description("User email (for User) or group name (for Group).")] string principalId)
        => _executor.Execute("remove_space_permission", async () =>
        {
            var model = BuildPermission(principalType, principalId, false, false, false, false);
            await _spaces.RemovePermission(spaceKey, model, _context.GetUser());
            return ToolJson.Ok();
        });

    private static SpaceTypeModel ParseSpaceType(string type)
    {
        if (Enum.TryParse<SpaceTypeModel>(type, ignoreCase: true, out var result) && Enum.IsDefined(result))
            return result;

        throw new McpException(
            $"invalid_argument: Unknown space type '{type}'. Allowed values: Personal, Private, Public.");
    }

    private static SpacePermissionModel BuildPermission(
        string principalType, string principalId, bool isAdmin, bool canView, bool canEdit, bool canRemove)
    {
        ToolValidation.EnsureNotEmpty(principalId, nameof(principalId));

        var model = new SpacePermissionModel
        {
            IsAdmin = isAdmin,
            CanView = canView,
            CanEdit = canEdit,
            CanRemove = canRemove
        };

        if (string.Equals(principalType, "User", StringComparison.OrdinalIgnoreCase))
            model.User = new UserModel { Email = principalId };
        else if (string.Equals(principalType, "Group", StringComparison.OrdinalIgnoreCase))
            model.Group = new GroupModel { Name = principalId };
        else
            throw new McpException("invalid_argument: principalType must be 'User' or 'Group'.");

        return model;
    }

    private static bool MatchesPrincipal(SpacePermissionModel permission, string principalType, string principalId)
    {
        if (string.Equals(principalType, "User", StringComparison.OrdinalIgnoreCase))
            return string.Equals(permission.User?.Email, principalId, StringComparison.OrdinalIgnoreCase);

        return string.Equals(permission.Group?.Name, principalId, StringComparison.OrdinalIgnoreCase);
    }
}
