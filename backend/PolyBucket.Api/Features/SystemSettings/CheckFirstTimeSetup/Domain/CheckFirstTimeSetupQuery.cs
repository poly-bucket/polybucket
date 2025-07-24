using MediatR;

namespace PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Domain
{
    public class CheckFirstTimeSetupQuery : IRequest<CheckFirstTimeSetupResponse>
    {
    }

    public class CheckFirstTimeSetupResponse
    {
        public bool IsFirstTimeSetup { get; set; }
        public bool IsAdminConfigured { get; set; }
        public bool IsSiteConfigured { get; set; }
        public bool IsEmailConfigured { get; set; }
        public bool IsModerationConfigured { get; set; }
        public string CurrentStep { get; set; } = string.Empty;
        public int TotalSteps { get; set; } = 4;
        public int CompletedSteps { get; set; } = 0;
    }
} 