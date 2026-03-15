using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.BLL.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<Notification>> GetByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyList<int>>> SendToUserAccountsAsync(NotificationComposeRequest request, NotificationType type, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyList<int>>> SendToSectionAsync(NotificationComposeRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> MarkAsReadAsync(int notificationId, int userAccountId, CancellationToken cancellationToken = default);
}
