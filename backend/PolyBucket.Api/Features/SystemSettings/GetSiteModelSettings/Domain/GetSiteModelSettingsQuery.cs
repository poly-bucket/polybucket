using MediatR;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain
{
    public class GetSiteModelSettingsQuery : IRequest<GetSiteModelSettingsResponse>
    {
    }

    public class GetSiteModelSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SiteModelSettingsData? Settings { get; set; }
    }

    public class SiteModelSettingsData
    {
        // File Upload Settings
        public long MaxFileSizeBytes { get; set; } = 100L * 1024 * 1024; // 100MB
        public string AllowedFileTypes { get; set; } = ".stl,.obj,.fbx,.3ds,.glb,.gltf,.ply,.step,.stp,.iges,.igs,.brep,.png,.jpg,.jpeg,.gif";
        public int MaxFilesPerUpload { get; set; } = 5;
        public bool EnableFileCompression { get; set; } = true;
        public bool AutoGeneratePreviews { get; set; } = true;
        
        // Default Model Settings
        public string DefaultModelPrivacy { get; set; } = "Public";
        public bool AutoApproveModels { get; set; } = false;
        public bool RequireModeration { get; set; } = true;
        
        // Upload Behavior
        public bool RequireLoginForUpload { get; set; } = true;
        public bool AllowPublicBrowsing { get; set; } = true;
    }
}
