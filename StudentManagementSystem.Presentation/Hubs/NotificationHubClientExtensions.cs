using Microsoft.AspNetCore.SignalR;

namespace StudentManagementSystem.Presentation.Hubs;

public static class NotificationHubClientExtensions
{
    public const string ReceiveNotificationMethod = "ReceiveNotification";
    public const string RefreshNotificationsMethod = "RefreshNotifications";

    public static Task SendRealtimeNotificationAsync(
        this IHubContext<NotificationHub> hubContext,
        int userAccountId,
        RealtimeNotificationPayload payload,
        CancellationToken cancellationToken = default) =>
        hubContext.SendRealtimeNotificationAsync([userAccountId], payload, cancellationToken);

    public static Task SendRealtimeNotificationAsync(
        this IHubContext<NotificationHub> hubContext,
        IEnumerable<int> userAccountIds,
        RealtimeNotificationPayload payload,
        CancellationToken cancellationToken = default)
    {
        var recipients = userAccountIds
            .Where(userAccountId => userAccountId > 0)
            .Select(userAccountId => userAccountId.ToString())
            .Distinct()
            .ToArray();

        if (recipients.Length == 0)
        {
            return Task.CompletedTask;
        }

        return Task.WhenAll(
            hubContext.Clients.Users(recipients).SendAsync(ReceiveNotificationMethod, payload, cancellationToken),
            hubContext.Clients.Users(recipients).SendAsync(RefreshNotificationsMethod, cancellationToken));
    }
}
