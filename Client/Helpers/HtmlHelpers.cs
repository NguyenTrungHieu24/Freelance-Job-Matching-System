using Microsoft.AspNetCore.Mvc.Rendering;

namespace Client.Helpers
{
    public static class HtmlHelpers
    {
        public static string IsActive(this IHtmlHelper html,
            string controller,
            string action = null)
        {
            var routeData = html.ViewContext.RouteData;

            var currentController = routeData.Values["Controller"]?.ToString();
            var currentAction = routeData.Values["Action"]?.ToString();

            if (currentController == controller &&
                (action == null || currentAction == action))
            {
                return "is-active";
            }

            return "";
        }
    }
}
