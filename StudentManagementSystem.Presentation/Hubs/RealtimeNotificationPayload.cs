namespace StudentManagementSystem.Presentation.Hubs;

public sealed class RealtimeNotificationPayload
{
    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string? Url { get; init; }

    public string SentAt { get; init; } = DateTimeOffset.UtcNow.ToString("O");
}
