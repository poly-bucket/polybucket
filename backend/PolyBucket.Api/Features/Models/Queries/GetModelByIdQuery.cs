using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Repository;

namespace PolyBucket.Api.Features.Models.Queries
{
    public class GetModelByIdRequest
    {
        public Guid Id { get; set; }
    }

    public class GetModelByIdResponse
    {
        public Model Model { get; set; }
    }

    public class GetModelByIdQueryHandler
    {
        private readonly IModelsRepository _repository;
        private readonly ILogger<GetModelByIdQueryHandler> _logger;

        public GetModelByIdQueryHandler(IModelsRepository repository, ILogger<GetModelByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetModelByIdResponse> ExecuteAsync(GetModelByIdRequest request)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model with ID {Id}", request.Id);
                throw;
            }
        }
    }
} 