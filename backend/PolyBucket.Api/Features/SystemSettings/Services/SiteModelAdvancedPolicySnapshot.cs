namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed record SiteModelAdvancedPolicySnapshot(
    bool EnableModelImport,
    bool EnableModelAnalytics,
    bool EnableModelReporting,
    bool RequireReportDetails,
    bool EnableModelRejection,
    bool RequireRejectionReason,
    bool EnableModelDeletion,
    bool EnableModelSharing)
{
    public static SiteModelAdvancedPolicySnapshot FailClosed { get; } = new(
        EnableModelImport: false,
        EnableModelAnalytics: false,
        EnableModelReporting: false,
        RequireReportDetails: true,
        EnableModelRejection: false,
        RequireRejectionReason: true,
        EnableModelDeletion: false,
        EnableModelSharing: false);
}