using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Presentation.Extensions;
using StudentManagementSystem.Presentation.Hubs;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace StudentManagementSystem.Presentation.Pages.Lecturer.Notifications;

[Authorize(Roles = AppRoles.Lecturer)]
public sealed class IndexModel(
    IAcademicService academicService,
    INotificationService notificationService,
    IHubContext<NotificationHub> hubContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? CourseSectionId { get; set; }

    [BindProperty]
    public ComposeInputModel Input { get; set; } = new();

    public IReadOnlyList<CourseSection> Sections { get; private set; } = [];
    public IReadOnlyList<Notification> Notifications { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        return await LoadPageAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostSendAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await LoadPageAsync(cancellationToken);
        }

        var lecturerId = User.GetLecturerId();
        if (!lecturerId.HasValue)
        {
            return Forbid();
        }

        Sections = await academicService.GetCourseSectionsAsync(null, lecturerId, null, null, cancellationToken);
        if (!Sections.Any(x => x.CourseSectionId == Input.CourseSectionId))
        {
            TempData["ErrorMessage"] = "Invalid course section.";
            return RedirectToPage();
        }

        var result = await notificationService.SendToSectionAsync(new NotificationComposeRequest
        {
            CourseSectionId = Input.CourseSectionId,
            Title = Input.Title,
            Message = Input.Message
        }, cancellationToken);

        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] =
            result.Succeeded ? "Notification sent successfully." : result.ErrorMessage ?? "Failed to send notification.";

        if (result.Succeeded)
        {
            await hubContext.SendRealtimeNotificationAsync(
                result.Data ?? [],
                new RealtimeNotificationPayload
                {
                    Title = Input.Title,
                    Message = Input.Message,
                    Type = "System"
                },
                cancellationToken);
        }

        return RedirectToPage(new { CourseSectionId = Input.CourseSectionId });
    }

    public async Task<IActionResult> OnPostReadAsync(int notificationId, CancellationToken cancellationToken)
    {
        var userAccountId = User.GetUserAccountId();
        if (!userAccountId.HasValue)
        {
            return Forbid();
        }

        await notificationService.MarkAsReadAsync(notificationId, userAccountId.Value, cancellationToken);
        return RedirectToPage(new { CourseSectionId });
    }

    private async Task<IActionResult> LoadPageAsync(CancellationToken cancellationToken)
    {
        var lecturerId = User.GetLecturerId();
        var userAccountId = User.GetUserAccountId();
        if (!lecturerId.HasValue || !userAccountId.HasValue)
        {
            return Forbid();
        }

        Sections = await academicService.GetCourseSectionsAsync(null, lecturerId, null, null, cancellationToken);
        Notifications = await notificationService.GetByUserAccountIdAsync(userAccountId.Value, cancellationToken);

        if (CourseSectionId.HasValue)
        {
            Input.CourseSectionId = CourseSectionId.Value;
        }
        else if (Input.CourseSectionId == 0 && Sections.Count > 0)
        {
            Input.CourseSectionId = Sections[0].CourseSectionId;
        }

        return Page();
    }

    public sealed class ComposeInputModel
    {
        [Range(1, int.MaxValue)]
        public int CourseSectionId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
    }
}
