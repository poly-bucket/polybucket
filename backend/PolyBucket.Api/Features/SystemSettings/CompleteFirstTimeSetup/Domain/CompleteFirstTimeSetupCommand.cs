using MediatR;

namespace PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Domain
{
    public class CompleteFirstTimeSetupCommand : IRequest<CompleteFirstTimeSetupResponse>
    {
    }

    public class CompleteFirstTimeSetupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
    }
} 