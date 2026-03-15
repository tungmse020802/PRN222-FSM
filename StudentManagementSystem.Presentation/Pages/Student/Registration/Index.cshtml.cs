using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Presentation.Extensions;
using StudentManagementSystem.Presentation.Hubs;
using StudentManagementSystem.Presentation.Models;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace StudentManagementSystem.Presentation.Pages.Student.Registration;

[Authorize(Roles = AppRoles.Student)]
public sealed class IndexModel(
    IAcademicService academicService,
    IEnrollmentService enrollmentService,
    IRecommendationService recommendationService,
    IHubContext<NotificationHub> hubContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? SemesterId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public IReadOnlyList<Semester> Semesters { get; private set; } = [];
    public IReadOnlyList<CourseSection> AvailableSections { get; private set; } = [];
    public IReadOnlyList<CourseRegistrationSectionViewModel> RegistrationSections { get; private set; } = [];
    public IReadOnlyList<Enrollment> CurrentEnrollments { get; private set; } = [];
    public Semester? SelectedSemester { get; private set; }
    public AIRecommendation? LatestRecommendation { get; private set; }
    public int CurrentCredits { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        return await LoadPageAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostRegisterAsync(int courseSectionId, int? semesterId, string? searchTerm, CancellationToken cancellationToken)
    {
        var studentId = User.GetStudentId();
        var userAccountId = User.GetUserAccountId();
        if (!studentId.HasValue || !userAccountId.HasValue)
        {
            return Forbid();
        }

        var result = await enrollmentService.RegisterAsync(studentId.Value, userAccountId.Value, courseSectionId, cancellationToken);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] =
            result.Succeeded ? "Registration successful." : result.ErrorMessage ?? "Registration failed.";

        if (result.Succeeded)
        {
            await hubContext.SendRealtimeNotificationAsync(
                userAccountId.Value,
                new RealtimeNotificationPayload
                {
                    Title = "Registration successful",
                    Message = "Your course registration has been completed successfully.",
                    Type = "Registration",
                    Url = "/Student/Notifications"
                },
                cancellationToken);
        }

        return RedirectToPage(new { SemesterId = semesterId, SearchTerm = searchTerm });
    }

    public async Task<IActionResult> OnPostCancelAsync(int courseSectionId, int? semesterId, string? searchTerm, CancellationToken cancellationToken)
    {
        var studentId = User.GetStudentId();
        var userAccountId = User.GetUserAccountId();
        if (!studentId.HasValue || !userAccountId.HasValue)
        {
            return Forbid();
        }

        var result = await enrollmentService.CancelAsync(studentId.Value, userAccountId.Value, courseSectionId, cancellationToken);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] =
            result.Succeeded ? "Registration cancelled." : result.ErrorMessage ?? "Cancellation failed.";

        if (result.Succeeded)
        {
            await hubContext.SendRealtimeNotificationAsync(
                userAccountId.Value,
                new RealtimeNotificationPayload
                {
                    Title = "Registration cancelled",
                    Message = "Your course registration has been cancelled.",
                    Type = "Registration",
                    Url = "/Student/Notifications"
                },
                cancellationToken);
        }

        return RedirectToPage(new { SemesterId = semesterId, SearchTerm = searchTerm });
    }

    private async Task<IActionResult> LoadPageAsync(CancellationToken cancellationToken)
    {
        var studentId = User.GetStudentId();
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        Semesters = await academicService.GetSemestersAsync(null, cancellationToken);
        if (!SemesterId.HasValue)
        {
            SemesterId = Semesters.FirstOrDefault(x => x.Status == SemesterStatus.OpenForRegistration)?.SemesterId
                ?? Semesters.FirstOrDefault()?.SemesterId;
        }

        SelectedSemester = SemesterId.HasValue ? Semesters.FirstOrDefault(x => x.SemesterId == SemesterId.Value) : null;
        AvailableSections = await academicService.GetCourseSectionsAsync(SemesterId, null, SearchTerm, true, cancellationToken);
        CurrentEnrollments = SemesterId.HasValue
            ? await enrollmentService.GetStudentEnrollmentsAsync(studentId.Value, SemesterId, cancellationToken)
            : [];
        RegistrationSections = AvailableSections
            .Select(section => CourseRegistrationSectionViewModel.Create(section, CurrentEnrollments, DateTime.Today))
            .ToList();
        LatestRecommendation = SemesterId.HasValue
            ? await recommendationService.GetLatestAsync(studentId.Value, SemesterId.Value, cancellationToken)
            : null;

        CurrentCredits = CurrentEnrollments
            .Where(x => x.Status == EnrollmentStatus.Registered)
            .Sum(x => x.CourseSection?.Subject?.Credits ?? 0);

        return Page();
    }
}
