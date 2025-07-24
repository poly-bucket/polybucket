using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolyBucket.Api.Features.Collections.CreateCollection.Domain
{
    public class CreateCollectionCommand : IRequest<Collection>
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [JsonConverter(typeof(CollectionVisibilityJsonConverter))]
        public CollectionVisibility Visibility { get; set; } = CollectionVisibility.Private;

        [StringLength(100, MinimumLength = 4)]
        public string? Password { get; set; }
    }
} 