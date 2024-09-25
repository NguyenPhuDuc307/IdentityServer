using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace vnLab.BackendServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize("Bearer")]
public class BaseController : Controller
{

}