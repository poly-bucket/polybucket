using Core.Models;

namespace Core.Plugins;

public interface ICommentsPlugin : IPlugin
{
    Task<Comment> CreateCommentAsync(string targetId, string authorId, string content);
    Task<Comment?> GetCommentByIdAsync(string commentId);
    Task<IEnumerable<Comment>> GetCommentsForTargetAsync(string targetId);
    Task DeleteCommentAsync(string commentId);
    Task<Comment> UpdateCommentAsync(string commentId, string newContent);
} 