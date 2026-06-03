using Client.Models.Components;
using Microsoft.AspNetCore.Mvc;

namespace Client.ViewComponents
{
    public class ActionDropdownViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
            List<ActionItemModel> actions,
            string buttonText = "Thao tác",
            string buttonClass = "btn btn-secondary btn-sm")
        {
            ViewBag.ButtonText = buttonText;
            ViewBag.ButtonClass = buttonClass;

            return View(actions.Where(x => x.Show).ToList());
        }
    }
}
