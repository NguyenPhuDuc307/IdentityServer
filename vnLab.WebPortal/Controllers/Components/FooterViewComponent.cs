using Microsoft.AspNetCore.Mvc;

namespace vnLab.WebPortal.Controllers.Components;

public class FooterViewComponent : ViewComponent
{
    public FooterViewComponent()
    {
    }

    public IViewComponentResult Invoke()
    {
        return View("Default", new { });
    }
}