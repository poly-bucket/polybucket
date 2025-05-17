using Core.Entities;
using Core.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Files
{
    public class File : Auditable
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// File extension (maps to allowed upload types)
        /// </summary>
        public FileExtension Extension { get; set; }

        /// <summary>
        /// The purpose of this file in relation to the model
        /// </summary>
        public FileType FileType { get; set; }

        /// <summary>
        /// MIME type of the file
        /// </summary>
        [MaxLength(100)]
        public string MimeType { get; set; }

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Has the file been virus scanned?
        /// </summary>
        [MaxLength(1)]
        public bool IsScanned { get; set; } // For virus scanning status

        /// <summary>
        /// Whether the file passed virus scan
        /// </summary>
        public bool IsSafe { get; set; }

        public bool IsPrimary { get; set; } // Is this the main file for the model?

        /// <summary>
        /// Hash of the file content for integrity checking
        /// </summary>
        [MaxLength(128)]
        public string Hash { get; set; } // For integrity checking

        public string StorageProvider { get; set; } // e.g., "MinIO", "S3", "Local"

        [Required]
        [MaxLength(500)]
        public string StoragePath { get; set; }
    }
}