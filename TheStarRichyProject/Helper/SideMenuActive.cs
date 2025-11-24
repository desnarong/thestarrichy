using Microsoft.AspNetCore.Mvc.Rendering;

namespace TheStarRichyProject.Helper
{
    public static class SideMenuActive
    {
        public static string IsActive(this IHtmlHelper htmlHelper, string controller, string action)
        {
            var routeData = new RouteData();
            routeData = htmlHelper.ViewContext.RouteData;

            var routeAction = "";
            var routeController = "";

            var tmpAction = routeData.Values["action"];
            if (tmpAction != null)
            {
                routeAction = tmpAction.ToString();
            }

            var tmpController = routeData.Values["controller"];
            if (tmpController != null)
            {
                routeController = tmpController.ToString();
            }

            bool returnActive = false;
            returnActive = (controller == routeController && (action == routeAction));
            return returnActive ? "active" : "";
        }
    }
}
