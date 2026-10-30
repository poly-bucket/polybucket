namespace PolyBucket.Api.Features.Files.GetFileConfig.Http;

        public class FileConfigResponse
        {
            public string StorageProvider { get; set; } = string.Empty;
            public long MaxTotalFileSize { get; set; }
            public int MaxFilesPerUpload { get; set; }
            public List<string> SupportedExtensions { get; set; } = new();
            public Dictionary<string, List<FileTypeConfig>> FileTypes { get; set; } = new();
            public GlobalFileSettings GlobalSettings { get; set; } = new();
        }

        public class FileTypeConfig
        {
            public string Extension { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string MimeType { get; set; } = string.Empty;
            public long MaxFileSizeBytes { get; set; }
            public int MaxPerUpload { get; set; }
            public bool RequiresPreview { get; set; }
            public bool IsCompressible { get; set; }
            public string Category { get; set; } = string.Empty;
            public int Priority { get; set; }
        }

        public class GlobalFileSettings
        {
            public bool AllowMultipleFiles { get; set; } = true;
            public bool RequirePreviewFor3D { get; set; } = true;
            public bool AutoCompressLargeFiles { get; set; } = true;
            public int MaxConcurrentUploads { get; set; } = 5;
        }
