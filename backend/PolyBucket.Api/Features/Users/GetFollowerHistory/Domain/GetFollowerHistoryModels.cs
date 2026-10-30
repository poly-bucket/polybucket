using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.GetFollowerHistory.Domain;

public class GetFollowerHistoryQuery
{
    public Guid UserId { get; set; }
    public int Days { get; set; } = 30;
    public Guid RequestingUserId { get; set; }
}

public class FollowerHistoryPointDto
{
    public DateTime Date { get; set; }
    public int FollowerCount { get; set; }
}

public class GetFollowerHistoryResult
{
    public int Days { get; set; }
    public IReadOnlyList<FollowerHistoryPointDto> Points { get; set; } = Array.Empty<FollowerHistoryPointDto>();
}

public enum GetFollowerHistoryOutcome
{
    Success,
    InvalidDays,
    UserNotFound,
    Forbidden
}
