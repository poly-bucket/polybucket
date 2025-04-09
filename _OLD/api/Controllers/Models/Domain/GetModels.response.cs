using Core.Models.Models;

namespace Api.Controllers.Models.Domain;

public class GetModelsResponse
{
    public IEnumerable<Model> Models { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
} 