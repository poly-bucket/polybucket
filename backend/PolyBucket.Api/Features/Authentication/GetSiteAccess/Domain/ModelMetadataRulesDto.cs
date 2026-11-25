namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

public class ModelMetadataRulesDto
{
    public bool RequireModelDescription { get; set; }

    public int MinDescriptionLength { get; set; }

    public int MaxDescriptionLength { get; set; }

    public bool RequireLicenseSelection { get; set; }

    public bool AllowCustomLicenses { get; set; }

    public bool AllowNsfwContent { get; set; }

    public bool AllowAiGeneratedContent { get; set; }

    public bool RequireThumbnail { get; set; }

    public int MaxTagsPerModel { get; set; }

    public bool RequireModelTags { get; set; }

    public int MaxCategoriesPerModel { get; set; }

    public bool RequireCategorySelection { get; set; }
}