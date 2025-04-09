using System;
using System.ComponentModel.DataAnnotations;
using PolyBucket.Core.Enums;

namespace PolyBucket.Core.Entities
{
    /// <summary>
    /// Base class for file uploads in the system
    /// </summary>
    public class File
    {
        /// <summary>
        /// Unique identifier for the file
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Original filename as uploaded by the user
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Storage filename in the system
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string StoragePath { get; set; }

        /// <summary>
        /// File extension (maps to allowed upload types)
        /// </summary>
        public FileExtension Extension { get; set; }

        /// <summary>
        /// MIME type of the file
        /// </summary>
        [MaxLength(100)]
        public string MimeType { get; set; }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// The purpose of this file in relation to the model
        /// </summary>
        public FileType FileType { get; set; }

        /// <summary>
        /// Hash of the file content for integrity checking
        /// </summary>
        [MaxLength(128)]
        public string ContentHash { get; set; }

        /// <summary>
        /// Public URL for accessing the file
        /// </summary>
        [MaxLength(1000)]
        public string PublicUrl { get; set; }

        /// <summary>
        /// Whether the file has been virus scanned
        /// </summary>
        public bool IsScanned { get; set; }

        /// <summary>
        /// Whether the file passed virus scan
        /// </summary>
        public bool IsSafe { get; set; }

        /// <summary>
        /// Date and time when the file was uploaded
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time when the file was last modified
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// Model ID this file is associated with
        /// </summary>
        public Guid? ModelId { get; set; }

        /// <summary>
        /// Navigation property to the associated model
        /// </summary>
        public virtual Model Model { get; set; }

        /// <summary>
        /// User ID of the uploader
        /// </summary>
        public Guid? UploadedById { get; set; }

        /// <summary>
        /// Additional metadata stored as JSON
        /// </summary>
        public string Metadata { get; set; }
    }
} 