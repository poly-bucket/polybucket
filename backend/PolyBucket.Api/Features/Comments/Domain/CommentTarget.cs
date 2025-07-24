using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Comments.Domain
{
    public enum CommentTargetType
    {
        Model,
        UserProfile,
        Collection,
        Report
    }

    public class CommentTarget
    {
        public Guid TargetId { get; set; }
        public CommentTargetType TargetType { get; set; }
        
        public static CommentTarget ForModel(Guid modelId) => new CommentTarget 
        { 
            TargetId = modelId, 
            TargetType = CommentTargetType.Model 
        };
        
        public static CommentTarget ForUserProfile(Guid userId) => new CommentTarget 
        { 
            TargetId = userId, 
            TargetType = CommentTargetType.UserProfile 
        };
        
        public static CommentTarget ForCollection(Guid collectionId) => new CommentTarget 
        { 
            TargetId = collectionId, 
            TargetType = CommentTargetType.Collection 
        };
        
        public static CommentTarget ForReport(Guid reportId) => new CommentTarget 
        { 
            TargetId = reportId, 
            TargetType = CommentTargetType.Report 
        };
    }
} 