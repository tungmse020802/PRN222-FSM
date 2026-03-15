using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.BLL.Services;

public sealed class NotificationService(INotificationRepository notificationRepository, IEnrollmentRepository enrollmentRepository) : INotificationService
{
    public Task<IReadOnlyList<Notification>> GetByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default) =>
        notificationRepository.GetByUserAccountIdAsync(userAccountId, cancellationToken);

    public async Task<ServiceResult<IReadOnlyList<int>>> SendToUserAccountsAsync(NotificationComposeRequest request, NotificationType type, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
        {
            return ServiceResult<IReadOnlyList<int>>.Failure("Notification content is required.");
        }

        var recipientIds = request.RecipientUserAccountIds.Distinct().ToList();
        if (recipientIds.Count == 0)
        {
            return ServiceResult<IReadOnlyList<int>>.Failure("No notification recipients were selected.");
        }

        var notifications = recipientIds.Select(userAccountId => new Notification
        {
            UserAccountId = userAccountId,
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await notificationRepository.AddRangeAsync(notifications, cancellationToken);
        return ServiceResult<IReadOnlyList<int>>.Success(recipientIds);
    }

    public async Task<ServiceResult<IReadOnlyList<int>>> SendToSectionAsync(NotificationComposeRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.CourseSectionId.HasValue)
        {
            return ServiceResult<IReadOnlyList<int>>.Failure("Course section is required.");
        }

        var recipients = await enrollmentRepository.GetSectionRecipientUserAccountIdsAsync(request.CourseSectionId.Value, cancellationToken);
        return await SendToUserAccountsAsync(new NotificationComposeRequest
        {
            Title = request.Title,
            Message = request.Message,
            RecipientUserAccountIds = recipients
        }, NotificationType.System, cancellationToken);
    }

    public async Task<ServiceResult> MarkAsReadAsync(int notificationId, int userAccountId, CancellationToken cancellationToken = default)
    {
        await notificationRepository.MarkAsReadAsync(notificationId, userAccountId, cancellationToken);
        return ServiceResult.Success();
    }
}
