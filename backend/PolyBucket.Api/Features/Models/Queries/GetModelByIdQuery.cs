using MediatR;
using System;

namespace PolyBucket.Api.Features.Models.Queries
{
    public class GetModelByIdQuery : IRequest<GetModelByIdResponse>
    {
        public Guid Id { get; set; }
    }
} 