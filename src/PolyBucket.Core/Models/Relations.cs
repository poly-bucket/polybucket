using System;
using PolyBucket.Core.Entities;

namespace PolyBucket.Core.Models
{
    public class ModelLike
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public Model3D Model { get; set; }
        public User User { get; set; }
    }
    
    public class ModelComment
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? ParentCommentId { get; set; } // For threaded comments
        
        // Navigation properties
        public Model3D Model { get; set; }
        public User User { get; set; }
        public ModelComment ParentComment { get; set; }
        public virtual ICollection<ModelComment> Replies { get; set; } = new List<ModelComment>();
    }
    
    public class UserFollow
    {
        public Guid Id { get; set; }
        public Guid FollowerId { get; set; }
        public Guid FollowedId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public User Follower { get; set; }
        public User Followed { get; set; }
    }
} 