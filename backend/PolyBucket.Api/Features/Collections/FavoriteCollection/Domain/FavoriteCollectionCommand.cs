using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Collections.FavoriteCollection.Domain
{
    public class FavoriteCollectionCommand : IRequest<FavoriteCollectionResponse>
    {
        [Required]
        public Guid CollectionId { get; set; }

        [Required]
        public bool IsFavorite { get; set; }
    }

    public class FavoriteCollectionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }
}
