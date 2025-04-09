namespace Api.Controllers.Models.Domain;

public class GetModelsRequest
{
    public int Page { get; set; } = 1;
    public int Take { get; set; } = 20;
} 