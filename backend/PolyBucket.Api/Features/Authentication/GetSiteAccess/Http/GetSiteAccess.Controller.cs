using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Http;

[ApiController]
[Route("api/auth")]
public class GetSiteAccessController(IGetSiteAccessService getSiteAccessService) : ControllerBase
{
    /// <summary>
    /// Returns public site access flags for browsing, model uploads, and downloads.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Public browsing, upload policy flags, and model metadata validation rules for the upload UI</returns>
    [HttpGet("site-access")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetSiteAccessResponse), 200)]
    public async Task<IActionResult> GetSiteAccess(CancellationToken cancellationToken)
    {
        var result = await getSiteAccessService.GetSiteAccessAsync(cancellationToken);
        var rules = result.ModelMetadataRules;
        return Ok(new
        {
            allowPublicBrowsing = result.AllowPublicBrowsing,
            requireLoginForUpload = result.RequireLoginForUpload,
            allowAnonymousModelUpload = result.AllowAnonymousModelUpload,
            allowAnonymousModelDownload = result.AllowAnonymousModelDownload,
            modelDownloadsEnabled = result.ModelDownloadsEnabled,
            defaultModelPrivacy = result.DefaultModelPrivacy,
            modelMetadataRules = new
            {
                requireModelDescription = rules.RequireModelDescription,
                minDescriptionLength = rules.MinDescriptionLength,
                maxDescriptionLength = rules.MaxDescriptionLength,
                requireLicenseSelection = rules.RequireLicenseSelection,
                allowCustomLicenses = rules.AllowCustomLicenses,
                allowNsfwContent = rules.AllowNsfwContent,
                allowAiGeneratedContent = rules.AllowAiGeneratedContent,
                requireThumbnail = rules.RequireThumbnail,
                maxTagsPerModel = rules.MaxTagsPerModel,
                requireModelTags = rules.RequireModelTags,
                maxCategoriesPerModel = rules.MaxCategoriesPerModel,
                requireCategorySelection = rules.RequireCategorySelection
            }
        });
    }
}