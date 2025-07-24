using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Data;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests
{
    [CollectionDefinition("TestCollection")]
    public class TestCollection : ICollectionFixture<TestCollectionFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class TestCollectionFixture : IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        public TestCollectionFixture()
        {
            var services = new ServiceCollection();
            TestDatabaseManager.ConfigureTestServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        public async Task InitializeAsync()
        {
            await TestDatabaseManager.EnsureTestDatabaseCreatedAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_serviceProvider is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
} 