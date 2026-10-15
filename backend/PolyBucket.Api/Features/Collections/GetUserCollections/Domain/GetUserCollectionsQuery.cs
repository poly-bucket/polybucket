using System;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Domain;

public class GetUserCollectionsQuery
{
    public Guid TargetUserId { get; set; }
    public Guid? RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? SearchQuery { get; set; }
}
