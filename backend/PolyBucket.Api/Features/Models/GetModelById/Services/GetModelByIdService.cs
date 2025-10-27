using System;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.GetModelById.Repository;

namespace PolyBucket.Api.Features.Models.GetModelById.Services
{
    public class GetModelByIdService : IGetModelByIdService
    {
        private readonly IGetModelByIdRepository _repository;

        public GetModelByIdService(IGetModelByIdRepository repository)
        {
            _repository = repository;
        }

        public async Task<Model?> GetModelByIdAsync(Guid id)
        {
            return await _repository.GetModelByIdAsync(id);
        }
    }
}
