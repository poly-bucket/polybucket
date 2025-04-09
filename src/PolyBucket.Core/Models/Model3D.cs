using System;
using System.Collections.Generic;
using PolyBucket.Core.Entities;

namespace PolyBucket.Core.Models
{
    public class Model3D
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public bool IsPublic { get; set; } = true;
        public bool IsFeatured { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Category { get; set; }
        public string License { get; set; }
        public int DownloadCount { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        
        // Navigation properties
        public Guid UserId { get; set; }
        public User User { get; set; }
        public virtual ICollection<ModelComment> Comments { get; set; }
        public virtual ICollection<ModelLike> Likes { get; set; }
        public virtual ICollection<ModelFile> Versions { get; set; }
    }
} 