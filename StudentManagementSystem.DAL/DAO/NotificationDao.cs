using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Data;
using StudentManagementSystem.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.DAL.DAO;

public sealed class NotificationDao(IDbContextFactory<StudentManagementDbContext> contextFactory) : INotificationDao
{
    public async Task<IReadOnlyList<Notification>> GetByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Notifications.AsNoTracking()
            .Where(x => x.UserAccountId == userAccountId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(int notificationId, int userAccountId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await context.Notifications.FirstOrDefaultAsync(
            x => x.NotificationId == notificationId && x.UserAccountId == userAccountId,
            cancellationToken);

        if (existing is null)
        {
            return;
        }

        existing.IsRead = true;
        await context.SaveChangesAsync(cancellationToken);
    }
}
