using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.Authentication.Services;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireVerifiedEmailAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public const string ErrorCode = "email_unverified";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var gate = context.HttpContext.RequestServices.GetService<IEmailVerificationGate>();
            if (gate == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            if (await gate.IsSatisfiedAsync(context.HttpContext.User, context.HttpContext.RequestAborted))
            {
                return;
            }

            context.Result = CreateForbiddenResult();
        }

        public static ObjectResult CreateForbiddenResult()
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Email verification required",
                Detail = "Verify your email address to continue. You can request a new verification link from your account."
            };
            problem.Extensions["code"] = ErrorCode;
            return new ObjectResult(problem) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}
