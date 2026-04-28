using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [JsonConverter(typeof(NullableCollectionVisibilityJsonConverter))]
        public CollectionVisibility? Visibility { get; set; }

        [StringLength(100, MinimumLength = 4)]
        public string? Password { get; set; }

        public string? Avatar { get; set; }
    }
} 