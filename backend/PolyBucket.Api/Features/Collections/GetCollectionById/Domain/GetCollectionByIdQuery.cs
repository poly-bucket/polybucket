using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using System;

namespace PolyBucket.Api.Features.Collections.GetCollectionById.Domain
{
    public class GetCollectionByIdQuery : IRequest<Collection?>
    {
        public Guid Id { get; set; }
    }
} 