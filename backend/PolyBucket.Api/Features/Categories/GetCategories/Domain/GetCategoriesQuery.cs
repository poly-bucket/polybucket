using MediatR;

namespace PolyBucket.Api.Features.Categories.GetCategories.Domain
{
    public class GetCategoriesQuery : IRequest<GetCategoriesResponse>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
    }
}
