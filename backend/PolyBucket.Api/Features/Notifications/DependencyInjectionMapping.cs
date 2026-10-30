using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Api.Features.Notifications.Repository;

namespace PolyBucket.Api.Features.Notifications;

public static class DependencyInjectionMapping
{
    public static IServiceCollection AddNotificationsFeature(this IServiceCollection services)
    {
        services.AddTransient<INotificationPublisherRepository, NotificationPublisherRepository>();
        services.AddScoped<INotificationPublisher, NotificationPublisher>();
        services.AddTransient<GetNotifications.Repository.IGetNotificationsRepository, GetNotifications.Repository.GetNotificationsRepository>();
        services.AddTransient<GetUnreadNotificationCount.Repository.IGetUnreadNotificationCountRepository, GetUnreadNotificationCount.Repository.GetUnreadNotificationCountRepository>();
        services.AddTransient<MarkNotificationRead.Repository.IMarkNotificationReadRepository, MarkNotificationRead.Repository.MarkNotificationReadRepository>();
        services.AddTransient<MarkAllNotificationsRead.Repository.IMarkAllNotificationsReadRepository, MarkAllNotificationsRead.Repository.MarkAllNotificationsReadRepository>();
        services.AddScoped<GetNotifications.Domain.IGetNotificationsService, GetNotifications.Domain.GetNotificationsService>();
        services.AddScoped<GetUnreadNotificationCount.Domain.IGetUnreadNotificationCountService, GetUnreadNotificationCount.Domain.GetUnreadNotificationCountService>();
        services.AddScoped<MarkNotificationRead.Domain.IMarkNotificationReadService, MarkNotificationRead.Domain.MarkNotificationReadService>();
        services.AddScoped<MarkAllNotificationsRead.Domain.IMarkAllNotificationsReadService, MarkAllNotificationsRead.Domain.MarkAllNotificationsReadService>();
        return services;
    }
}
