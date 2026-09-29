using System.Collections.Generic;

namespace PolyBucket.Api.Features.ModelModeration.Domain;

public class ModelsAwaitingModerationResponse
{
    public List<ModelModerationQueueItem> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
