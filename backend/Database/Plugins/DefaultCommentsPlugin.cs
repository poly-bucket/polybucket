using Core.Models.Comments;
using Core.Models.Users;
using Core.Plugins.Comments;
using Microsoft.EntityFrameworkCore;

namespace Database.Plugins;

public class DefaultCommentsPlugin : ICommentsPlugin
{
    private readonly Context _context;

    public DefaultCommentsPlugin(Context context)
    {
        _context = context;
    }

    public string Id => "default-comments-plugin";
    public string Name => "Default Comments Plugin";
    public string Description => "Default implementation of the comments plugin using Entity Framework Core";
    public string Version => "1.0.0";
    public string Author => "Polybucket Team";

    public Task InitializeAsync() => Task.CompletedTask;
    public Task UnloadAsync() => Task.CompletedTask;

    public async Task<Comment> AddCommentAsync(Guid modelId, Guid authorId, string content)
    {
        var author = await _context.Users.FindAsync(authorId);
        if (author == null)
            throw new InvalidOperationException("Author not found");

        var model = await _context.Models.FindAsync(modelId);
        if (model == null)
            throw new InvalidOperationException("Model not found");

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = content,
            Author = author,
            Model = model,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<IEnumerable<Comment>> GetCommentsForModelAsync(Guid modelId)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.Model.Id == modelId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task LikeCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            throw new InvalidOperationException("Comment not found");

        comment.Likes++;
        await _context.SaveChangesAsync();
    }

    public async Task DislikeCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            throw new InvalidOperationException("Comment not found");

        comment.Dislikes++;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
} 