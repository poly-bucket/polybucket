using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class Comment : Auditable
    {
        public Guid Id { get; set; }
        public required string Content { get; set; }
        public Guid AuthorId { get; set; }
        public required User Author { get; set; }
        public Guid ModelId { get; set; }
        public required Model Model { get; set; }
        public bool IsEdited { get; set; }
        public new bool IsDeleted { get; set; }
    }
} 