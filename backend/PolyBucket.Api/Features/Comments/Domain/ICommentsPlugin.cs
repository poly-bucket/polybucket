using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Comments.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Comments.Domain
{
    public interface ICommentsPlugin : IPlugin
    {
        Task<Comment> AddCommentAsync(Guid modelId, Guid authorId, string content);
        Task<IEnumerable<Comment>> GetCommentsForModelAsync(Guid modelId);
        Task LikeCommentAsync(Guid commentId, Guid userId);
        Task DislikeCommentAsync(Guid commentId, Guid userId);
        Task DeleteCommentAsync(Guid commentId, Guid userId);
    }
} 