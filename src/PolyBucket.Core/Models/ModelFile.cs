using System;

namespace PolyBucket.Core.Models
{
    public class ModelFile
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; } // e.g., "STL", "OBJ", "3MF", "GCODE"
        public long FileSize { get; set; }
        public string StoragePath { get; set; }
        public string StorageProvider { get; set; } // e.g., "MinIO", "S3", "Local"
        public DateTime UploadedAt { get; set; }
        public string FileHash { get; set; } // For integrity checking
        public bool IsScanned { get; set; } // For virus scanning status
        public bool IsPrimary { get; set; } // Is this the main file for the model?
        
        // Navigation property
        public virtual Model3D Model { get; set; }
    }
} 