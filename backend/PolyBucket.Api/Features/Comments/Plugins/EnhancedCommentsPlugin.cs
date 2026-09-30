using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Repository;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Notifications.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Comments.Plugins
{
    public class EnhancedCommentsPlugin(
        PolyBucketDbContext context,
        IReportingPlugin reportingPlugin,
        ICommentReactionService reactionService,
        ICommentReactionRepository reactionRepository,
        INotificationPublisher notificationPublisher) : IEnhancedCommentsPlugin
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly INotificationPublisher _notificationPublisher = notificationPublisher;
        private readonly IReportingPlugin _reportingPlugin = reportingPlugin;
        private readonly ICommentReactionService _reactionService = reactionService;
        private readonly ICommentReactionRepository _reactionRepository = reactionRepository;

        public string Id => "enhanced-comments-plugin";
        public string Name => "Enhanced Comments Plugin";
        public string Description => "Comprehensive comments plugin supporting multiple target types, moderation, and threading";
        public string Version => "2.0.0";
        public string Author => "Polybucket Team";

        public IEnumerable<PluginComponent> FrontendComponents => new List<PluginComponent>
        {
            new PluginComponent
            {
                Id = "enhanced-comments-widget",
                Name = "Enhanced Comments Widget",
                ComponentPath = "plugins/comments/EnhancedCommentsWidget",
                Type = ComponentType.Widget,
                Hooks = new List<PluginHook>
                {
                    new PluginHook
                    {
                        HookName = "model-details-sidebar",
                        ComponentId = "enhanced-comments-widget",
                        Priority = 50,
                        Config = new Dictionary<string, object> { { "targetType", "model" } }
                    },
                    new PluginHook
                    {
                        HookName = "user-profile-tabs",
                        ComponentId = "enhanced-comments-widget",
                        Priority = 30,
                        Config = new Dictionary<string, object> { { "targetType", "user" } }
                    },
                    new PluginHook
                    {
                        HookName = "collection-details-sidebar",
                        ComponentId = "enhanced-comments-widget",
                        Priority = 40,
                        Config = new Dictionary<string, object> { { "targetType", "collection" } }
                    }
                }
            }
        };

        public PluginMetadata Metadata => new PluginMetadata
        {
            MinimumAppVersion = "1.0.0",
            RequiredPermissions = new List<string> { "comment.create", "comment.view" },
            OptionalPermissions = new List<string> 
            { 
                "comment.moderate", 
                "comment.delete.any", 
                "comment.edit.any",
                "comment.report"
            },
            Settings = new Dictionary<string, PluginSetting>
            {
                ["maxCommentLength"] = new PluginSetting
                {
                    Name = "Max Comment Length",
                    Description = "Maximum number of characters allowed in a comment",
                    Type = PluginSettingType.Number,
                    DefaultValue = CommentLimits.MaxContentLength,
                    Required = true
                },
                ["allowNestedComments"] = new PluginSetting
                {
                    Name = "Allow Nested Comments",
                    Description = "Enable replies to comments",
                    Type = PluginSettingType.Boolean,
                    DefaultValue = true,
                    Required = false
                },
                ["maxNestingLevel"] = new PluginSetting
                {
                    Name = "Max Nesting Level",
                    Description = "Maximum depth of comment replies",
                    Type = PluginSettingType.Number,
                    DefaultValue = 3,
                    Required = false
                },
                ["autoModeration"] = new PluginSetting
                {
                    Name = "Auto Moderation",
                    Description = "Enable automatic comment moderation",
                    Type = PluginSettingType.Boolean,
                    DefaultValue = false,
                    Required = false
                }
            },
            Lifecycle = new PluginLifecycle
            {
                AutoStart = true,
                CanDisable = true,
                CanUninstall = false
            }
        };

        public Task InitializeAsync() => Task.CompletedTask;
        public Task UnloadAsync() => Task.CompletedTask;

        public async Task<EnhancedComment> AddCommentAsync(CommentTarget target, Guid authorId, string content, Guid? parentCommentId = null)
        {
            var normalizedContent = NormalizeContent(content);

            var author = await _context.Users.FindAsync(authorId);
            if (author == null)
                throw new InvalidOperationException("Author not found");

            await ValidateTargetExistsAsync(target);

            Guid? repliedToAuthorId = null;
            if (parentCommentId.HasValue)
            {
                var parentComment = await _context.EnhancedComments
                    .FirstOrDefaultAsync(c => c.Id == parentCommentId.Value);
                if (parentComment == null)
                    throw new InvalidOperationException("Parent comment not found");

                if (parentComment.TargetId != target.TargetId || parentComment.TargetType != target.TargetType)
                    throw new InvalidOperationException("Parent comment is not for the same target");

                repliedToAuthorId = parentComment.AuthorId;
                parentCommentId = parentComment.ParentCommentId ?? parentComment.Id;
            }

            var now = DateTime.UtcNow;
            var comment = new EnhancedComment
            {
                Id = Guid.NewGuid(),
                Content = normalizedContent,
                AuthorId = authorId,
                Author = author,
                ParentCommentId = parentCommentId,
                CreatedAt = now,
                CreatedById = authorId,
                UpdatedAt = now,
                UpdatedById = authorId
            };

            comment.SetTarget(target);

            _context.EnhancedComments.Add(comment);
            await PublishCommentNotificationsAsync(comment, target, repliedToAuthorId);
            await _context.SaveChangesAsync();
            
            return comment;
        }

        private async Task PublishCommentNotificationsAsync(EnhancedComment comment, CommentTarget target, Guid? repliedToAuthorId)
        {
            Guid? modelAuthorId = null;
            string? actionUrl = null;
            if (target.TargetType == CommentTargetType.Model)
            {
                var model = await _context.Models
                    .AsNoTracking()
                    .Where(m => m.Id == target.TargetId)
                    .Select(m => new { m.AuthorId, m.Name })
                    .FirstOrDefaultAsync();
                if (model != null)
                {
                    modelAuthorId = model.AuthorId;
                    actionUrl = $"/models/{target.TargetId}#comments";
                    await _notificationPublisher.PublishAsync(new NotificationRequest
                    {
                        RecipientUserId = model.AuthorId,
                        ActorUserId = comment.AuthorId,
                        Type = NotificationType.CommentAdded,
                        Title = $"{NotificationRequest.ActorToken} commented on your model",
                        Message = $"{NotificationRequest.ActorToken} commented on \"{model.Name}\": {Preview(comment.Content)}",
                        ActionUrl = actionUrl,
                        RelatedEntityId = comment.Id,
                        RelatedEntityType = "Comment"
                    });
                }
            }

            if (repliedToAuthorId.HasValue && repliedToAuthorId != modelAuthorId)
            {
                await _notificationPublisher.PublishAsync(new NotificationRequest
                {
                    RecipientUserId = repliedToAuthorId.Value,
                    ActorUserId = comment.AuthorId,
                    Type = NotificationType.CommentReplied,
                    Title = $"{NotificationRequest.ActorToken} replied to your comment",
                    Message = $"{NotificationRequest.ActorToken} replied: {Preview(comment.Content)}",
                    ActionUrl = actionUrl,
                    RelatedEntityId = comment.Id,
                    RelatedEntityType = "Comment"
                });
            }
        }

        private static string Preview(string content)
        {
            const int maxLength = 140;
            return content.Length <= maxLength ? content : content[..maxLength].TrimEnd() + "…";
        }

        public async Task<EnhancedComment> UpdateCommentAsync(Guid commentId, Guid userId, string content)
        {
            var normalizedContent = NormalizeContent(content);

            var comment = await _context.EnhancedComments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new InvalidOperationException("Comment not found");

            if (!comment.CanBeEditedBy(userId))
                throw new UnauthorizedAccessException("User cannot edit this comment");

            comment.Content = normalizedContent;
            comment.UpdatedById = userId;
            comment.MarkAsEdited();

            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, bool isAdmin = false)
        {
            var comment = await _context.EnhancedComments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                return false;

            if (!comment.CanBeDeletedBy(userId, isAdmin))
                return false;

            _context.EnhancedComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EnhancedComment>> GetCommentsForTargetAsync(CommentTarget target, bool includeHidden = false, int page = 1, int pageSize = 20)
        {
            var query = _context.EnhancedComments
                .AsNoTracking()
                .Include(c => c.Author)
                .Where(c => c.TargetId == target.TargetId && c.TargetType == target.TargetType && c.ParentCommentId == null);

            if (!includeHidden)
            {
                query = query.Where(c => !c.IsHidden);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<EnhancedComment?> GetCommentByIdAsync(Guid commentId)
        {
            return await _context.EnhancedComments
                .AsNoTracking()
                .Include(c => c.Author)
                .Include(c => c.ParentComment)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<IEnumerable<EnhancedComment>> GetRepliesAsync(Guid parentCommentId, bool includeHidden = false)
        {
            var query = _context.EnhancedComments
                .AsNoTracking()
                .Include(c => c.Author)
                .Where(c => c.ParentCommentId == parentCommentId);

            if (!includeHidden)
            {
                query = query.Where(c => !c.IsHidden);
            }

            return await query
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<int> GetCommentCountForTargetAsync(CommentTarget target, bool includeHidden = false)
        {
            var query = _context.EnhancedComments
                .Where(c => c.TargetId == target.TargetId && c.TargetType == target.TargetType && c.ParentCommentId == null);

            if (!includeHidden)
            {
                query = query.Where(c => !c.IsHidden);
            }

            return await query.CountAsync();
        }

        public async Task<bool> LikeCommentAsync(Guid commentId, Guid userId)
        {
            var outcome = await _reactionService.ReactAsync(commentId, userId, CommentReactionType.Like);
            return outcome.Change != CommentReactionChange.NotFound;
        }

        public async Task<bool> DislikeCommentAsync(Guid commentId, Guid userId)
        {
            var outcome = await _reactionService.ReactAsync(commentId, userId, CommentReactionType.Dislike);
            return outcome.Change != CommentReactionChange.NotFound;
        }

        public async Task<bool> RemoveLikeAsync(Guid commentId, Guid userId)
        {
            var outcome = await _reactionService.RemoveReactionAsync(commentId, userId, CommentReactionType.Like);
            return outcome.Change == CommentReactionChange.Applied;
        }

        public async Task<bool> RemoveDislikeAsync(Guid commentId, Guid userId)
        {
            var outcome = await _reactionService.RemoveReactionAsync(commentId, userId, CommentReactionType.Dislike);
            return outcome.Change == CommentReactionChange.Applied;
        }

        public async Task<bool> HasUserLikedCommentAsync(Guid commentId, Guid userId)
        {
            return await _reactionRepository.GetUserReactionAsync(commentId, userId) == CommentReactionType.Like;
        }

        public async Task<bool> HasUserDislikedCommentAsync(Guid commentId, Guid userId)
        {
            return await _reactionRepository.GetUserReactionAsync(commentId, userId) == CommentReactionType.Dislike;
        }

        public async Task<bool> ModerateCommentAsync(Guid commentId, Guid moderatorId, string reason)
        {
            var comment = await _context.EnhancedComments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                return false;

            comment.MarkAsModerated(moderatorId, reason);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnmoderateCommentAsync(Guid commentId, Guid moderatorId)
        {
            var comment = await _context.EnhancedComments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                return false;

            comment.IsModerated = false;
            comment.IsHidden = false;
            comment.ModerationReason = null;
            comment.ModeratedByUserId = null;
            comment.ModeratedAt = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EnhancedComment>> GetModeratedCommentsAsync(int page = 1, int pageSize = 20)
        {
            return await _context.EnhancedComments
                .AsNoTracking()
                .Include(c => c.Author)
                .Include(c => c.ModeratedByUser)
                .Where(c => c.IsModerated)
                .OrderByDescending(c => c.ModeratedAt)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> ReportCommentAsync(Guid commentId, Guid reporterId, string reason)
        {
            if (!await _context.EnhancedComments.AnyAsync(c => c.Id == commentId))
            {
                return false;
            }

            try
            {
                await _reportingPlugin.SubmitReportAsync(
                    ReportType.Comment,
                    commentId,
                    reporterId,
                    ReportReason.Other,
                    reason
                );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAllCommentsForTargetAsync(CommentTarget target, Guid deletedByUserId)
        {
            var comments = await _context.EnhancedComments
                .Where(c => c.TargetId == target.TargetId && c.TargetType == target.TargetType)
                .ToListAsync();

            _context.EnhancedComments.RemoveRange(comments);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ModerateAllCommentsForUserAsync(Guid userId, Guid moderatorId, string reason)
        {
            var comments = await _context.EnhancedComments
                .Where(c => c.AuthorId == userId && !c.IsModerated)
                .ToListAsync();

            foreach (var comment in comments)
            {
                comment.MarkAsModerated(moderatorId, reason);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CommentStatistics> GetCommentStatisticsAsync(CommentTarget target)
        {
            var comments = await _context.EnhancedComments
                .AsNoTracking()
                .Include(c => c.Author)
                .Where(c => c.TargetId == target.TargetId && c.TargetType == target.TargetType)
                .ToListAsync();

            var stats = new CommentStatistics
            {
                TotalComments = comments.Count(c => !c.ParentCommentId.HasValue),
                TotalReplies = comments.Count(c => c.ParentCommentId.HasValue),
                TotalLikes = comments.Sum(c => c.Likes),
                TotalDislikes = comments.Sum(c => c.Dislikes),
                ModeratedComments = comments.Count(c => c.IsModerated),
                LastCommentAt = comments.OrderByDescending(c => c.CreatedAt).FirstOrDefault()?.CreatedAt,
                TopCommenters = comments
                    .Where(c => !c.IsHidden)
                    .GroupBy(c => new { c.AuthorId, c.Author.Username })
                    .Select(g => new TopCommenter
                    {
                        UserId = g.Key.AuthorId,
                        Username = g.Key.Username ?? "Unknown",
                        CommentCount = g.Count(),
                        TotalLikes = g.Sum(c => c.Likes)
                    })
                    .OrderByDescending(t => t.CommentCount)
                    .Take(5)
                    .ToList()
            };

            return stats;
        }

        public async Task<UserCommentStatistics> GetUserCommentStatisticsAsync(Guid userId)
        {
            var comments = await _context.EnhancedComments
                .AsNoTracking()
                .Where(c => c.AuthorId == userId)
                .ToListAsync();

            var stats = new UserCommentStatistics
            {
                UserId = userId,
                TotalComments = comments.Count,
                TotalLikes = comments.Sum(c => c.Likes),
                TotalDislikes = comments.Sum(c => c.Dislikes),
                ModeratedComments = comments.Count(c => c.IsModerated),
                LastCommentAt = comments.OrderByDescending(c => c.CreatedAt).FirstOrDefault()?.CreatedAt,
                AverageLikesPerComment = comments.Count > 0 ? (double)comments.Sum(c => c.Likes) / comments.Count : 0
            };

            return stats;
        }

        private static string NormalizeContent(string content)
        {
            var trimmed = content?.Trim() ?? string.Empty;
            if (trimmed.Length == 0)
                throw new InvalidOperationException("Comment content is required");

            if (trimmed.Length > CommentLimits.MaxContentLength)
                throw new InvalidOperationException($"Comment content must be {CommentLimits.MaxContentLength} characters or fewer");

            return trimmed;
        }

        private async Task ValidateTargetExistsAsync(CommentTarget target)
        {
            bool exists = target.TargetType switch
            {
                CommentTargetType.Model => await _context.Models.AnyAsync(m => m.Id == target.TargetId && m.DeletedAt == null),
                CommentTargetType.UserProfile => await _context.Users.AnyAsync(u => u.Id == target.TargetId && u.DeletedAt == null),
                CommentTargetType.Collection => await _context.Collections.AnyAsync(c => c.Id == target.TargetId && c.DeletedAt == null),
                _ => false
            };

            if (!exists)
            {
                throw new InvalidOperationException($"Target {target.TargetType} with ID {target.TargetId} not found");
            }
        }
    }
} 