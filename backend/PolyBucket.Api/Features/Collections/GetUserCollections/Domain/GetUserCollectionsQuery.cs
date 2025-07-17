using MediatR;
using PolyBucket.Api.Features.Collections.Domain;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Domain
{
    public class GetUserCollectionsQuery : IRequest<IEnumerable<Collection>>
    {
        // UserId will be extracted from the HTTP context
    }
} 