using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.AddTagToModel.Domain
{
    public class Tag : Auditable
    {
        public new Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public ICollection<Model> Models { get; set; } = new List<Model>();
    }
}
