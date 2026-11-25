namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed record ModelMetadataPolicySnapshot(
    bool RequireModelDescription,
    int MinDescriptionLength,
    int MaxDescriptionLength,
    bool RequireLicenseSelection,
    bool AllowCustomLicenses,
    bool AllowNsfwContent,
    bool AllowAiGeneratedContent,
    bool RequireThumbnail,
    int MaxTagsPerModel,
    bool RequireModelTags,
    int MaxCategoriesPerModel,
    bool RequireCategorySelection,
    bool AllowModelRemixing,
    bool RequireRemixAttribution)
{
    public static ModelMetadataPolicySnapshot FailClosed { get; } = new(
        RequireModelDescription: true,
        MinDescriptionLength: 10,
        MaxDescriptionLength: 2000,
        RequireLicenseSelection: true,
        AllowCustomLicenses: false,
        AllowNsfwContent: false,
        AllowAiGeneratedContent: true,
        RequireThumbnail: false,
        MaxTagsPerModel: 10,
        RequireModelTags: false,
        MaxCategoriesPerModel: 3,
        RequireCategorySelection: false,
        AllowModelRemixing: false,
        RequireRemixAttribution: true);
}