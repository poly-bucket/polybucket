using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Collections.CreateCollection.Domain
{
    public class CreateCollectionCommand : IRequest<Collection>
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public CollectionVisibility Visibility { get; set; } = CollectionVisibility.Private;
    }
} 