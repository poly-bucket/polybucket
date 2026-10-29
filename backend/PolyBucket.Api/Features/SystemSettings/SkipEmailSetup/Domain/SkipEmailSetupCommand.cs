using MediatR;

namespace PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Domain;

public class SkipEmailSetupCommand : IRequest<SkipEmailSetupResponse>
{
}

public class SkipEmailSetupResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
