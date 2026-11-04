using MediatR;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Categories.DeleteCategories.Domain
{
    public class DeleteCategoriesCommand : IRequest<DeleteCategoriesResponse>
    {
        public List<Guid> Ids { get; set; } = new();
    }
}
