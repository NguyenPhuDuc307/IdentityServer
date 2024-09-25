using Microsoft.AspNetCore.Mvc;
using vnLab.WebPortal.Services;

namespace vnLab.WebPortal.Controllers.Components;

public class SearchViewComponent : ViewComponent
{
    private readonly IUserApiClient _userApiClient;
    public SearchViewComponent(IUserApiClient userApiClient)
    {
        _userApiClient = userApiClient;
    }

    public IViewComponentResult Invoke()
    {
        return View("Default", new { });
    }
}