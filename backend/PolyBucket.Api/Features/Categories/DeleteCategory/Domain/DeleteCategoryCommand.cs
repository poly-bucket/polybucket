using MediatR;
using System;

namespace PolyBucket.Api.Features.Categories.DeleteCategory.Domain
{
    public class DeleteCategoryCommand : IRequest<DeleteCategoryResponse>
    {
        public Guid Id { get; set; }
    }
}
