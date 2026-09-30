using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Search;

public class SearchCapabilitiesTests
{
    [Fact(DisplayName = "When the database is not PostgreSQL, search uses basic matching without probing for extensions.")]
    public async Task GetModeAsync_NonPostgresProvider_ReturnsBasic()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new PolyBucketDbContext(options);
        var capabilities = new SearchCapabilities(TimeProvider.System, NullLogger<SearchCapabilities>.Instance);

        // Act
        var mode = await capabilities.GetModeAsync(context);

        // Assert
        mode.ShouldBe(SearchTextMode.Basic);
    }
}
