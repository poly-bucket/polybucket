using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Collections.AddModelToCollection.Domain
{
    public class AddModelToCollectionCommand : IRequest
    {
        [Required]
        public Guid CollectionId { get; set; }

        [Required]
        public Guid ModelId { get; set; }
    }
} 