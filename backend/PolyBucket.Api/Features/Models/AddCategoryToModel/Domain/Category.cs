using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Features.Models.Shared.Domain;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.AddCategoryToModel.Domain
{
    public class Category : Auditable
    {
        public new Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public ICollection<Model> Models { get; set; } = new List<Model>();
    }
}
