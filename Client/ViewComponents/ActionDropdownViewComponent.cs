using Client.Models.Components;
using Microsoft.AspNetCore.Mvc;

namespace Client.ViewComponents
{
    public class ActionDropdownViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
            List<ActionItemModel> actions,
            string buttonText = "...",
            string buttonClass = "btn btn-outline-secondary btn-sm")
        {
            ViewBag.ButtonText = buttonText;
            ViewBag.ButtonClass = buttonClass;

            return View(actions.Where(x => x.Show).ToList());
        }
    }
}
