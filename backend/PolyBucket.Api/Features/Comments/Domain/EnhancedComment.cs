using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Comments.Domain
{
    public class EnhancedComment : Auditable
    {
        public string Content { get; set; } = string.Empty;
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        
        // Flexible target system
        public Guid TargetId { get; set; }
        public CommentTargetType TargetType { get; set; }
        
        // Author information
        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;
        
        // Threading support for nested comments
        public Guid? ParentCommentId { get; set; }
        public virtual EnhancedComment? ParentComment { get; set; }
        
        // Moderation
        public bool IsModerated { get; set; }
        public bool IsHidden { get; set; }
        public string? ModerationReason { get; set; }
        public Guid? ModeratedByUserId { get; set; }
        public virtual User? ModeratedByUser { get; set; }
        public DateTime? ModeratedAt { get; set; }
        
        // Editing
        public bool IsEdited { get; set; }
        public DateTime? LastEditedAt { get; set; }
        
        // Helper methods
        public CommentTarget GetTarget() => new CommentTarget
        {
            TargetId = TargetId,
            TargetType = TargetType
        };
        
        public void SetTarget(CommentTarget target)
        {
            TargetId = target.TargetId;
            TargetType = target.TargetType;
        }
        
        public bool IsReply => ParentCommentId.HasValue;
        
        public bool CanBeEditedBy(Guid userId)
        {
            return AuthorId == userId && !IsModerated;
        }
        
        public bool CanBeDeletedBy(Guid userId, bool isAdmin = false)
        {
            return AuthorId == userId || isAdmin;
        }
        
        public void MarkAsModerated(Guid moderatorId, string reason)
        {
            IsModerated = true;
            IsHidden = true;
            ModerationReason = reason;
            ModeratedByUserId = moderatorId;
            ModeratedAt = DateTime.UtcNow;
        }
        
        public void MarkAsEdited()
        {
            IsEdited = true;
            LastEditedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 