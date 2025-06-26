using MediatR;

namespace PolyBucket.Api.Features.Models.Queries
{
    public class GetModelsQuery : IRequest<GetModelsResponse>
    {
        public int Page { get; set; } = 1;
        public int Take { get; set; } = 10;
    }
} 