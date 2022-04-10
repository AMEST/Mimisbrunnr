using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mimisbrunnr.Web.User;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController
{

}