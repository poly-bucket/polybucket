using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Repository;

namespace PolyBucket.Api.Features.Models.Queries
{
    public class GetModelByIdQueryHandler : IRequestHandler<GetModelByIdQuery, GetModelByIdResponse>
    {
        private readonly IModelsRepository _repository;
        private readonly ILogger<GetModelByIdQueryHandler> _logger;

        public GetModelByIdQueryHandler(IModelsRepository repository, ILogger<GetModelByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetModelByIdResponse> Handle(GetModelByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var model = await _repository.GetModelByIdAsync(request.Id);
            
                if (model == null)
                {
                    throw new KeyNotFoundException($"Model with ID {request.Id} not found");
                }

                return new GetModelByIdResponse
                {
                    Model = model
                };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error getting model with ID {Id}", request.Id);
                throw;
            }
        }
    }
} 