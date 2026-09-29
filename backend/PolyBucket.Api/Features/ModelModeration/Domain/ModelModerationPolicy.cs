namespace PolyBucket.Api.Features.ModelModeration.Domain;

public class ModelModerationPolicy : IModelModerationPolicy
{
    public bool IsModerationRequired(ModelModerationSettingsSnapshot settings)
    {
        return settings.RequireModeration
            && settings.RequireUploadModeration
            && settings.RequireModeratorApproval;
    }

    public bool ShouldAutoApprove(ModelModerationSettingsSnapshot settings, bool authorEmailVerified)
    {
        if (!IsModerationRequired(settings))
        {
            return true;
        }

        if (settings.AutoApproveModels)
        {
            return true;
        }

        return settings.AutoApproveVerifiedUsers && authorEmailVerified;
    }
}
