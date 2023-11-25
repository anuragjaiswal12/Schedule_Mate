using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Schedule_Mate.Filters
{
    public class LoginFilter : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // FilterContext filterContext=new
            bool isAuthenticatedUser = context.HttpContext.Session.GetString("User") != null;
            if (!isAuthenticatedUser)
                context.Result = new RedirectToRouteResult(
            new RouteValueDictionary {
                { "Controller", "User" },
                { "Action", "Login" }
            });
            // context.Result = new RedirectResult("/User/Login");
            //context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Login", controller = "User" }));
            // throw new NotImplementedException();
        }
    }
}