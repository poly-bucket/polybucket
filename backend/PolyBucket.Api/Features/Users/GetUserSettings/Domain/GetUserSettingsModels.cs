using System;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Api.Features.Users.GetUserSettings.Domain;

public class GetUserSettingsRequest
{
    public Guid UserId { get; set; }
}

public class GetUserSettingsResult
{
    public required UserSettings Settings { get; set; }
}
