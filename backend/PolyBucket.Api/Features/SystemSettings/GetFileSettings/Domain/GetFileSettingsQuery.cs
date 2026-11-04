using MediatR;

namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain
{
    public class GetFileSettingsQuery : IRequest<GetFileSettingsResponse>
    {
    }

    public class GetFileSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<FileTypeSettingsData>? FileTypes { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public List<string> Categories { get; set; } = new();
        public string SiteAllowedFileTypes { get; set; } = string.Empty;
        public string DerivedAllowedFileTypes { get; set; } = string.Empty;
        public bool AllowedFileTypesInSync { get; set; }
    }

    public class FileTypeSettingsData
    {
        public Guid Id { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public long MaxFileSizeBytes { get; set; }
        public int MaxPerUpload { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public bool RequiresPreview { get; set; }
        public bool IsCompressible { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsDefault { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string CreatedByUsername { get; set; } = string.Empty;
        public string CreatedByEmail { get; set; } = string.Empty;
        public string UpdatedByUsername { get; set; } = string.Empty;
    }
}
