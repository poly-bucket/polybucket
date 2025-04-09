using MediatR;
using System;

namespace PolyBucket.Api.Features.Models.GetModelById.Domain
{
    public class GetModelByIdQuery : IRequest<GetModelByIdResponse>
    {
        public Guid Id { get; set; }
    }
} 