namespace PolyBucket.Api.Features.ModelModeration.Domain;

public class ModelModerationSettingsSnapshot
{
    public bool RequireModeration { get; set; } = true;
    public bool AutoApproveModels { get; set; }
    public bool RequireUploadModeration { get; set; } = true;
    public bool RequireModeratorApproval { get; set; } = true;
    public bool AutoApproveVerifiedUsers { get; set; }
}
