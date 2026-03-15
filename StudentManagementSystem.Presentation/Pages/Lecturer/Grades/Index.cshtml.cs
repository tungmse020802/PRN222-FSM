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

namespace StudentManagementSystem.Presentation.Pages.Lecturer.Grades;

[Authorize(Roles = AppRoles.Lecturer)]
public sealed class IndexModel(
    IAcademicService academicService,
    IEnrollmentService enrollmentService,
    IHubContext<NotificationHub> hubContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? CourseSectionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public IReadOnlyList<CourseSection> Sections { get; private set; } = [];
    public IReadOnlyList<Enrollment> GradeBook { get; private set; } = [];
    public CourseSection? SelectedSection { get; private set; }
    public int TotalStudents { get; private set; }
    public int GradedStudents { get; private set; }
    public int PassedStudents { get; private set; }
    public int AtRiskStudents { get; private set; }
    public decimal AverageTotalScore { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var lecturerId = User.GetLecturerId();
        if (!lecturerId.HasValue)
        {
            return Forbid();
        }

        Sections = await academicService.GetCourseSectionsAsync(null, lecturerId, null, null, cancellationToken);

        if (!CourseSectionId.HasValue && Sections.Count > 0)
        {
            CourseSectionId = Sections[0].CourseSectionId;
        }

        if (CourseSectionId.HasValue)
        {
            var ownsSection = Sections.Any(x => x.CourseSectionId == CourseSectionId.Value);
            if (!ownsSection)
            {
                TempData["ErrorMessage"] = "You do not have permission for that course section.";
                CourseSectionId = null;
            }
            else
            {
                SelectedSection = Sections.FirstOrDefault(x => x.CourseSectionId == CourseSectionId.Value);
                var fullGradeBook = await enrollmentService.GetGradeBookAsync(CourseSectionId.Value, cancellationToken);
                TotalStudents = fullGradeBook.Count;
                GradedStudents = fullGradeBook.Count(x => x.GradeRecord is not null);
                PassedStudents = fullGradeBook.Count(x => x.GradeRecord?.IsPassed == true);
                AtRiskStudents = fullGradeBook.Count(x => x.GradeRecord is not null && x.GradeRecord.IsPassed == false);
                AverageTotalScore = fullGradeBook.Count(x => x.GradeRecord is not null) == 0
                    ? 0m
                    : Math.Round(fullGradeBook.Where(x => x.GradeRecord is not null).Average(x => x.GradeRecord!.TotalScore), 2);

                GradeBook = string.IsNullOrWhiteSpace(SearchTerm)
                    ? fullGradeBook
                    : fullGradeBook.Where(MatchesSearchTerm).ToList();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(
        int courseSectionId,
        int studentId,
        string? searchTerm,
        decimal? assignmentScore,
        decimal? quizScore,
        decimal? midtermScore,
        decimal? finalScore,
        CancellationToken cancellationToken)
    {
        var lecturerUserAccountId = User.GetUserAccountId();
        if (!lecturerUserAccountId.HasValue)
        {
            return Forbid();
        }

        var result = await enrollmentService.SaveGradeAsync(
            lecturerUserAccountId.Value,
            new GradeUpsertRequest
            {
                CourseSectionId = courseSectionId,
                StudentId = studentId,
                AssignmentScore = assignmentScore,
                QuizScore = quizScore,
                MidtermScore = midtermScore,
                FinalScore = finalScore
            },
            cancellationToken);

        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] =
            result.Succeeded ? "Grade saved successfully." : result.ErrorMessage ?? "Failed to save grade.";

        if (result.Succeeded)
        {
            await hubContext.SendRealtimeNotificationAsync(
                result.Data,
                new RealtimeNotificationPayload
                {
                    Title = "New grade available",
                    Message = "A lecturer has updated one of your grades.",
                    Type = "Grade",
                    Url = "/Student/Notifications"
                },
                cancellationToken);
        }

        return RedirectToPage(new { CourseSectionId = courseSectionId, SearchTerm = searchTerm });
    }

    private bool MatchesSearchTerm(Enrollment enrollment)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            return true;
        }

        var search = SearchTerm.Trim();
        return (enrollment.Student?.StudentCode?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
               || (enrollment.Student?.UserAccount?.FullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
               || (enrollment.Student?.UserAccount?.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
