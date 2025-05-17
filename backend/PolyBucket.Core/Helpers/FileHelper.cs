using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Enumerations;

namespace Core.Helpers
{
    /// <summary>
    /// Helper methods for working with files
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Dictionary mapping FileExtension enum values to their string representation
        /// </summary>
        private static readonly Dictionary<FileExtension, string> ExtensionStrings = new Dictionary<FileExtension, string>
        {
            // 3D Model formats
            { FileExtension.Stl, ".stl" },
            { FileExtension.Obj, ".obj" },
            { FileExtension.Fbx, ".fbx" },
            { FileExtension.Gltf, ".gltf" },
            { FileExtension.Glb, ".glb" },
            { FileExtension.ThreeMf, ".3mf" },
            { FileExtension.Ply, ".ply" },
            { FileExtension.Step, ".step" },
            { FileExtension.Stp, ".stp" },
            { FileExtension.Iges, ".iges" },
            { FileExtension.Igs, ".igs" },
            { FileExtension.Brep, ".brep" },
            { FileExtension.Gcode, ".gcode" },
            { FileExtension.Scad, ".scad" },
            { FileExtension.Amf, ".amf" },
            { FileExtension.Blend, ".blend" },
            { FileExtension.Max, ".max" },
            { FileExtension.Dae, ".dae" },
            
            // Image formats
            { FileExtension.Jpg, ".jpg" },
            { FileExtension.Jpeg, ".jpeg" },
            { FileExtension.Png, ".png" },
            { FileExtension.Gif, ".gif" },
            { FileExtension.Bmp, ".bmp" },
            { FileExtension.Tiff, ".tiff" },
            { FileExtension.Webp, ".webp" },
            { FileExtension.Tga, ".tga" },
            { FileExtension.Hdr, ".hdr" },
            { FileExtension.Exr, ".exr" },
            
            // Vector formats
            { FileExtension.Ai, ".ai" },
            { FileExtension.Eps, ".eps" },
            { FileExtension.Pdf, ".pdf" },
            { FileExtension.Svg, ".svg" },
            { FileExtension.Dxf, ".dxf" },
            
            // Document formats
            { FileExtension.Doc, ".doc" },
            { FileExtension.Docx, ".docx" },
            { FileExtension.Txt, ".txt" },
            { FileExtension.Rtf, ".rtf" },
            { FileExtension.Md, ".md" },
            
            // Archive formats
            { FileExtension.Zip, ".zip" },
            { FileExtension.Rar, ".rar" },
            { FileExtension.SevenZ, ".7z" },
            { FileExtension.Tar, ".tar" },
            { FileExtension.TarGz, ".tar.gz" },
            
            // Other formats
            { FileExtension.Json, ".json" },
            { FileExtension.Xml, ".xml" },
            { FileExtension.Yaml, ".yaml" },
            { FileExtension.Csv, ".csv" }
        };
        
        /// <summary>
        /// Dictionary mapping FileExtension enum values to their MIME types
        /// </summary>
        private static readonly Dictionary<FileExtension, string> MimeTypes = new Dictionary<FileExtension, string>
        {
            // 3D Model formats
            { FileExtension.Stl, "application/vnd.ms-pki.stl" },
            { FileExtension.Obj, "application/x-tgif" },
            { FileExtension.Fbx, "application/octet-stream" },
            { FileExtension.Gltf, "model/gltf+json" },
            { FileExtension.Glb, "model/gltf-binary" },
            { FileExtension.ThreeMf, "application/vnd.ms-package.3dmanufacturing-3dmodel+xml" },
            { FileExtension.Ply, "application/ply" },
            { FileExtension.Step, "application/step" },
            { FileExtension.Stp, "application/step" },
            { FileExtension.Iges, "application/iges" },
            { FileExtension.Igs, "application/iges" },
            { FileExtension.Brep, "application/octet-stream" },
            { FileExtension.Gcode, "text/plain" },
            { FileExtension.Scad, "text/plain" },
            { FileExtension.Amf, "application/amf+xml" },
            { FileExtension.Blend, "application/octet-stream" },
            { FileExtension.Max, "application/octet-stream" },
            { FileExtension.Dae, "model/vnd.collada+xml" },
            
            // Image formats
            { FileExtension.Jpg, "image/jpeg" },
            { FileExtension.Jpeg, "image/jpeg" },
            { FileExtension.Png, "image/png" },
            { FileExtension.Gif, "image/gif" },
            { FileExtension.Bmp, "image/bmp" },
            { FileExtension.Tiff, "image/tiff" },
            { FileExtension.Webp, "image/webp" },
            { FileExtension.Tga, "image/x-tga" },
            { FileExtension.Hdr, "image/vnd.radiance" },
            { FileExtension.Exr, "image/x-exr" },
            
            // Vector formats
            { FileExtension.Ai, "application/postscript" },
            { FileExtension.Eps, "application/postscript" },
            { FileExtension.Pdf, "application/pdf" },
            { FileExtension.Svg, "image/svg+xml" },
            { FileExtension.Dxf, "application/dxf" },
            
            // Document formats
            { FileExtension.Doc, "application/msword" },
            { FileExtension.Docx, "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { FileExtension.Txt, "text/plain" },
            { FileExtension.Rtf, "application/rtf" },
            { FileExtension.Md, "text/markdown" },
            
            // Archive formats
            { FileExtension.Zip, "application/zip" },
            { FileExtension.Rar, "application/vnd.rar" },
            { FileExtension.SevenZ, "application/x-7z-compressed" },
            { FileExtension.Tar, "application/x-tar" },
            { FileExtension.TarGz, "application/gzip" },
            
            // Other formats
            { FileExtension.Json, "application/json" },
            { FileExtension.Xml, "application/xml" },
            { FileExtension.Yaml, "application/yaml" },
            { FileExtension.Csv, "text/csv" }
        };
        
        /// <summary>
        /// Gets all supported file extensions as string values
        /// </summary>
        /// <returns>A list of supported file extension strings</returns>
        public static List<string> GetSupportedExtensions()
        {
            return ExtensionStrings.Values.ToList();
        }
        
        /// <summary>
        /// Gets all supported file extensions for a specific file type
        /// </summary>
        /// <param name="fileType">The type of file</param>
        /// <returns>A list of supported file extension strings for the specified file type</returns>
        public static List<string> GetSupportedExtensionsForType(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.MainModel:
                case FileType.AlternativeFormat:
                    return ExtensionStrings
                        .Where(x => x.Key >= FileExtension.Stl && x.Key <= FileExtension.Dae)
                        .Select(x => x.Value)
                        .ToList();
                case FileType.Thumbnail:
                case FileType.Render:
                    return ExtensionStrings
                        .Where(x => x.Key >= FileExtension.Jpg && x.Key <= FileExtension.Exr)
                        .Select(x => x.Value)
                        .ToList();
                case FileType.Documentation:
                    return ExtensionStrings
                        .Where(x => x.Key >= FileExtension.Doc && x.Key <= FileExtension.Md ||
                                   x.Key == FileExtension.Pdf)
                        .Select(x => x.Value)
                        .ToList();
                default:
                    return GetSupportedExtensions();
            }
        }
        
        /// <summary>
        /// Gets the string representation of a file extension
        /// </summary>
        /// <param name="extension">The FileExtension enum value</param>
        /// <returns>The string representation of the extension</returns>
        public static string GetExtensionString(FileExtension extension)
        {
            return ExtensionStrings.TryGetValue(extension, out var extensionString) 
                ? extensionString 
                : string.Empty;
        }
        
        /// <summary>
        /// Gets the file extension from a file path
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <returns>The FileExtension enum value for the file</returns>
        public static FileExtension GetExtensionFromPath(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            
            if (string.IsNullOrEmpty(extension))
                return FileExtension.None;
            
            return ExtensionStrings
                .FirstOrDefault(x => string.Equals(x.Value, extension, StringComparison.OrdinalIgnoreCase))
                .Key;
        }
        
        /// <summary>
        /// Gets the MIME type for a file extension
        /// </summary>
        /// <param name="extension">The FileExtension enum value</param>
        /// <returns>The MIME type for the extension</returns>
        public static string GetMimeType(FileExtension extension)
        {
            return MimeTypes.TryGetValue(extension, out var mimeType)
                ? mimeType
                : "application/octet-stream"; // Default MIME type for unknown extensions
        }
        
        /// <summary>
        /// Determines if an extension is supported for upload
        /// </summary>
        /// <param name="extension">The extension to check</param>
        /// <returns>True if the extension is supported</returns>
        public static bool IsExtensionSupported(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;
                
            // Ensure the extension starts with a dot
            if (!extension.StartsWith("."))
                extension = "." + extension;
                
            return ExtensionStrings.Values.Any(x => 
                string.Equals(x, extension, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Generates a unique filename for storage
        /// </summary>
        /// <param name="originalFileName">The original file name</param>
        /// <returns>A unique filename for storage</returns>
        public static string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            return $"{timestamp}_{uniqueId}{extension}";
        }
        
        /// <summary>
        /// Suggests appropriate FileType based on file extension
        /// </summary>
        /// <param name="extension">The FileExtension enum value</param>
        /// <returns>A suggested FileType for the extension</returns>
        public static FileType SuggestFileType(FileExtension extension)
        {
            if (extension >= FileExtension.Stl && extension <= FileExtension.Dae)
                return FileType.MainModel;
                
            if (extension >= FileExtension.Jpg && extension <= FileExtension.Exr)
                return FileType.Thumbnail;
                
            if (extension >= FileExtension.Doc && extension <= FileExtension.Md)
                return FileType.Documentation;
                
            if (extension == FileExtension.Gcode)
                return FileType.MachineCode;
                
            return FileType.Other;
        }
    }
} 