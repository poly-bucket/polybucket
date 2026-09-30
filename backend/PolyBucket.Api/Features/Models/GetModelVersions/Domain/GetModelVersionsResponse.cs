using System;
using System.Collections.Generic;
using PolyBucket.Api.Features.Models.Common;

namespace PolyBucket.Api.Features.Models.GetModelVersions.Domain;

public class GetModelVersionsResponse
{
    public Guid ModelId { get; set; }
    public List<ModelVersionDto> Versions { get; set; } = new();
}
