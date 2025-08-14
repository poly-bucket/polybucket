using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.CreateCategory.Domain
{
    public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CreateCategoryResponse?>
    {
        private readonly ICreateCategoryService _service;

        public GetCategoryByIdHandler(ICreateCategoryService service)
        {
            _service = service;
        }

        public async Task<CreateCategoryResponse?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetCategoryByIdAsync(request.Id);
        }
    }
}
