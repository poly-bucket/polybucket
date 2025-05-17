namespace Api.Controllers.Models.GetModels.Domain
{
    public class GetModelsRequest
    {
        public string? query { get; set; }
        public int Page { get; set; } = 1;
        public int Take { get; set; } = 20;
    }
}