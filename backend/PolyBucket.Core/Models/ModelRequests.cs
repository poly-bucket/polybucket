using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Core.Entities;
using Core.Enums;

namespace Core.Models
{
    public class ModelUploadRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile ModelFile { get; set; }
        public IFormFile ThumbnailImage { get; set; }
        public List<IFormFile> AdditionalFiles { get; set; }
        public List<string> Tags { get; set; }
        public List<Guid> CategoryIds { get; set; }
        public string License { get; set; }
    }

    public class ModelUpdateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile ThumbnailImage { get; set; }
        public List<string> Tags { get; set; }
        public List<Guid> CategoryIds { get; set; }
        public string License { get; set; }
    }

    public class ModelVersionUploadRequest
    {
        public string VersionLabel { get; set; }
        public string Description { get; set; }
        public IFormFile ModelFile { get; set; }
        public List<IFormFile> AdditionalFiles { get; set; }
    }

    public class ModelUploadResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Message { get; set; }
        public Guid? ModelId { get; set; }
        public string ModelUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class ModelVersionUploadResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Message { get; set; }
        public Guid? ModelId { get; set; }
        public string ModelUrl { get; set; }
        public string VersionLabel { get; set; }
    }

    public class ModelWithDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string UserName { get; set; }
        public Guid UserId { get; set; }
        public int DownloadCount { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FileFormat { get; set; }
        public long FileSizeBytes { get; set; }
        public string License { get; set; }
        public bool IsFeatured { get; set; }
        public ModerationStatus ModerationStatus { get; set; }
        public string ModerationReason { get; set; }
        public List<string> Tags { get; set; }
        public List<CategoryInfo> Categories { get; set; }
        public List<ModelFileInfo> AdditionalFiles { get; set; }
        public List<VersionInfo> Versions { get; set; }
    }

    public class CategoryInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }

    public class ModelFileInfo
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileFormat { get; set; }
        public long FileSizeBytes { get; set; }
        public string FileType { get; set; }
    }

    public class VersionInfo
    {
        public Guid Id { get; set; }
        public string VersionLabel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}