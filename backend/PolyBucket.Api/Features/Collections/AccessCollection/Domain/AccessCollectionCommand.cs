using MediatR;
using PolyBucket.Api.Features.Collections.AccessCollection.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Collections.AccessCollection.Domain
{
    public class AccessCollectionCommand : IRequest<AccessCollectionResponse>
    {
        [Required]
        public Guid CollectionId { get; set; }

        public string? Password { get; set; }
    }

    public class AccessCollectionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AccessCollectionCollectionDto? Collection { get; set; }
        public bool RequiresPassword { get; set; }
    }
} 