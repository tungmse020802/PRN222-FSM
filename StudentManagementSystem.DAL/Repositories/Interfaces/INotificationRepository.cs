using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int notificationId, int userAccountId, CancellationToken cancellationToken = default);
}
