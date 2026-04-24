using LoanApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace LoanApp.Filters
{
    public sealed class AdminOnlyAttribute : TypeFilterAttribute
    {
        public AdminOnlyAttribute() : base(typeof(AdminOnlyFilter))
        {
        }
    }

    public sealed class AdminOnlyFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

            if (!currentUser.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
            }

            if (!currentUser.IsAdmin)
            {
                context.Result = new RedirectToActionResult("MyProfile", "Employees", null);
                return;
            }

            await next();
        }
    }
}