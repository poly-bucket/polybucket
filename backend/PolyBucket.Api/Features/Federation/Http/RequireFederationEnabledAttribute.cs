using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Features.Federation.Http;

public sealed class RequireFederationEnabledAttribute : TypeFilterAttribute
{
    public RequireFederationEnabledAttribute(FederationGateMode mode)
        : base(typeof(RequireFederationEnabledFilter))
    {
        Arguments = [mode];
    }
}