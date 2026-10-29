using MediatR;

namespace PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Domain;

public class ApplyStorageSetupCommand : IRequest<ApplyStorageSetupResponse>
{
}

public class ApplyStorageSetupResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool RestartScheduled { get; set; }
}
