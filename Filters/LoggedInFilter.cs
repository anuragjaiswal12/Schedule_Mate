using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Schedule_Mate.Filters
{
    class LoggedInFilter : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            bool isAuthenticatedUser = context.HttpContext.Session.GetString("User") != null;
            if (isAuthenticatedUser)
                context.Result = new RedirectToRouteResult(
            new RouteValueDictionary {
                { "Controller", "User" },
                { "Action", "Index" }
            });
            // throw new NotImplementedException();
        }
    }
}