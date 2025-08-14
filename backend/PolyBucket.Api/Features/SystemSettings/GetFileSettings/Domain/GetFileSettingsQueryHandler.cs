using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain
{
    public class GetFileSettingsQueryHandler(
        PolyBucketDbContext context,
        ILogger<GetFileSettingsQueryHandler> logger) : IRequestHandler<GetFileSettingsQuery, GetFileSettingsResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<GetFileSettingsQueryHandler> _logger = logger;

        public async Task<GetFileSettingsResponse> Handle(GetFileSettingsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var fileTypes = await _context.FileTypeSettings
                    .Where(ft => !ft.IsDeleted)
                    .OrderBy(ft => ft.Category)
                    .ThenBy(ft => ft.Priority)
                    .ThenBy(ft => ft.FileExtension)
                    .Select(ft => new FileTypeSettingsData
                    {
                        Id = ft.Id,
                        FileExtension = ft.FileExtension,
                        Enabled = ft.Enabled,
                        MaxFileSizeBytes = ft.MaxFileSizeBytes,
                        MaxPerUpload = ft.MaxPerUpload,
                        DisplayName = ft.DisplayName,
                        Description = ft.Description,
                        MimeType = ft.MimeType,
                        RequiresPreview = ft.RequiresPreview,
                        IsCompressible = ft.IsCompressible,
                        Category = ft.Category,
                        Priority = ft.Priority,
                        IsDefault = ft.IsDefault
                    })
                    .ToListAsync(cancellationToken);

                return new GetFileSettingsResponse
                {
                    Success = true,
                    Message = "File type settings retrieved successfully",
                    FileTypes = fileTypes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve file type settings");
                return new GetFileSettingsResponse
                {
                    Success = false,
                    Message = "Failed to retrieve file type settings. Please try again."
                };
            }
        }
    }
}
