using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Repository;
using System;

namespace PolyBucket.Api.Features.Models.Queries
{
    public class GetModelsRequest
    {
        public int Page { get; set; } = 1;
        public int Take { get; set; } = 10;
    }

    public class GetModelsResponse
    {
        public IEnumerable<Model> Models { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetModelsQueryHandler
    {
        private readonly IModelsRepository _repository;
        private readonly ILogger<GetModelsQueryHandler> _logger;

        public GetModelsQueryHandler(IModelsRepository repository, ILogger<GetModelsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetModelsResponse> ExecuteAsync(GetModelsRequest request)
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