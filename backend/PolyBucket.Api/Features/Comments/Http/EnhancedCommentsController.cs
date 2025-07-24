using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Comments.Domain;
using System.Collections.Generic;
using System.Linq;

namespace PolyBucket.Api.Features.Comments.Http
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
    public class EnhancedCommentsController(IEnhancedCommentsPlugin commentsPlugin) : ControllerBase
    {
        private readonly IEnhancedCommentsPlugin _commentsPlugin = commentsPlugin;

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            var userId = GetCurrentUserId();

            try
            {
                var comment = await _commentsPlugin.AddCommentAsync(
                    request.Target, 
                    userId, 
                    request.Content, 
                    request.ParentCommentId
                );

                var response = await MapToResponse(comment, userId);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("target/{targetType}/{targetId}")]
        public async Task<IActionResult> GetCommentsForTarget(
            string targetType, 
            Guid targetId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeHidden = false)
        {
            var target = new CommentTarget
            {
                TargetId = targetId,
                TargetType = ParseTargetType(targetType)
            };

            var comments = await _commentsPlugin.GetCommentsForTargetAsync(target, includeHidden, page, pageSize);
            var totalCount = await _commentsPlugin.GetCommentCountForTargetAsync(target, includeHidden);
            var statistics = await _commentsPlugin.GetCommentStatisticsAsync(target);

            var userId = GetCurrentUserId();
            var commentResponses = new List<CommentResponse>();
            foreach (var comment in comments)
            {
                commentResponses.Add(await MapToResponse(comment, userId));
            }
            
            var response = new CommentsPagedResponse
            {
                Comments = commentResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Statistics = statistics
            };

            return Ok(response);
        }

        [HttpGet("{commentId}")]
        public async Task<IActionResult> GetComment(Guid commentId)
        {
            var comment = await _commentsPlugin.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var response = await MapToResponse(comment, userId);
            return Ok(response);
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentRequest request)
        {
            var userId = GetCurrentUserId();

            try
            {
                var comment = await _commentsPlugin.UpdateCommentAsync(commentId, userId, request.Content);
                var response = await MapToResponse(comment, userId);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var result = await _commentsPlugin.DeleteCommentAsync(commentId, userId, isAdmin);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("{commentId}/like")]
        public async Task<IActionResult> LikeComment(Guid commentId)
        {
            var userId = GetCurrentUserId();
            var result = await _commentsPlugin.LikeCommentAsync(commentId, userId);
            
            if (!result)
            {
                return BadRequest(new { message = "Unable to like comment" });
            }

            return Ok(new { message = "Comment liked successfully" });
        }

        [HttpPost("{commentId}/dislike")]
        public async Task<IActionResult> DislikeComment(Guid commentId)
        {
            var userId = GetCurrentUserId();
            var result = await _commentsPlugin.DislikeCommentAsync(commentId, userId);
            
            if (!result)
            {
                return BadRequest(new { message = "Unable to dislike comment" });
            }

            return Ok(new { message = "Comment disliked successfully" });
        }

        [HttpDelete("{commentId}/like")]
        public async Task<IActionResult> RemoveLike(Guid commentId)
        {
            var userId = GetCurrentUserId();
            var result = await _commentsPlugin.RemoveLikeAsync(commentId, userId);
            
            if (!result)
            {
                return BadRequest(new { message = "Unable to remove like" });
            }

            return Ok(new { message = "Like removed successfully" });
        }

        [HttpDelete("{commentId}/dislike")]
        public async Task<IActionResult> RemoveDislike(Guid commentId)
        {
            var userId = GetCurrentUserId();
            var result = await _commentsPlugin.RemoveDislikeAsync(commentId, userId);
            
            if (!result)
            {
                return BadRequest(new { message = "Unable to remove dislike" });
            }

            return Ok(new { message = "Dislike removed successfully" });
        }

        [HttpPost("{commentId}/report")]
        public async Task<IActionResult> ReportComment(Guid commentId, [FromBody] ReportCommentRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _commentsPlugin.ReportCommentAsync(commentId, userId, request.Reason);
            
            if (!result)
            {
                return BadRequest(new { message = "Unable to report comment" });
            }

            return Ok(new { message = "Comment reported successfully" });
        }

        [HttpGet("statistics/{targetType}/{targetId}")]
        public async Task<IActionResult> GetStatistics(string targetType, Guid targetId)
        {
            var target = new CommentTarget
            {
                TargetId = targetId,
                TargetType = ParseTargetType(targetType)
            };

            var statistics = await _commentsPlugin.GetCommentStatisticsAsync(target);
            return Ok(statistics);
        }

        [HttpGet("user/{userId}/statistics")]
        public async Task<IActionResult> GetUserStatistics(Guid userId)
        {
            var statistics = await _commentsPlugin.GetUserCommentStatisticsAsync(userId);
            return Ok(statistics);
        }

        [HttpPost("{commentId}/moderate")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> ModerateComment(Guid commentId, [FromBody] ModerateCommentRequest request)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _commentsPlugin.ModerateCommentAsync(commentId, moderatorId, request.Reason);
            
            if (!result)
            {
                return NotFound();
            }

            return Ok(new { message = "Comment moderated successfully" });
        }

        [HttpPost("{commentId}/unmoderate")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> UnmoderateComment(Guid commentId)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _commentsPlugin.UnmoderateCommentAsync(commentId, moderatorId);
            
            if (!result)
            {
                return NotFound();
            }

            return Ok(new { message = "Comment unmoderated successfully" });
        }

        [HttpGet("moderated")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> GetModeratedComments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var comments = await _commentsPlugin.GetModeratedCommentsAsync(page, pageSize);
            var userId = GetCurrentUserId();
            var commentResponses = new List<CommentResponse>();
            foreach (var comment in comments)
            {
                commentResponses.Add(await MapToResponse(comment, userId));
            }
            
            return Ok(commentResponses);
        }

        [HttpDelete("target/{targetType}/{targetId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllCommentsForTarget(string targetType, Guid targetId)
        {
            var target = new CommentTarget
            {
                TargetId = targetId,
                TargetType = ParseTargetType(targetType)
            };

            var userId = GetCurrentUserId();
            var result = await _commentsPlugin.DeleteAllCommentsForTargetAsync(target, userId);
            
            if (!result)
            {
                return BadRequest(new { message = "Unable to delete comments" });
            }

            return Ok(new { message = "All comments deleted successfully" });
        }

        [HttpPost("user/{targetUserId}/moderate-all")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> ModerateAllUserComments(Guid targetUserId, [FromBody] ModerateCommentRequest request)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _commentsPlugin.ModerateAllCommentsForUserAsync(targetUserId, moderatorId, request.Reason);
            
            if (!result)
            {
                return BadRequest(new { message = "Unable to moderate user comments" });
            }

            return Ok(new { message = "All user comments moderated successfully" });
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));
        }

        private CommentTargetType ParseTargetType(string targetType)
        {
            return targetType.ToLowerInvariant() switch
            {
                "model" => CommentTargetType.Model,
                "user" => CommentTargetType.UserProfile,
                "userprofile" => CommentTargetType.UserProfile,
                "collection" => CommentTargetType.Collection,
                "report" => CommentTargetType.Report,
                _ => throw new ArgumentException($"Invalid target type: {targetType}")
            };
        }

        private async Task<CommentResponse> MapToResponse(EnhancedComment comment, Guid currentUserId)
        {
            // Load replies if this is a top-level comment
            var replies = new List<CommentResponse>();
            if (comment.ParentCommentId == null)
            {
                var commentReplies = await _commentsPlugin.GetRepliesAsync(comment.Id);
                replies = commentReplies.Select(r => MapToResponseInternal(r, currentUserId)).ToList();
            }

            return new CommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorId = comment.AuthorId,
                AuthorUsername = comment.Author?.Username ?? "Unknown",
                Target = comment.GetTarget(),
                Likes = comment.Likes,
                Dislikes = comment.Dislikes,
                IsEdited = comment.IsEdited,
                IsModerated = comment.IsModerated,
                IsHidden = comment.IsHidden,
                ParentCommentId = comment.ParentCommentId,
                CreatedAt = comment.CreatedAt,
                LastEditedAt = comment.LastEditedAt,
                UserHasLiked = await _commentsPlugin.HasUserLikedCommentAsync(comment.Id, currentUserId),
                UserHasDisliked = await _commentsPlugin.HasUserDislikedCommentAsync(comment.Id, currentUserId),
                CanEdit = comment.CanBeEditedBy(currentUserId),
                CanDelete = comment.CanBeDeletedBy(currentUserId, User.IsInRole("Admin")),
                Replies = replies
            };
        }

        private CommentResponse MapToResponseInternal(EnhancedComment comment, Guid currentUserId)
        {
            return new CommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorId = comment.AuthorId,
                AuthorUsername = comment.Author?.Username ?? "Unknown",
                Target = comment.GetTarget(),
                Likes = comment.Likes,
                Dislikes = comment.Dislikes,
                IsEdited = comment.IsEdited,
                IsModerated = comment.IsModerated,
                IsHidden = comment.IsHidden,
                ParentCommentId = comment.ParentCommentId,
                CreatedAt = comment.CreatedAt,
                LastEditedAt = comment.LastEditedAt,
                UserHasLiked = false, // TODO: Implement proper like tracking
                UserHasDisliked = false, // TODO: Implement proper dislike tracking
                CanEdit = comment.CanBeEditedBy(currentUserId),
                CanDelete = comment.CanBeDeletedBy(currentUserId, User.IsInRole("Admin")),
                Replies = new List<CommentResponse>() // No nested replies to avoid infinite recursion
            };
        }
    }

    public class ReportCommentRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ModerateCommentRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
} 