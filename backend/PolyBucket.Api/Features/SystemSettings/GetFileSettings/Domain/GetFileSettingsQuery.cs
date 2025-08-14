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
    }
}
