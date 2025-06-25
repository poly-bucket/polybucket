using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.Domain;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.Queries
{
    public class GetUserSettingsRequest
    {
        public Guid UserId { get; set; }
    }

    public class GetUserSettingsResponse
    {
        public UserSettings Settings { get; set; }
    }

    public class GetUserSettingsQueryHandler
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILogger<GetUserSettingsQueryHandler> _logger;

        public GetUserSettingsQueryHandler(PolyBucketDbContext context, ILogger<GetUserSettingsQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GetUserSettingsResponse> ExecuteAsync(GetUserSettingsRequest request)
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == request.UserId);

            if (settings == null)
            {
                throw new KeyNotFoundException($"Settings not found for user {request.UserId}");
            }

            return new GetUserSettingsResponse
            {
                Settings = settings
            };
        }
    }
} 