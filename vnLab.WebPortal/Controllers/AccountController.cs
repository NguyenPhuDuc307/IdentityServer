using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vnLab.WebPortal.Extensions;
using vnLab.WebPortal.Services;

namespace vnLab.WebPortal.Controllers;

public class AccountController : Controller
{
    private readonly IUserApiClient _userApiClient;

    public AccountController(IUserApiClient userApiClient)
    {
        _userApiClient = userApiClient;
    }

    public IActionResult SignIn()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "oidc");
    }

    public new IActionResult SignOut()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/" }, "Cookies", "oidc");
    }

    [Authorize]
    [Route("my-profile")]
    public async Task<ActionResult> MyProfile()
    {
        var user = await _userApiClient.GetById(User.GetUserId());
        return View(user);
    }

    [Route("profile")]
    public async Task<ActionResult> Profile(string id)
    {
        var user = await _userApiClient.GetById(id);
        ViewBag.User = user;
        return View();
    }

    [Route("search")]
    [Authorize]
    public async Task<ActionResult> Search(string keyword)
    {
        var users = await _userApiClient.GetPagination(keyword);
        return new JsonResult(users);
    }
}