using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Presentation.Extensions;
using StudentManagementSystem.Presentation.Hubs;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace StudentManagementSystem.Presentation.Pages.Student.Recommendations;

[Authorize(Roles = AppRoles.Student)]
public sealed class IndexModel(
    IAcademicService academicService,
    IRecommendationService recommendationService,
    IHubContext<NotificationHub> hubContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? SemesterId { get; set; }

    public IReadOnlyList<Semester> Semesters { get; private set; } = [];
    public Semester? SelectedSemester { get; private set; }
    public AIRecommendation? Recommendation { get; private set; }
    public IReadOnlyList<string> RecommendedSubjects { get; private set; } = [];
    public IReadOnlyList<string> AvoidSubjects { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        return await LoadPageAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostGenerateAsync(int semesterId, CancellationToken cancellationToken)
    {
        var studentId = User.GetStudentId();
        var userAccountId = User.GetUserAccountId();
        if (!studentId.HasValue || !userAccountId.HasValue)
        {
            return Forbid();
        }

        var result = await recommendationService.GenerateAsync(studentId.Value, userAccountId.Value, semesterId, cancellationToken);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] =
            result.Succeeded ? "Study recommendation generated successfully." : result.ErrorMessage ?? "Failed to generate recommendation.";

        if (result.Succeeded)
        {
            await hubContext.SendRealtimeNotificationAsync(
                userAccountId.Value,
                new RealtimeNotificationPayload
                {
                    Title = "New study recommendation",
                    Message = "Your suggested study plan has been updated.",
                    Type = "Recommendation",
                    Url = "/Student/Recommendations"
                },
                cancellationToken);
        }

        return RedirectToPage(new { SemesterId = semesterId });
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

        if (SemesterId.HasValue)
        {
            SelectedSemester = Semesters.FirstOrDefault(x => x.SemesterId == SemesterId.Value);
            Recommendation = await recommendationService.GetLatestAsync(studentId.Value, SemesterId.Value, cancellationToken);
            RecommendedSubjects = SplitItems(Recommendation?.RecommendedSubjects);
            AvoidSubjects = SplitItems(Recommendation?.AvoidSubjects);
        }

        return Page();
    }

    private static IReadOnlyList<string> SplitItems(string? rawText) =>
        string.IsNullOrWhiteSpace(rawText)
            ? []
            : rawText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
