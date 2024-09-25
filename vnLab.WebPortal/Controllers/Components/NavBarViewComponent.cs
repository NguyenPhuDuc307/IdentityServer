using Microsoft.AspNetCore.Mvc;

namespace vnLab.WebPortal.Controllers.Components;

public class NavBarViewComponent : ViewComponent
{
    public NavBarViewComponent()
    {
    }

    public IViewComponentResult Invoke()
    {
        return View("Default", new { });
    }
}