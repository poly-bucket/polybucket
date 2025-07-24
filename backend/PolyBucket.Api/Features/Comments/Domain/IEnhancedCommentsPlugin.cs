using PolyBucket.Api.Common.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Comments.Domain
{
    public interface IEnhancedCommentsPlugin : IPlugin
    {
        // Basic comment operations
        Task<EnhancedComment> AddCommentAsync(CommentTarget target, Guid authorId, string content, Guid? parentCommentId = null);
        Task<EnhancedComment> UpdateCommentAsync(Guid commentId, Guid userId, string content);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, bool isAdmin = false);
        
        // Retrieval operations
        Task<IEnumerable<EnhancedComment>> GetCommentsForTargetAsync(CommentTarget target, bool includeHidden = false, int page = 1, int pageSize = 20);
        Task<EnhancedComment?> GetCommentByIdAsync(Guid commentId);
        Task<IEnumerable<EnhancedComment>> GetRepliesAsync(Guid parentCommentId, bool includeHidden = false);
        Task<int> GetCommentCountForTargetAsync(CommentTarget target, bool includeHidden = false);
        
        // Interaction operations  
        Task<bool> LikeCommentAsync(Guid commentId, Guid userId);
        Task<bool> DislikeCommentAsync(Guid commentId, Guid userId);
        Task<bool> RemoveLikeAsync(Guid commentId, Guid userId);
        Task<bool> RemoveDislikeAsync(Guid commentId, Guid userId);
        Task<bool> HasUserLikedCommentAsync(Guid commentId, Guid userId);
        Task<bool> HasUserDislikedCommentAsync(Guid commentId, Guid userId);
        
        // Moderation operations
        Task<bool> ModerateCommentAsync(Guid commentId, Guid moderatorId, string reason);
        Task<bool> UnmoderateCommentAsync(Guid commentId, Guid moderatorId);
        Task<IEnumerable<EnhancedComment>> GetModeratedCommentsAsync(int page = 1, int pageSize = 20);
        
        // Reporting integration
        Task<bool> ReportCommentAsync(Guid commentId, Guid reporterId, string reason);
        
        // Bulk operations
        Task<bool> DeleteAllCommentsForTargetAsync(CommentTarget target, Guid deletedByUserId);
        Task<bool> ModerateAllCommentsForUserAsync(Guid userId, Guid moderatorId, string reason);
        
        // Statistics
        Task<CommentStatistics> GetCommentStatisticsAsync(CommentTarget target);
        Task<UserCommentStatistics> GetUserCommentStatisticsAsync(Guid userId);
    }

    public class CommentStatistics
    {
        public int TotalComments { get; set; }
        public int TotalReplies { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public int ModeratedComments { get; set; }
        public DateTime? LastCommentAt { get; set; }
        public IEnumerable<TopCommenter> TopCommenters { get; set; } = new List<TopCommenter>();
    }

    public class UserCommentStatistics
    {
        public Guid UserId { get; set; }
        public int TotalComments { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public int ModeratedComments { get; set; }
        public DateTime? LastCommentAt { get; set; }
        public double AverageLikesPerComment { get; set; }
    }

    public class TopCommenter
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int CommentCount { get; set; }
        public int TotalLikes { get; set; }
    }

    // Request/Response DTOs
    public class AddCommentRequest
    {
        public CommentTarget Target { get; set; } = new CommentTarget();
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
    }

    public class UpdateCommentRequest
    {
        public string Content { get; set; } = string.Empty;
    }

    public class CommentResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public CommentTarget Target { get; set; } = new CommentTarget();
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public bool IsEdited { get; set; }
        public bool IsModerated { get; set; }
        public bool IsHidden { get; set; }
        public Guid? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastEditedAt { get; set; }
        public bool UserHasLiked { get; set; }
        public bool UserHasDisliked { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public IEnumerable<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
    }

    public class CommentsPagedResponse
    {
        public IEnumerable<CommentResponse> Comments { get; set; } = new List<CommentResponse>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public CommentStatistics Statistics { get; set; } = new CommentStatistics();
    }
} 