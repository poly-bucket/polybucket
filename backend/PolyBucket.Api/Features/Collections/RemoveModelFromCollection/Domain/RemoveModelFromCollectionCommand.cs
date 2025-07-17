using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Domain
{
    public class RemoveModelFromCollectionCommand : IRequest
    {
        [Required]
        public Guid CollectionId { get; set; }

        [Required]
        public Guid ModelId { get; set; }
    }
} 