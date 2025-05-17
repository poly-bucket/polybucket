using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Core.Helpers;
using System;
using System.Threading.Tasks;
using System.Linq;
using Core.Enumerations;
using Core.Models.Files;

namespace PolyBucket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        /// <summary>
        /// Gets all supported file extensions
        /// </summary>
        /// <returns>A list of all supported file extensions</returns>
        [HttpGet("extensions")]
        public ActionResult<IEnumerable<string>> GetSupportedExtensions()
        {
            return Ok(FileHelper.GetSupportedExtensions());
        }
        
        /// <summary>
        /// Gets all supported file extensions for a specific file type
        /// </summary>
        /// <param name="fileType">The type of file</param>
        /// <returns>A list of supported file extensions for the specified file type</returns>
        [HttpGet("extensions/by-type/{fileType}")]
        public ActionResult<IEnumerable<string>> GetSupportedExtensionsForType(FileType fileType)
        {
            return Ok(FileHelper.GetSupportedExtensionsForType(fileType));
        }
        
        /// <summary>
        /// Gets file extension information including allowed file types and size limits
        /// </summary>
        /// <returns>File extension configuration</returns>
        [HttpGet("config")]
        public ActionResult<FileUploadConfiguration> GetFileConfig()
        {
            // Create enum values with their names for frontend
            var fileExtensions = Enum.GetValues(typeof(FileExtension))
                .Cast<FileExtension>()
                .Where(e => e != FileExtension.None)
                .Select(e => new FileExtensionInfo
                {
                    Id = (int)e,
                    Name = e.ToString(),
                    Extension = FileHelper.GetExtensionString(e),
                    MimeType = FileHelper.GetMimeType(e),
                    Category = GetCategoryForExtension(e)
                })
                .ToList();
                
            var fileTypes = Enum.GetValues(typeof(FileType))
                .Cast<FileType>()
                .Select(t => new FileTypeInfo
                {
                    Id = (int)t,
                    Name = t.ToString(),
                    Description = GetDescriptionForFileType(t)
                })
                .ToList();
                
            // Return complete configuration
            return Ok(new FileUploadConfiguration
            {
                MaxFileSizeBytes = 1_073_741_824, // 1GB default
                SupportedExtensions = fileExtensions,
                FileTypes = fileTypes,
                ExtensionsByType = EnumerateExtensionsByType()
            });
        }
        
        /// <summary>
        /// Gets a list of which extensions are allowed for each file type
        /// </summary>
        private Dictionary<string, List<string>> EnumerateExtensionsByType()
        {
            var result = new Dictionary<string, List<string>>();
            
            foreach (FileType fileType in Enum.GetValues(typeof(FileType)))
            {
                result[fileType.ToString()] = FileHelper.GetSupportedExtensionsForType(fileType);
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets the category for a file extension
        /// </summary>
        private string GetCategoryForExtension(FileExtension extension)
        {
            if (extension >= FileExtension.Stl && extension <= FileExtension.Dae)
                return "3D Model";
                
            if (extension >= FileExtension.Jpg && extension <= FileExtension.Exr)
                return "Image";
                
            if (extension >= FileExtension.Ai && extension <= FileExtension.Dxf)
                return "Vector";
                
            if (extension >= FileExtension.Doc && extension <= FileExtension.Md)
                return "Document";
                
            if (extension >= FileExtension.Zip && extension <= FileExtension.TarGz)
                return "Archive";
                
            return "Other";
        }
        
        /// <summary>
        /// Gets a description for a file type
        /// </summary>
        private string GetDescriptionForFileType(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.MainModel:
                    return "Primary 3D model file";
                case FileType.Thumbnail:
                    return "Image used as a thumbnail";
                case FileType.Render:
                    return "High-quality rendered image of the model";
                case FileType.SourceFile:
                    return "Original source file (e.g., Blender, Maya)";
                case FileType.Documentation:
                    return "Documentation, instructions, or guides";
                case FileType.PrintSettings:
                    return "Slicer settings or printer profiles";
                case FileType.Material:
                    return "Material definitions or textures";
                case FileType.License:
                    return "License information";
                case FileType.MachineCode:
                    return "G-code or machine instructions";
                case FileType.AlternativeFormat:
                    return "Alternative 3D model format";
                case FileType.Other:
                default:
                    return "Miscellaneous file";
            }
        }
    }
} 