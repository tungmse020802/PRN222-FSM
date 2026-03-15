using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.Repositories;

public sealed class NotificationRepository(INotificationDao dao) : INotificationRepository
{
    public Task<IReadOnlyList<Notification>> GetByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default) => dao.GetByUserAccountIdAsync(userAccountId, cancellationToken);
    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default) => dao.AddAsync(notification, cancellationToken);
    public Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default) => dao.AddRangeAsync(notifications, cancellationToken);
    public Task MarkAsReadAsync(int notificationId, int userAccountId, CancellationToken cancellationToken = default) => dao.MarkAsReadAsync(notificationId, userAccountId, cancellationToken);
}
