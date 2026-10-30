using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.InstallPlugin.Repository;

namespace PolyBucket.Api.Features.Plugins.InstallPlugin.Domain;

public class InstallPluginService(IInstallPluginRepository repository) : IInstallPluginService
{
    private readonly IInstallPluginRepository _repository = repository;

    public async Task<InstallPluginOutcome> InstallAsync(PluginInstallationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Source))
        {
            return new InstallPluginOutcome
            {
                HttpStatus = InstallPluginHttpStatus.BadRequest,
                ErrorBody = new { message = "Installation source is required" }
            };
        }

        PluginInstallationResult result;

        switch (request.Source.ToLowerInvariant())
        {
            case "url":
                if (string.IsNullOrWhiteSpace(request.Url))
                {
                    return new InstallPluginOutcome
                    {
                        HttpStatus = InstallPluginHttpStatus.BadRequest,
                        ErrorBody = new { message = "URL is required for URL installation" }
                    };
                }

                result = await _repository.InstallFromUrlAsync(request.Url, cancellationToken);
                break;

            case "github":
                if (string.IsNullOrWhiteSpace(request.Url))
                {
                    return new InstallPluginOutcome
                    {
                        HttpStatus = InstallPluginHttpStatus.BadRequest,
                        ErrorBody = new { message = "Repository URL is required for GitHub installation" }
                    };
                }

                result = await _repository.InstallFromGitHubAsync(request.Url, request.Branch, cancellationToken);
                break;

            case "marketplace":
                if (string.IsNullOrWhiteSpace(request.PluginId))
                {
                    return new InstallPluginOutcome
                    {
                        HttpStatus = InstallPluginHttpStatus.BadRequest,
                        ErrorBody = new { message = "Plugin ID is required for marketplace installation" }
                    };
                }

                var listing = await _repository.GetMarketplacePluginDetailsAsync(request.PluginId, cancellationToken);
                if (listing == null)
                {
                    return new InstallPluginOutcome
                    {
                        HttpStatus = InstallPluginHttpStatus.NotFound,
                        ErrorBody = new { message = $"Plugin {request.PluginId} not found in marketplace" }
                    };
                }

                result = await _repository.InstallFromMarketplaceAsync(listing, cancellationToken);
                break;

            default:
                return new InstallPluginOutcome
                {
                    HttpStatus = InstallPluginHttpStatus.BadRequest,
                    ErrorBody = new { message = "Invalid installation source. Must be 'url', 'github', or 'marketplace'" }
                };
        }

        if (result.Success)
        {
            return new InstallPluginOutcome { HttpStatus = InstallPluginHttpStatus.Ok, Result = result };
        }

        return new InstallPluginOutcome { HttpStatus = InstallPluginHttpStatus.BadRequest, Result = result };
    }
}
