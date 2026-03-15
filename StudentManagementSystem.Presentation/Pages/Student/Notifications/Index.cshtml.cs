using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Presentation.Extensions;
using StudentManagementSystem.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Student.Notifications;

[Authorize(Roles = AppRoles.Student)]
public sealed class IndexModel(INotificationService notificationService) : PageModel
{
    public IReadOnlyList<StudentManagementSystem.Shared.Entities.Notification> Notifications { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userAccountId = User.GetUserAccountId();
        if (!userAccountId.HasValue)
        {
            return Forbid();
        }

        Notifications = await notificationService.GetByUserAccountIdAsync(userAccountId.Value, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostReadAsync(int notificationId, CancellationToken cancellationToken)
    {
        var userAccountId = User.GetUserAccountId();
        if (!userAccountId.HasValue)
        {
            return Forbid();
        }

        await notificationService.MarkAsReadAsync(notificationId, userAccountId.Value, cancellationToken);
        return RedirectToPage();
    }
}
