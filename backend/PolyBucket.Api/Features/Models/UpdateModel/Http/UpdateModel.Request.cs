using PolyBucket.Api.Features.Models.Shared.Domain.Enums;

namespace PolyBucket.Api.Features.Models.UpdateModel.Http;

public class ModelUpdateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public LicenseTypes? License { get; set; }
        public PrivacySettings? Privacy { get; set; }
        public bool? AIGenerated { get; set; }
        public bool? WIP { get; set; }
        public bool? NSFW { get; set; }
        public bool? IsRemix { get; set; }
        public string? RemixUrl { get; set; }
        public bool? IsFeatured { get; set; }
    }