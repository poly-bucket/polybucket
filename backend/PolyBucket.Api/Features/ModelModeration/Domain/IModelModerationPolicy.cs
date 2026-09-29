namespace PolyBucket.Api.Features.ModelModeration.Domain;

public interface IModelModerationPolicy
{
    bool IsModerationRequired(ModelModerationSettingsSnapshot settings);

    bool ShouldAutoApprove(ModelModerationSettingsSnapshot settings, bool authorEmailVerified);
}
