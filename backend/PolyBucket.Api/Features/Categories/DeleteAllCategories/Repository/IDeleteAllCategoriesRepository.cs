using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteAllCategories.Repository
{
    public interface IDeleteAllCategoriesRepository
    {
        Task<int> DeleteAllCategoriesAndReturnCountAsync(Guid deletedById, CancellationToken cancellationToken = default);
    }
}
