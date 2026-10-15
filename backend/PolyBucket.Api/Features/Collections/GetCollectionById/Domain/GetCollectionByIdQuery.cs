using MediatR;
using PolyBucket.Api.Features.Collections.GetCollectionById.Http;
using System;

namespace PolyBucket.Api.Features.Collections.GetCollectionById.Domain
{
    public class GetCollectionByIdQuery : IRequest<GetCollectionByIdResponseDto?>
    {
        public Guid Id { get; set; }
    }
} 