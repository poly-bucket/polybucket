using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class Tag : Auditable
    {
        public new Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
} 