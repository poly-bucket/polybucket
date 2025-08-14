using MediatR;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.GetModels.Domain;
using PolyBucket.Api.Common.Storage;

namespace PolyBucket.Api.Features.Models.GetModels.Domain
{
    public class GetModelsQueryHandler(IModelsRepository repository, ILogger<GetModelsQueryHandler> logger, IStorageService storageService) : IRequestHandler<GetModelsQuery, GetModelsResponse>
    {
        private readonly IModelsRepository _repository = repository;
        private readonly ILogger<GetModelsQueryHandler> _logger = logger;
        private readonly IStorageService _storageService = storageService;

        public async Task<GetModelsResponse> Handle(GetModelsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (models, totalCount) = await _repository.GetModelsAsync(request.Page, request.Take);
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.Take);

                // Generate fresh presigned URLs for all models
                foreach (var model in models)
                {
                    // Generate fresh presigned URL for FileUrl (stored as object key)
                    if (!string.IsNullOrEmpty(model.FileUrl))
                    {
                        model.FileUrl = await _storageService.GetPresignedUrlAsync(model.FileUrl, TimeSpan.FromHours(1), cancellationToken);
                    }

                    // Generate fresh presigned URL for ThumbnailUrl (stored as object key)
                    if (!string.IsNullOrEmpty(model.ThumbnailUrl))
                    {
                        model.ThumbnailUrl = await _storageService.GetPresignedUrlAsync(model.ThumbnailUrl, TimeSpan.FromHours(1), cancellationToken);
                    }
                }

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