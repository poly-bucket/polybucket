using MediatR;
using System;

namespace PolyBucket.Api.Features.Collections.DeleteCollection.Domain
{
    public class DeleteCollectionCommand : IRequest
    {
        public Guid Id { get; set; }
    }
} 