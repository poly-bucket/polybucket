using MediatR;
using System;

namespace PolyBucket.Api.Features.Categories.CreateCategory.Domain
{
    public class GetCategoryByIdQuery : IRequest<CreateCategoryResponse?>
    {
        public Guid Id { get; set; }
    }
}
