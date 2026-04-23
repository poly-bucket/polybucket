using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserPrinters.Domain;

namespace PolyBucket.Api.Features.Users.GetUserPrinters.Repository;

public class GetUserPrintersRepository : IGetUserPrintersRepository
{
    public Task<GetUserPrintersResult> GetUserPrintersAsync(GetUserPrintersQuery query, CancellationToken cancellationToken = default)
    {
        var printers = new List<UserPrinterListItemDto>();
        var filaments = new List<UserFilamentListItemDto>();
        const int totalPrinterCount = 0;
        const int totalFilamentCount = 0;

        return Task.FromResult(new GetUserPrintersResult
        {
            Printers = printers,
            Filaments = filaments,
            TotalPrinterCount = totalPrinterCount,
            TotalFilamentCount = totalFilamentCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)Math.Max(totalPrinterCount, totalFilamentCount) / query.PageSize)
        });
    }
}
