using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskManagement.API.Attributes
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .Select(ms => new
                    {
                        Field = ms.Key,
                        Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    }).ToArray();

                var errorResponse = new
                {
                    Message = "Validation failed",
                    Errors = errors
                };

                context.Result = new BadRequestObjectResult(errorResponse);
            }
        }
    }
}
