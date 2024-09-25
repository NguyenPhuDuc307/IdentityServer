using Microsoft.AspNetCore.Mvc;

namespace vnLab.WebPortal.Controllers.Components;

public class SideBarViewComponent : ViewComponent
{
    public SideBarViewComponent()
    {
    }

    public IViewComponentResult Invoke()
    {
        return View("Default", new { });
    }
}