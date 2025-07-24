using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Common.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Comments.Plugins
{
    public class DefaultCommentsPlugin(PolyBucketDbContext context) : ICommentsPlugin
    {
        private readonly PolyBucketDbContext _context = context;

        public string Id => "default-comments-plugin";
        public string Name => "Default Comments Plugin";
        public string Description => "Default implementation of the comments plugin using Entity Framework Core";
        public string Version => "1.0.0";
        public string Author => "Polybucket Team";

        public IEnumerable<PluginComponent> FrontendComponents => new List<PluginComponent>
        {
            new PluginComponent
            {
                Id = "comments-widget",
                Name = "Comments Widget",
                ComponentPath = "plugins/comments/CommentsWidget",
                Type = ComponentType.Widget,
                Hooks = new List<PluginHook>
                {
                    new PluginHook
                    {
                        HookName = "model-details-sidebar",
                        ComponentId = "comments-widget",
                        Priority = 50
                    },
                    new PluginHook
                    {
                        HookName = "user-profile-tabs",
                        ComponentId = "comments-widget",
                        Priority = 30,
                        Config = new Dictionary<string, object> { { "targetType", "user" } }
                    }
                }
            }
        };

        public PluginMetadata Metadata => new PluginMetadata
        {
            MinimumAppVersion = "1.0.0",
            RequiredPermissions = new List<string> { "comment.create", "comment.view" },
            OptionalPermissions = new List<string> { "comment.moderate", "comment.delete.any" },
            Settings = new Dictionary<string, PluginSetting>
            {
                ["maxCommentLength"] = new PluginSetting
                {
                    Name = "Max Comment Length",
                    Description = "Maximum number of characters allowed in a comment",
                    Type = PluginSettingType.Number,
                    DefaultValue = 1000,
                    Required = true
                },
                ["allowNestedComments"] = new PluginSetting
                {
                    Name = "Allow Nested Comments",
                    Description = "Enable replies to comments",
                    Type = PluginSettingType.Boolean,
                    DefaultValue = true,
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
} 