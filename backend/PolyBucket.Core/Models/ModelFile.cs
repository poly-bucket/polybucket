using System;

namespace Core.Models
{
    public class ModelFile
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // e.g., "STL", "OBJ", "3MF", "GCODE"
        public long FileSize { get; set; }
        public string StoragePath { get; set; }
        public string StorageProvider { get; set; } // e.g., "MinIO", "S3", "Local"

        // Navigation property
        public virtual Model3D Model { get; set; }
    }
}