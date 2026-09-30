namespace PolyBucket.Api.Features.Notifications.Domain;

public static class NotificationLimits
{
    public const int MaxTitleLength = 200;
    public const int MaxMessageLength = 1000;
    public const int MaxActionUrlLength = 500;
    public const int MaxDedupeKeyLength = 200;
    public const int MaxPageSize = 50;
}
