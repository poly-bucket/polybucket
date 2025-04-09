using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class Comment : Auditable
    {
        public new Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;
        public Guid ModelId { get; set; }
        public Model Model { get; set; } = null!;
        public bool IsEdited { get; set; }
    }
} 