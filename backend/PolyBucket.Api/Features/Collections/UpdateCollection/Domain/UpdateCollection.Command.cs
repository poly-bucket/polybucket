using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Collections.UpdateCollection.Domain
{
    public class UpdateCollectionCommand : IRequest<Collection>
    {
        [Required]
        public Guid Id { get; set; }

        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public CollectionVisibility? Visibility { get; set; }
    }
} 