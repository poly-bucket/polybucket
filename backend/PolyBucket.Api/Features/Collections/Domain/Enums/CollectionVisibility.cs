using System.Text.Json.Serialization;

namespace PolyBucket.Api.Features.Collections.Domain.Enums
{
    [JsonConverter(typeof(CollectionVisibilityJsonConverter))]
    public enum CollectionVisibility
    {
        Public,
        Private,
        Unlisted
    }
} 