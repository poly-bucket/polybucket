using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Features.Federation.Services;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Federation.Http;

public sealed class RequireFederationEnabledFilter(
    FederationGateMode mode,
    ISiteFederationPolicy federationPolicy,
    IFederationInstanceIdentity instanceIdentity) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (mode == FederationGateMode.AdminRead)
        {
            return;
        }

        var policy = await federationPolicy.GetSnapshotAsync(context.HttpContext.RequestAborted);
        if (!policy.IsEnabled)
        {
            context.Result = Forbidden("Federation is disabled on this site.");
            return;
        }

        if (mode == FederationGateMode.Peer)
        {
            var identity = await instanceIdentity.GetSnapshotAsync(context.HttpContext.RequestAborted);
            if (!identity.IsReadyForPeering)
            {
                context.Result = ServiceUnavailable(
                    "Federation peer access requires a configured instance name and a public base URL (Admin → Email public site URL, or Instance:BaseUrl).");
            }
        }
    }

    private static ObjectResult Forbidden(string detail) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Federation disabled",
            Detail = detail
        })
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

    private static ObjectResult ServiceUnavailable(string detail) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "Federation identity incomplete",
            Detail = detail
        })
        {
            StatusCode = StatusCodes.Status503ServiceUnavailable
        };
}