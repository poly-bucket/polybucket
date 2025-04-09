using MediatR;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.GetModels.Domain;

namespace PolyBucket.Api.Features.Models.GetModels.Domain
{
    public class GetModelsQueryHandler : IRequestHandler<GetModelsQuery, GetModelsResponse>
    {
        private readonly IModelsRepository _repository;
        private readonly ILogger<GetModelsQueryHandler> _logger;

        public GetModelsQueryHandler(IModelsRepository repository, ILogger<GetModelsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetModelsResponse> Handle(GetModelsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (models, totalCount) = await _repository.GetModelsAsync(request.Page, request.Take);
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.Take);

                return new GetModelsResponse
                {
                    Models = models,
                    TotalCount = totalCount,
                    Page = request.Page,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting models for page {Page}", request.Page);
                throw;
            }
        }
    }
} 