using System.Collections.Generic;

namespace PolyBucket.Core.Models
{
    /// <summary>
    /// Configuration for file uploads including allowed extensions and types
    /// </summary>
    public class FileUploadConfiguration
    {
        /// <summary>
        /// Maximum file size in bytes
        /// </summary>
        public long MaxFileSizeBytes { get; set; }
        
        /// <summary>
        /// All supported file extensions
        /// </summary>
        public List<FileExtensionInfo> SupportedExtensions { get; set; }
        
        /// <summary>
        /// All file types
        /// </summary>
        public List<FileTypeInfo> FileTypes { get; set; }
        
        /// <summary>
        /// Dictionary mapping file types to allowed extensions
        /// </summary>
        public Dictionary<string, List<string>> ExtensionsByType { get; set; }
    }
    
    /// <summary>
    /// Information about a file extension
    /// </summary>
    public class FileExtensionInfo
    {
        /// <summary>
        /// Numeric ID (enum value)
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the extension
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The extension string with dot prefix (e.g., ".stl")
        /// </summary>
        public string Extension { get; set; }
        
        /// <summary>
        /// MIME type for the extension
        /// </summary>
        public string MimeType { get; set; }
        
        /// <summary>
        /// Category of the file (e.g., "3D Model", "Image")
        /// </summary>
        public string Category { get; set; }
    }
    
    /// <summary>
    /// Information about a file type
    /// </summary>
    public class FileTypeInfo
    {
        /// <summary>
        /// Numeric ID (enum value)
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the file type
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the file type's purpose
        /// </summary>
        public string Description { get; set; }
    }
} 