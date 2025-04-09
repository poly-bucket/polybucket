using Core.Models.Comments;

namespace Core.Plugins.Comments
{
    /// <summary>
    /// Interface that must be implemented by comment plugins
    /// </summary>
    public interface ICommentsPlugin : IPlugin
    {
        /// <summary>
        /// Add a comment to a model
        /// </summary>
        Task<Comment> AddCommentAsync(Guid modelId, Guid authorId, string content);

        /// <summary>
        /// Get all comments for a model
        /// </summary>
        Task<IEnumerable<Comment>> GetCommentsForModelAsync(Guid modelId);

        /// <summary>
        /// Like a comment
        /// </summary>
        Task LikeCommentAsync(Guid commentId, Guid userId);

        /// <summary>
        /// Dislike a comment
        /// </summary>
        Task DislikeCommentAsync(Guid commentId, Guid userId);

        /// <summary>
        /// Delete a comment
        /// </summary>
        Task DeleteCommentAsync(Guid commentId, Guid userId);
    }
} 