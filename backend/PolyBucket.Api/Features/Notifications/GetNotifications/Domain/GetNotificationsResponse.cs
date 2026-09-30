using System.Collections.Generic;
using PolyBucket.Api.Features.Notifications.Domain;

namespace PolyBucket.Api.Features.Notifications.GetNotifications.Domain;

public class GetNotificationsResponse
{
    public List<NotificationDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
